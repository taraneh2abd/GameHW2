using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 5f, -9f);
    [SerializeField] private float followSharpness = 8f;
    [SerializeField] private float lookAheadDistance = 6f;

    private Vector3 smoothedPosition;
    private bool initialized;
    private float shakeTimeLeft;
    private float shakeDuration;
    private float shakeMagnitude;

    public void Shake(float duration, float magnitude)
    {
        if (duration <= 0f || magnitude <= 0f)
        {
            return;
        }

        if (shakeTimeLeft > 0f && magnitude < shakeMagnitude * (shakeTimeLeft / shakeDuration))
        {
            return;
        }

        shakeDuration = duration;
        shakeTimeLeft = duration;
        shakeMagnitude = magnitude;
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desired = target.position + offset;
        if (!initialized)
        {
            smoothedPosition = transform.position;
            initialized = true;
        }

        float blend = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
        smoothedPosition = Vector3.Lerp(smoothedPosition, desired, blend);

        Vector3 lookPoint = target.position + Vector3.forward * lookAheadDistance;
        Quaternion rotation = Quaternion.LookRotation(lookPoint - smoothedPosition, Vector3.up);

        Vector3 shakeOffset = Vector3.zero;
        if (shakeTimeLeft > 0f)
        {
            shakeTimeLeft -= Time.deltaTime;
            float falloff = Mathf.Clamp01(shakeTimeLeft / shakeDuration);
            Vector2 random = Random.insideUnitCircle * (shakeMagnitude * falloff);
            shakeOffset = rotation * new Vector3(random.x, random.y, 0f);
        }

        transform.SetPositionAndRotation(smoothedPosition + shakeOffset, rotation);
    }
}
