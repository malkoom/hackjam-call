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
        // Cierra la puerta al iniciar
        doorController.Close();
    }

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
            Debug.LogWarning("No se ha asignado el TMP_Text de descripción del minijuego en UIManager.");
            return;
        }

        minigameText.gameObject.SetActive(true);
        minigameText.text = text;
    }

    public void HideMinigameText()
    {
        if (minigameText == null)
            return;

        minigameText.gameObject.SetActive(false);
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
