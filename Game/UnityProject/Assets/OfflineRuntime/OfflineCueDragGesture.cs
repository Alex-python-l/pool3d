// New desktop gesture arbitration. Pure input policy; no physics or rule code.
using System;

namespace Pool3D.Offline
{
    public enum OfflineCueDragKind { None, Pending, Aim, Power }

    public sealed class OfflineCueDragGesture
    {
        public OfflineCueDragKind Kind { get; private set; }
        public float Power { get; private set; }
        private float startX, startY, backX, backY;

        public void Begin(bool onCue, float x, float y, float backwardX, float backwardY)
        {
            Cancel();
            startX = x; startY = y;
            float length = (float)Math.Sqrt(backwardX * backwardX + backwardY * backwardY);
            if (length > 0.001f) { backX = backwardX / length; backY = backwardY / length; }
            Kind = onCue && length > 0.001f ? OfflineCueDragKind.Pending : OfflineCueDragKind.Aim;
        }

        public void Move(float x, float y)
        {
            if (Kind == OfflineCueDragKind.None) return;
            float dx = x - startX, dy = y - startY;
            float along = dx * backX + dy * backY;
            if (Kind == OfflineCueDragKind.Pending)
            {
                if (dx * dx + dy * dy < 36f) return;
                float sideways = dx * -backY + dy * backX;
                Kind = along > 0f && along > Math.Abs(sideways) ? OfflineCueDragKind.Power : OfflineCueDragKind.Aim;
            }
            if (Kind == OfflineCueDragKind.Power)
                Power = Math.Max(0f, Math.Min(1f, along / 200f)); // Original ForceCtr: 200px for full pull, now along the on-screen cue.
        }

        public bool Release()
        {
            bool shoot = Kind == OfflineCueDragKind.Power && Power >= 0.0001f;
            Cancel();
            return shoot;
        }
        public void Cancel() { Kind = OfflineCueDragKind.None; Power = backX = backY = 0f; }
    }
}
