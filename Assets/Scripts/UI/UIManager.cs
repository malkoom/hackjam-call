using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Singleton;

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

    private void Start() { }

    public float AssignPieceToPlayer(int destination, Sprite sprite)
    {
        return pieceToPlayer.FlyAndHide(destination, sprite);
    }

    public void SetPieceAndShow(Sprite sprite)
    {
        pieceToPlayer.GetComponent<Image>().sprite = sprite;
        pieceToPlayer.gameObject.SetActive(true);
    }

    public void HidePiece()
    {
        pieceToPlayer.gameObject.SetActive(false);
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
            float offsetY = Mathf.Sin(Time.unscaledTime * minigameTextWaveSpeed)
                * minigameTextWaveAmplitude;
            minigameTextTransform.anchoredPosition = minigameTextInitialPosition
                + Vector2.up * offsetY;
            yield return null;
        }
    }

    public void ShowGarage()
    {
        garage.SetActive(true);
    }

    public void HideGarage()
    {
        garage.SetActive(false);
    }

    public void OpenDoor() => doorController.Open();

    public void CloseDoor() => doorController.Close();
}
