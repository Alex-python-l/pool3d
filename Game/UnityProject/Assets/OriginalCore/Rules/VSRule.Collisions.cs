// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using UnityEngine;
using Debug = UnityEngineInternalExport.Debug;

public partial class VSRule
{
    // RVA: 0x92E3F0. The surviving native body performs boundary reads but no position writes.
    protected void HandDestage(PoolBall[] balls)
    {
        for (int i = 0; i < balls.Length; i++)
        {
            if (GInside[0] <= balls[i].x && balls[i].x <= GInside[1]
                && GInside[3] <= balls[i].y)
            {
                // Preserve the final original array read, including its bounds/null contract.
                float originalBoundaryRead = GInside[2];
            }
        }
    }

    // RVA: 0x92FE68
    public override void onHitBall(RoundState state, int ballid1, int ballid2, PoolBall[] balls)
    {
        if (ballid1 != 0 && ballid2 != 0) return;
        GameRoundState rs = (GameRoundState)state;
        if (rs.ballRoundStates[0] == null) rs.Reset();
        int target = ballid1 == 0 ? ballid2 : ballid1;
        rs.ballRoundStates[target].hitByCueball = true;
        if (rs.firsthit < 1)
        {
            rs.firsthit = target;
            if (!isAvailableTarget(state, balls, target))
            {
                rs.fouled = true;
                bool currentHasSolid;
                if (rs.campstate == 3) currentHasSolid = rs.currentPlayer == 1;
                else if (rs.campstate == 2) currentHasSolid = rs.currentPlayer == 0;
                else { foulTipId = 8; return; }
                foulTipId = currentHasSolid ? 5 : 4;
            }
        }
    }

    // RVA: 0x930060
    public override void onHitBlocker(RoundState roundstate, int ballid, int blockerid,
        PoolBall[] balls, PoolBlocker[] blockers)
    {
        GameRoundState rs = (GameRoundState)roundstate;
        if (rs == null) return;
        rs.blockcount++;
        if (unchecked((uint)(blockerid - 4)) < 6u)
        {
            if (rs.ballRoundStates[ballid] == null) rs.Reset();
            rs.ballRoundStates[ballid].blocked = true;
        }
    }

    // RVA: 0x92FAF4
    public override bool onPotBall(RoundState state, int ballid, PoolBall[] balls, PoolHole hole)
    {
        GameRoundState rs = (GameRoundState)state;
        if (rs.ballRoundStates[0] == null) rs.Reset();
        bool valid;
        if (ballid == 0)
        {
            valid = false;
            rs.fouled = true;
        }
        else
        {
            rs.potCnt++;
            rs.ballRoundStates[ballid].potted = true;
            if (rs.firstpot <= -1) rs.firstpot = ballid;
            valid = isAvailableTarget(state, balls, ballid);
            if (valid) { rs.combo++; rs.validPotCnt++; }
            if (ballid == 8)
            {
                if (!isAvailableTarget(state, balls, 8)) rs.fouled = true;
                rs.pooledball8 = true;
                rs.targetPocket_HoleId = hole.id;
            }
        }
        if (m_Listener != null) m_Listener.OnPotBall(balls[ballid]);
        return valid;
    }

    // RVA: 0x92F084. The original normal tail ends at0x92F120; following geometry is another method.
    public override void onPutFreeball(RoundState state, PoolBall[] balls, int ballid)
    {
        GameRoundState rs = (GameRoundState)state;
        if (rs.campstate == 0) rs.campstate = 1;
        Debug.LogError(" on put free ball");
    }

    // RVA: 0x92F144
    public override bool checkFreeballArea(RoundState state, PoolBall ball0)
    {
        float radius = BALL_RADIUS;
        float x = ball0.x;
        float y = ball0.y;
        GameRoundState rs = (GameRoundState)state;
        if (rs.campstate == 0)
        {
            if (x - radius < GFree[0]) return false;
            if (x + radius > GFree[1]) return false;
            if (y + radius > GFree[2]) return false;
            return GFree[3] <= y - radius;
        }
        if (x - radius < GInside[0]) return false;
        if (x + radius > GInside[1]) return false;
        if (y + radius > GInside[2]) return false;
        return GInside[3] <= y - radius;
    }

    // RVA: 0x920B58
    public bool isUnderlineFreeball(RoundState state) { return ((GameRoundState)state).campstate == 0; }
}
