// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Restored from the original ARM32 IL2CPP bodies, not original source text.
// libil2cpp.so SHA256:
// 4145521bc05cfcce7b4838edee06ef963f5bae0a62c0426ecf930089f90a7380
// Evidence: research/3DPoolBall/20261018_module_restore/collision_search/.
using UnityEngine;

public partial class PoolTable
{
    // RVA: 0x924014. The original body does not consume either argument.
    // Cache planes: 0 ball/ball, 1 ball/blocker, 2 ball/hole,
    // 3 ball/current inHole, 4 and 5 blocker contact x/y.
    private float getNextHitTime(bool forceAll, float lasttime)
    {
        // 0x924050..0x924AE0: refresh the original candidate cache.
        for (int i = 0; i < balls.Length; ++i)
        {
            if (balls[i].visible && balls[i].isAvailable)
            {
                for (int j = 0; j < i; ++j)
                {
                    float time;
                    if (balls[j].visible && balls[j].isAvailable)
                        time = getNextHitTime(balls[i], balls[j]);
                    else
                        time = MAX_TIME;

                    nextHitTime[0, j, i] = time;
                    nextHitTime[0, i, j] = time;
                }

                for (int j = 0; j < blockers.Length; ++j)
                {
                    if (blockers[j].visible)
                    {
                        nextHitTime[1, i, j] = getNextHitTime(balls[i], blockers[j]);
                        nextHitTime[4, i, j] = hitx0;
                        nextHitTime[5, i, j] = hity0;
                    }
                    else
                    {
                        // The original leaves planes 4/5 unchanged here.
                        nextHitTime[1, i, j] = MAX_TIME;
                    }
                }

                if (balls[i].inHole != null)
                {
                    nextHitTime[3, i, 0] = getNextHitTime(balls[i], balls[i].inHole);
                }
                else
                {
                    for (int j = 0; j < holes.Length; ++j)
                    {
                        if (holes[j].visible)
                            nextHitTime[2, i, j] = getNextHitTime(balls[i], holes[j]);
                        else
                            nextHitTime[2, i, j] = MAX_TIME;
                    }
                }
            }
            else
            {
                // 0x9245AC: an invisible/unavailable ball clears these planes.
                for (int j = 0; j < i; ++j)
                {
                    nextHitTime[0, j, i] = MAX_TIME;
                    nextHitTime[0, i, j] = MAX_TIME;
                }
                for (int j = 0; j < blockers.Length; ++j)
                    nextHitTime[1, i, j] = MAX_TIME;
                for (int j = 0; j < holes.Length; ++j)
                    nextHitTime[2, i, j] = MAX_TIME;
                nextHitTime[3, i, 0] = MAX_TIME;
            }
        }

        // 0x924AE4..0x925148: strict comparisons preserve the original tie order.
        // No candidate below MAX_TIME means the prior hit metadata is retained.
        float nearest = MAX_TIME;
        for (int i = 0; i < balls.Length; ++i)
        {
            for (int j = 0; j < i; ++j)
            {
                if (nextHitTime[0, i, j] < nearest)
                {
                    hitb = j;
                    hita = i;
                    nearest = nextHitTime[0, i, j];
                    hittype = 0;
                }
            }

            for (int j = 0; j < blockers.Length; ++j)
            {
                if (nextHitTime[1, i, j] < nearest)
                {
                    hitb = j;
                    hita = i;
                    nearest = nextHitTime[1, i, j];
                    hittype = 1;
                    hitx = nextHitTime[4, i, j];
                    hity = nextHitTime[5, i, j];
                }
            }

            if (balls[i].inHole != null)
            {
                if (nextHitTime[3, i, 0] < nearest)
                {
                    hitb = 0;
                    hita = i;
                    nearest = nextHitTime[3, i, 0];
                    hittype = 3;
                }
            }
            else
            {
                for (int j = 0; j < holes.Length; ++j)
                {
                    if (nextHitTime[2, i, j] < nearest)
                    {
                        hitb = j;
                        hita = i;
                        nearest = nextHitTime[2, i, j];
                        hittype = 2;
                    }
                }
            }
        }
        return nearest;
    }

    // RVA: 0x9266F0. Endpoint candidates are considered before the segment body.
    private float getNextHitTime(PoolBall ball, PoolBlocker blocker)
    {
        float x0 = blocker.getX0();
        float y0 = blocker.getY0();
        float x1 = blocker.getX1();
        float y1 = blocker.getY1();

        // 0x926760..0x92682C: keep the winning endpoint as a side effect.
        float nearest = workout(ball.x - x0, ball.y - y0,
                                ball.vx, ball.vy, ball.scale);
        hitx0 = x0;
        hity0 = y0;
        float endTime = workout(ball.x - x1, ball.y - y1,
                                ball.vx, ball.vy, ball.scale);
        if (endTime < nearest)
        {
            hitx0 = x1;
            hity0 = y1;
            nearest = endTime;
        }

        // 0x926834..0x9268CC: original direction, normal and sign branch.
        // Sqrt's original 0xA02638 body uses vsqrt.f32 at 0xA0268C.
        float dx = x1 - x0;
        float dy = y1 - y0;
        float length = Mathf.Sqrt(dy * dy + dx * dx);
        float unitX = dx / length;
        float unitY = dy / length;
        float offsetY = -(unitX * ball.scale);
        float offsetX = unitY * ball.scale;
        if (offsetY * ball.vy + offsetX * ball.vx < 0f)
        {
            offsetX = -(unitY * ball.scale);
            offsetY = -(ball.scale * -unitX);
        }

        float contactX = ball.x + offsetX;
        float contactY = ball.y + offsetY;
        float lineTime = workout(x0 - contactX, y0 - contactY,
                                 unitX, unitY, ball.vx, ball.vy, length);
        if (lineTime < nearest)
        {
            // 0x926918..0x92692C: coordinates are saved for the later doHit.
            hitx0 = contactX + lineTime * ball.vx;
            hity0 = contactY + lineTime * ball.vy;
            nearest = lineTime;
        }
        return nearest;
    }
}
