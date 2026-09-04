using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Singleton;

    [Header("Submódulos de UI")]
    [SerializeField]
    private DoorController doorController;

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

    // Métodos públicos que otros scripts o eventos de botones pueden invocar
    public void OpenDoor() => doorController.Open();

    public void CloseDoor() => doorController.Close();
}
