using System.Collections;
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

    [Header("Animación de vista previa")]
    [SerializeField]
    private float previewPulseAmplitude = 0.12f;

    [SerializeField]
    private float previewPulseSpeed = 3f;

    private Image image;
    private Transform imageTransform;
    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;
    private Vector3 initialLocalScale;
    private Vector3 initialImageLocalScale;
    private Vector2 initialAnchoredPosition;
    private RectTransform imageRectTransform;
    private Coroutine previewPulseCoroutine;

    private void Awake()
    {
        ResolveImage();

        if (imageTransform == null)
            return;

        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
        initialLocalScale = transform.localScale;
        initialImageLocalScale = imageTransform.localScale;
        imageRectTransform = transform as RectTransform;
        if (imageRectTransform != null)
            initialAnchoredPosition = imageRectTransform.anchoredPosition;
        InitialPos = new Vector2(imageTransform.position.x, imageTransform.position.y);
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
        if (!ResolveImage())
            return 0f;

        StopPreviewPulse();
        imageObject.SetActive(true);
        ResetToInitialTransform();
        image.sprite = sprite;
        Transform dest = (destination == 1) ? player1 : player2;

        if (dest == null)
        {
            Debug.LogWarning("No se ha asignado el destino del vuelo de la pieza.");
            return 0f;
        }

        StopAllCoroutines();
        StartCoroutine(
            FlyRoutine(
                imageTransform.position,
                dest.position,
                imageTransform.rotation,
                dest.rotation
            )
        );

        return travelDuration;
    }

    public void Show(Sprite sprite)
    {
        if (!ResolveImage())
            return;

        ResetToInitialTransform();
        image.sprite = sprite;
        imageObject.SetActive(true);
        StartPreviewPulse();
    }

    public void Hide()
    {
        if (!ResolveImage())
            return;

        StopPreviewPulse();
        imageObject.SetActive(false);
        ResetToInitialTransform();
    }

    private IEnumerator FlyRoutine(
        Vector3 startPos,
        Vector3 endPos,
        Quaternion startRot,
        Quaternion endRot
    )
    {
        gameObject.SetActive(true);
        transform.position = startPos;
        transform.rotation = startRot;

        float elapsedTime = 0f;

        while (elapsedTime < travelDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / travelDuration;

            // Interpolación de posición y rotación
            transform.position = Vector3.Lerp(startPos, endPos, t);
            transform.rotation = Quaternion.Slerp(startRot, endRot, t);

            yield return null;
        }

        // Fijar destino exacto antes de ocultar
        transform.position = endPos;
        transform.rotation = endRot;

        // Desaparece al completarse
        gameObject.SetActive(false);
        ResetToInitialTransform();
    }

    private bool ResolveImage()
    {
        if (image != null)
            return true;

        if (imageObject != null)
            image = imageObject.GetComponentInChildren<Image>(true);
        else
            image = GetComponentInChildren<Image>(true);

        if (image == null)
        {
            Debug.LogWarning("ImageFlyController no encuentra una Image hija para animar.");
            return false;
        }

        imageObject = image.gameObject;
        imageTransform = image.transform;
        return true;
    }

    // Guardamos la posición en el espacio del padre. Así no se arrastra una
    // coordenada global obsoleta si el canvas o la escena cambian entre minijuegos.
    private void ResetToInitialTransform()
    {
        transform.localPosition = initialLocalPosition;
        transform.localRotation = initialLocalRotation;
        transform.localScale = initialLocalScale;

        // La pieza puede ser hija de una Image usada como fondo. Cada una
        // conserva su propia escala base, en vez de reutilizar la del padre.
        if (imageTransform != transform)
            imageTransform.localScale = initialImageLocalScale;

        if (imageRectTransform != null)
            imageRectTransform.anchoredPosition = initialAnchoredPosition;
    }

    private void StartPreviewPulse()
    {
        StopPreviewPulse();
        ResetToInitialTransform();
        previewPulseCoroutine = StartCoroutine(AnimatePreviewPulse());
    }

    private void StopPreviewPulse()
    {
        if (previewPulseCoroutine != null)
        {
            StopCoroutine(previewPulseCoroutine);
            previewPulseCoroutine = null;
            ResetToInitialTransform();
        }
    }

    private IEnumerator AnimatePreviewPulse()
    {
        while (true)
        {
            float scaleMultiplier =
                1f + Mathf.Sin(Time.unscaledTime * previewPulseSpeed) * previewPulseAmplitude;
            transform.localScale = initialLocalScale * scaleMultiplier;

            // Aunque sea hija del fondo, también se anima su escala local para
            // que ambas imágenes participen explícitamente en el pulso.
            if (imageTransform != transform)
                imageTransform.localScale = initialImageLocalScale * scaleMultiplier;
            yield return null;
        }
    }
}
