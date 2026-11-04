// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using UnityEngine;

public partial class PoolTable
{
    // RVA: 0x926664
    private float getNextHitTime(PoolBall ball, PoolBall other)
    {
        return workout(ball.x - other.x, ball.y - other.y,
            ball.vx - other.vx, ball.vy - other.vy, ball.scale + other.scale);
    }

    // RVA: 0x926954
    private float getNextHitTime(PoolBall ball, PoolHole hole)
    {
        // Native 0x92699C..0x9269A8 accesses/checks ball and hole before the equality exit.
        float ballX = ball.x;
        float holeX = hole.x;
        if (ball.inHole == hole) return MAX_TIME;
        return workout(ballX - holeX, ball.y - hole.y, ball.vx, ball.vy, hole.width * 0.5f);
    }

    // RVA: 0x926A38. Epsilon literal at 0x926B74; no zero-distance repair is added.
    private float workout(float x, float y, float vx, float vy, float r)
    {
        float c = (y * y + x * x) - r * r;
        float b = (y * vy + x * vx) * -2f;
        if (c <= 0.0001f && b > 0f) return 0f;
        float a = vy * vy + vx * vx;
        if (a < 0.0001f) return MAX_TIME;
        float discriminant = (a * -4f) * c + b * b;
        if (discriminant <= 0.0001f) return MAX_TIME;
        float time = (b - Mathf.Sqrt(discriminant)) / (a + a);
        if (time < 0f) return MAX_TIME;
        return time; // ARM BPL at 0x926B38 also returns an unordered result.
    }

    // RVA: 0x926B90. The threshold is the original double literal, not float epsilon.
    private float workout(float x0, float y0, float dx, float dy, float vx, float vy, float t)
    {
        float determinant = dy * vx - dx * vy;
        if ((double)Mathf.Abs(determinant) < 0.0001) return MAX_TIME;
        float time = (x0 * dy - y0 * dx) / determinant;
        if (!(time > 0f)) return MAX_TIME;
        float projection = (time * vy - y0) * dy + (time * vx - x0) * dx;
        if (projection < 0f || !(projection <= t)) return MAX_TIME;
        return time;
    }
}
