using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;

    // Diccionario Pieza ---> Minijuegos
    // Cambiar luego por la versión final de pieza
    [SerializeField]
    private Dictionary<int, List<AMiniGame>> miniGameDictionary =
        new Dictionary<int, List<AMiniGame>>();
    private AMiniGame currentMinigame;
    private int currentPiece;
    private int currentWinner;

    private System.Random randomGenerator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        if (!Singleton)
        {
            Singleton = this;
            DontDestroyOnLoad(Singleton);
        }
        else
        {
            Destroy(this);
        }
    }

    // Decide el siguiente minijuego de forma aleatoria
    private bool PullMinigame()
    {
        int randPiece = randomGenerator.Next(miniGameDictionary.Keys.Count);
        int randMinigame = randomGenerator.Next(miniGameDictionary[randPiece].Count);

        currentMinigame = miniGameDictionary[randPiece][randMinigame];
        currentPiece = randPiece;

        return currentMinigame;
    }

    // Marca el jugador ganador y la pieza a consumir
    public void SetWinner(int player, int piece)
    {
        currentWinner = player;
        currentPiece = piece;
    }

    public void FeedBack()
    {
        //1 . Aplicar la pieza al jugador
        // currentWinner.GivePiece(currentPiece);
        // 2. Lerpeo de la pieza al coche del player.
        // Sleep 1.5 segundos
        // 3. Aparece nueva pieza y minijuego
        // PullMinigame();
        // Sleep 1.5
        // Puerta abajo
    }
}
