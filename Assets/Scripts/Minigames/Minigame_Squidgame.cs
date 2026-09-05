using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Minigame_Squidgame : AMiniGame
{
    [Header("Game")]
    public float StartingX;
    public float ObjectiveX;

    public GameObject Police;
    private bool isLooking = false;
    private bool ActiveGame = true;

    public GameObject Alert;

    [Header("Timer")]
    public float alertTime = .5f;
    private float currentTimer = 0.0f;
    public float Timer = 1.0f;
    public float LookTimer = 0.5f;
    public Vector2 MinNMaxCoolDown = new Vector2(0.5f,1f);

    [Header("Players")]
    public GameObject Player1;
    public GameObject Player2;

    private bool P1Death = false;
    private bool P2Death = false;

    public Sprite Explotion;

    public float StepSpeed = 0.5f;
    void Start()
    {
        InitMiniGame();
    }
    public override void InitMiniGame()
    {
        Alert.SetActive(false);

        GameObject[] PlayerArray = { Player1, Player2 };

        for (int i = 0; i < PlayerArray.Length; i++)
        {
            Vector3 tempPos = PlayerArray[i].transform.position;
            tempPos.x = StartingX;
            PlayerArray[i].transform.position = tempPos;
        }

    }
    void Update()
    {
        if (!ActiveGame)
        {
            return;
        }

        currentTimer += Time.deltaTime;

        if (currentTimer >= Timer - alertTime && !isLooking)
        {
            Alert.SetActive(true);
        }

        if (currentTimer >= Timer && !isLooking)
        {
            Alert.SetActive(false);
            currentTimer = 0.0f;
            isLooking = true;
            Police.transform.localScale = new Vector3(-1,1,1);
            Timer = Random.Range(MinNMaxCoolDown.x, MinNMaxCoolDown.y);
        }

        if (currentTimer >= LookTimer && isLooking)
        {
            currentTimer = 0.0f;
            isLooking = false;
            Police.transform.localScale = new Vector3(1, 1, 1);
            LookTimer = Random.Range(MinNMaxCoolDown.x, MinNMaxCoolDown.y);
        }

        if (Keyboard.current.wKey.wasPressedThisFrame && !P1Death)
        {
            Vector3 tempPos = Player1.transform.position;
            tempPos.x += StepSpeed;
            Player1.transform.position = tempPos;

            if (isLooking)
            {
                P1Death = true;
                Player1.GetComponent<SpriteRenderer>().sprite = Explotion;
            }
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame && !P2Death)
        {
            Vector3 tempPos = Player2.transform.position;

            tempPos.x += StepSpeed;

            Player2.transform.position = tempPos;


            if (isLooking)
            {
                P2Death = true;
                Player2.GetComponent<SpriteRenderer>().sprite = Explotion;
            }
        }

        if (Player1.transform.position.x >= ObjectiveX || Player2.transform.position.x >= ObjectiveX)
        {
            ActiveGame = false;

            if (Player2.transform.position.x >= ObjectiveX)
            {
                NotifyWinner(2);
            }
            else
            {
                NotifyWinner(1);
            }

            EndMiniGame();
            return;
        }

        if (P1Death || P2Death)
        {
            ActiveGame = false;

            if (P1Death)
            {
                NotifyWinner(2);
            } else
            {
                NotifyWinner(1);
            }

            EndMiniGame();
            return;
        }
    }

    public override void EndMiniGame()
    {
        print("End");
        ReturnToMiddleScene();
    }
}
