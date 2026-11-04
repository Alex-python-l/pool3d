// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using UnityEngine;

public partial class PoolTable
{
    // RVA: 0x926238. Original method name only: this body has not been executed for restoration.
    private void Test_ProcessFirction(float t, PoolBall ball)
    {
        float dx = ball.vx - ball.rvx, dy = ball.vy - ball.rvy;
        float limit = ROLL_K * rule.getForce() * t;
        float speed = Mathf.Sqrt(dy * dy + dx * dx);
        if (speed > limit)
        {
            float force = rule.getForce();
            dx = dx * (force / speed) * t;
            dy = dy * (force / speed) * t;
            ball.vx -= dx;
            ball.vy -= dy;
            stillRunning = true;
            ball.rvx += dx * ROLL_K;
            ball.rvy += dy * ROLL_K;
        }
        speed = Mathf.Sqrt(ball.rvy * ball.rvy + ball.rvx * ball.rvx);
        float rolling = rule.getRollForce() * t;
        if (speed > rolling)
        {
            rolling = (rule.getRollForce() / speed) * t;
            stillRunning = true;
            // Both components subtract the same scalar in the actual ARM (0x926484/0x926488).
            ball.rvx -= rolling;
            ball.rvy -= rolling;
        }
        else
        {
            ball.rvx = 0f;
            ball.rvy = 0f;
        }
    }

    // RVA: 0x925EB0. Original spelling preserved.
    private void ProcessFircetion(float t, PoolBall ball)
    {
        if (TryEnhancedFriction(ball, t)) return;
        float sx = ball.rvx + ball.vx;
        float sy = ball.rvy + ball.vy;
        float slip = Mathf.Sqrt(sy * sy + sx * sx);
        float limit = ROLL_K * PoolRule.SLIDING_FORCE * t;
        if (!(slip > limit)) // ARM BLE also takes the unordered comparison.
        {
            ball.rvx = -ball.vx;
            ball.rvy = -ball.vy;
        }
        else
        {
            float dx = sx * (PoolRule.SLIDING_FORCE / slip) * t;
            float dy = sy * (PoolRule.SLIDING_FORCE / slip) * t;
            ball.vx -= dx;
            ball.vy -= dy;
            stillRunning = true;
            ball.rvx -= dx * ROLL_K;
            ball.rvy -= dy * ROLL_K;
        }
        float spinLoss = PoolRule.SPIN_DECE_RATE * t;
        if (Mathf.Abs(ball.w) > spinLoss)
        {
            if (ball.w >= 0f) ball.w -= spinLoss;
            else ball.w += spinLoss;
        }
        else ball.w = 0f;
        if (!(slip > limit))
        {
            float speed = Mathf.Sqrt(ball.vy * ball.vy + ball.vx * ball.vx);
            if (!(speed > PoolRule.ROLLING_FORCE * t))
            {
                ball.vx = 0f;
                ball.vy = 0f;
                // ARM also adds its zero literal to the two rv fields here.
                ball.rvx += 0f;
                ball.rvy += 0f;
            }
            else
            {
                stillRunning = true;
                float dy = (PoolRule.ROLLING_FORCE / speed) * ball.vy * t;
                float dx = ball.vx * (PoolRule.ROLLING_FORCE / speed) * t;
                ball.vy -= dy;
                ball.vx -= dx;
                ball.rvx = dx + ball.rvx;
                ball.rvy = dy + ball.rvy;
            }
        }
    }

    // RVA: 0x9264C0
    private void ProcessInHole(PoolBall ball, float t)
    {
        float dy = ball.inHole.y - ball.y;
        float dx = ball.inHole.x - ball.x;
        float distance2 = dy * dy + dx * dx;
        float distance = Mathf.Sqrt(distance2);
        dy /= distance;
        dx /= distance;
        float gravity = rule.getHoleGravity();
        float vy = ball.vy;
        float vx = ball.vx;
        float radial = dy * vy + dx * vx;
        float damping = t * -5f + 1f;
        if (radial < 0f) { vy -= dy * radial; vx -= dx * radial; }
        float acceleration = t * 0.5f * gravity;
        vy = damping * vy + dy * acceleration;
        ball.vy = vy;
        vx = damping * vx + dx * acceleration;
        ball.vx = vx;
        float displacement2 = (vy * vy + vx * vx) * t * t;
        if (displacement2 > distance2)
        {
            ball.vy = (distance2 / displacement2) * vy;
            ball.vx = (distance2 / displacement2) * vx;
        }
        ball.rvx += dx * 0.1f * t;
        ball.rvy += dy * 0.1f * t;
    }
}
