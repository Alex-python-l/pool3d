// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using MStateMachine;

// The original shares these state instances between all AIStateMgr instances.
public class AIStateMgr
{
    public static AdjustSpinState g_AdjustSpinState;
    public static AimState g_AimState;
    public static PickPocketState g_PickPocketState;
    public static PutBallState g_PutBallState;
    public static ShootState g_ShootState;
    public static ThinkState g_ThinkState;
    private StateMachine<PoolAIBase> m_StateMachine;

    // RVA: 0xC4473C
    public AIStateMgr() { m_StateMachine = new StateMachine<PoolAIBase>(); }
    // RVA: 0xC447D4
    static AIStateMgr()
    {
        g_AdjustSpinState = new AdjustSpinState();
        g_AimState = new AimState();
        g_PickPocketState = new PickPocketState();
        g_PutBallState = new PutBallState();
        g_ShootState = new ShootState();
        g_ThinkState = new ThinkState();
    }
    // RVA: 0xC4464C. Native passes null for all three state arguments.
    public void Init(PoolAIBase owner) { m_StateMachine.Init(owner, null, null, null); }
    // RVA: 0xC446D4. Original tail branch b 0xC4464C, with owner retained in r1.
    public void Reset(PoolAIBase owner) { Init(owner); }
    // RVA: 0xC44580
    public void ChangeState(State<PoolAIBase> state) { m_StateMachine.ChangeState(state); }
    // RVA: 0xC446D8
    public void Update() { m_StateMachine.Update(); }
}
