// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using SoundManager = Pool3D.Offline.OfflineAudio;
using UnityEngine;

public partial class PoolTable
{
    // RVA: 0x923918. Original synchronous result loop, including its logging and visibility gate.
    public void clickAndGetResult(float strength, float px, float py, float angle)
    {
        long started = TimeUtil.CurrentTimeMills();
        click(strength, px, py, angle);
        Debug.LogError(string.Concat("roundstate.state= ", roundstate.state.ToString(), " strength=", strength.ToString()));
        while (true)
        {
            if (visible && roundstate.state == 2) FixedUpdate(0.1f);
            if (roundstate.state != 2) break;
        }
        long elapsed = TimeUtil.CurrentTimeMills() - started;
        Debug.LogError(string.Concat("get result cost time=", elapsed.ToString()));
    }

    // RVA: 0x923B1C
    public void click(float strength, float px, float py, float angle)
    {
        if (roundstate.state != 1) return;
        PoolBall ball = balls[0];
        // Experimental optional hook; disabled mode executes the original body below.
        if (!TryEnhancedStrike(strength, px, py, angle))
        {
        px = Mathf.Clamp(px, -1f, 1f);
        py = Mathf.Clamp(py, -1f, 1f);
        float magnitude = (1f - Mathf.Abs(px) * 0.5f)
                        * (1f - Mathf.Abs(py) * 0.5f) * strength;
        float rolling = (strength + strength) * py;
        float cos = MathUtil.Cos(angle);
        float sin = MathUtil.Sin(angle);
        ball.rvy = rolling * sin;
        ball.rvx = rolling * cos;
        ball.vy = magnitude * sin;
        ball.vx = magnitude * cos;
        ball.w = -(px * strength) / rule.getBallRadius();
        }
        time_LastLeft = 0f;
        rule.onClick(roundstate, balls);
        int range = Var.GetStrenthRangeId(strength);
        if (range == 3) SoundManager.Instance().PlaySound2D(15, 1f);
        else if (range == 2) SoundManager.Instance().PlaySound2D(17, 1f);
        else if (range == 1) SoundManager.Instance().PlaySound2D(16, 1f);
    }

    // RVA: 0x923E88. The original legacy path does not clamp spin or play a shot sound.
    public void click_old(float strength, float px, float py, float angle)
    {
        if (roundstate.state != 1) return;
        PoolBall ball = balls[0];
        float cos = MathUtil.Cos(angle), sin = MathUtil.Sin(angle);
        ball.vy = sin * strength;
        ball.vx = cos * strength;
        ball.rvy = strength * py * sin;
        ball.rvx = strength * py * cos;
        ball.w = -(strength * px) / rule.getBallRadius();
        time_LastLeft = 0f;
        rule.onClick(roundstate, balls);
    }
}
