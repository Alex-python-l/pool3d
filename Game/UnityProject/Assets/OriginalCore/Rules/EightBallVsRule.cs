// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using UnityEngine;
using Random = UnityEngineInternalExport.Random;
using Mathf = UnityEngineInternalExport.Mathf;

public class EightBallVsRule : VSRule
{
    // Original field initializer in constructor0x928B6C:34floats from metadata field13473,
    // offset0x40E0A8; SHA256 DB76DE9E705236156ECD2661565AF4B2DF87521781FFDDF4FC524D85D175D478.
    public float[] ballsPos = new float[] {
        0f,0f,-200f,0f,0f,200f,200f,0f,0f,-200f,-350f,0f,350f,0f,
        -500f,0f,500f,0f,-650f,0f,650f,0f,-800f,0f,800f,0f,-950f,0f,
        950f,0f,-1100f,0f,1100f,0f
    };
    private static bool IsRandomPos;

    // RVA: 0x928B6C; recovered field initializer above precedes original base constructor.
    public EightBallVsRule() { }

    // RVA: 0x928CA8
    static EightBallVsRule() { IsRandomPos = true; }

    // RVA: 0x927FB4
    public override int getBallCount() { return 16; }

    // RVA: 0x927FBC
    public void swapPos(int i, int j)
    {
        float value = ballsPos[i * 2];
        ballsPos[i * 2] = ballsPos[j * 2];
        ballsPos[j * 2] = value;
        value = ballsPos[(i * 2) | 1];
        ballsPos[(i * 2) | 1] = ballsPos[(j * 2) | 1];
        ballsPos[(j * 2) | 1] = value;
    }

    // RVA: 0x928108; native constants at0x9284EC/0x9284F0 are0.0/505.0.
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
        swapPos(5, 8);
        if (IsRandomPos)
        {
            swapPos(2, 10);
            swapPos(6, 11);
            swapPos(7, 14);
        }
    }

    // RVA: 0x92851C
    private void resetPos()
    {
        if (IsRandomPos)
        {
            for (int i = 1; i != 16; i++) ballsPos[i * 2 + 1] = -ballsPos[i * 2 + 1];
            if (Random.Range(0, 2) == 1)
                for (int i = 1; i != 8; i++) swapPos(i, i + 8);
            for (int i = 1; i != 8; i++)
            {
                int index = Random.Range(0, i);
                if (i != index + 1) swapPos(i, index + 1);
                index = Random.Range(0, i);
                if (i != index + 1) swapPos(i + 8, index + 9);
            }
        }
    }

    // RVA: 0x928688. Reset(true) is re-expressed from the identical native inlined Reset body;
    // original ARM0x928884..0x9288C4 does not clear isAvailable (Ghidra NEON split was inaccurate).
    public void reset8BallBalls(PoolBall[] balls)
    {
        resetPos();
        bool reverse = IsRandomPos && Random.Range(0, 2) == 1;
        for (int i = 0; i < balls.Length; i++)
        {
            if (i >= ballsPos.Length) return;
            balls[i].x = ballsPos[i * 2];
            balls[i].y = ballsPos[(i * 2) | 1];
            if (reverse) balls[i].y = -balls[i].y;
            balls[i].Reset(true);
            balls[i].isAvailable = true;
            balls[i].visible = true;
        }
    }

    // RVA: 0x928948
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
        reset8BallBalls(balls);
    }
}
