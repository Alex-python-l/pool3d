// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using SoundManager = Pool3D.Offline.OfflineAudio;
using UnityEngine;

public partial class PoolTable
{
    // RVA: 0x927014
    private void doHit(PoolBall ball, PoolBall other)
    {
        if (TryEnhancedBallHit(ball, other)) return; // Optional, no duplicate original losses.
        float dy = other.y - ball.y, dx = other.x - ball.x;
        float distance2 = dy * dy + dx * dx;
        float angle = Mathf.Atan2(ball.vx, ball.vy);
        float sin = Mathf.Sin(angle), cos = Mathf.Cos(angle);
        float overlap = ball.scale + ball.scale - Mathf.Sqrt(distance2);
        if (overlap > 0f)
        {
            ball.y -= cos * overlap;
            ball.x -= sin * overlap;
            dy = other.y - ball.y;
            dx = other.x - ball.x;
            distance2 = dy * dy + dx * dx;
        }
        float distance = Mathf.Sqrt(distance2);
        dy /= distance;
        dx /= distance;
        float oldRvx = ball.rvx, oldRvy = ball.rvy;
        float relative = (dy * ball.vy + ball.vx * dx) - (dy * other.vy + dx * other.vx);
        float impulseY = dy * relative, impulseX = dx * relative;
        ball.vy -= impulseY;
        ball.vx -= impulseX;
        other.vy += impulseY;
        other.vx += impulseX;
        ball.vy *= 0.95f;
        ball.vx *= 0.95f;
        ball.rvx = oldRvx * 0.8f;
        ball.rvy = oldRvy * 0.8f;
        float volume = Mathf.Clamp(Mathf.Sqrt(ball.vy * ball.vy + ball.vx * ball.vx) * 0.001f * 0.2f, 0.02f, 1f);
        SoundManager.Instance().PlaySound2D(0, volume);
    }

    // RVA: 0x9272B4. The blocker parameter is not read; the selected contact is static hitx/hity.
    private void doHit(PoolBall ball, PoolBlocker blocker)
    {
        if (TryEnhancedCushionHit(ball)) return;
        float nx = ball.x - hitx, ny = ball.y - hity;
        float distance = Mathf.Sqrt(ny * ny + nx * nx);
        nx /= distance;
        ny /= distance;
        float vx = ball.vx, vy = ball.vy;
        float normal = ny * vy + nx * vx;
        if (normal < 0f)
        {
            vy += ny * (normal * -2f);
            vx += nx * (normal * -2f);
            ball.vy = vy;
            ball.vx = vx;
            if (Mathf.Abs(ball.w) > 0f)
            {
                float delta = ROLL_K * rule.getBallRadius() * ball.w * 0.5f * 0.1f;
                vx = ball.vx - ny * delta;
                vy = ball.vy + nx * delta;
                ball.vx = vx;
                ball.vy = vy;
            }
            else vx = ball.vx;
            vx *= BLOCK_HIT_LOST;
            vy = ball.vy * BLOCK_HIT_LOST;
            ball.vx = vx;
            ball.vy = vy;
            ball.w *= 0.5f;
            ball.rvx = (ball.rvx - vx) * 0.5f;
            ball.rvy = (ball.rvy - vy) * 0.5f;
        }
        float volume = Mathf.Clamp(Mathf.Sqrt(vy * vy + vx * vx) * 0.001f * 0.5f, 0.2f, 1f);
        SoundManager.Instance().PlaySound2D(1, volume);
    }

    // RVA: 0x9275A4
    private void doHit(PoolBall ball, PoolHole hole)
    {
        ball.inHole = hole;
        ball.StartFallInHole();
    }

    // RVA: 0x9275D8, original ARM 1eff2fe1 (bx lr).
    private void onPotBall(PoolBall ball, PoolHole hole) { }

    // RVA: 0x926CCC. Cache writes are row i and column j exactly as in the original.
    private void doHitAllBall()
    {
        float threshold = nextHitTime[0, hita, hitb] + 0.008f;
        for (int i = 0; i < balls.Length; ++i)
        {
            for (int j = 0; j < i; ++j)
            {
                if (!(nextHitTime[0, i, j] <= threshold)) continue;
                doHit(balls[i], balls[j]);
                rule.onHitBall(roundstate, i, j, balls);
                for (int k = 0; k < balls.Length; ++k)
                {
                    nextHitTime[0, i, k] = MAX_TIME;
                    nextHitTime[0, k, j] = MAX_TIME;
                }
            }
        }
    }

    // RVA: 0x9253E4. Native 0x17E43FC is String.Format, with its return value discarded.
    private void processHit()
    {
        switch (hittype)
        {
            case 0:
                if (!isFirstShoot)
                {
                    doHit(balls[hita], balls[hitb]);
                    rule.onHitBall(roundstate, hita, hitb, balls);
                }
                else doHitAllBall();
                break;
            case 1:
                doHit(balls[hita], blockers[hitb]);
                rule.onHitBlocker(roundstate, hita, hitb, balls, blockers);
                break;
            case 2:
                doHit(balls[hita], holes[hitb]);
                break;
            case 3:
                // The native branch only performs array/object checks; no position or velocity write.
                PoolBall checkedBall = balls[hita];
                PoolHole checkedHole = checkedBall.inHole;
                break;
        }
        switch (hittype)
        {
            case 0: string.Format("0:ball--ball, ball1Id = {0}, ball2Id = {1}", hita, hitb); break;
            case 1: string.Format("1:ball--blocker, ballId = {0}, blockerId = {1}", hita, hitb); break;
            case 2: string.Format("2:ball--hole, ballId = {0}, holeId = {1}", hita, hitb); break;
            case 3: string.Format("3:ball in hole, ballId = {0}, holeId = {1}", hita, balls[hita].inHole); break;
        }
    }
}
