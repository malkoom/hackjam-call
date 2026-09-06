using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Singleton;
    private static bool openDoorOnStart;

    [Header("Submódulos de UI")]
    [SerializeField]
    private DoorController doorController;

    [SerializeField]
    private ImageFlyController pieceToPlayer;

    [SerializeField]
    private GameObject garage;

    [SerializeField]
    private TMP_Text minigameText;

    [Header("Animación del texto de minijuego")]
    [SerializeField]
    private float minigameTextWaveAmplitude = 12f;

    [SerializeField]
    private float minigameTextWaveSpeed = 2.5f;

    private Coroutine minigameTextWaveCoroutine;
    private RectTransform minigameTextTransform;
    private Vector2 minigameTextInitialPosition;

    [SerializeField]
    private CanvasFader fader;

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (!openDoorOnStart)
            return;

        openDoorOnStart = false;
        OpenDoor();
    }

    public static void RequestDoorOpenOnNextScene()
    {
        openDoorOnStart = true;
    }

    public float AssignPieceToPlayer(int destination, Sprite sprite)
    {
        return pieceToPlayer.FlyAndHide(destination, sprite);
    }

    public float AssignCurrentPieceToPlayer(int destination)
    {
        Sprite currentPieceSprite = GetCurrentPieceSprite();
        return currentPieceSprite != null
            ? AssignPieceToPlayer(destination, currentPieceSprite)
            : 0f;
    }

    public void SetPieceAndShow(Sprite sprite)
    {
        if (garage != null)
            garage.SetActive(true);

        // El controlador puede estar en un padre de la imagen dentro del garaje.
        pieceToPlayer.gameObject.SetActive(true);
        pieceToPlayer.Show(sprite);
    }

    public void SetCurrentPieceAndShow()
    {
        Sprite currentPieceSprite = GetCurrentPieceSprite();
        if (currentPieceSprite != null)
            SetPieceAndShow(currentPieceSprite);
    }

    public void HidePiece()
    {
        pieceToPlayer.Hide();
    }

    public void SetMinigameTextAndShow(string text)
    {
        if (minigameText == null)
        {
            Debug.LogWarning(
                "No se ha asignado el TMP_Text de descripción del minijuego en UIManager."
            );
            return;
        }

        minigameText.gameObject.SetActive(true);
        minigameText.text = text;
        StartMinigameTextWave();
    }

    public void HideMinigameText()
    {
        if (minigameText == null)
            return;

        StopMinigameTextWave();
        minigameText.gameObject.SetActive(false);
    }

    private void StartMinigameTextWave()
    {
        StopMinigameTextWave();

        minigameTextTransform = minigameText.rectTransform;
        minigameTextInitialPosition = minigameTextTransform.anchoredPosition;
        minigameTextWaveCoroutine = StartCoroutine(AnimateMinigameText());
    }

    private void StopMinigameTextWave()
    {
        if (minigameTextWaveCoroutine != null)
        {
            StopCoroutine(minigameTextWaveCoroutine);
            minigameTextWaveCoroutine = null;
        }

        if (minigameTextTransform != null)
            minigameTextTransform.anchoredPosition = minigameTextInitialPosition;
    }

    private System.Collections.IEnumerator AnimateMinigameText()
    {
        while (true)
        {
            float offsetY =
                Mathf.Sin(Time.unscaledTime * minigameTextWaveSpeed) * minigameTextWaveAmplitude;
            minigameTextTransform.anchoredPosition =
                minigameTextInitialPosition + Vector2.up * offsetY;
            yield return null;
        }
    }

    public void ShowGarage()
    {
        if (garage == null)
        {
            Debug.LogWarning("No se ha asignado el objeto Garaje en UIManager.");
            return;
        }

        garage.SetActive(true);
    }

    public void HideGarage()
    {
        if (garage != null)
            garage.SetActive(false);
    }

    public void OpenDoor() => doorController.Open();

    public void CloseDoor() => doorController.Close();

    private Sprite GetCurrentPieceSprite()
    {
        if (GameManager.Singleton != null && GameManager.Singleton.CurrentPieceSprite != null)
            return GameManager.Singleton.CurrentPieceSprite;

        Debug.LogWarning("No hay una pieza actual con sprite en GameManager.");
        return null;
    }

    public void FadeIn()
    {
        fader.FadeIn();
    }

    public void FadeOut()
    {
        fader.FadeOut();
    }
}
