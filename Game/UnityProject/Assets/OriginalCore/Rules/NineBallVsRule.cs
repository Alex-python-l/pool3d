// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using Debug = UnityEngineInternalExport.Debug;
using Random = UnityEngineInternalExport.Random;
using Mathf = UnityEngineInternalExport.Mathf;

public class NineBallVsRule : VSRule
{
    // Original20-float constructor initializer: field13387, metadata0x3F6C68,
    // SHA25650BA6BA558947C2ED7851254916F57228E533055175BE1D24E68BE3CE6BB4077.
    public float[] ballsPos = new float[] {
        0f,0f,-200f,0f,0f,200f,200f,0f,0f,-200f,-350f,0f,350f,0f,-500f,0f,500f,0f,-650f,0f
    };
    private static bool IsRandomPos;

    // RVA: 0x92B2E8. No original NineBallVsRule static constructor or true assignment is present here.
    public NineBallVsRule() { }

    // RVA: 0x928CFC
    public override int getBallCount() { return 10; }

    // RVA: 0x928D04
    public void swapPos(int i, int j)
    {
        float value = ballsPos[i * 2];
        ballsPos[i * 2] = ballsPos[j * 2];
        ballsPos[j * 2] = value;
        value = ballsPos[(i * 2) | 1];
        ballsPos[(i * 2) | 1] = ballsPos[(j * 2) | 1];
        ballsPos[(j * 2) | 1] = value;
    }

    // RVA: 0x928E50; original literal0x9291DC=505f, diamond branch row3/4 skips retained.
    public void InitPos()
    {
        ballsPos[0] = Geometry.CueStartX;
        ballsPos[1] = 0f;
        float radius = BALL_RADIUS + 2f;
        float cos = MathUtil.Cos(30f);
        float sin = MathUtil.Sin(30f);
        int index = 2;
        for (int row = 0; row != 5; row++)
        {
            float baseY = 0f + (-(radius * sin) + -(radius * sin)) * row;
            float baseX = Geometry.RackX + (radius * cos + radius * cos) * row;
            for (int column = 0; column != row + 1; column++)
            {
                if (row == 4 && column != 2) continue;
                if (row == 3 && (column == 0 || column == 3)) continue;
                float offset = 0f;
                if ((row & 1) == 0) offset = Mathf.Abs(column - (row >> 1)) * -2f;
                if (IsRandomPos)
                {
                    ballsPos[index] = baseX + column * 0f + offset + Random.value() - 0.5f;
                    ballsPos[index + 1] = baseY + (radius + radius) * column + Random.value() - 0.5f;
                }
                else
                {
                    ballsPos[index] = baseX + column * 0f + offset - 0.5f;
                    ballsPos[index + 1] = baseY + (radius + radius) * column - 0.5f;
                }
                index += 2;
            }
        }
        swapPos(5, 9);
    }

    // RVA: 0x929204
    private void resetPos()
    {
        if (IsRandomPos)
        {
            for (int max = 2; max != 9; max++)
            {
                int index = Random.Range(0, max);
                if (max != index + 1) swapPos(max, index + 1);
            }
        }
    }

    // RVA: 0x929298; native inlined Reset(true) matches original PoolBall.Reset.
    public void reset9BallBalls(PoolBall[] balls)
    {
        resetPos();
        for (int i = 0; i < balls.Length; i++)
        {
            if (i >= ballsPos.Length) return;
            balls[i].x = ballsPos[i * 2];
            balls[i].y = ballsPos[(i * 2) | 1];
            balls[i].Reset(true);
            balls[i].isAvailable = true;
            balls[i].visible = true;
        }
    }

    // RVA: 0x929460
    public override void resetBalls(PoolBall[] balls, PoolHole[] holes)
    {
        if (isBallPosFromNet)
        {
            for (int i = 0; i < balls.Length; i++)
            {
                var ball = ccballs[i];
                if (ball == null)
                {
                    balls[i].isAvailable = false;
                    balls[i].visible = false;
                }
                else
                {
                    balls[i].x = ball.PosX;
                    balls[i].y = ball.PosY;
                    balls[i].isAvailable = ball.IsAvailable;
                    balls[i].visible = ball.IsVisible;
                    balls[i].Reset(true);
                }
            }
            return;
        }
        InitPos();
        reset9BallBalls(balls);
    }

    // RVA: 0x929684; original checks lower IDs, not candidate availability or an upper ID bound.
    public override bool isAvailableTarget(RoundState state, PoolBall[] balls, int id)
    {
        if (id == 0) return false;
        if (id < 2) return true;
        for (int i = 1; i < id; i++) if (balls[i].isAvailable) return false;
        return true;
    }

    // RVA: 0x929708
    public override void onHitBall(RoundState state, int ballid1, int ballid2, PoolBall[] balls)
    {
        if (ballid1 != 0 && ballid2 != 0) return;
        GameRoundState rs = (GameRoundState)state;
        int target = ballid1 == 0 ? ballid2 : ballid1;
        rs.ballRoundStates[target].hitByCueball = true;
        if (rs.firsthit <= 0)
        {
            int push = rs.pushOutState;
            rs.firsthit = target;
            if (push == 2) return;
            if (!isAvailableTarget(state, balls, target))
            {
                foulTipId = 8;
                rs.fouled = true;
            }
        }
    }

    // RVA: 0x9298C4
    public override bool onPotBall(RoundState state, int ballid, PoolBall[] balls, PoolHole hole)
    {
        GameRoundState rs = (GameRoundState)state;
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
            valid = true;
            if (rs.firstpot <= -1) rs.firstpot = ballid;
            Debug.LogError("Show ball in hole effect");
            rs.combo++;
            rs.validPotCnt++;
            if (ballid == 9)
            {
                rs.pooledball9 = true;
                rs.targetPocket_HoleId = hole.id;
            }
        }
        if (m_Listener != null) m_Listener.OnPotBall(balls[ballid]);
        return valid;
    }

    // RVA: 0x929AE0
    public override void onRoundOver(RoundState state, PoolBall[] balls, PoolHole[] holes)
    {
        state.state = 1;
        GameRoundState rs = (GameRoundState)state;
        if (rs.campstate == 0)
        {
            if (OnRoundOver_BreakOff(rs, balls)) return;
        }
        else if (rs.campstate == 1)
        {
            if (OnRoundOver_OpenCamp(rs, balls)) return;
        }
        if (!rs.isSwitchPlayer && m_Listener != null) m_Listener.OnContinueTurn();
        if (!Pool3D.Offline.OfflineMatchController.Instance.IsNet) SetStateToNextRound(rs);
    }

    // RVA: 0x929CF0
    private bool OnRoundOver_BreakOff(GameRoundState rs, PoolBall[] balls)
    {
        if (!balls[9].isAvailable)
        {
            if (!rs.fouled && balls[0].isAvailable) { setWin(rs); return true; }
            putBall(null, balls, 0, Geometry.CueStartX, 0f, true);
            putBall(rs, balls, 9, Geometry.RespottedObjectX, 0f, false);
        }
        else
        {
            if (balls[0].isAvailable && (rs.validPotCnt != 0 || rs.blockcount > 3))
            {
                rs.pushOutState = 1;
                rs.campstate = 1;
                CheckSwitchPlayer(rs, balls, false);
                return false;
            }
            OnTip(2, rs.currentPlayer);
            rs.fouled = true;
            putBall(null, balls, 0, Geometry.CueStartX, 0f, true);
        }
        CheckSwitchPlayer(rs, balls, false);
        return false;
    }

    // RVA: 0x929EB0
    private bool OnRoundOver_OpenCamp(GameRoundState rs, PoolBall[] balls)
    {
        CheckFoul1(rs, balls);
        bool over = CheckOver(rs, balls);
        if (!over)
        {
            int push = rs.pushOutState;
            if (push != 0) rs.pushOutState = push == 2 ? 3 : 4;
            CheckSwitchPlayer(rs, balls, push == 2);
        }
        return over;
    }

    // RVA: 0x92AEC0
    private bool IsValidPushOut(GameRoundState rs) { return rs.pushOutState == 2; }

    // RVA: 0x92AEE8
    private void updatePushoutState(GameRoundState rs)
    {
        if (rs.pushOutState != 0) rs.pushOutState = rs.pushOutState == 2 ? 3 : 4;
    }

    // RVA: 0x92A764
    private void CheckSwitchPlayer(GameRoundState rs, PoolBall[] balls, bool isPushOuting = false)
    {
        if (!isPushOuting)
        {
            if (!rs.fouled && rs.validPotCnt != 0) return;
        }
        else { rs.fouled = false; rs.isFreeBall = false; }
        switchPlayer(rs, balls);
    }

    // RVA: 0x92ADF0
    private bool CheckOver(GameRoundState rs, PoolBall[] balls)
    {
        if (!rs.pooledball9) return false;
        if (rs.pushOutState == 2 || rs.fouled)
        {
            putBall(rs, balls, 9, 0f, 0f, false);
            return false;
        }
        if (IsCallPocketBall(rs) && rs.targetPocket_HoleId != rs.callPocket_HoleId) setLose(rs);
        else setWin(rs);
        return true;
    }

    // RVA: 0x92AC74
    private void CheckFoul1(GameRoundState rs, PoolBall[] balls)
    {
        if (rs.pushOutState != 2)
        {
            if (rs.validPotCnt == 0 && rs.blockcount == 0) { foulTipId = 8; rs.fouled = true; }
            if (rs.firsthit == -1) { foulTipId = 7; rs.fouled = true; }
        }
        if (!balls[0].isAvailable)
        {
            putBall(rs, balls, 0, 0f, 0f, false);
            foulTipId = 6;
            rs.fouled = true;
        }
        if (!checkFreeballArea(rs, balls[0]) && rs.fouled) putBall(rs, balls, 0, 0f, 0f, false);
        OnTip(foulTipId, rs.currentPlayer);
        foulTipId = -1;
        rs.isFreeBall = rs.fouled;
        if (rs.isFreeBall) OnTip(0, 1 - rs.currentPlayer);
    }

    // RVA: 0x92B0A8
    public override void onTimeOver(RoundState state, PoolBall[] balls)
    {
        GameRoundState rs = (GameRoundState)state;
        updatePushoutState(rs);
        base.onTimeOver(rs, balls);
    }
}
