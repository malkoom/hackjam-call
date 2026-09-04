using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;

    // Diccionario Pieza ---> Minijuegos
    [SerializeField]
    private Dictionary<int, List<AMiniGame>> miniGameDictionary =
        new Dictionary<int, List<AMiniGame>>();
    private AMiniGame currentMinigame;

    private System.Random randomGenerator;

    private
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
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

    private bool PullMinigame()
    {
        int randPiece = randomGenerator.Next(miniGameDictionary.Keys.Count);
        int randMinigame = randomGenerator.Next(miniGameDictionary[randPiece].Count);

        currentMinigame = miniGameDictionary[randPiece][randMinigame];

        return currentMinigame;
    }
}
