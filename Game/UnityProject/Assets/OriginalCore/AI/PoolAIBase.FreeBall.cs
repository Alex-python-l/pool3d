// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Original ARM free-ball search, including its retry/reset behavior.
using UnityEngine;

public partial class PoolAIBase
{
    // RVA: 0x920C1C
    protected float checkFreeballPot(PoolBall[] balls, int ind, int hole)
    {
        float hx = quickholesPosition[hole * 2], hy = quickholesPosition[hole * 2 + 1];
        float dx = hx - balls[ind].x, dy = hy - balls[ind].y;
        float distance = Mathf.Sqrt(dy * dy + dx * dx);
        if (!checkRoad1(balls, hx, hy, ind, distance)) return -1f;
        float directionX = -dx / distance, directionY = -dy / distance;
        float radius = balls[0].scale;
        float nearest = -1f;
        for (int i = 1; i < balls.Length; ++i)
        {
            if (i == ind || !balls[i].isAvailable) continue;
            float hit = calcAimTargetWith(directionX, directionY,
                balls[i].x - balls[ind].x, balls[i].y - balls[ind].y, radius + radius);
            if (hit > 0f && (hit < nearest || nearest < 0f)) nearest = hit;
        }
        for (int i = 0; i < quickblockersPosition.Length / 4; ++i)
        {
            float x0 = quickblockersPosition[i * 4], y0 = quickblockersPosition[i * 4 + 1];
            float segmentX = quickblockersPosition[i * 4 + 2] - x0;
            float segmentY = quickblockersPosition[i * 4 + 3] - y0;
            // The original adds these coordinates. This is not replaced with Euclidean length.
            float extent = segmentX + segmentY;
            segmentX /= extent;
            segmentY /= extent;
            float oy = -(segmentX * radius), ox = radius * segmentY;
            if (directionY * -(segmentX * radius) + directionX * radius * segmentY < 0f)
            {
                oy = -(radius * -segmentX);
                ox = -(radius * segmentY);
            }
            float hit = workout(x0 - (ox + balls[ind].x), y0 - (oy + balls[ind].y),
                segmentX, segmentY, directionX, directionY, extent);
            if (hit > 0f && (hit < nearest || nearest < 0f)) nearest = hit;
        }
        float diameter = radius + radius;
        if (nearest < diameter) return -1f;
        float separation = nearest * 0.5f;
        if (separation > diameter * 5f) separation = diameter * 5f;
        if (separation < diameter) separation = diameter;
        targetTarx = balls[ind].x + directionX * separation;
        targetTary = balls[ind].y + directionY * separation;
        targetAngle = MathUtil.Atan2(-directionY, -directionX);
        targetStrength = Mathf.Sqrt((distance + separation) * PoolRule.ROLLING_FORCE);
        if (targetStrength > Var.MAX_STRENGTH) targetStrength = Var.MAX_STRENGTH;
        return distance;
    }

    // RVA: 0x9204E4. The roundstate argument is replaced by the owning PoolTable's state in native code.
    public virtual void thinkFreeball(RoundState roundstate, PoolBall[] balls, bool isTargetPocketPicked)
    {
        strength = Var.MAX_STRENGTH;
        VSRule rule = (VSRule)m_PoolCtr.M_PoolRule;
        RoundState state = m_PoolCtr.M_PoolTable.getRoundState();
        float bestDistance = 0f;
        if (rule.isUnderlineFreeball(state))
        {
            bool found = false;
            for (int i = 1; i < balls.Length; ++i)
            {
                if (!balls[i].isAvailable || !m_PoolCtr.M_PoolRule.isAvailableTarget(state, balls, i)) continue;
                for (int h = 0; h < quickholesPosition.Length / 2; ++h)
                {
                    if (isTargetPocketPicked && targetPocketId != h / 3) continue;
                    float candidate = checkFreeballPot(balls, i, h);
                    if (!(candidate >= 0f) || (found && !(candidate < bestDistance))) continue;
                    found = true;
                    angle = targetAngle;
                    strength = targetStrength;
                    tarx = targetTarx;
                    tary = targetTary;
                    bestDistance = candidate;
                    if (!isTargetPocketPicked) targetPocketId = h / 3;
                }
            }
            if (found) return;
        }
        maxlevel = -1;
        float originalX = balls[0].x, originalY = balls[0].y;
        float diameter = PoolRule.BALL_RADIUS + PoolRule.BALL_RADIUS;
        float width = Geometry.LongSide - diameter;
        float height = Geometry.ShortSide - diameter;
        float maxX = rule.isUnderlineFreeball(state) ? Geometry.HeadStringX - diameter : width * 0.5f;
        ignoreLog = true;
        float bestAngle = 0f, bestStrength = 0f, bestCos = 0f;
        int rejected = 0;
        while (rejected < 30)
        {
            float x = UnityEngine.Random.Range(width * -0.5f, maxX);
            float y = UnityEngine.Random.Range(height * -0.5f, height * 0.5f);
            // A geometrically rejected location does not increment the original counter.
            if (!rule.canPut(balls, 0, x, y)) continue;
            balls[0].x = x;
            balls[0].y = y;
            think(state, balls, isTargetPocketPicked);
            bool better = level > maxlevel;
            if (level == maxlevel)
            {
                if (level == 1) better = score < bestDistance;
                else if (level == 2) better = cosda > bestCos;
            }
            if (!better) { ++rejected; continue; }
            bestCos = cosda;
            bestAngle = angle;
            bestStrength = strength * 1.25f;
            bestDistance = score;
            maxlevel = level;
            tarx = x;
            tary = y;
            // Any improvement resets the counter: this is not a maximum of thirty total samples.
            rejected = 0;
        }
        angle = bestAngle;
        ignoreLog = false;
        strength = bestStrength;
        balls[0].x = originalX;
        balls[0].y = originalY;
        maxlevel = -1;
    }
}
