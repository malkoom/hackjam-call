using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class CanvasFader : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float defaultDuration = 0.35f;
    [SerializeField] private bool fadeInOnStart;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        if (fadeInOnStart)
        {
            canvasGroup.alpha = 0f;
            FadeIn();
        }
    }

    public void FadeIn()
    {
        FadeTo(1f, defaultDuration);
    }

    public void FadeOut()
    {
        FadeTo(0f, defaultDuration);
    }

    public void FadeIn(float duration)
    {
        FadeTo(1f, duration);
    }

    public void FadeOut(float duration)
    {
        FadeTo(0f, duration);
    }

    public void FadeTo(float targetAlpha, float duration)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeRoutine(Mathf.Clamp01(targetAlpha), duration));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;

        if (duration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = targetAlpha;
        }

        bool visible = targetAlpha > 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
        fadeCoroutine = null;
    }
}
