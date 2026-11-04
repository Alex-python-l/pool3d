// Desktop input/UI adaptation. Hand feel is explicitly pending comparison with the original game.
using UnityEngine;

namespace Pool3D.Offline
{
    public sealed partial class OfflineControls : MonoBehaviour
    {
        private OfflineMatchController host;
        private Vector3 previousMouse;
        private Font font;
        private GUIStyle title, text, button;
        private Vector2 panelScroll;
        private readonly OfflineCueDragGesture gesture = new OfflineCueDragGesture();
        private bool panelPull;
        private float strengthBeforeDrag;
        private int pullControl;
        private bool clearPullCapture;
        private long dragTurn;
        private const float Width = 304f;

        private void Awake() { host = GetComponent<OfflineMatchController>(); previousMouse = Input.mousePosition; }
        private void Update()
        {
            Vector3 mouse = Input.mousePosition;
            Vector2 delta = new Vector2(mouse.x - previousMouse.x, mouse.y - previousMouse.y);
            previousMouse = mouse; // Always update, including menu/pause, to avoid reentry jumps.
            if (host == null || host.InitializationError != null || !host.InMatch) { CancelDrag(); return; }
            if (Input.GetKeyDown(KeyCode.Escape)) { if (host.HasResult) host.ReturnToMenu(); else host.TogglePause(); }
            if (Input.GetKeyDown(KeyCode.Space) && gesture.Kind == OfflineCueDragKind.None) host.Shoot();
            if (Input.GetKeyDown(KeyCode.V)) { CancelDrag(); host.ToggleView(); }
            if (Input.GetKeyDown(KeyCode.R)) { CancelDrag(); host.ResetView(); }
            Camera camera = host.MatchCamera;
            camera.pixelRect = new Rect(Width + 32f, 0f, Mathf.Max(1f, Screen.width - Width - 32f), Screen.height);
            if (host.Paused || host.HasResult || !Application.isFocused) { CancelDrag(); return; }
            if (gesture.Kind != OfflineCueDragKind.None && (!host.CanShoot || dragTurn != host.TurnSerial)) { CancelDrag(); return; }
            if (panelPull) return; // IMGUI owns the power-strip pointer until mouse-up.
            // Consume release even if the pointer has left the table/sidebar.
            if (Input.GetMouseButtonUp(0)) { ReleaseDrag(); return; }
            if (mouse.x <= Width + 32f || !camera.pixelRect.Contains(mouse)) return;
            if (Input.GetMouseButton(2)) { CancelDrag(); host.OrbitCamera(delta); }
            if (Input.mouseScrollDelta.y != 0f) host.ZoomCamera(Input.mouseScrollDelta.y);
            if (Input.GetMouseButtonDown(0) && host.CanShoot)
            {
                Vector2 backward;
                bool onCue = host.HitCue(mouse, out backward);
                strengthBeforeDrag = host.StrengthUnit;
                dragTurn = host.TurnSerial;
                gesture.Begin(onCue, mouse.x, mouse.y, backward.x, backward.y);
                delta = Vector2.zero;
            }
            if (Input.GetMouseButton(0) && gesture.Kind != OfflineCueDragKind.None)
            {
                gesture.Move(mouse.x, mouse.y);
                if (gesture.Kind == OfflineCueDragKind.Power)
                {
                    host.PreviewCuePull(gesture.Power);
                    host.StrengthUnit = gesture.Power;
                    return;
                }
                if (gesture.Kind != OfflineCueDragKind.Aim) return;
                if (!host.IsView2D()) { host.OrbitCamera(delta); return; }
            }
            if (!Input.GetMouseButton(1) && !(Input.GetMouseButton(0) && gesture.Kind == OfflineCueDragKind.Aim && host.IsView2D())) return;
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = camera.ScreenPointToRay(mouse);
            float distance;
            if (!plane.Raycast(ray, out distance)) return;
            Vector3 point = ray.GetPoint(distance);
            Vector3 simulation=Pool3D.Offline.Geometry.TableSpace.WorldToSim(point);
            float x=simulation.x,y=simulation.y;
            if (Input.GetMouseButton(1)) host.MoveCueBall(x, y);
            else if (host.CanShoot)
            {
                PoolBall cue = host.M_PoolTable.balls[0];
                host.AimAngle = MathUtil.Atan2(y - cue.y, x - cue.x);
            }
        }

        private void ReleaseDrag()
        {
            float force = gesture.Power;
            bool shoot = gesture.Release();
            panelPull = false;
            if (host == null) return;
            if (shoot && host.CanShoot && dragTurn == host.TurnSerial) host.ReleaseCuePull(force);
            else { host.CancelCuePull(); if (force > 0f) host.StrengthUnit = strengthBeforeDrag; }
            host.EndAimDrag();
        }
        private void CancelDrag()
        {
            bool wasPower = gesture.Kind == OfflineCueDragKind.Power;
            gesture.Cancel();
            if (panelPull) clearPullCapture = true;
            panelPull = false;
            if (host == null) return;
            host.CancelCuePull();
            if (wasPower) host.StrengthUnit = strengthBeforeDrag;
            if (host.InMatch) host.EndAimDrag();
        }
        private void OnApplicationFocus(bool focus) { if (!focus) CancelDrag(); }
        private void OnApplicationPause(bool pause) { if (pause) CancelDrag(); }
        private void OnDisable() { CancelDrag(); }

        private void Styles()
        {
            if (text != null) return;
            font = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "Noto Sans CJK SC", "Arial" }, 17);
            title = new GUIStyle(GUI.skin.label) { font = font, fontSize = 26, fontStyle = FontStyle.Bold, wordWrap = true };
            text = new GUIStyle(GUI.skin.label) { font = font, fontSize = 16, wordWrap = true };
            button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 17, fixedHeight = 44f };
            title.normal.textColor = text.normal.textColor = new Color(0.93f, 0.95f, 0.97f);
        }
        private bool Button(string label) { GUILayout.Space(6f); return GUILayout.Button(label, button); }
        private void Label(string value) { GUILayout.Label(value, text); }
        private void EndPanel() { GUILayout.EndScrollView(); GUILayout.EndArea(); }

        private void OnGUI()
        {
            if (host == null) return;
            if (gesture.Kind != OfflineCueDragKind.None && (!host.CanShoot || dragTurn != host.TurnSerial || !Application.isFocused)) CancelDrag();
            if (clearPullCapture)
            {
                if (GUIUtility.hotControl == pullControl) GUIUtility.hotControl = 0;
                clearPullCapture = false;
            }
            Styles();
            GUI.Box(new Rect(12f, 12f, Width, Screen.height - 24f), GUIContent.none);
            GUILayout.BeginArea(new Rect(28f, 26f, Width - 32f, Screen.height - 52f));
            GUILayout.Label("离线台球", title);
            panelScroll = GUILayout.BeginScrollView(panelScroll, false, true);
            if (host.InitializationError != null)
            {
                Label("无法继续：" + host.InitializationError);
                Label("错误已保留在 Unity 日志；没有修改原存档。");
                EndPanel();
                return;
            }
            if (!host.InMatch)
            {
                Label("不提供联网、广告、商城或账号功能。");
                host.UseEnhancedSpin = GUILayout.Toggle(host.UseEnhancedSpin, " 加塞增强（实验）", text);
                Label(host.UseEnhancedSpin ? "使用圆形击球点与旋转碰撞" : "使用原始旋转与碰撞算法");
                GUILayout.Space(12f);
                if (Button("8球 · 本地双人")) host.StartMatch(false, false);
                if (Button("8球 · 对战电脑")) host.StartMatch(true, false);
                if (Button("9球 · 本地双人")) host.StartMatch(false, true);
                if (Button("9球 · 对战电脑")) host.StartMatch(true, true);
                GUILayout.Space(18f);
                Label(host.SavedResults);
                Label("3D：左键拖动转向与俯仰\n球杆侧向拖动瞄准，沿杆后拉松手击球\n右键摆球 · 滚轮调整远近\nV 切换俯视 · R 复位 · Esc 暂停");
                if (Button("退出试玩")) Application.Quit();
                EndPanel();
                return;
            }
            Label((host.NineBall ? "9球" : "8球") + (host.AgainstComputer ? " · 对战电脑" : " · 本地双人"));
            Label(host.Status);
            Label(host.EnhancedSpinActive ? "加塞增强" : "原始模式");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(host.IsView2D() ? "切换 3D" : "切换俯视", button)) { CancelDrag(); host.ToggleView(); }
            if (GUILayout.Button("视角复位", button)) { CancelDrag(); host.ResetView(); }
            GUILayout.EndHorizontal();
            if (host.HasResult)
            {
                Label(host.SavedResults);
                if (Button("再来一局")) host.Restart();
                if (Button("返回菜单")) host.ReturnToMenu();
                EndPanel();
                return;
            }
            Label("当前：" + host.PlayerDisplayName(host.Round.currentPlayer) + "   剩余 " + Mathf.CeilToInt(host.RoundTime) + " 秒");
            if (!host.NineBall)
            {
                int camp = host.Round.campstate;
                Label(camp < 2 ? "花色尚未确定" : camp == 2 ? "玩家一：全色   对手：花色" : "玩家一：花色   对手：全色");
            }
            if (host.Paused)
            {
                Label("已暂停");
                if (Button("继续对局")) host.TogglePause();
                if (Button("返回菜单")) host.ReturnToMenu();
                EndPanel();
                return;
            }
            GUI.enabled = host.CanShoot && !host.CuePulling;
            Label("力度 " + Mathf.RoundToInt(host.StrengthUnit * 100f) + "%");
            host.StrengthUnit = GUILayout.HorizontalSlider(host.StrengthUnit, 0.02f, 1f, GUILayout.Height(24f));
            DrawSpinInput();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("微调 −", button)) host.FineAdjust(-0.1f);
            if (GUILayout.Button("微调 +", button)) host.FineAdjust(0.1f);
            GUILayout.EndHorizontal();
            if (Button("击球  [空格]")) host.Shoot();
            GUI.enabled = true;
            DrawPullStrip();
            Label(host.IsView2D() ? "俯视：拖动指向瞄准" : "3D：左键拖动转向 / 俯仰");
            Label("直接沿球杆后拉，松手击球；也可拖下方拉杆区。");
            if (host.ComputerTurn) Label("电脑力度 " + Mathf.RoundToInt(host.DisplayForceUnit * 100f) + "%");
            if (host.CanPlaceCueBall) Label("可右键拖动摆放母球");
            if (host.IsTargetPocketPicking && !host.ComputerTurn)
            {
                Label("选择目标袋");
                GUILayout.BeginHorizontal();
                for (int i = 0; i < 6; i++)
                    if (GUILayout.Button((i + 1).ToString(), button)) host.PickTargetPocket(i);
                GUILayout.EndHorizontal();
            }
            if (!host.ComputerTurn && host.IsCanPushOut() && Button("推杆")) host.HumanPushOut();
            if (!host.ComputerTurn && host.IsCanGiveUp() && Button("让回对手")) host.HumanGiveUp();
            if (Button("暂停  [Esc]")) host.TogglePause();
            EndPanel();
        }

        private void DrawPullStrip()
        {
            Rect rect = GUILayoutUtility.GetRect(0f, 100f, GUILayout.ExpandWidth(true));
            Color oldColor = GUI.color;
            GUI.color = new Color(0.08f, 0.22f, 0.19f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = new Color(0.75f, 0.57f, 0.25f);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 7f, rect.width * (panelPull ? gesture.Power : 0f), 7f), Texture2D.whiteTexture);
            GUI.color = oldColor;
            GUI.Label(new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height - 16f), panelPull ? "力度 " + Mathf.RoundToInt(gesture.Power * 100f) + "% · 松手击球" : "拉杆区\n按住向下拖动蓄力，松手击球", text);
            Event e = Event.current;
            int id = GUIUtility.GetControlID("OfflineCuePull".GetHashCode(), FocusType.Passive, rect);
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition) && host.CanShoot && gesture.Kind == OfflineCueDragKind.None)
            {
                pullControl = id; GUIUtility.hotControl = id;
                panelPull = true; strengthBeforeDrag = host.StrengthUnit;
                dragTurn = host.TurnSerial;
                gesture.Begin(true, e.mousePosition.x, e.mousePosition.y, 0f, 1f);
                e.Use();
            }
            else if (panelPull && GUIUtility.hotControl == pullControl && e.type == EventType.MouseDrag)
            {
                gesture.Move(e.mousePosition.x, e.mousePosition.y);
                if (gesture.Kind == OfflineCueDragKind.Power) { host.PreviewCuePull(gesture.Power); host.StrengthUnit = gesture.Power; }
                e.Use();
            }
            else if (panelPull && GUIUtility.hotControl == pullControl && e.type == EventType.MouseUp && e.button == 0)
            {
                GUIUtility.hotControl = 0; ReleaseDrag(); e.Use();
            }
        }

        private void OnDestroy() { ReleaseSpinDisc(); if (font != null) Destroy(font); }
    }
}
