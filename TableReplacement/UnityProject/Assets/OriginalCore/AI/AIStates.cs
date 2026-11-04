// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using UnityEngine;

public class AdjustSpinState : State<PoolAIBase>
{
    // RVA: 0xC44134. Only calls the original base constructor; no spin callbacks in this state.
    public AdjustSpinState() : base() { }
}

public class ThinkState : State<PoolAIBase>
{
    private float time;
    public float thinkMaxTime;
    // RVA: 0xC44A9C
    public ThinkState() : base() { thinkMaxTime = 2f; }
    // RVA: 0xC45030
    public override void Enter(PoolAIBase t) { t.EnterThink(); time = 0f; }
    // RVA: 0xC45060
    public override void Execute(PoolAIBase t)
    {
        if (time < thinkMaxTime) { time += Time.deltaTime; return; }
        t.HandleThink();
    }
    // RVA: 0xC450C0. Original bx lr.
    public override void Exit(PoolAIBase t) { }
}

public class AimState : State<PoolAIBase>
{
    private float time;
    private float pitch;
    // RVA: 0xC445F0
    public AimState() : base() { time = 2f; }
    // RVA: 0xC44188
    public override void Enter(PoolAIBase tt) { time = 2f; tt.m_PoolCtr.AIStartAim(); }
    // RVA: 0xC4452C
    private static float getAngleDiff(float angle, float angle2)
    {
        float difference = angle - angle2;
        if (difference < -180f) difference += 360f;
        if (difference > 180f) difference += -360f;
        return difference;
    }
    // RVA: 0xC441C4
    public override void Execute(PoolAIBase ai)
    {
        bool view2D = Pool3D.Offline.OfflineMatchController.Instance.IsView2D();
        float viewSpeed = view2D ? 1f : 0.5f;
        float waitDelta = Time.deltaTime;
        float delta = Time.deltaTime;
        float difference = getAngleDiff(ai.getAngle(), ai.aimAngle);
        float absolute = Mathf.Abs(difference);
        if (!(absolute > 11f))
        {
            if (!(absolute > 1f))
            {
                if (!(absolute > 0.1f))
                {
                    time -= waitDelta;
                    ai.aimAngle = ai.getAngle();
                    if (time < 0f) ai.m_AIStateMgr.ChangeState(AIStateMgr.g_ShootState);
                }
                else ai.aimAngle += difference < 0f ? -0.02f : 0.02f;
            }
            else
            {
                if (delta > 0.25f) delta = 0.25f;
                ai.aimAngle = ai.getAngle() - (1f - (delta + delta)) * difference;
            }
        }
        else if (delta <= (absolute + -10f) / 150f)
            ai.aimAngle += viewSpeed * delta * (difference / absolute) * 150f;
        else ai.aimAngle = ai.getAngle() + (difference / absolute) * -10f;
        // The original still invokes these after ChangeState to ShootState above.
        ai.m_PoolCtr.AIAimming();
        if (view2D) return;
        float previousPitch = ai.pitch, desiredPitch = ai.targetPitch;
        float pitchDelta = Time.deltaTime;
        float clamped = 0f;
        if (pitchDelta >= 0f)
        {
            if (pitchDelta > 1f) pitchDelta = 1f;
            clamped = pitchDelta;
        }
        ai.pitch = previousPitch + (desiredPitch - previousPitch) * clamped;
        ai.m_PoolCtr.AIPitch();
    }
    // RVA: 0xC445EC. Original bx lr.
    public override void Exit(PoolAIBase t) { }
}

public class PickPocketState : State<PoolAIBase>
{
    // RVA: 0xC449A0
    public PickPocketState() : base() { }
    // RVA: 0xC44AF8
    public override void Enter(PoolAIBase ai)
    {
        Debug.Log(" ******** pick pocket pocket ***** enter");
        int pocket = ai.GetTargetPocketId();
        ai.m_PoolCtr.PickTargetPocket(pocket == -1 ? 0 : pocket);
        ai.m_AIStateMgr.ChangeState(ai.m_PoolCtr.IsAIFreeBall()
            ? (State<PoolAIBase>)AIStateMgr.g_PutBallState : AIStateMgr.g_AimState);
    }
    // RVA: 0xC44C40. Original bx lr.
    public override void Execute(PoolAIBase ai) { }
    // RVA: 0xC44C44. Original bx lr.
    public override void Exit(PoolAIBase ai) { }
}

public class PutBallState : State<PoolAIBase>
{
    // RVA: 0xC449F4
    public PutBallState() : base() { }
    // RVA: 0xC44C48
    public override void Enter(PoolAIBase ai) { ai.m_PoolCtr.AI_StartMoveBall(); }
    // RVA: 0xC44C7C
    public override void Execute(PoolAIBase ai) { ai.m_PoolCtr.AI_MoveBall(); }
    // RVA: 0xC44CB0. Original bx lr.
    public override void Exit(PoolAIBase ai) { }
}

public class ShootState : State<PoolAIBase>
{
    // RVA: 0xC44A48
    public ShootState() : base() { }
    // RVA: 0xC44CB4
    public override void Enter(PoolAIBase ai)
    {
        ai.aiStrength = 0f;
        Pool3D.Offline.OfflineMatchController.Instance.ForcePullStart();
    }
    // RVA: 0xC44D54
    public override void Execute(PoolAIBase ai)
    {
        float speed = ai.getStrength() / 10000f;
        if (speed < 0.1f) speed = 0.1f;
        float delta = Time.deltaTime;
        if (ai.aiStrength < ai.getStrength())
        {
            ai.aiStrength += delta * speed * Var.MAX_STRENGTH;
            int cueId = Pool3D.Offline.OfflineMatchController.Instance.CueId;
            float unit = Pool3D.Offline.OfflineMatchController.Instance.AIForceUnit(cueId, ai.aiStrength, 0);
            Pool3D.Offline.OfflineMatchController.Instance.ForcePull(unit);
            return;
        }
        ai.m_PoolCtr.AIShoot(true);
        ai.m_AIStateMgr.ChangeState(null);
        Pool3D.Offline.OfflineMatchController.Instance.ForcePullOver();
    }
    // RVA: 0xC4502C. Original bx lr.
    public override void Exit(PoolAIBase ai) { }
}
