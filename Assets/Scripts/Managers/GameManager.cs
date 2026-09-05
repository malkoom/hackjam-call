using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;

    // Estructura auxiliar para poder editarlo desde el Inspector de Unity
    [System.Serializable]
    public struct PieceMinigameEntry
    {
        public Piece piece;
        public List<AMiniGame> minigames;
    }

    [Header("Configuración de Minijuegos")]
    [SerializeField]
    private List<PieceMinigameEntry> minigameSetup = new List<PieceMinigameEntry>();

    private Dictionary<Piece, List<AMiniGame>> miniGameDictionary =
        new Dictionary<Piece, List<AMiniGame>>();

    private AMiniGame currentMinigame;
    private Piece currentPiece;
    private Player currentWinner;
    public Player player1;
    public Player player2;

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            DontDestroyOnLoad(gameObject);
            InitializeDictionary();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeDictionary()
    {
        miniGameDictionary.Clear();
        foreach (var entry in minigameSetup)
        {
            if (entry.piece != null && !miniGameDictionary.ContainsKey(entry.piece))
            {
                miniGameDictionary.Add(entry.piece, entry.minigames);
            }
        }
    }

    // Selecciona aleatoriamente una pieza y uno de sus minijuegos disponibles
    public AMiniGame PullMinigame()
    {
        if (miniGameDictionary.Count == 0)
        {
            Debug.LogWarning("No hay piezas ni minijuegos configurados en el diccionario.");
            return null;
        }

        // Obtener una clave aleatoria (Piece)
        List<Piece> availablePieces = miniGameDictionary.Keys.ToList();
        int randPieceIndex = UnityEngine.Random.Range(0, availablePieces.Count);
        currentPiece = availablePieces[randPieceIndex];

        List<AMiniGame> availableMinigames = miniGameDictionary[currentPiece];

        if (availableMinigames == null || availableMinigames.Count == 0)
        {
            Debug.LogWarning($"La pieza {currentPiece} no tiene minijuegos asignados.");
            return null;
        }

        // Obtener un minijuego aleatorio para esa pieza
        int randMinigameIndex = UnityEngine.Random.Range(0, availableMinigames.Count);
        currentMinigame = availableMinigames[randMinigameIndex];

        return currentMinigame;

        // TODO
        // Quitar la pieza tomada
    }

    public void SetWinner(int player, Piece piece)
    {
        if (player == 1)
        {
            currentWinner = player1;
        }
        if (player == 2)
        {
            currentWinner = player2;
        }
        else
        {
            Debug.LogError("Numero de player no existe");
        }

        currentPiece = piece;
    }

    public void StartFeedbackSequence()
    {
        StopAllCoroutines();
        StartCoroutine(FeedbackRoutine());
    }

    private IEnumerator FeedbackRoutine()
    {
        UIManager.Singleton.CloseDoor();
        yield return new WaitForSeconds(0.5f);
        // 1. Aplicar la pieza al jugador ganador
        // PlayerManager.Instance.GetPlayer(currentWinner).GivePiece(currentPiece);
        currentMinigame.gameObject.SetActive(false);
        // 2. Espera para el lerp/animación de la pieza viajando al coche
        yield return new WaitForSeconds(1.5f);

        // 3. Seleccionar nuevo minijuego y pieza
        PullMinigame();

        // 4. Pausa antes de cerrar/bajar la compuerta de transición
        yield return new WaitForSeconds(1.5f);
    }
}
