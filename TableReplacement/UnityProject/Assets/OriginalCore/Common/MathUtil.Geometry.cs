// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using UnityEngine;

// Recovered global type. This partial file contains only bodies tied to the RVAs below.
// It does not supply substitutes for other original MathUtil methods.
public partial class MathUtil
{
    // Static initializer RVA 0xDE68E0, literal bits 0x3727c5ac.
    public static float EPSILON = 0.00001f;

    // RVA 0xDE4C0C; original single-precision degree conversion precedes cosf.
    public static float Cos(float angle) { return Mathf.Cos(angle * 0.01745329238474369f); }

    // RVA 0xDE4C24.
    public static float Sin(float angle) { return Mathf.Sin(angle * 0.01745329238474369f); }

    // RVA 0xDE4C3C; float return then single-precision scale.
    public static float Atan2(float y, float x) { return Mathf.Atan2(y, x) * 57.295780181884766f; }

    // RVA 0xDE5274.
    public static void Swap(ref float a, ref float b)
    {
        float previous = a;
        a = b;
        b = previous;
    }

    // RVA 0xDE5A54.
    public static float Cross(float x1, float y1, float x2, float y2)
    {
        return x1 * y2 - y1 * x2;
    }

    // RVA 0xDE5A74. Product/addition order follows the native expression.
    public static float area(Vector2 p0, Vector2 p1, Vector2 p2)
    {
        return (p1.x - p0.x) * (p2.y - p0.y) - (p1.y - p0.y) * (p2.x - p0.x);
    }

    // RVA 0xDE5858. Collinear/parallel segments are rejected by the original body.
    public static bool LineIntersection2D(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3,
        ref Vector2 intersectPoint)
    {
        float determinant = (p1.x - p0.x) * (p3.y - p2.y) -
                            (p1.y - p0.y) * (p3.x - p2.x);
        if (determinant == 0f) return false;
        float a = area(p0, p1, p2);
        float b = area(p0, p1, p3);
        float product = a * b;
        if (!(product <= 0f)) return false;
        a = area(p0, p2, p3);
        b = area(p1, p2, p3);
        product = a * b;
        if (!(product <= 0f)) return false;
        float t = Mathf.Abs(a) / (Mathf.Abs(a) + Mathf.Abs(b));
        if (t > 1f) t = 1f;
        // The numerator is nonnegative for finite inputs; no invented epsilon branch.
        intersectPoint.y = p0.y + (p1.y - p0.y) * t;
        intersectPoint.x = p0.x + (p1.x - p0.x) * t;
        return true;
    }

    // RVA 0xDE5B20.
    public static bool LineIntersect2D(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        float determinant = (p1.x - p0.x) * (p3.y - p2.y) -
                            (p1.y - p0.y) * (p3.x - p2.x);
        if (determinant == 0f) return false;
        float product = area(p0, p1, p2) * area(p0, p1, p3);
        if (!(product <= 0f)) return false;
        product = area(p0, p2, p3) * area(p1, p2, p3);
        return product <= 0f;
    }

    // RVA 0xDE5CC0. Original array accesses preserve their normal managed bounds checks.
    public static bool pInQuadrangle(Vector2[] pots, Vector2 p)
    {
        return pInQuadrangle(pots[0], pots[1], pots[2], pots[3], p);
    }

    // RVA 0xDE5DD0. The exact-area equality is intentional; no replacement tolerance.
    public static bool pInQuadrangle(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Vector2 p)
    {
        float first = Mathf.Abs(((((d.y * p.x + a.y * d.x + a.x * p.y) - a.x * d.y) - a.y * p.x) - d.x * p.y) * 0.5f);
        float second = Mathf.Abs(((((c.y * p.x + c.x * d.y + d.x * p.y) - c.y * d.x) - d.y * p.x) - c.x * p.y) * 0.5f);
        float third = Mathf.Abs(((((a.y * p.x + a.x * b.y + b.x * p.y) - a.y * b.x) - b.y * p.x) - a.x * p.y) * 0.5f);
        float fourth = Mathf.Abs(((((b.y * p.x + b.x * c.y + c.x * p.y) - b.y * c.x) - c.y * p.x) - b.x * p.y) * 0.5f);
        float baseFirst = Mathf.Abs(((((a.y * c.x + a.x * b.y + b.x * c.y) - a.y * b.x) - b.y * c.x) - a.x * c.y) * 0.5f);
        float baseSecond = Mathf.Abs(((((a.x * c.y + c.x * d.y + a.y * d.x) - c.y * d.x) - a.x * d.y) - a.y * c.x) * 0.5f);
        return ((first + second) + third) + fourth == baseFirst + baseSecond;
    }

    // RVA 0xDE5FA0.
    private static float triangleArea(Vector2 a, Vector2 b, Vector2 c)
    {
        return Mathf.Abs(((((b.x * c.y + a.x * b.y + a.y * c.x) - a.y * b.x) - b.y * c.x) - a.x * c.y) * 0.5f);
    }

    // RVA 0xDE67A4.
    public static long Min(long x, long y) { return x < y ? x : y; }

    // RVA 0xDE67C8.
    public static float calcAimTargetWith(float cos, float sin, float dx, float dy, float r)
    {
        float projection = sin * dy + cos * dx;
        if (!(projection >= 0f)) return -1f;
        float twiceProjection = projection + projection;
        float coefficient = sin * sin + cos * cos;
        float discriminant = coefficient * -4f * ((dy * dy + dx * dx) - r * r) +
                             twiceProjection * twiceProjection;
        if (!(discriminant >= 0f)) return -1f;
        return (twiceProjection - Mathf.Sqrt(discriminant)) / (coefficient + coefficient);
    }

    // RVA 0xDE6840. Literal 0xDE68D0 is recorded in parent_source/literal_evidence.json.
    public static float workout(float x0, float y0, float dx, float dy, float vx, float vy, float t)
    {
        float denominator = dy * vx - dx * vy;
        if ((double)Mathf.Abs(denominator) < 0.001) return -1f;
        float time = (x0 * dy - y0 * dx) / denominator;
        if (!(time > 0f)) return -1f;
        float projection = (time * vy - y0) * dy + (time * vx - x0) * dx;
        if (projection < 0f || t < projection) return -1f;
        return time;
    }

    // RVA 0xDE68D8: original object-constructor tail call only.
    public MathUtil() { }

    // RVA: 0xDE5218. The PLT slot 0x1DE5600 resolves to fmodf (GOT 0x1E8BC1C).
    public static float ClampAngleInPI(float angle)
    {
        angle %= 360f;
        if (angle > 180f) angle -= 360f;
        if (angle < -180f) angle += 360f;
        return angle;
    }

    // RVA: 0xDE53D4. These Unity Vector3 operations are inlined in the original body.
    public static float AngleAroundAxis(Vector3 dirA, Vector3 dirB, Vector3 axis)
    {
        dirA -= Vector3.Project(dirA, axis);
        dirB -= Vector3.Project(dirB, axis);
        float result = Vector3.Angle(dirA, dirB);
        if (Vector3.Dot(Vector3.Cross(dirA, dirB), axis) < 0f) result = -result;
        return result;
    }

    // RVA: 0xDE5288. Original sincosf and normalized return.
    public static Vector3 RotateOnUp(Vector3 original, float angle)
    {
        float radians = angle * 0.01745329238474369f;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector3(cos * original.x - sin * original.z, original.y,
            cos * original.z + sin * original.x).normalized;
    }
}
