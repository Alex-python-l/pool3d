// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Transcribed from IL2CPP metadata and original ARM; not original source text.
using UnityEngine;

public class PoolObject
{
    public int id;       // ARM32 +0x08
    public float x;      // +0x0c
    public float y;      // +0x10
    public float z;      // +0x14
    public float width;  // +0x18
    public float height; // +0x1c
    public float scale;  // +0x20
    public bool visible; // +0x24

    // RVA: 0x922A10
    public PoolObject()
    {
        visible = true;
        scale = 1f;
    }

    // RVA: 0x922FFC
    public bool IsVisible() { return visible; }

    // RVA: 0x923004
    public void SetVisible(bool visible) { this.visible = visible; }

    // RVA: 0x92300C
    public void SetSize(float w, float h) { width = w; height = h; }

    // RVA: 0x923018
    public void SetPosition(float x, float y) { this.x = x; this.y = y; }

    // RVA: 0x922C00
    public void SetPosition(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    // RVA: 0x923024
    public float GetX() { return x; }

    // RVA: 0x92302C
    public float GetY() { return y; }

    // RVA: 0x923034
    public Vector3 MapPot2Real()
    {
        return new Vector3(y * PoolRule.REAL_SIMULATOR_SCALE,
                           z * PoolRule.REAL_SIMULATOR_SCALE,
                           PoolRule.REAL_SIMULATOR_SCALE * x);
    }
}
