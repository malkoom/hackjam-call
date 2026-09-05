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
    private RectTransform canvasToShake;

    [SerializeField]
    private float shakeDuration = 0.3f;

    [SerializeField]
    private float shakeStrength = 0.35f;

    [SerializeField]
    private float canvasShakeStrength = 28f;

    private void Reset()
    {
        doorTransform = transform;
    }

    public void Open()
    {
        StopAllCoroutines();
        StartCoroutine(MoveDoor(maxY));
    }

    public void Close()
    {
        StopAllCoroutines();
        StartCoroutine(MoveDoor(minY));
        StartCoroutine(ShakeCamera());
    }

    private IEnumerator MoveDoor(float targetY)
    {
        float elapsedTime = 0f;
        float startY = doorTransform.localPosition.y;

        if (Mathf.Approximately(startY, targetY))
            yield break;

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

    }

    private IEnumerator ShakeCamera()
    {
        Camera targetCamera = cameraToShake != null ? cameraToShake : Camera.main;
        Canvas parentCanvas = doorTransform.GetComponentInParent<Canvas>();
        RectTransform targetCanvas = canvasToShake != null
            ? canvasToShake
            : parentCanvas != null
                ? parentCanvas.transform as RectTransform
                : null;

        if (shakeDuration <= 0f || (targetCamera == null && targetCanvas == null))
            yield break;

        Transform cameraTransform = targetCamera != null ? targetCamera.transform : null;
        Vector3 initialCameraPosition = cameraTransform != null
            ? cameraTransform.localPosition
            : Vector3.zero;
        Vector2 initialCanvasPosition = targetCanvas != null
            ? targetCanvas.anchoredPosition
            : Vector2.zero;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float falloff = 1f - elapsedTime / shakeDuration;
            Vector2 offset = Random.insideUnitCircle * falloff;

            if (cameraTransform != null)
                cameraTransform.localPosition = initialCameraPosition + new Vector3(
                    offset.x * shakeStrength,
                    offset.y * shakeStrength,
                    0f
                );

            if (targetCanvas != null)
                targetCanvas.anchoredPosition = initialCanvasPosition + offset * canvasShakeStrength;
            yield return null;
        }

        if (cameraTransform != null)
            cameraTransform.localPosition = initialCameraPosition;

        if (targetCanvas != null)
            targetCanvas.anchoredPosition = initialCanvasPosition;
    }
}
