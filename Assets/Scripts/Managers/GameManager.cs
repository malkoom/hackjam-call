using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;

    [System.Serializable]
    public struct PieceMinigameEntry
    {
        public Piece piece;

        [Tooltip(
            "Nombre de las escenas que pueden otorgar esta pieza. Deben estar en Build Settings."
        )]
        public List<string> minigameScenes;
    }

    [Header("Configuración de Minijuegos")]
    [SerializeField]
    private List<PieceMinigameEntry> minigameSetup = new List<PieceMinigameEntry>();

    [Header("Transición")]
    [SerializeField]
    private float fadeOutDelay = 0.75f;

    [SerializeField]
    private float fadeInDelay = 0.75f;

    private Dictionary<Piece, List<string>> minigameSceneDictionary =
        new Dictionary<Piece, List<string>>();

    private Piece currentPiece;
    private Player currentWinner;
    public Player player1;
    public Player player2;

    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
        DontDestroyOnLoad(gameObject);
        InitializeDictionary();
    }

    private void Start()
    {
        StartNextMinigame();
    }

    private void InitializeDictionary()
    {
        minigameSceneDictionary.Clear();
        foreach (var entry in minigameSetup)
        {
            if (entry.piece == null || entry.minigameScenes == null)
                continue;

            List<string> validScenes = entry
                .minigameScenes.Where(sceneName => !string.IsNullOrWhiteSpace(sceneName))
                .ToList();

            if (validScenes.Count > 0 && !minigameSceneDictionary.ContainsKey(entry.piece))
                minigameSceneDictionary.Add(entry.piece, validScenes);
        }
    }

    // Selecciona una pieza y la escena de uno de sus minijuegos disponibles.
    public string PullMinigame()
    {
        List<Piece> availablePieces = minigameSceneDictionary
            .Where(entry => entry.Value.Count > 0)
            .Select(entry => entry.Key)
            .ToList();

        if (availablePieces.Count == 0)
        {
            Debug.LogWarning("No hay piezas ni escenas de minijuegos configuradas.");
            return null;
        }

        currentPiece = availablePieces[Random.Range(0, availablePieces.Count)];

        List<string> availableScenes = minigameSceneDictionary[currentPiece];
        return availableScenes[Random.Range(0, availableScenes.Count)];
    }

    public void SetWinner(int player, Piece piece)
    {
        if (player == 1)
        {
            currentWinner = player1;
        }
        else if (player == 2)
        {
            currentWinner = player2;
        }
        else
        {
            Debug.LogError("Numero de player no existe");
        }
        if (piece != null)
            currentPiece = piece;
    }

    public void StartNextMinigame()
    {
        StopAllCoroutines();
        StartCoroutine(LoadNextMinigameRoutine());
    }

    public void StartFeedbackSequence()
    {
        //1. Bajamos la puerta.
    }

    private IEnumerator FeedBackRoutine()
    {
        UIManager.Singleton.CloseDoor();
        yield return new WaitForSeconds(fadeInDelay);

        UIManager.Singleton.OpenDoor();
        yield return new WaitForSeconds(fadeOutDelay);
    }

    private IEnumerator LoadNextMinigameRoutine()
    {
        string nextScene = PullMinigame();
        if (string.IsNullOrEmpty(nextScene))
            yield break;

        if (UIManager.Singleton != null)
            UIManager.Singleton.CloseDoor();

        yield return new WaitForSeconds(fadeOutDelay);

        SceneManager.LoadScene(nextScene);

        if (UIManager.Singleton != null)
            UIManager.Singleton.OpenDoor();

        yield return new WaitForSeconds(fadeInDelay);
    }
}
