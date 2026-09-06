using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CameraSlideTransition : MonoBehaviour
{
    [Header("Cámaras")]
    [Tooltip("Cámara que realiza la animación de slide")]
    public Camera camera1;

    [Tooltip("Cámara del segundo escenario")]
    public Camera camera2;

    [Header("Tiempos")]
    [Tooltip("Espera en segundos tras terminar la animación")]
    public float delayAfterSlide = 2.0f;

    [Tooltip("Duración del fundido de pantalla")]
    public float fadeDuration = 0.8f;

    public Color fadeColor = Color.black;

    private Image fadeImage;
    private bool transitionTriggered = false;

    void Start()
    {
        // 1. Configurar estado inicial de las cámaras
        if (camera1 != null) camera1.enabled = true;
        if (camera2 != null) camera2.enabled = false;

        SetAudioListener(camera1, true);
        SetAudioListener(camera2, false);

        // 2. Crear pantalla de fundido
        CreateFadeOverlay();
    }

    private void CreateFadeOverlay()
    {
        GameObject canvasObj = new GameObject("Runtime_SlideFadeCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GameObject imgObj = new GameObject("FadeImage");
        imgObj.transform.SetParent(canvasObj.transform, false);

        fadeImage = imgObj.AddComponent<Image>();
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f); // Transparente

        RectTransform rect = fadeImage.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }

    /// <summary>
    /// Esta función se llama desde el Animation Event al final del slide.
    /// </summary>
    public void OnSlideFinished()
    {
        if (transitionTriggered) return;
        transitionTriggered = true;

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        Debug.Log($"[Slide Finished] Esperando {delayAfterSlide}s antes de cambiar de cámara...");

        // 1. Esperar 2 segundos tras terminar la animación
        yield return new WaitForSeconds(delayAfterSlide);

        // 2. Fundido a negro
        float halfFade = fadeDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < halfFade)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / halfFade);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        // 3. Conmutar a la Cámara 2 a oscuras
        if (camera1 != null) camera1.enabled = false;
        if (camera2 != null) camera2.enabled = true;

        SetAudioListener(camera1, false);
        SetAudioListener(camera2, true);

        // 4. Aclarar la pantalla
        elapsed = 0f;
        while (elapsed < halfFade)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / halfFade);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        Debug.Log("¡Transición completada con éxito!");
    }

    private void SetAudioListener(Camera cam, bool state)
    {
        if (cam == null) return;
        AudioListener listener = cam.GetComponent<AudioListener>();
        if (listener != null) listener.enabled = state;
    }
}