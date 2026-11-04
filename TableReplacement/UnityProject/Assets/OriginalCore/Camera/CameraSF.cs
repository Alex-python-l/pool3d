// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using PinchRecognizer = UnityEngine.Component;
// Recovered from the original ARM32 implementation, not a replacement camera.
// Source/ABI caveats and field mappings: module_restore/parent_source/source_map.json.
using UnityEngine;

public partial class CameraSF : MonoBehaviour
{
    public Transform target;
    public float initialDistance = 10f;
    public float minDistance = 1f;
    public float maxDistance = 30f;
    public float yawSensitivity = 45f;
    public float pitchSensitivity = 45f;
    public bool clampYawAngle;
    public float minYaw = -75f;
    public float maxYaw = 75f;
    public bool clampPitchAngle = true;
    public float minPitch = -20f;
    public float maxPitch = 80f;
    public bool allowPinchZoom = true;
    public float pinchZoomSensitivity = 5f;
    public bool smoothMotion = true;
    public float smoothZoomSpeed = 5f;
    public float smoothOrbitSpeed = 10f;
    public bool allowPanning;
    public bool invertPanningDirections;
    public float panningSensitivity = 1f;
    public Transform panningPlane;
    public bool smoothPanning = true;
    public float smoothPanningSpeed = 12f;
    public LayerMask collisionLayerMask;
    private float distance = 10f;
    private float yaw;
    private float pitch;
    private float idealDistance;
    private float idealYaw;
    private float idealPitch;
    private Vector3 idealPanOffset = Vector3.zero;
    private Vector3 panOffset = Vector3.zero;
    private PinchRecognizer pinchRecognizer;
    private float nextDragTime;
    public AnimationCurve curve;
    private Vector3 centerPos = Vector3.zero;
    public bool isOnDrageScreen;
    public Vector2 dragDelta = Vector2.zero;
    public bool isStartFling;
    public Vector2 flingVector2;
    private float maxVelocity = 500f;
    public Transform dirTarget;
    public bool isRotateTargetToCameraDir;
    private float rotationAngle;
    public bool smoothPosition;
    public bool isLookAtTarget;
    private bool smoothToTargetAngle;
    private float target_IdealPitch;
    private float target_IdealYaw;
    private bool isFineAdjust;
    private bool dynamicFollow;

    // RVA: 0x857BF8. Original field initializers execute before the MonoBehaviour base constructor.
    // The remaining fields retain their original zero defaults.
    public CameraSF() { }

    // Accessor RVAs are independent original methods, not synthesized state.
    public float Distance { get { return distance; } } // RVA: 0x855E6C
    public float IdealDistance
    {
        get { return idealDistance; } // RVA: 0x855E74
        set { idealDistance = Mathf.Clamp(value, minDistance, maxDistance); } // RVA: 0x855E7C
    }
    public float Yaw { get { return yaw; } } // RVA: 0x855EA8
    public float IdealYaw
    {
        get { return idealYaw; } // RVA: 0x855EB0
        set { idealYaw = clampYawAngle ? ClampAngle(value, minYaw, maxYaw) : value; } // RVA: 0x855EB8
    }
    public float Pitch { get { return pitch; } } // RVA: 0x855F84
    public float IdealPitch
    {
        get { return idealPitch; } // RVA: 0x855F8C
        set { idealPitch = clampPitchAngle ? ClampAngle(value, minPitch, maxPitch) : value; } // RVA: 0x855F94
    }
    public Vector3 IdealPanOffset
    {
        get { return idealPanOffset; } // RVA: 0x856004
        set { idealPanOffset = value; } // RVA: 0x856018
    }
    public Vector3 PanOffset { get { return panOffset; } } // RVA: 0x856024
    public bool IsFineAdjust
    {
        get { return isFineAdjust; } // RVA: 0x8579F8
        set { isFineAdjust = value; } // RVA: 0x857A00
    }

    // RVA: 0x855F28. Single-turn corrections, not a modulo normalization.
    private static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f) angle += 360f;
        if (angle > 360f) angle -= 360f;
        return Mathf.Clamp(angle, min, max);
    }

    // RVA: 0x856038.
    private void Start()
    {
        if (!panningPlane) panningPlane = transform;
        Vector3 angles = transform.eulerAngles;
        distance = initialDistance;
        IdealDistance = distance;
        yaw = angles.y;
        IdealYaw = yaw;
        pitch = angles.x;
        IdealPitch = pitch;
        if (GetComponent<Rigidbody>()) GetComponent<Rigidbody>().freezeRotation = true;
        Apply();
    }

    // RVA: 0x856938. Preserves the original 0.25-second post-pinch drag lockout.
    public void OnDrag(Vector2 delta)
    {
        if (Time.time < nextDragTime) return;
        if (target)
        {
            IdealYaw = idealYaw + yawSensitivity * delta.x * 0.006f;
            IdealPitch = idealPitch + pitchSensitivity * delta.y * -0.006f;
        }
    }

    // RVA: 0x856AD0.
    public void OnPinch(float pinchValue)
    {
        if (!allowPinchZoom) return;
        IdealDistance = idealDistance - pinchZoomSensitivity * pinchValue;
        nextDragTime = Time.time + 0.25f;
    }

    // RVA: 0x85780C. Does not also clear panOffset.
    public void ResetPanning() { idealPanOffset = Vector3.zero; }

    // RVA: 0x857870.
    public void OnFling(float velocityX, float velocityY)
    {
        isStartFling = true;
        flingVector2.y = Mathf.Clamp(velocityY, -maxVelocity, maxVelocity);
        flingVector2.x = Mathf.Clamp(velocityX, -maxVelocity, maxVelocity);
    }
    // RVA: 0x8578D0.
    public void EndFling() { isStartFling = false; }
    // RVA: 0x8578DC.
    public void SetTargetCenter(Transform tf) { target = tf; }
    // RVA: 0x8578E4.
    public void InitCameraDir(float angleY) { yaw = angleY; IdealYaw = angleY; }
    // RVA: 0x857958.
    public void RotateCamareY(float y)
    {
        IdealYaw = idealYaw + y * 0.04f;
        smoothPosition = false;
    }
    // RVA: 0x8579E0.
    public void SetAngle(float idealPitch, float idealYaw, bool isFineAdjust)
    {
        target_IdealPitch = idealPitch;
        target_IdealYaw = idealYaw;
        smoothToTargetAngle = true;
        this.isFineAdjust = isFineAdjust;
    }
    // RVA: 0x857A08.
    public void ResetNetSmooth() { smoothToTargetAngle = false; isFineAdjust = false; }
    // RVA: 0x857A14.
    public void _Reset()
    {
        transform.position = new Vector3(-6.2f, 82.5f, -10f);
        transform.eulerAngles = new Vector3(32.75f, 0f, 0f);
        Vector3 angles = transform.eulerAngles;
        distance = initialDistance;
        IdealDistance = distance;
        yaw = angles.y;
        IdealYaw = yaw;
        pitch = angles.x;
        IdealPitch = pitch;
        smoothToTargetAngle = false;
    }
    // RVA: 0x857BE0.
    public void StartDynamicFollow() { dynamicFollow = true; }
    // RVA: 0x857BEC.
    public void EndDynamicFollow() { dynamicFollow = false; }
}
