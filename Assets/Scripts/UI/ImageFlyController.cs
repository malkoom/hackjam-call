using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageFlyController : MonoBehaviour
{
    [SerializeField]
    private GameObject imageObject;
    public Vector2 InitialPos;

    [SerializeField]
    Transform player1;

    [SerializeField]
    Transform player2;

    [SerializeField]
    private float travelDuration = 0.6f;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        InitialPos = new Vector2(transform.position.x, transform.position.y);
    }

    private void Reset()
    {
        imageObject = gameObject;
    }

    /// <summary>
    /// Inicia el trayecto entre dos Transforms y oculta la imagen al llegar.
    /// </summary>
    public float FlyAndHide(int destination, Sprite sprite)
    {
        gameObject.SetActive(true);
        image.sprite = sprite;
        Transform dest = (destination == 1) ? player1 : player2;
        StopAllCoroutines();
        StartCoroutine(FlyRoutine(InitialPos, dest.position, dest.rotation, dest.rotation));

        return travelDuration;
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
        imageObject.transform.position = InitialPos;
    }
}
