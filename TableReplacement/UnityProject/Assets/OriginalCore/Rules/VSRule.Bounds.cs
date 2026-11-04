using UnityEngine;
// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
public partial class VSRule
{
    // RVA: 0x92F4AC. Original cached x/y and pocket-border intervals are retained.
    public override void checkBallPosition(PoolBall ball)
    {
        if(!Geometry.IsLegacy) { if(ball.inHole==null) { Vector2 p=Geometry.Constrain(new Vector2(ball.x,ball.y),ball.scale);ball.x=p.x;ball.y=p.y; } return; }
        float radius = BALL_RADIUS;
        float x = ball.x;
        float y = ball.y;
        float left = x - radius;
        if (GInside[0] <= left)
        {
            if (radius + x > GInside[1])
            {
                if (radius + y < GBorder[0] && y - radius > GBorder[1])
                    ball.x = GInside[1] - radius;
            }
        }
        else if (radius + y < GBorder[0] && y - radius > GBorder[1])
            ball.x = radius + GInside[0];

        // The ARM unordered branch is represented by !(a>b), rather than a<=b.
        if (!(radius + y > GInside[2]))
        {
            if (GInside[3] <= y - radius) return;
            if (!(left > GBorder[2]) || GBorder[3] <= radius + x)
            {
                if (!(left > GBorder[4])) return;
                if (GBorder[5] <= radius + x) return;
            }
            ball.y = radius + GInside[3];
        }
        else
        {
            if (!(left > GBorder[2]) || GBorder[3] <= radius + x)
            {
                if (!(left > GBorder[4])) return;
                if (GBorder[5] <= radius + x) return;
            }
            ball.y = GInside[2] - radius;
        }
    }
}
