// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using UnityEngine;

public partial class PoolTable
{
    // RVA: 0x9275DC
    public float calcAimTarget(float aimAngle)
    {
        int target = rule.getClickTarget(roundstate);
        PoolBall ball = balls[target];
        float result = rule.getTableHeight() + rule.getTableWidth();
        float cos = MathUtil.Cos(aimAngle);
        float sin = MathUtil.Sin(aimAngle);
        lastAimBallIndex = -1;
        for (int i = 0; i < blockers.Length; ++i)
        {
            if (!blockers[i].visible) continue;
            float distance = calcAimTargetWith(ball, blockers[i], aimAngle, cos, sin);
            if (distance < result && distance >= 0.0001f)
            {
                result = distance;
                lastAimBallIndex = -1;
            }
        }
        for (int i = 0; i < balls.Length; ++i)
        {
            if (i == target || !balls[i].isAvailable) continue;
            float distance = calcAimTargetWith(ball, balls[i], aimAngle, cos, sin);
            if (distance < result && distance >= 0.0001f)
            {
                result = distance;
                lastAimBallIndex = i;
            }
        }
        return result;
    }

    // RVA: 0x927C68
    public int getLastAimBallIndex() { return lastAimBallIndex; }

    // RVA: 0x927C70
    public bool IsValidAimBall()
    {
        return lastAimBallIndex == -1 || rule.isAvailableTarget(roundstate, balls, lastAimBallIndex);
    }

    // RVA: 0x927BEC. The original angel parameter is unused.
    private float calcAimTargetWith(PoolBall ball, PoolBall other, float angel, float cos, float sin)
    {
        return calcAimTargetWith(cos, sin, other.x - ball.x, other.y - ball.y, ball.scale + other.scale);
    }

    // RVA: 0x927CD0. Original invalid-result literal at 0x927DA4 is zero.
    private float calcAimTargetWith(float cos, float sin, float dx, float dy, float r)
    {
        float projection = sin * dy + cos * dx;
        if (projection < 0f) return 0f;
        float a = sin * sin + cos * cos;
        float b = projection + projection;
        float discriminant = (a * -4f) * ((dy * dy + dx * dx) - r * r) + b * b;
        if (discriminant < 0f) return 0f;
        return (b - Mathf.Sqrt(discriminant)) / (a + a);
    }

    // RVA: 0x927DB8. Nonpositive time is returned, not replaced with the zero sentinel.
    private float calcAimTargetWith(float x0, float y0, float dx, float dy, float cos, float sin, float len)
    {
        float determinant = dy * cos - dx * sin;
        if ((double)Mathf.Abs(determinant) < 0.0001) return 0f;
        float time = (x0 * dy - y0 * dx) / determinant;
        if (!(time > 0f)) return time;
        float projection = (time * sin - y0) * dy + (time * cos - x0) * dx;
        if (projection < 0f) return 0f;
        if (!(projection > len)) return time; // Original VMOVLE includes unordered.
        return 0f;
    }

    // RVA: 0x9278C4. Tests both endpoints and both radius-offset segment sides.
    private float calcAimTargetWith(PoolBall ball, PoolBlocker blocker, float angel, float cos, float sin)
    {
        float x0 = blocker.getX0(), y0 = blocker.getY0();
        float x1 = blocker.getX1(), y1 = blocker.getY1();
        float result = rule.getTableHeight() + rule.getTableWidth();
        float first = calcAimTargetWith(cos, sin, x0 - ball.x, y0 - ball.y, ball.scale);
        if (first >= 0.0001f) result = Mathf.Min(result, first);
        float second = calcAimTargetWith(cos, sin, x1 - ball.x, y1 - ball.y, ball.scale);
        if (second >= 0.0001f) result = Mathf.Min(result, second);
        float dx = x1 - x0, dy = y1 - y0;
        float length = Mathf.Sqrt(dy * dy + dx * dx);
        if (length < 0.0001f) return second; // Actual short-segment branch returns second endpoint result.
        dx /= length;
        dy /= length;
        float ox = dy * ball.scale;
        float oy = -(dx * ball.scale);
        first = calcAimTargetWith(x0 - (ball.x - ox), y0 - (ball.y - oy), dx, dy, cos, sin, length);
        second = calcAimTargetWith(x0 - (ball.x + ox), y0 - (oy + ball.y), dx, dy, cos, sin, length);
        if (first >= 0.0001f) result = Mathf.Min(result, first);
        if (second >= 0.0001f) return Mathf.Min(result, second);
        return result;
    }
}
