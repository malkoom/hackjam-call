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
    }

    private IEnumerator MoveDoor(float targetY)
    {
        float elapsedTime = 0f;
        float startY = doorTransform.localPosition.y;

        if (Mathf.Approximately(startY, targetY))
        {
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
    }
}
