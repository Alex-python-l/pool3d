// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Original AI/FSM handoff bodies, transcribed from original ARM32.
public partial class PoolAIBase
{
    // RVA: 0x9213A4
    public void Update() { m_AIStateMgr.Update(); }

    // RVA: 0x922128. Original strh clears the adjacent flags together.
    public void EnterThink() { pushout = false; giveUp = false; }

    // RVA: 0x922134
    public void HandleThink()
    {
        if (m_PoolCtr.IsAITargetPocketPicked()) targetPocketId = m_PoolCtr.GetAITargetPocketId();
        bool freeBall = m_PoolCtr.IsAIFreeBall();
        RoundState state = m_PoolCtr.M_PoolTable.getRoundState();
        PoolBall[] balls = m_PoolCtr.M_PoolTable.balls;
        bool picked = m_PoolCtr.IsAITargetPocketPicked();
        if (freeBall) thinkFreeball(state, balls, picked);
        else think(state, balls, picked);
        if (pushout) m_PoolCtr.AI_PushOut();
        else if (giveUp)
        {
            m_PoolCtr.AI_GiveUp();
            m_AIStateMgr.ChangeState(null);
            return;
        }
        if (m_PoolCtr.IsTargetPocketPicking)
            m_AIStateMgr.ChangeState(AIStateMgr.g_PickPocketState);
        else if (freeBall)
            m_AIStateMgr.ChangeState(AIStateMgr.g_PutBallState);
        else
            m_AIStateMgr.ChangeState(AIStateMgr.g_AimState);
    }
}
