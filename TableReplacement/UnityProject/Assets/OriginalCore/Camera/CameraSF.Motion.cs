// Generated offline projection; edit Recovered or the recorded binding projection, not this copy.
using UnityEngine;

public partial class CameraSF
{
    // RVA: 0x856B34. Uses the original curve, 40+180 distance and +/-20 follow margin.
    private void Adjust()
    {
        float curvePosition = (pitch - minPitch) / (maxPitch - minPitch);
        float value = curve.Evaluate(curvePosition);
        IdealDistance = 40f + Mathf.Clamp01(value) * 180f;
        centerPos.y = target.position.y;
        Vector3 targetPosition = target.position;
        Vector3 forward = transform.forward;
        forward.y = 0f;
        Vector3 normalized = forward.normalized;
        // Raw ARM s21 retains targetPosition.y across get_forward. The old Ghidra
        // stack alias incorrectly renders this as target.y - forward.y.
        Vector3 difference = centerPos - targetPosition;
        if (!dynamicFollow)
        {
            Vector3 direction = difference.normalized;
            float margin = Vector3.Dot(difference, normalized) < 0f ? -20f : 20f;
            difference.x = difference.x + margin * direction.x;
            difference.y = difference.y + margin * direction.y;
            difference.z = difference.z + margin * direction.z;
        }
        normalized = forward.normalized;
        float projection = Vector3.Dot(difference, normalized);
        normalized = forward.normalized;
        idealPanOffset = normalized * projection;
        idealPanOffset = Vector3.Lerp(Vector3.zero, idealPanOffset,
            curve.Evaluate(curvePosition));
    }

    // RVA: 0x856290. No replacement camera smoothing or collision solver.
    private void Apply()
    {
        Adjust();
        if (!smoothMotion)
        {
            distance = idealDistance;
            yaw = idealYaw;
            pitch = idealPitch;
        }
        else
        {
            distance = Mathf.Lerp(distance, idealDistance, Time.deltaTime * smoothZoomSpeed);
            yaw = Mathf.LerpAngle(yaw, idealYaw, Time.deltaTime * smoothOrbitSpeed);
            pitch = Mathf.LerpAngle(pitch, idealPitch, Time.deltaTime * smoothOrbitSpeed);
        }
        if (!smoothPanning) panOffset = idealPanOffset;
        else panOffset = Vector3.Lerp(panOffset, idealPanOffset, Time.deltaTime * smoothPanningSpeed);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 targetPosition = target.position;
        Vector3 point = targetPosition + panOffset;
        // The first forward getter is present in ARM although its result is discarded.
        Vector3 unusedForward = transform.forward;
        smoothPosition = true;
        Vector3 position = transform.position;
        float currentDistance = distance;
        Vector3 forward = transform.forward;
        position = Vector3.Lerp(position, point - currentDistance * forward, Time.deltaTime * 2f);
        if ((int)collisionLayerMask != 0)
        {
            Vector3 direction = position - point;
            float length = direction.magnitude;
            direction.Normalize();
            RaycastHit hit;
            if (Physics.Raycast(point, direction, out hit, length, collisionLayerMask))
            {
                position = hit.point - direction * 0.1f;
                distance = hit.distance;
            }
        }
        transform.position = position;
        RotateTargetToCameraDir();
    }

    // RVA: 0x85708C. Original GameMgr/PoolCtr gating is retained.
    private void RotateTargetToCameraDir()
    {
        if (dirTarget == null) return;
        if (Pool3D.Offline.OfflineMatchController.Instance.CanCameraAim)
        {
            Vector3 first = dirTarget.transform.forward;
            Vector3 second = transform.forward;
            rotationAngle = MathUtil.AngleAroundAxis(first, second, Vector3.up);
            float unusedDeltaTime = Time.deltaTime;
            dirTarget.transform.RotateAround(target.position, Vector3.up, rotationAngle * 0.3f);
        }
    }

    // RVA: 0x8573C4.
    private void LateUpdate()
    {
        Update_SmoothToTargeAngle();
        Apply();
        /* Offline 8/9-ball has no tutorial; original CheckLockCue returns at !IsTutorial (0xAE4940). */
    }

    // RVA: 0x857468. Original remote-angle smoothing is also a 3D camera dependency.
    private void Update_SmoothToTargeAngle()
    {
        if (!smoothToTargetAngle) return;
        if (Pool3D.Offline.OfflineMatchController.Instance.IsView2D())
        {
            smoothToTargetAngle = false;
            return;
        }
        float previousYaw = idealYaw;
        float previousPitch = idealPitch;
        IdealPitch = Mathf.Lerp(idealPitch, target_IdealPitch, Time.deltaTime * 4f);
        IdealYaw = Mathf.Lerp(idealYaw, target_IdealYaw, Time.deltaTime * 4f);
        if (Mathf.Abs(idealPitch - target_IdealPitch) < 0.005f &&
            Mathf.Abs(idealYaw - target_IdealYaw) < 0.005f)
        {
            IdealPitch = target_IdealPitch;
            IdealYaw = target_IdealYaw;
            smoothToTargetAngle = false;
        }
        if (isFineAdjust)
            Pool3D.Offline.OfflineMatchController.Instance.OnCameraFineAdjustment((idealYaw - previousYaw) * 25f);
    }
}
