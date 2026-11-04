// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Original custom API wrappers. These are not blanket UnityEngine.Mathf forwarding implementations.
using System;

namespace UnityEngineInternalEx
{
    public struct MathfInternal
    {
        public static float FloatMinNormal;
        public static float FloatMinDenormal;
        public static bool IsFlushToZeroEnabled;

        // RVA: 0xA041D0; raw native field words0x00800000,0x00000001 and bool0.
        static MathfInternal()
        {
            FloatMinNormal = 1.17549435e-38f;
            FloatMinDenormal = 1.40129846e-45f;
            IsFlushToZeroEnabled = false;
        }
    }
}

namespace UnityEngineInternalExport
{
    public struct Mathf
    {
        public const float PI = 3.1415927f;
        public const float Infinity = float.PositiveInfinity;
        public const float NegativeInfinity = float.NegativeInfinity;
        public const float Deg2Rad = 0.017453292f;
        public const float Rad2Deg = 57.29578f;
        public static readonly float Epsilon;

        // RVA: 0xA03A80
        static Mathf()
        {
            Epsilon = UnityEngineInternalEx.MathfInternal.IsFlushToZeroEnabled
                ? UnityEngineInternalEx.MathfInternal.FloatMinNormal
                : UnityEngineInternalEx.MathfInternal.FloatMinDenormal;
        }

        // RVA: 0xA022A8; original promotes float to double, calls sin and narrows back to float.
        public static float Sin(float f) { return (float)Math.Sin((double)f); }
        // RVA: 0xA02328; original double cos path.
        public static float Cos(float f) { return (float)Math.Cos((double)f); }
        // RVA: 0xA025A8; original double atan2 path.
        public static float Atan2(float y, float x) { return (float)Math.Atan2((double)y, (double)x); }
        // RVA: 0xA02638; original vsqrt.f32 re-expressed through the original Unity core API,
        // independently confirmed at0x1B32AB4/0x1B32B08 to contain the same float32 operation.
        public static float Sqrt(float f) { return UnityEngine.Mathf.Sqrt(f); }
        // RVA: 0xA026A8; exact native sign-bit clear, including negative zero and signed NaN payloads.
        public static float Abs(float f)
        {
            return BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(f) & 0x7fffffff);
        }
        // RVA: 0xA02708; original rsbmi does not throw for Int32.MinValue.
        public static int Abs(int value) { return value < 0 ? unchecked(-value) : value; }
        // RVA: 0xA02770; ties/unordered select b, as in original conditional instructions.
        public static float Min(float a, float b) { return a < b ? a : b; }
        // RVA: 0xA0278C; original empty array returns0f; null is not converted to an empty array.
        public static float Min(float[] values)
        {
            if (values.Length == 0) return 0f;
            float result = values[0];
            for (int i = 1; i < values.Length; i++) if (values[i] < result) result = values[i];
            return result;
        }
        // RVA: 0xA0282C
        public static int Min(int a, int b) { return b <= a ? b : a; }
        // RVA: 0xA02838
        public static int Min(int[] values)
        {
            if (values.Length == 0) return 0;
            int result = values[0];
            for (int i = 1; i < values.Length; i++) if (values[i] < result) result = values[i];
            return result;
        }
        // RVA: 0xA028C4; original strict comparison selects second operand for ties/unordered.
        public static float Max(float a, float b) { return a > b ? a : b; }
        // RVA: 0xA028E0
        public static float Max(float[] values)
        {
            if (values.Length == 0) return 0f;
            float result = values[0];
            for (int i = 1; i < values.Length; i++) if (values[i] > result) result = values[i];
            return result;
        }
        // RVA: 0xA02980
        public static int Max(int a, int b) { return a <= b ? b : a; }
        // RVA: 0xA0298C
        public static int Max(int[] values)
        {
            if (values.Length == 0) return 0;
            int result = values[0];
            for (int i = 1; i < values.Length; i++) if (values[i] > result) result = values[i];
            return result;
        }
        // RVA: 0xA030EC; native min<=value must pass first: NaN value therefore returns min.
        public static float Clamp(float value, float min, float max)
        {
            if (min <= value)
            {
                if (value > max) return max;
                return value;
            }
            return min;
        }
        // RVA: 0xA0311C
        public static int Clamp(int value, int min, int max)
        {
            if (min <= value) return max < value ? max : value;
            return min;
        }
        // RVA: 0xA03134; original0<=value gate returns0 for NaN.
        public static float Clamp01(float value)
        {
            if (0f <= value) return value > 1f ? 1f : value;
            return 0f;
        }
    }

    public class Debug
    {
        // RVA: 0xA03CCC; verified original tail call to UnityEngine.Debug.Log.
        public static void Log(string msg) { UnityEngine.Debug.Log(msg); }
        // RVA: 0xA03D34
        public static void LogError(string msg) { UnityEngine.Debug.LogError(msg); }
        // RVA: 0xA03D9C
        public static void LogWarning(string msg) { UnityEngine.Debug.LogWarning(msg); }
        // RVA: 0xA03E04; original System.Object-only constructor.
        public Debug() { }
    }

    public class Random
    {
        // RVA: 0xA03E0C; only original int overload exists in this custom class.
        public static int Range(int min, int max) { return UnityEngine.Random.Range(min, max); }
        // RVA: 0xA03E14
        public static float value() { return UnityEngine.Random.value; }
        // RVA: 0xA03E1C; original System.Object-only constructor.
        public Random() { }
    }
}
