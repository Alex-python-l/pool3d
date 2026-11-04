// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using UnityEngine;
using Mathf = UnityEngineInternalExport.Mathf;

public partial class PoolRule
{
    // RVA: 0x92A0E0. Original finite candidate search and fallback retained.
    // Native literal positions0x92A630/634/638:0.0,1217.0,-1225.0.
    public void putBall(RoundState roundstate, PoolBall[] balls, int ind, float basex, float basey, bool bIgnoreOtherball)
    {
        PoolBall ball = balls[ind];
        ball.Reset(true);
        ball.isAvailable = true;
        ball.visible = true;
        if (!bIgnoreOtherball)
        {
            float diameter = BALL_RADIUS + BALL_RADIUS + 1f;
            float minDistanceSquared = diameter * diameter;
            int first;
            for (first = 0; first < balls.Length; first++)
            {
                if (ind != first && balls[first].isAvailable)
                {
                    float dx = balls[first].x - basex;
                    float dy = balls[first].y - basey;
                    if (dy * dy + dx * dx < minDistanceSquared) break;
                }
            }
            if (first < balls.Length)
            {
                bool found = false;
                float bestDistance = 0f;
                float bestX = basex;
                float bestY = basey;
                for (int i = 0; i < balls.Length; i++)
                {
                    if (i == ind || !balls[i].isAvailable) continue;
                    float dy = balls[i].y - basey;
                    float remainder = minDistanceSquared - dy * dy;
                    if (0f <= remainder)
                    {
                        float root = Mathf.Sqrt(remainder);
                        // Original examines the right tangent and then the left tangent of this ball.
                        for (int side = 0; side < 2; side++)
                        {
                            float candidate = side == 0 ? root + balls[i].x : balls[i].x - root;
                            if (side == 0) ball.y = basey;
                            ball.x = candidate;
                            if (candidate <= (Geometry.IsLegacy?1217f:Geometry.Inside[1]) - BALL_RADIUS && ball.x >= BALL_RADIUS + (Geometry.IsLegacy?-1225f:Geometry.Inside[0])
                                && (Mathf.Abs(candidate - basex) < bestDistance || !found))
                            {
                                int j;
                                for (j = 0; j < balls.Length; j++)
                                {
                                    if (j != ind && j != i && balls[j].isAvailable)
                                    {
                                        float dxOther = balls[j].x - ball.x;
                                        float dyOther = balls[j].y - ball.y;
                                        if (dyOther * dyOther + dxOther * dxOther < minDistanceSquared) break;
                                    }
                                }
                                if (j >= balls.Length)
                                {
                                    bestX = ball.x;
                                    bestY = ball.y;
                                    found = true;
                                    bestDistance = Mathf.Abs(candidate - basex);
                                }
                            }
                        }
                    }
                }
                ball.x = bestX;
                ball.y = bestY;
                return;
            }
        }
        ball.x = basex;
        ball.y = basey;
    }
}
