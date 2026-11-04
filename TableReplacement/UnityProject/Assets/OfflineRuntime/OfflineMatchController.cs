// New offline composition. Physics/rule/AI decisions remain in the recovered classes.
using System;
using System.Globalization;
using UnityEngine;

namespace Pool3D.Offline
{
    [DefaultExecutionOrder(100)] // Read the cue after the original CameraSF.LateUpdate.
    public partial class OfflineMatchController : MonoBehaviour, RuleListener
    {
        public static OfflineMatchController Instance { get; private set; }
        public PoolTable M_PoolTable { get; private set; }
        public PoolRule M_PoolRule { get; private set; }
        public PoolAIBase AI { get; private set; }
        public bool IsNet { get { return false; } } // This host provides no network mode.
        public bool InMatch { get; private set; }
        public bool AgainstComputer { get; private set; }
        public bool NineBall { get; private set; }
        public bool Paused { get; private set; }
        public bool HasResult { get; private set; }
        public string Status { get; private set; }
        public string InitializationError { get; private set; }
        public float AimAngle;
        public float StrengthUnit = 0.5f;
        public float SpinX;
        public float SpinY;
        public float RoundTime { get; private set; }
        public float DisplayForceUnit { get; private set; }
        public float CueForce { get; private set; }
        public int CueId { get { return Var.DEFAULT_CUE_ID; } }
        public float CueSpin { get; private set; }
        public float CueTime { get; private set; }
        public int SelectedPocket { get; private set; } = -1;
        public int Winner { get; private set; } = -1;
        public bool CuePulling { get; private set; }
        public float CuePullUnit { get; private set; }
        public long TurnSerial { get; private set; }
        public bool CanCameraAim { get { return CanShoot && !CuePulling && !IsView2D(); } }
        public VSRule.GameRoundState Round { get { return (VSRule.GameRoundState)M_PoolTable.roundstate; } }
        public bool ComputerTurn { get { return InMatch && AgainstComputer && Round.currentPlayer == 1 && !HasResult; } }
        public bool CanShoot { get { return InMatch && !pendingTurn && !Paused && !HasResult && !ComputerTurn && Round.state == PoolRule.STATE_WAITING && !IsTargetPocketPicking; } }
        private Pool3D.Offline.Geometry.TableGeometryConfig tableGeometry;
        private int ruleBits;
        private bool pendingTurn;
        private OfflineSceneView view;
        private OfflineMatchSummary summary;
        private Pool3D.Offline.Physics.SpinPhysicsConfig spinSettings;
        public bool UseEnhancedSpin { get; set; }
        public bool EnhancedSpinActive { get { return M_PoolTable != null && M_PoolTable.EnhancedSpinEnabled; } }
        public float MaximumTipOffset { get { return spinSettings.maxTipOffset; } }
        public void SetSpinContact(float left, float top)
        {
            if (!CanShoot || CuePulling) return;
            Vector2 point = Pool3D.Offline.Physics.SpinPhysicsMath.ClampContact(new Vector2(left, top), MaximumTipOffset);
            SpinX = point.x; SpinY = point.y;
        }

        private void Awake()
        {
            if (Instance != null) throw new InvalidOperationException("Only one offline match host is supported.");
            Instance = this;
            gameObject.AddComponent<OfflineControls>();
            try
            {
                // Bootstrap selects the profile BEFORE AddComponent invokes this Awake.
                if (!OfflinePreferences.IsOffline) throw new InvalidOperationException("Offline profile was not selected before initialization.");
                var cues = OfflineConfigTable.Parse(RequireText("OfflineConfig/Cue"));
                var cue = cues.Find(Var.DEFAULT_CUE_ID); // Original level-0 Wood Cue, no invented stats.
                CueForce = int.Parse(cue["m_Force"], CultureInfo.InvariantCulture);
                CueSpin = int.Parse(cue["m_Spin"], CultureInfo.InvariantCulture);
                CueTime = int.Parse(cue["m_Time"], CultureInfo.InvariantCulture);
                spinSettings = Pool3D.Offline.Physics.SpinPhysicsConfig.Load();
                UseEnhancedSpin = spinSettings.enabledByDefault;
                tableGeometry = Pool3D.Offline.Geometry.TableGeometryConfig.Load(System.IO.Path.Combine(Application.streamingAssetsPath,"TableGeometry.json"));
                var modes = OfflineConfigTable.Parse(RequireText("OfflineConfig/VsMode"));
                ruleBits = int.Parse(modes.Find(1)["m_Rule"], CultureInfo.InvariantCulture); // Original StartPlay_Offline selects ID1.
                summary = LoadSummary();
                view = new OfflineSceneView(transform,()=>CanCameraAim);
                view.PrepareResources();
                gameObject.AddComponent<OfflineAudio>();
                Status = "选择离线对局";
            }
            catch (Exception error) { InitializationError = error.Message; Debug.LogException(error); }
        }

        private static string RequireText(string path)
        {
            TextAsset asset = Resources.Load<TextAsset>(path);
            if (asset == null) throw new InvalidOperationException("Missing original config: " + path);
            return asset.text;
        }

        public void StartMatch(bool computer, bool nineBall)
        {
            try { StartMatchCore(computer, nineBall); }
            catch (Exception error) { InMatch = false; InitializationError = error.Message; Debug.LogException(error); }
        }

        private void StartMatchCore(bool computer, bool nineBall)
        {
            if (InitializationError != null) return;
            ReturnToMenu();
            AgainstComputer = computer;
            NineBall = nineBall;
            Winner = -1;
            HasResult = false;
            Paused = false;
            AimAngle = 0f;
            VSRule.not_ai_player_count = computer ? 1 : 2;
            M_PoolRule = nineBall ? (PoolRule)new NineBallVsRule() : new EightBallVsRule();
            M_PoolRule.ConfigureGeometry(tableGeometry);
            M_PoolRule.SetListener(this);
            M_PoolTable = new PoolTable();
            M_PoolTable.ConfigureSpin(UseEnhancedSpin ? spinSettings : null);
            PoolRule.SetBlockerAndHolePosition(false); // Original local 8/9-ball ResetPoolTable, not tutorial.
            M_PoolTable.setRule(M_PoolRule);
            M_PoolTable.reset();
            Round.rule = ruleBits;
            Round.currentPlayer = 0; // Original offline Openning overrides the reset's random first player.
            AI = nineBall ? (PoolAIBase)new PoolAI9Ball(this) : new PoolAI8Ball(this);
            // Original local SetAILv uses player level to choose difficulty; no new AI curve.
            AI.SetAILv(false, 1, Var.GetCueForce(CueForce, 1f), 0);
            view.Build(M_PoolTable);
            InMatch = true;
            BeginTurn();
        }

        public void ReturnToMenu()
        {
            CancelCuePull();
            InMatch = false;
            Paused = false;
            pendingTurn = false;
            if (AI != null) AI.Reset();
            if (view != null) view.Clear();
            Status = "选择离线对局";
            OfflinePreferences.Save();
        }

        public void TogglePause() { if (InMatch && !HasResult) Paused = !Paused; }
        public void Restart() { StartMatch(AgainstComputer, NineBall); }

        private void BeginTurn()
        {
            if (!InMatch || HasResult) return;
            CancelCuePull();
            TurnSerial++;
            pendingTurn = false;
            RoundTime = Var.STANDARD_ROUNDTIME_LOC + CueTime;
            SpinX = 0f; SpinY = 0f; DisplayForceUnit = 0f;
            SelectedPocket = -1;
            Round.callPocket_HoleId = -1;
            AI.Reset();
            Status = PlayerDisplayName(Round.currentPlayer) + " 的回合";
            if (ComputerTurn) AI.m_AIStateMgr.ChangeState(AIStateMgr.g_ThinkState);
        }

        private void Update()
        {
            if (!InMatch || Paused || HasResult || InitializationError != null) return;
            try
            {
                if (pendingTurn) BeginTurn(); // Defer callbacks until the previous rule method has returned.
                if (Round.state == PoolRule.STATE_RUNNING) M_PoolTable.tick(Time.deltaTime);
                else if (Round.state == PoolRule.STATE_WAITING)
                {
                    RoundTime -= Time.deltaTime; // Same timer/callback order as PoolCtr.HandleRoundTime.
                    if (RoundTime <= 0f) M_PoolRule.onTimeOver(Round, M_PoolTable.balls);
                    if (!pendingTurn && ComputerTurn) AI.Update();
                }
                view.UpdateBalls(M_PoolTable, AimAngle, InMatch && !HasResult && Round.state == PoolRule.STATE_WAITING,
                    CuePulling ? CuePullUnit : ComputerTurn ? DisplayForceUnit : 0f,
                    EnhancedSpinActive && !ComputerTurn ? (Vector2?)new Vector2(SpinX, SpinY) : null);
            }
            catch (Exception error)
            {
                // Preserve the failed state for diagnosis; do not guess a new rule result.
                Paused = true;
                InitializationError = error.Message;
                Debug.LogException(error);
            }
        }

        public string PlayerDisplayName(int player) { return player == 0 ? "玩家一" : AgainstComputer ? "电脑" : "玩家二"; }
        private void LateUpdate()
        {
            if (view != null && CanCameraAim) AimAngle = view.ReadCueAngle();
        }
        public bool IsView2D() { return view != null && view.IsOverview; }
        public Camera MatchCamera { get { return view.Camera; } }
        public void OrbitCamera(Vector2 delta) { view.Orbit(delta); }
        public void ZoomCamera(float delta) { view.Zoom(delta); }
        public void ToggleView() { if (InMatch) { CancelCuePull(); view.SetOverview(!view.IsOverview, AimAngle); } }
        public void ResetView() { if (InMatch) view.ResetCamera(AimAngle); }
        public void FineAdjust(float angle) { if (CanShoot) { if (IsView2D()) AimAngle += angle; else view.FineAdjust(angle); } }
        public bool HitCue(Vector2 screen, out Vector2 backward) { return view.HitCue(screen, out backward); }
        public void PreviewCuePull(float force) { if (CanShoot) { CuePulling = true; CuePullUnit = Mathf.Clamp01(force); } }
        public void CancelCuePull() { CuePulling = false; CuePullUnit = 0f; }
        public void ReleaseCuePull(float force)
        {
            bool canFire = CuePulling && CanShoot && force >= 0.0001f;
            CancelCuePull();
            if (canFire) { StrengthUnit = force; Shoot(); }
        }
        public void EndAimDrag() { view.EndDrag(); }
        public void OnCameraFineAdjustment(float delta) { AimAngle += delta / 25f; } // Local observable counterpart of the original remote NGUI knob callback.

        public void Shoot()
        {
            if (!CanShoot) return;
            float sx = EnhancedSpinActive ? SpinX : Var.GetCueSpin(CueSpin, SpinX);
            float sy = EnhancedSpinActive ? SpinY : Var.GetCueSpin(CueSpin, SpinY);
            M_PoolTable.click(Var.GetCueForce(CueForce, StrengthUnit), sx, sy, AimAngle);
        }

        public bool CanPlaceCueBall { get { return CanShoot && (Round.isFreeBall || Round.campstate == 0); } }
        public void MoveCueBall(float x, float y)
        {
            if (!CanPlaceCueBall) return;
            PoolBall ball = M_PoolTable.balls[0];
            // Original PoolCtr.MoveBall validates and reverts each axis separately.
            float oldX = ball.x, oldY = ball.y;
            ball.x = x;
            if (!M_PoolRule.checkFreeballArea(Round, ball) || !M_PoolRule.canPut(M_PoolTable.balls, 0, ball.x, ball.y)) ball.x = oldX;
            ball.y = y;
            if (!M_PoolRule.checkFreeballArea(Round, ball) || !M_PoolRule.canPut(M_PoolTable.balls, 0, ball.x, ball.y)) ball.y = oldY;
        }

        public bool IsTargetPocketPicking
        {
            get
            {
                if (!InMatch || pendingTurn || SelectedPocket >= 0 || !((VSRule)M_PoolRule).IsCallPocketBall(Round)) return false;
                int remaining = 0;
                // Original PoolCtr.UpdateLeftBalls_8Ball/_9Ball (0xAECA14/0xAECE70),
                // not the rule's legal-first-target predicate (different for nine-ball).
                for (int i = 1; i < M_PoolTable.balls.Length; i++)
                {
                    PoolBall ball = M_PoolTable.balls[i];
                    if (NineBall) { if (ball.isAvailable) remaining++; }
                    else if ((uint)Round.campstate < 2u) remaining++;
                    else if (Round.campstate == 2 && ball.isAvailable && (IsSelfTurn ? ball.id < 9 : ball.id >= 8)) remaining++;
                    else if (Round.campstate == 3 && ball.isAvailable && (IsSelfTurn ? ball.id >= 8 : ball.id < 9)) remaining++;
                }
                return remaining == 1;
            }
        }

        public void OnPotBall(PoolBall ball) { Status = "进袋：" + ball.id; }
        public void OnRoundOver() { /* Original callback has only tutorial handling, not used in 8/9-ball. */ }
        public void ConfirmCamp() { Status = "花色已确定"; }
        public void SwitchPlayer()
        {
            // Original PoolCtr._SwitchPlayer, with UI/ads/network effects isolated.
            Round.currentPlayer = unchecked(1 - Round.currentPlayer);
            Round.combo = 0;
            pendingTurn = true;
        }
        public void Tip(string text) { Status = text; }
        public void OnContinueTurn() { pendingTurn = true; }
        public void OnWin(int player)
        {
            if (HasResult) return;
            Winner = player;
            HasResult = true;
            pendingTurn = false;
            Status = PlayerDisplayName(player) + " 获胜";
            if (player == 0) summary.playerOneWins++; else summary.playerTwoWins++;
            summary.completedMatches++;
            summary.lastWinner = player;
            summary.lastMode = NineBall ? "9-ball" : "8-ball";
            OfflinePreferences.SetString("MatchSummary", JsonUtility.ToJson(summary));
            OfflinePreferences.Save();
            OfflineAudio.Instance().PlaySound2D(player == 0 ? 18 : 5, 1f);
        }

        public string SavedResults { get { return summary == null ? "" : "已完成 " + summary.completedMatches + " 局 · 玩家一 " + summary.playerOneWins + " 胜 · 对手 " + summary.playerTwoWins + " 胜"; } }
        private static OfflineMatchSummary LoadSummary()
        {
            string text = OfflinePreferences.GetString("MatchSummary", "");
            if (text == "") return new OfflineMatchSummary();
            OfflineMatchSummary data = JsonUtility.FromJson<OfflineMatchSummary>(text);
            if (data == null || data.version != 1 || data.completedMatches < 0 || data.playerOneWins < 0 || data.playerTwoWins < 0)
                throw new InvalidOperationException("Offline result save is invalid; original save has not been changed.");
            return data;
        }
        private void OnApplicationPause(bool pause) { if (pause && InMatch && !HasResult) Paused = true; if (pause) OfflinePreferences.Save(); }
        private void OnApplicationFocus(bool focus) { if (!focus && InMatch && !HasResult) Paused = true; }
        private void OnApplicationQuit() { OfflinePreferences.Save(); }
        private void OnDestroy() { if (view != null) view.Dispose(); if (Instance == this) Instance = null; }
    }

    [Serializable]
    public sealed class OfflineMatchSummary
    {
        public int version = 1;
        public int completedMatches;
        public int playerOneWins;
        public int playerTwoWins;
        public int lastWinner = -1;
        public string lastMode = "";
    }
}
