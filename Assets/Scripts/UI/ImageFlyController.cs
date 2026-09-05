using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFlyController : MonoBehaviour
{
    [SerializeField]
    private GameObject imageObject;
    public Transform InitialTransform;

    [SerializeField]
    Transform player1;

    [SerializeField]
    Transform player2;

    [SerializeField]
    private float travelDuration = 0.6f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        InitialTransform = transform;
    }

    private void Reset()
    {
        imageObject = gameObject;
    }

    /// <summary>
    /// Inicia el trayecto entre dos Transforms y oculta la imagen al llegar.
    /// </summary>
    public void FlyAndHide(Transform origin, Transform destination, Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
        StopAllCoroutines();
        StartCoroutine(
            FlyRoutine(origin.position, destination.position, origin.rotation, destination.rotation)
        );
    }

    private IEnumerator FlyRoutine(
        Vector3 startPos,
        Vector3 endPos,
        Quaternion startRot,
        Quaternion endRot
    )
    {
        imageObject.SetActive(true);
        imageObject.transform.position = startPos;
        imageObject.transform.rotation = startRot;

        float elapsedTime = 0f;

        while (elapsedTime < travelDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / travelDuration;

            // Interpolación de posición y rotación
            imageObject.transform.position = Vector3.Lerp(startPos, endPos, t);
            imageObject.transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        // Fijar destino exacto antes de ocultar
        imageObject.transform.position = endPos;
        imageObject.transform.rotation = endRot;

        // Desaparece al completarse
        imageObject.SetActive(false);
    }
}
