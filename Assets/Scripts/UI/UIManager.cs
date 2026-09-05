using UnityEngine;

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

    public void AssignPieceToPlayer(int destination, Sprite sprite)
    {
        pieceToPlayer.FlyAndHide(destination, sprite);
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
