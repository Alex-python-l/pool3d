// Offline UI/scene bridge for the original AI FSM. No replacement decision/search code.
using UnityEngine;

namespace Pool3D.Offline
{
    public partial class OfflineMatchController
    {
        public bool IsAITargetPocketPicked() { return !IsSelfTurn && SelectedPocket != -1; }
        public bool IsSelfTurn { get { return Round.currentPlayer == 0; } }
        public int GetAITargetPocketId() { return SelectedPocket; }
        public bool IsAIFreeBall() { return Round.isFreeBall || Round.campstate == 0; }
        public bool IsCanPushOut() { return Round.pushOutState == 1; }
        public bool IsCanGiveUp() { return Round.pushOutState == 3; }
        public void PickTargetPocket(int id)
        {
            if (!InMatch || pendingTurn || id < 0 || id >= M_PoolTable.holes.Length || Paused || HasResult) return;
            SelectedPocket = id;
            Round.callPocket_HoleId = id;
        }
        public void AIStartAim()
        {
            AI.aimAngle = AimAngle;
            AI.pitch = view.Pitch;
            AI.targetPitch = view.Pitch; // Camera transition adaptation; not original CameraSF pitch behavior.
        }
        public void AIAimming() { AimAngle = AI.aimAngle; }
        public void AIPitch() { view.Pitch = AI.pitch; }
        public void AI_StartMoveBall() { Status = "电脑摆放母球"; }
        public void AI_MoveBall()
        {
            // Original AI_MoveBall movement and threshold; UI-only effects omitted.
            PoolBall ball = M_PoolTable.balls[0];
            float dx = AI.getTarX() - ball.x, dy = AI.getTarY() - ball.y;
            float distance = UnityEngineInternalExport.Mathf.Sqrt(dy * dy + dx * dx);
            if (distance < 10f)
            {
                ball.x = AI.getTarX(); ball.y = AI.getTarY();
                AI.m_AIStateMgr.ChangeState(AIStateMgr.g_AimState);
                return;
            }
            float ratio = 10f / distance;
            ball.y += dy * ratio; ball.x += dx * ratio;
        }
        public void AIShoot(bool netRequest = true)
        {
            if (!ComputerTurn || pendingTurn || Paused || HasResult || Round.state != PoolRule.STATE_WAITING) return;
            // Original AIShoot strength/angle and zero-spin engine call; no network/analytics branch.
            AimAngle = AI.getAngle();
            M_PoolTable.click(AI.getStrength(), 0f, 0f, AimAngle);
        }
        public void AI_PushOut() { ((VSRule)M_PoolRule).onPushOut(Round); }
        public void AI_GiveUp() { ((VSRule)M_PoolRule).giveUpStrick(Round, M_PoolTable.balls); }
        public void HumanPushOut() { if (CanShoot && IsCanPushOut()) AI_PushOut(); }
        public void HumanGiveUp() { if (CanShoot && IsCanGiveUp()) AI_GiveUp(); }
        public void ForcePullStart() { DisplayForceUnit = 0f; }
        public void ForcePull(float unit) { DisplayForceUnit = Mathf.Clamp01(unit); }
        public void ForcePullOver() { DisplayForceUnit = 0f; }
        public float AIForceUnit(int cueId, float strength, int level)
        {
            if (cueId != CueId || level != 0) throw new System.NotSupportedException("This offline profile uses the original level-0 Wood Cue only.");
            // Original PoolData.GetCueStrength_Unit, level0 original cue config.
            float maximum = Var.GetCueForce(CueForce, 1f);
            float value = maximum < strength ? maximum : strength;
            if (strength < 0f) value = 0f;
            return value / maximum;
        }
    }
}
