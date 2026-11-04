// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using UnityEngine;

public class PoolBall : PoolObject
{
    public delegate void delegate_InHole();

    public float vx;  // +0x28
    public float vy;  // +0x2c
    public float vz;  // +0x30
    public float rvx; // +0x34
    public float rvy; // +0x38
    public float w;   // +0x3c
    public PoolHole inHole; // +0x40
    public bool isAvailable; // +0x44
    public Quaternion quat; // +0x48. Constructor does not initialize identity.
    public delegate_InHole dg_StartInHole; // +0x58
    public delegate_InHole dg_EndInHole;   // +0x5c
    private bool isPot; // +0x60
    private bool isStartFallInHole; // +0x61
    private bool isEndFallInHole; // +0x62
    public bool isFalling; // +0x63
    private float gravity; // +0x64

    // RVA: 0x9229E8
    public PoolBall() : base()
    {
        gravity = -1470f;
        isAvailable = true;
    }

    // RVA: 0x922A28. This copies only the fields actually read by the ARM body.
    public void Update(PoolBall other)
    {
        id = other.id;
        x = other.x;
        y = other.y;
        isAvailable = other.isAvailable;
        visible = other.visible;
        z = other.z;
    }

    // RVA: 0x922A78
    public void Reset(bool resetQuat = true)
    {
        vx = 0f;
        vy = 0f;
        rvx = 0f;
        rvy = 0f;
        w = 0f;
        inHole = null;
        if (resetQuat) quat = Quaternion.identity;
        isPot = false;
        isStartFallInHole = false;
        isEndFallInHole = false;
        isFalling = false;
        vz = 0f;
        z = 0f;
    }

    // RVA: 0x922AE0. Original bytes: 1eff2fe1 (bx lr).
    public void tick(long timeSpan) { }

    // RVA: 0x922AE4. Signed comparisons are original; no Abs has been added.
    public bool IsMoving() { return vx > 5f || vy > 5f; }

    // RVA: 0x922B18
    public bool OnPot()
    {
        if (isPot) return false;
        isPot = true;
        if (!isEndFallInHole) return false;
        delegate_InHole callback = dg_EndInHole;
        if (callback != null) callback();
        return true;
    }

    // RVA: 0x922B6C
    public void OnPotted()
    {
        delegate_InHole callback = dg_EndInHole;
        if (callback != null) callback();
    }

    // RVA: 0x922B88
    public void OnPottedOver()
    {
        visible = false;
        vx = 0f;
        vy = 0f;
        isAvailable = false;
        rvx = 0f;
        rvy = 0f;
        w = 0f;
        inHole = null;
        isPot = false;
        isStartFallInHole = false;
        isEndFallInHole = false;
        isFalling = false;
        quat = Quaternion.identity;
        vz = 0f;
        x = 0f;
        y = 0f;
        z = -150f;
    }

    // RVA: 0x922C0C
    public bool CheckFallOver(float t)
    {
        if (!isStartFallInHole || inHole == null) return false;
        if (isFalling)
        {
            vz += gravity * t;
            z += vz * t;
            if (z < PoolRule.BALL_RADIUS * -3f) return EndFallInHole();
            return false;
        }
        float dx = inHole.x - x;
        float dy = inHole.y - y;
        if (dx * dx + dy * dy < inHole.width * inHole.width * 0.25f)
        {
            vz = 0f;
            isFalling = true;
            z = 0f;
        }
        return false;
    }

    // RVA: 0x922D50
    private bool EndFallInHole()
    {
        isStartFallInHole = false;
        isEndFallInHole = true;
        delegate_InHole callback = dg_EndInHole;
        if (callback != null) callback();
        return true;
    }

    // RVA: 0x922D80
    public void StartFallInHole()
    {
        isStartFallInHole = true;
        isEndFallInHole = false;
    }
}
