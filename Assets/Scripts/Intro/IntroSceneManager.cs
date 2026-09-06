using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroSceneManager : MonoBehaviour
{
    [Header("Transición")]
    [SerializeField]
    private DoorController doorController;

    [SerializeField]
    private float fallbackDoorCloseDuration = 0.5f;

    [Header("Pantalla de inicio")]
    [SerializeField]
    private GameObject startGameUI;

    [SerializeField]
    private Button startGameButton;

    [SerializeField]
    private string gameSceneName = "Minigame Sumo";

    private bool cameraPathFinished;
    private bool isLoadingGame;

    private void Awake()
    {
        if (startGameUI != null)
            SetActiveRecursively(startGameUI, false);

        if (startGameButton != null)
            startGameButton.gameObject.SetActive(false);

        if (startGameButton != null)
            startGameButton.onClick.AddListener(StartGame);
    }

    private void OnDestroy()
    {
        if (startGameButton != null)
            startGameButton.onClick.RemoveListener(StartGame);
    }

    // Asigna este método como Animation Event al final del clip Camera_Path.
    public void OnCameraPathFinished()
    {
        if (cameraPathFinished)
            return;

        cameraPathFinished = true;
        StartCoroutine(ShowStartButtonAfterDoorCloses());
    }

    private IEnumerator ShowStartButtonAfterDoorCloses()
    {
        yield return new WaitForSeconds(2);

        CloseDoor();

        float closeDuration =
            doorController != null ? doorController.TransitionDuration : fallbackDoorCloseDuration;
        yield return new WaitForSeconds(closeDuration);

        if (startGameUI != null)
            SetActiveRecursively(startGameUI, true);
        else
            Debug.LogWarning("No se ha asignado la UI de inicio en IntroSceneManager.");

        if (startGameButton != null)
            startGameButton.gameObject.SetActive(true);
        else
            Debug.LogWarning("No se ha asignado el botón de inicio en IntroSceneManager.");
    }

    public void StartGame()
    {
        if (isLoadingGame)
            return;

        if (string.IsNullOrWhiteSpace(gameSceneName))
        {
            Debug.LogWarning("No se ha configurado la escena de juego en IntroSceneManager.");
            return;
        }

        isLoadingGame = true;
        if (startGameButton != null)
        {
            startGameButton.interactable = false;
            startGameButton.gameObject.SetActive(false);
        }
        SceneManager.LoadScene(gameSceneName);
    }

    private IEnumerator OpenDoorThenLoadGame()
    {
        if (doorController != null)
        {
            doorController.Open();
            yield return new WaitForSeconds(doorController.TransitionDuration);
        }
        else
        {
            Debug.LogWarning("No se ha asignado una puerta para abrir antes de cambiar de escena.");
        }

        SceneManager.LoadScene(gameSceneName);
    }

    private void CloseDoor()
    {
        if (doorController != null)
            doorController.Close();
        else if (UIManager.Singleton != null)
            UIManager.Singleton.CloseDoor();
        else
            Debug.LogWarning("No se ha asignado una puerta para cerrar al acabar la intro.");
    }

    private static void SetActiveRecursively(GameObject target, bool isActive)
    {
        target.SetActive(isActive);

        foreach (Transform child in target.transform)
            SetActiveRecursively(child.gameObject, isActive);
    }
}
