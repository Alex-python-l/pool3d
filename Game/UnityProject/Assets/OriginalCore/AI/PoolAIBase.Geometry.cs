// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Original ARM control-flow transcription. Source mapping: module_restore/ai_source.
using UnityEngine;

public partial class PoolAIBase
{
    // RVA: 0x921658
    protected static float calcAimTargetWith(float cos, float sin, float dx, float dy, float r)
    {
        float result = -1f;
        float dot = sin * dy + cos * dx;
        if (!(dot < 0f))
        {
            float twice = dot + dot;
            float a = sin * sin + cos * cos;
            float discriminant = a * -4f * ((dy * dy + dx * dx) - r * r) + twice * twice;
            if (!(discriminant < 0f))
                result = (twice - Mathf.Sqrt(discriminant)) / (a + a);
        }
        return result;
    }

    // RVA: 0x9216D0. Ordered comparisons preserve the ARM NaN rejection branches.
    private static float workout(float x0, float y0, float dx, float dy, float vx, float vy, float t)
    {
        float denominator = dy * vx - dx * vy;
        if ((double)Mathf.Abs(denominator) < 0.001) return -1f;
        float time = (x0 * dy - y0 * dx) / denominator;
        if (!(time > 0f)) return -1f;
        float along = (time * vy - y0) * dy + (time * vx - x0) * dx;
        if (!(along >= 0f) || !(along <= t)) return -1f;
        return time;
    }

    // RVA: 0x9213C8
    protected bool checkRoad1(PoolBall[] balls, float tarx, float tary, int ind, float len)
    {
        float vx = (tarx - balls[ind].x) / len;
        float vy = (tary - balls[ind].y) / len;
        float radius = balls[0].scale;
        for (int i = 1; i < balls.Length; ++i)
        {
            if (i == ind || !balls[i].isAvailable) continue;
            float hit = calcAimTargetWith(vx, vy, balls[i].x - balls[ind].x,
                balls[i].y - balls[ind].y, radius + radius);
            if (hit > 0f && hit < len) return false;
        }
        return checkRoadWithBlocker(balls, tarx, tary, ind, len);
    }

    // RVA: 0x921768
    protected bool checkRoad(PoolBall[] balls, float tarx, float tary, int ind, float len)
    {
        float vx = (tarx - balls[0].x) / len;
        float vy = (tary - balls[0].y) / len;
        float radius = balls[0].scale;
        for (int i = 1; i < balls.Length; ++i)
        {
            if (i == ind || !balls[i].isAvailable) continue;
            float hit = calcAimTargetWith(vx, vy, balls[i].x - balls[0].x,
                balls[i].y - balls[0].y, radius + radius);
            if (hit > 0f && hit < len)
            {
                blocker = i;
                return false;
            }
        }
        return checkRoadWithBlocker(balls, tarx, tary, 0, len);
    }

    // RVA: 0x9219E8
    private bool checkRoadWithBlocker(PoolBall[] balls, float tarx, float tary, int ind, float len)
    {
        float vx = (tarx - balls[ind].x) / len;
        float vy = (tary - balls[ind].y) / len;
        float radius = balls[0].scale;
        for (int i = 0; i < blockersPosition.Length / 4; ++i)
        {
            float x0 = blockersPosition[i * 4], y0 = blockersPosition[i * 4 + 1];
            float x1 = blockersPosition[i * 4 + 2], y1 = blockersPosition[i * 4 + 3];
            float nearest = len;
            float hit = calcAimTargetWith(vx, vy, x0 - balls[ind].x, y0 - balls[ind].y, balls[ind].scale);
            if (hit > 0f) nearest = hit;
            hit = calcAimTargetWith(vx, vy, x1 - balls[ind].x, y1 - balls[ind].y, balls[ind].scale);
            if (hit > 0f && hit < nearest) nearest = hit;
            float dx = x1 - x0, dy = y1 - y0;
            float segmentLength = Mathf.Sqrt(dy * dy + dx * dx);
            dx /= segmentLength;
            dy /= segmentLength;
            float ox = radius * dy, oy = -(dx * radius);
            hit = workout(x0 - (balls[ind].x - ox), y0 - (balls[ind].y - oy),
                dx, dy, vx, vy, segmentLength);
            if (hit > 0f && hit < nearest) nearest = hit;
            hit = workout(x0 - (balls[ind].x + ox), y0 - (balls[ind].y + oy),
                dx, dy, vx, vy, segmentLength);
            if (hit > 0f && hit < nearest) nearest = hit;
            if (nearest > 0f && nearest < len) return false;
        }
        return true;
    }

    // RVA: 0x91EC30
    protected float checkStraightPot(PoolBall[] balls, int ind, int hole)
    {
        float hx = quickholesPosition[hole * 2], hy = quickholesPosition[hole * 2 + 1];
        float dx = hx - balls[ind].x, dy = hy - balls[ind].y;
        float distance = Mathf.Sqrt(dy * dy + dx * dx);
        if (!checkRoad1(balls, hx, hy, ind, distance)) return -1f;
        float diameter = balls[ind].scale + balls[0].scale;
        float ghostX = balls[ind].x - (dx / distance) * diameter;
        float ghostY = balls[ind].y - (dy / distance) * diameter;
        float cueX = ghostX - balls[0].x, cueY = ghostY - balls[0].y;
        float cueDistance = Mathf.Sqrt(cueY * cueY + cueX * cueX);
        float candidateCos = ((dy * cueY + dx * cueX) / distance) / cueDistance;
        if (!((double)candidateCos < 0.1) && checkRoad(balls, ghostX, ghostY, ind, cueDistance))
        {
            targetAngle = MathUtil.Atan2(cueY, cueX);
            targetCosda = candidateCos;
            targetStrength = Mathf.Sqrt(((distance + cueDistance) * PoolRule.ROLLING_FORCE * 20f) / candidateCos);
            if (targetStrength > Var.MAX_STRENGTH) return -1f;
            return distance + cueDistance;
        }
        return -1f;
    }

    // RVA: 0x91F8E4
    protected float checkStraightShot(PoolBall[] balls, int ind, bool b_checkRoad)
    {
        float dx = balls[ind].x - balls[0].x, dy = balls[ind].y - balls[0].y;
        float distance = Mathf.Sqrt(dy * dy + dx * dx);
        targetAngle = MathUtil.Atan2(dy, dx);
        if (b_checkRoad && !checkRoad(balls, balls[ind].x, balls[ind].y, ind, distance))
        {
            float cross = dx * (balls[blocker].y - balls[0].y) - dy * (balls[blocker].x - balls[0].x);
            float step = cross > 0f ? -0.1f : 0.1f;
            float diameter = balls[0].scale + balls[0].scale;
            while (true)
            {
                targetAngle += step;
                float cos = MathUtil.Cos(targetAngle), sin = MathUtil.Sin(targetAngle);
                distance = calcAimTargetWith(cos, sin, balls[ind].x - balls[0].x,
                    balls[ind].y - balls[0].y, diameter);
                if (distance <= 0f) return -1f;
                int i;
                for (i = 1; i < balls.Length; ++i)
                {
                    if (i == ind || !balls[i].isAvailable) continue;
                    float hit = calcAimTargetWith(cos, sin, balls[i].x - balls[0].x,
                        balls[i].y - balls[0].y, diameter);
                    if (hit >= 0f && hit < distance) break;
                }
                if (i == balls.Length) break;
            }
        }
        return distance;
    }

    // RVA: 0x91F170. Reflection constants are original literal bytes, not reconstructed table dimensions.
    protected float checkPotByBlocker(PoolBall[] balls, int ind, int hole)
    {
        float hx = quickholesPosition[hole * 2], hy = quickholesPosition[hole * 2 + 1];
        float dx = hx - balls[ind].x, dy = hy - balls[ind].y;
        float distance = Mathf.Sqrt(dy * dy + dx * dx);
        if (!checkRoad1(balls, hx, hy, ind, distance)) return -1f;
        float diameter = balls[ind].scale + balls[0].scale;
        float ghostX = balls[ind].x - (dx / distance) * diameter;
        float ghostY = balls[ind].y - (dy / distance) * diameter;
        bool found = false;
        float bestDistance = 0f;
        for (int side = 0; side < 4; ++side)
        {
            float cueX = balls[0].x, cueY = balls[0].y;
            Vector2 reflected=Geometry.ReflectCue(new Vector2(cueX,cueY),side);
            float reflectedX=reflected.x,reflectedY=reflected.y;
            float x = ghostX - reflectedX, y = ghostY - reflectedY;
            float cueDistance = Mathf.Sqrt(x * x + y * y);
            float candidateCos = ((dx * x + dy * y) / distance) / cueDistance;
            if ((double)candidateCos < 0.1) continue;
            float tx = 0f, ty = 0f;
            switch (side)
            {
                case 0: case 2:
                    tx = (cueX + reflectedX) * 0.5f;
                    ty = reflectedY + (y / x) * (tx - reflectedX);
                    break;
                case 1: case 3:
                    ty = (cueY + reflectedY) * 0.5f;
                    tx = reflectedX + (x / y) * (ty - reflectedY);
                    break;
            }
            float pocketClearance = PoolRule.BALL_RADIUS + 80f;
            bool nearPocket = false;
            // Original method checks the first six coordinate pairs, even though the array has eighteen pairs.
            for (int h = 0; h < 6; ++h)
            {
                float px = quickholesPosition[h * 2] - tx, py = quickholesPosition[h * 2 + 1] - ty;
                if (py * py + px * px < pocketClearance * pocketClearance) { nearPocket = true; break; }
            }
            if (nearPocket || !checkRoad(balls, tx, ty, -1, cueDistance)) continue;
            balls[0].x = reflectedX;
            balls[0].y = reflectedY;
            bool clear = checkRoad(balls, ghostX, ghostY, ind, cueDistance);
            balls[0].x = cueX;
            balls[0].y = cueY;
            if (!clear) continue;
            switch (side) { case 0: case 2: x = -x; break; case 1: case 3: y = -y; break; }
            float candidateAngle = MathUtil.Atan2(y, x);
            float total = distance + cueDistance;
            float candidateStrength = Mathf.Sqrt((total * PoolRule.ROLLING_FORCE * 30f) / candidateCos);
            if (!(candidateStrength > Var.MAX_STRENGTH) && (total < bestDistance || !found))
            {
                targetCosda = candidateCos;
                targetStrength = candidateStrength;
                targetAngle = candidateAngle;
                found = true;
                bestDistance = total;
            }
        }
        return found ? bestDistance : -1f;
    }

    // RVA: 0x91FE7C. Original writes angle, not targetAngle; this observable difference is retained.
    protected float checkShotByBlocker(PoolBall[] balls, int ind)
    {
        bool found = false;
        float bestDistance = 0f;
        for (int side = 0; side < 4; ++side)
        {
            float cueX = balls[0].x, cueY = balls[0].y;
            Vector2 reflected=Geometry.ReflectCue(new Vector2(cueX,cueY),side);
            float reflectedX=reflected.x,reflectedY=reflected.y;
            float x = balls[ind].x - reflectedX, y = balls[ind].y - reflectedY;
            float tx = 0f, ty = 0f;
            switch (side)
            {
                case 0: case 2:
                    tx = (cueX + reflectedX) * 0.5f;
                    ty = reflectedY + (tx - reflectedX) * (y / x);
                    break;
                case 1: case 3:
                    ty = (cueY + reflectedY) * 0.5f;
                    tx = reflectedX + (ty - reflectedY) * (x / y);
                    break;
            }
            float clearance = PoolRule.BALL_RADIUS + 80f;
            bool nearPocket = false;
            for (int h = 0; h < quickholesPosition.Length / 2; ++h)
            {
                float px = quickholesPosition[h * 2] - tx, py = quickholesPosition[h * 2 + 1] - ty;
                if (py * py + px * px < clearance * clearance) { nearPocket = true; break; }
            }
            if (nearPocket) continue;
            float distance = Mathf.Sqrt(y * y + x * x);
            if (!checkRoad(balls, tx, ty, -1, distance)) continue;
            balls[0].x = reflectedX;
            balls[0].y = reflectedY;
            bool clear = checkRoad(balls, balls[ind].x, balls[ind].y, ind, distance);
            if (clear && !(bestDistance <= distance && found))
            {
                switch (side) { case 0: case 2: x = -x; break; case 1: case 3: y = -y; break; }
                angle = MathUtil.Atan2(y, x);
                found = true;
                bestDistance = distance;
            }
            balls[0].x = cueX;
            balls[0].y = cueY;
        }
        return found ? bestDistance : -1f;
    }
}
