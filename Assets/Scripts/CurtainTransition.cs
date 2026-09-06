using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CurtainTransition : MonoBehaviour
{
    public static CurtainTransition Instance;

    [Header("Referencias de UI")]
    public RectTransform leftCurtain;
    public RectTransform rightCurtain;
    public RectTransform canvasRect;

    [Header("Escena a Cargar")]
    public string nextSceneName = "ScalextricScene";

    [Header("Tiempos")]
    [Tooltip("Tiempo en cerrarse en el centro")]
    public float closeDuration = 0.5f;

    [Tooltip("Pausa a oscuras antes de abrirse")]
    public float pauseBetween = 0.2f;

    [Tooltip("Tiempo en abrirse en la nueva escena")]
    public float openDuration = 0.5f;

    private bool hasExecuted = false;

    void Awake()
    {
        // Singleton persistente
        if (Instance == null)
        {
            Instance = this;
            if (transform.parent == null)
                DontDestroyOnLoad(gameObject);
            else
                DontDestroyOnLoad(transform.root.gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (canvasRect == null)
            canvasRect = GetComponentInParent<Canvas>()?.GetComponent<RectTransform>();

        // 1. Ocultar completamente al inicio para que no asome ningún borde
        if (leftCurtain != null) leftCurtain.gameObject.SetActive(false);
        if (rightCurtain != null) rightCurtain.gameObject.SetActive(false);
    }

    /// <summary>
    /// Llamado desde el Animation Event
    /// </summary>
    public void StartTransition()
    {
        // Si ya se ejecutó una vez, ignorar cualquier llamada repetida del Animator
        if (hasExecuted) return;
        hasExecuted = true;

        StartCoroutine(TransitionRoutine(nextSceneName));
    }

    private IEnumerator TransitionRoutine(string targetScene)
    {
        // Activar y colocar bien fuera de la pantalla con margen extra
        if (leftCurtain != null) leftCurtain.gameObject.SetActive(true);
        if (rightCurtain != null) rightCurtain.gameObject.SetActive(true);

        SetCurtainsPosition(0f); // 0 = Fuera de pantalla

        // 1. CERRAR: Deslizar hacia el centro
        float elapsed = 0f;
        while (elapsed < closeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / closeDuration);
            SetCurtainsPosition(t);
            yield return null;
        }

        SetCurtainsPosition(1f); // 100% cubierto en el centro

        yield return new WaitForSeconds(pauseBetween);

        // 2. CARGAR LA SIGUIENTE ESCENA
        if (!string.IsNullOrEmpty(targetScene))
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);
            if (asyncLoad != null)
            {
                while (!asyncLoad.isDone)
                {
                    yield return null;
                }
            }
            else
            {
                Debug.LogError($"[CurtainTransition] La escena '{targetScene}' no existe o no está en Build Settings.");
            }
        }

        yield return new WaitForSeconds(0.1f);

        // 3. ABRIR: Deslizar hacia los lados en la nueva escena
        elapsed = 0f;
        while (elapsed < openDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(1f, 0f, elapsed / openDuration);
            SetCurtainsPosition(t);
            yield return null;
        }

        SetCurtainsPosition(0f);

        // 4. AUTODESTRUCCIÓN: Tras el cambio ya no se necesita más
        Debug.Log("[CurtainTransition] Transición terminada con éxito. Destruyendo cortinas.");
        
        if (transform.parent == null)
            Destroy(gameObject);
        else
            Destroy(transform.root.gameObject);
    }

    private void SetCurtainsPosition(float t)
    {
        if (canvasRect == null || leftCurtain == null || rightCurtain == null) return;

        // Añadimos 250 píxeles de margen extra para garantizar que queden 100% fuera de la vista
        float offscreenOffset = (canvasRect.rect.width * 0.5f) + 250f;

        // Panel Izquierdo: de -offscreenOffset a 0
        float leftX = Mathf.Lerp(-offscreenOffset, 0f, t);
        leftCurtain.anchoredPosition = new Vector2(leftX, leftCurtain.anchoredPosition.y);

        // Panel Derecho: de +offscreenOffset a 0
        float rightX = Mathf.Lerp(offscreenOffset, 0f, t);
        rightCurtain.anchoredPosition = new Vector2(rightX, rightCurtain.anchoredPosition.y);
    }
}