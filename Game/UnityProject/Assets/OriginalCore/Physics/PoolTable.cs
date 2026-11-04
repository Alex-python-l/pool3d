// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using UnityEngine;

// Original global type, split across files solely to organize recovered bodies.
public partial class PoolTable : PoolObject
{
    public static float ROLL_K;          // static +0x00
    public static float ROLLW_FORCE_K;   // static +0x04
    public static float BLOCK_HIT_LOST;  // static +0x08
    private PoolRule rule;              // +0x28
    public PoolBall[] balls;            // +0x2c
    public PoolBlocker[] blockers;      // +0x30
    public PoolHole[] holes;            // +0x34
    public RoundState roundstate;      // +0x38
    private bool targetVisible;         // +0x3c
    public const float fixedTimeSpan = 0.0032000002f;
    private bool isFirstShoot;          // +0x3d
    private float time_LastLeft;        // +0x40
    private bool stillRunning;          // +0x44
    private Vector3 tmp;                // +0x48
    private static Quaternion tempQuat; // static +0x0c
    private bool forceAll;              // +0x54
    private static int hittype;         // static +0x1c
    private static int hita;            // static +0x20
    private static int hitb;            // static +0x24
    private static float hitx0;         // static +0x28
    private static float hity0;         // static +0x2c
    private static float hitx1;         // static +0x30
    private static float hity1;         // static +0x34
    private static float hitx;          // static +0x38
    private static float hity;          // static +0x3c
    public static float MAX_TIME;       // static +0x40
    public static float[,,] nextHitTime; // static +0x44
    private int lastAimBallIndex;       // +0x58

    // RVA: 0x9230CC
    public PoolTable() : base() { forceAll = true; }

    // RVA: 0x927EC8. Dimension literal at 0x358548: int32 {6,30,30}.
    static PoolTable()
    {
        ROLL_K = 2.5f;
        ROLLW_FORCE_K = 0.15f;
        tempQuat = new Quaternion(0f, 0f, 0f, 0f);
        BLOCK_HIT_LOST = 0.6f;
        MAX_TIME = 3600f;
        nextHitTime = new float[6, 30, 30];
    }

    // RVA: 0x9230E8
    public RoundState getRoundState() { return roundstate; }

    // RVA: 0x9230F0
    public int getAvailableBallCount()
    {
        if (balls == null) return 0;
        int count = 0;
        for (int i = 0; i < balls.Length; ++i)
            if (balls[i] != null && balls[i].isAvailable) ++count;
        return count;
    }

    // RVA: 0x9231A4
    public void setRule(PoolRule rule)
    {
        this.rule = rule;
        clear();
    }

    // RVA: 0x9231C0
    public void clear()
    {
        balls = null;
        roundstate = null;
        blockers = null;
        holes = null;
    }

    // RVA: 0x923210. Original bytes: 1eff2fe1 (bx lr).
    private void updateTargetSign() { }

    // RVA: 0x92320C. Original bytes: 1eff2fe1 (bx lr).
    public void resetScale() { }

    // RVA: 0x923388. Original bytes: 1eff2fe1 (bx lr).
    private void updateShadow() { }

    public bool IsFirstShoot
    {
        // RVA: 0x923394
        get { return isFirstShoot; }
        // RVA: 0x92338C
        set { isFirstShoot = value; }
    }

    // RVA: 0x92339C
    public void reset()
    {
        if (rule == null) { clear(); return; }
        if (blockers == null || blockers.Length != rule.getBlockerCount())
        {
            blockers = new PoolBlocker[rule.getBlockerCount()];
            for (int i = 0; i < blockers.Length; ++i) blockers[i] = new PoolBlocker();
        }
        rule.resetBlockers(blockers);
        if (holes == null || holes.Length != rule.getHoleCount())
        {
            holes = new PoolHole[rule.getHoleCount()];
            for (int i = 0; i < holes.Length; ++i) holes[i] = new PoolHole();
        }
        rule.resetHoles(holes);
        if (balls == null || balls.Length != rule.getBallCount())
        {
            balls = new PoolBall[rule.getBallCount()];
            for (int i = 0; i < balls.Length; ++i)
            {
                balls[i] = new PoolBall();
                balls[i].id = i;
                balls[i].scale = rule.getBallRadius();
            }
        }
        EnsureCollisionCapacity();
        rule.resetBalls(balls, holes);
        if (roundstate == null) roundstate = rule.createRoundState();
        rule.resetRoundState(roundstate);
        targetVisible = false;
        isFirstShoot = true;
    }
}
