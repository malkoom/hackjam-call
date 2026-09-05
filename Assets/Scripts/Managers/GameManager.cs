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

        [Tooltip("Descripción de cada minijuego, en el mismo orden que minigameScenes.")]
        public List<string> descriptions;
    }

    [Header("Configuración de Minijuegos")]
    [SerializeField]
    private List<PieceMinigameEntry> minigameSetup = new List<PieceMinigameEntry>();

    [Header("Transición")]
    [SerializeField]
    private float fadeOutDelay = 0.5f;

    [SerializeField]
    private float fadeInDelay = 0.5f;

    private Dictionary<Piece, List<string>> minigameSceneDictionary =
        new Dictionary<Piece, List<string>>();

    private Dictionary<string, string> minigameDescriptions = new Dictionary<string, string>();

    [SerializeField]
    private Piece currentPiece;
    private int currentWinner;
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
        DontDestroyOnLoad(player1);
        DontDestroyOnLoad(player2);
        InitializeDictionary();
    }

    private void Start() { }

    private void InitializeDictionary()
    {
        minigameSceneDictionary.Clear();
        minigameDescriptions.Clear();
        foreach (var entry in minigameSetup)
        {
            if (entry.piece == null || entry.minigameScenes == null)
                continue;

            List<string> validScenes = new List<string>();

            for (int i = 0; i < entry.minigameScenes.Count; i++)
            {
                string sceneName = entry.minigameScenes[i];
                if (string.IsNullOrWhiteSpace(sceneName))
                    continue;

                validScenes.Add(sceneName);

                string description =
                    entry.descriptions != null && i < entry.descriptions.Count
                        ? entry.descriptions[i]
                        : string.Empty;

                if (!minigameDescriptions.ContainsKey(sceneName))
                    minigameDescriptions.Add(sceneName, description);
            }

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
        string selectedScene = availableScenes[Random.Range(0, availableScenes.Count)];

        return selectedScene;
    }

    public void SetWinner(int player, Piece piece)
    {
        currentWinner = player;
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
        StopAllCoroutines();
        StartCoroutine(FeedBackRoutine());
    }

    private IEnumerator FeedBackRoutine()
    {
        UIManager.Singleton.CloseDoor();

        yield return new WaitForSeconds(fadeInDelay);

        UIManager.Singleton.ShowGarage();
        UIManager.Singleton.HideMinigameText();

        SceneManager.LoadScene(0);

        UIManager.Singleton.OpenDoor();
        yield return new WaitForSeconds(fadeOutDelay);

        float duration = UIManager.Singleton.AssignPieceToPlayer(
            currentWinner,
            currentPiece.PieceTexture
        );
        yield return new WaitForSeconds(duration + 1);

        StartCoroutine(LoadNextMinigameRoutine());
    }

    private IEnumerator LoadNextMinigameRoutine()
    {
        string nextScene = PullMinigame();
        if (string.IsNullOrEmpty(nextScene))
            yield break;

        UIManager.Singleton.CloseDoor();
        yield return new WaitForSeconds(fadeOutDelay);
        UIManager.Singleton.SetPieceAndShow(currentPiece.PieceTexture);

        minigameDescriptions.TryGetValue(nextScene, out string description);
        UIManager.Singleton.SetMinigameTextAndShow(description ?? string.Empty);

        yield return new WaitForSeconds(1.5f);
        UIManager.Singleton.HideGarage();

        SceneManager.LoadScene(nextScene);

        if (UIManager.Singleton != null)
            UIManager.Singleton.OpenDoor();

        yield return new WaitForSeconds(fadeInDelay);
    }
}
