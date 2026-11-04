// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using UnityEngine;

public class PoolBlocker : PoolObject
{
    public float force;  // +0x28
    protected float x0; // +0x2c
    protected float y0; // +0x30
    protected float x1; // +0x34
    protected float y1; // +0x38

    // RVA: 0x922FCC. Original constructor contains only inherited initialization.
    public PoolBlocker() : base() { }

    // RVA: 0x922E74
    public void setLinePosition(float x0, float y0, float x1, float y1)
    {
        this.x0 = x0;
        this.y0 = y0;
        this.x1 = x1;
        this.y1 = y1;
    }

    // RVA: 0x922E84
    public float getX0() { return x0; }
    // RVA: 0x922E8C
    public float getY0() { return y0; }
    // RVA: 0x922E94
    public float getX1() { return x1; }
    // RVA: 0x922E9C
    public float getY1() { return y1; }

    // RVA: 0x922EA4
    public Vector3 MapPot2Real_Start()
    {
        return new Vector3(y0 * PoolRule.REAL_SIMULATOR_SCALE,
                           0f, PoolRule.REAL_SIMULATOR_SCALE * x0);
    }

    // RVA: 0x922F38
    public Vector3 MapPot2Real_End()
    {
        return new Vector3(y1 * PoolRule.REAL_SIMULATOR_SCALE,
                           0f, PoolRule.REAL_SIMULATOR_SCALE * x1);
    }
}
