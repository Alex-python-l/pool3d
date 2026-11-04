// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using SoundManager = Pool3D.Offline.OfflineAudio;
using UnityEngine;

public partial class PoolTable
{
    // RVA: 0x923214
    public void tick(float timeSpan)
    {
        if (visible && roundstate != null && roundstate.state == 2)
            FixedUpdate(timeSpan);
    }

    // RVA: 0x92323C. The lasttime argument is not read by the original callee.
    private void FixedUpdate(float timeSpam)
    {
        stillRunning = false;
        time_LastLeft += timeSpam;
        while (time_LastLeft >= fixedTimeSpan)
        {
            forceAll = true;
            hittype = -1;
            float hitTime = getNextHitTime(false, 0f);
            forceAll = false;
            float left = fixedTimeSpan;
            if (!(hitTime >= fixedTimeSpan))
            {
                do
                {
                    doRunning(hitTime);
                    processHit();
                    float next = getNextHitTime(false, 0f);
                    left -= hitTime;
                    hitTime = next;
                    forceAll = false;
                }
                while (!(hitTime >= left));
            }
            doRunning(left);
            ProcessForce(fixedTimeSpan);
            time_LastLeft += -fixedTimeSpan;
            if (!stillRunning) return;
        }
    }

    // RVA: 0x925180
    private void doRunning(float t)
    {
        for (int i = 0; i < balls.Length; ++i)
        {
            PoolBall ball = balls[i];
            if (ball.CheckFallOver(t))
            {
                ball.isAvailable = false;
                rule.onPotBall(roundstate, ball.id, balls, ball.inHole);
                // The native body inlines precisely PoolBall.OnPottedOver (0x922B88).
                ball.OnPottedOver();
                SoundManager.Instance().PlaySound2D(14, 1f);
            }
            if (ball.visible && ball.isAvailable)
            {
                if (ball.inHole != null) ProcessInHole(ball, t);
                ball.x += ball.vx * t;
                ball.y += ball.vy * t;
            }
        }
    }

    // RVA: 0x925A2C. Quaternion multiplication is delta * previous, not the reverse.
    private void ProcessForce(float t)
    {
        stillRunning = false;
        for (int i = 0; i < balls.Length; ++i)
        {
            PoolBall ball = balls[i];
            if (!ball.visible) continue;
            if (!ball.isAvailable)
            {
                stillRunning = true;
                continue;
            }
            if (ball.inHole != null) stillRunning = true;
            float angularSpeed = Mathf.Sqrt(ball.rvy * ball.rvy + ball.rvx * ball.rvx);
            if (angularSpeed > 1f)
            {
                tmp.x = ball.rvy;
                tmp.y = -ball.rvx;
                tmp.z = 0f;
                tempQuat = Quaternion.AngleAxis(angularSpeed * t * PoolRule.V2W, tmp);
                tempQuat = tempQuat * ball.quat;
                ball.quat = tempQuat;
            }
            // 0x925E7C: 0x42652EE1 = 57.295780181884766f; Vector3 static +0x48 = forwardVector.
            tempQuat = Quaternion.AngleAxis(ball.w * t * 57.29578f, Vector3.forward);
            tempQuat = tempQuat * ball.quat;
            ball.quat = tempQuat;
            rule.checkBallPosition(ball);
            ProcessFircetion(t, ball);
        }
        if (!stillRunning)
        {
            isFirstShoot = false;
            rule.onRoundOver(roundstate, balls, holes);
        }
    }
}
