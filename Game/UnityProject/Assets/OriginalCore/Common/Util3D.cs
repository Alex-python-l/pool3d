// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
// Original Type493; ARM/metadata correspondence: 20261020_unity_dependencies/shared.
using UnityEngine;
using System.Collections.Generic;

public class Util3D
{
    public static float Epsilon;

    // RVA: 0xDE8D48. Retain the original half-length factor and full-angle trig.
    public static Quaternion QuaternionLoadWith(Quaternion quaternion, float a, float x, float y, float z)
    {
        float scale = 0.5f / Mathf.Sqrt(y * y + x * x + z * z);
        float angle = a * 0.0174532924f;
        float sine = Mathf.Sin(angle);
        quaternion.x = sine * (scale * x);
        quaternion.y = sine * (scale * y);
        quaternion.z = sine * (scale * z);
        quaternion.w = Mathf.Cos(angle);
        return quaternion;
    }

    // RVA: 0xDE8E28. The original body is literally BX LR; both arguments are values.
    public static void QuaternionCopyForm(Quaternion source, Quaternion other) { }

    // RVA: 0xDE8E2C. Hidden-return ARM stores y,z,x,w, preserving the original swizzle.
    public static Quaternion XYZ_2_ZNXY(Quaternion qua)
    {
        return new Quaternion(qua.y, qua.z, qua.x, qua.w);
    }

    // RVA: 0xDE8E40. Original compares object names, rather than transform identities.
    public static void DestoryAllChild(GameObject parent)
    {
        Transform[] transforms = parent.GetComponentsInChildren<Transform>();
        for (int i = 0; i < transforms.Length; i++)
            if (transforms[i] != null && transforms[i].name != parent.name)
                Object.DestroyImmediate(transforms[i].gameObject);
    }

    // RVA: 0xDD8E84
    public static void SetItemVisible(MonoBehaviour behavior, bool visible) { SetItemVisible(behavior, visible, true); }
    // RVA: 0xDDF88C
    public static void SetItemVisible(MonoBehaviour behavior, bool visible, bool all)
    {
        if (behavior == null) return;
        SetItemVisible(behavior.gameObject, visible, all);
    }
    // RVA: 0xDD9A58
    public static void SetItemVisible(GameObject gameObject, bool visible) { SetItemVisible(gameObject, visible, true); }
    // RVA: 0xDE8FD4. Keep original Unity legacy APIs and its active-state guard.
    public static void SetItemVisible(GameObject gameObject, bool visible, bool all)
    {
        if (gameObject == null || gameObject.active == visible) return;
        if (!all) gameObject.active = visible;
        else gameObject.SetActiveRecursively(visible);
    }

    // RVA: 0xDE90B0
    public static void Destory_ListGameObjects(List<GameObject> objects)
    {
        if (objects == null) return;
        for (int i = 0; i < objects.Count; i++)
            if (objects[i] != null) Object.DestroyImmediate(objects[i]);
        objects.Clear();
    }

    // RVA: 0xDE9214
    public static bool IsInteract(Vector3 f, Vector3 t, int lay)
    {
        Vector3 direction = t - f;
        return Physics.Raycast(f, direction, direction.magnitude, lay);
    }

    // RVA: 0xDE9348
    public static bool IsActive(MonoBehaviour behavior)
    {
        if (behavior == null) return false;
        return behavior.gameObject.active;
    }

    // RVA: 0xDE93F8; GOT literals are the exact '(Clone)' and empty string.
    public static string GetTrueName(string name) { return name.Replace("(Clone)", ""); }

    // RVA: 0xDE947C. The original compares against full collider.size, with no half factor.
    public static bool IsInBoxColliderBounds(BoxCollider collider, Vector3 pot)
    {
        Vector3 local = collider.transform.InverseTransformPoint(pot) - collider.center;
        if (Mathf.Abs(local.x) < collider.size.x)
            if (Mathf.Abs(local.y) < collider.size.y)
                return Mathf.Abs(local.z) < collider.size.z;
        return false;
    }

    // RVA: 0xDE95C4. ARM BPL rejects unordered values as well as >=; preserve the strict < gate.
    public static bool IsInBoxColliderBounds_XZ(BoxCollider collider, Vector3 pot)
    {
        Vector3 local = collider.transform.InverseTransformPoint(pot) - collider.center;
        if (!(Mathf.Abs(local.x) < collider.size.x)) return false;
        return Mathf.Abs(local.z) < collider.size.z;
    }

    // RVA: 0xDE96D0. Repeated component lookups are present in the original.
    public static bool IsNoKimematicRigidbody(GameObject go)
    {
        return go.GetComponent<Rigidbody>() != null && !go.GetComponent<Rigidbody>().isKinematic;
    }

    // RVA: 0xDE97BC
    public static GameObject GetParentOfTheChild(Transform childTf, string parentName)
    {
        if (childTf.name != parentName)
        {
            do
            {
                if (childTf.parent == null) return null;
                childTf = childTf.parent;
            } while (childTf.name != parentName);
        }
        return childTf.gameObject;
    }

    // RVA: 0xDE98E4. An immediate self-name match returns false in the original.
    public static bool IsTheChildOfParent(Transform childTf, string parentName)
    {
        bool result = false;
        if (childTf.name != parentName)
        {
            do
            {
                result = childTf.parent != null;
                if (!result) return result;
                childTf = childTf.parent;
            } while (childTf.name != parentName);
        }
        return result;
    }

    // RVA: 0xDE99F8
    public static Transform GetObjectParent(Transform tf)
    {
        while (tf.parent != null) tf = tf.parent;
        return tf;
    }

    // RVA: 0xDE9AAC
    public static Transform GetObjectChild(Transform parent, string name)
    {
        Transform[] transforms = parent.GetComponentsInChildren<Transform>();
        for (int i = 0; i < transforms.Length; i++)
            if (transforms[i].name == name) return transforms[i];
        return null;
    }

    // RVA: 0xDE9B94. Original inline Vector3.Normalize threshold is 1e-5.
    public static bool IsInViewFeild(Vector3 selfDir, Vector3 toTargetDir, float feildAnagle)
    {
        toTargetDir.Normalize();
        selfDir.Normalize();
        return Vector3.Dot(toTargetDir, selfDir) > Mathf.Cos(feildAnagle * 0.5f * 0.0174532924f);
    }

    // RVA: 0xDE9D88. Original strict distance gate rejects unordered values (ARM VCMP/BPL).
    public static bool IsInValidateAttackRange(Vector3 selfPot, Vector3 targetPot, float vDst,
        Vector3 selfDir, Vector3 targetDir, float feildAngle)
    {
        if (!((selfPot - targetPot).sqrMagnitude < vDst * vDst)) return false;
        return IsInViewFeild(selfDir, targetDir, feildAngle);
    }

    // RVA: 0xDE9E8C. The original detaches the created effect after local initialization.
    public static void AddEffect(GameObject parent, GameObject prefab, Vector3 positionSpan)
    {
        GameObject effect = Object.Instantiate(prefab);
        effect.transform.parent = parent.transform;
        effect.transform.localPosition = Vector3.zero;
        effect.transform.localRotation = Quaternion.identity;
        effect.transform.localScale = Vector3.one;
        effect.transform.localPosition += positionSpan;
        effect.transform.parent = null;
    }

    // RVA: 0xDEA1EC
    public static void AddEffect_World(GameObject parent, GameObject prefab, Vector3 worldPot)
    {
        GameObject effect = Object.Instantiate(prefab);
        effect.transform.parent = parent.transform;
        effect.transform.localPosition = Vector3.zero;
        effect.transform.localRotation = Quaternion.identity;
        effect.transform.localScale = Vector3.one;
        effect.transform.position = worldPot;
        effect.transform.parent = null;
    }

    // RVA: 0xDEA4FC
    public static void InitLocalTf(Transform tf)
    {
        tf.localPosition = Vector3.zero;
        tf.localRotation = Quaternion.identity;
        tf.localScale = Vector3.one;
    }
    // RVA: 0xDD99D4
    public static void SetParent(Transform parent, Transform child) { child.parent = parent; InitLocalTf(child); }
    // RVA: 0xDEA664. Tail-call to System.Object constructor.
    public Util3D() { }
    // RVA: 0xDEA66C. Original literal bits 0x358637BD.
    static Util3D() { Epsilon = 0.000001f; }
}
