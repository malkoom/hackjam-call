using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [SerializeField]
    private Transform doorTransform;

    [SerializeField]
    public float TransitionDuration = 0.5f;

    [SerializeField]
    private float minY = 0f;

    [SerializeField]
    private float maxY = 500f;

    [Header("Camera Shake")]
    [SerializeField]
    private Camera cameraToShake;

    [SerializeField]
    private float shakeDuration = 0.12f;

    [SerializeField]
    private float shakeStrength = 0.15f;

    private void Reset()
    {
        doorTransform = transform;
    }

    public void Open()
    {
        StopAllCoroutines();
        StartCoroutine(MoveDoor(maxY, false));
    }

    public void Close()
    {
        StopAllCoroutines();
        StartCoroutine(MoveDoor(minY, true));
    }

    private IEnumerator MoveDoor(float targetY, bool shakeWhenFinished)
    {
        float elapsedTime = 0f;
        float startY = doorTransform.localPosition.y;

        if (Mathf.Approximately(startY, targetY))
        {
            if (shakeWhenFinished)
                yield return StartCoroutine(ShakeCamera());

            yield break;
        }

        Vector3 currentPos = doorTransform.localPosition;

        while (elapsedTime < TransitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float newY = Mathf.Lerp(startY, targetY, elapsedTime / TransitionDuration);

            doorTransform.localPosition = new Vector3(currentPos.x, newY, currentPos.z);
            yield return null;
        }

        // Posición final asegurada
        doorTransform.localPosition = new Vector3(currentPos.x, targetY, currentPos.z);

        if (shakeWhenFinished)
            yield return StartCoroutine(ShakeCamera());
    }

    private IEnumerator ShakeCamera()
    {
        Camera targetCamera = cameraToShake != null ? cameraToShake : Camera.main;
        if (targetCamera == null || shakeDuration <= 0f || shakeStrength <= 0f)
            yield break;

        Transform cameraTransform = targetCamera.transform;
        Vector3 initialPosition = cameraTransform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float intensity = shakeStrength * (1f - elapsedTime / shakeDuration);
            Vector2 offset = Random.insideUnitCircle * intensity;
            cameraTransform.localPosition = initialPosition + new Vector3(offset.x, offset.y, 0f);
            yield return null;
        }

        cameraTransform.localPosition = initialPosition;
    }
}
