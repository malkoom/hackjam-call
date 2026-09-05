
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Minigame_Snail : AMiniGame
{

    [Header("Snail")]

    public Sprite Snail_Sprite1;
    public Sprite Snail_Sprite2;

    public Sprite Snail_Back_Sprite1;
    public Sprite Snail_Back_Sprite2;

    public Vector3[] InitialPos = new Vector3[2];
    public float FinalPosX;

    public GameObject Snail;
    public GameObject Snail2;

    private bool ActiveGame = true;

    [Header("Keys")]
    private Key[] player1KeySequence = { Key.W, Key.D, Key.S, Key.A };
    private int playerKeyIndex = 0;
    private Key[] player2KeySequence = {Key.UpArrow,Key.RightArrow, Key.DownArrow, Key.LeftArrow};
    private int player2KeyIndex = 0;

    [Header("UI")]
    public GameObject key;
    public GameObject key2;

    [SerializeField] public Dictionary<Key, Sprite> p1KeysDictionary = new Dictionary<Key, Sprite>();
    [SerializeField] public Dictionary<Key, Sprite> p2KeysDictionary = new Dictionary<Key, Sprite>();


    void Start()
    {
        InitMiniGame();
    }
    public override void InitMiniGame()
    {
        
        Snail.GetComponent<SpriteRenderer>().sprite = Snail_Sprite1;
        Snail2.GetComponent<SpriteRenderer>().sprite = Snail_Sprite2;

        Snail.transform.position = InitialPos[0];
        Snail2.transform.position = InitialPos[1];
    }

    void Update()
    {


        if (!ActiveGame)
        {
            return;
        }

        if (Keyboard.current[player1KeySequence[playerKeyIndex]].wasPressedThisFrame)
        {
            Snail.transform.position += new Vector3(0.3f, 0, 0);
            playerKeyIndex++;

            if (playerKeyIndex >= player1KeySequence.Length)
            {
                playerKeyIndex = 0;
            }

            key.GetComponent<SpriteRenderer>().sprite = p1KeysDictionary[player1KeySequence[playerKeyIndex]];
        }

        if (Keyboard.current[player2KeySequence[player2KeyIndex]].wasPressedThisFrame)
        {
            Snail2.transform.position += new Vector3(0.3f, 0, 0);
            player2KeyIndex++;

            if (player2KeyIndex >= player2KeySequence.Length)
            {
                player2KeyIndex = 0;
            }

            key2.GetComponent<SpriteRenderer>().sprite = p2KeysDictionary[player2KeySequence[player2KeyIndex]];
        }

        if (Snail.transform.position.x >= FinalPosX || Snail2.transform.position.x >= FinalPosX)
        {
            ActiveGame = false;

            if (Snail.transform.position.x >= FinalPosX)
            {
                NotifyWinner(1);
            }
            else if (Snail2.transform.position.x >= FinalPosX)
            {
                NotifyWinner(2);
            }
            else
            {
                NotifyWinner(1);
            }

            EndMiniGame();
        }
    }

    public override void EndMiniGame()
    {
        Destroy(Snail);
        Destroy(Snail2);

        ReturnToMiddleScene();
    }
}
