using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public struct Obstacle
{
    public Sprite Ready, Set, Jump;
}

[System.Serializable]
public class RhythmNote
{
    public float HitTime;
    public Key KeyP1;
    public Key KeyP2;
}

public enum RythmObjectType
{
    READY,
    SET,
    JUMP,
}

[System.Serializable]
public class RythmObject
{
    public float Time;
    public string Name = "Null";
    public RythmObjectType Type = RythmObjectType.READY;
}

public class Minigame_Police : AMiniGame
{
    public float TimeLimit = 6.0f;

    [Header("Obstacles")]
    public Sprite Winner;
    public Sprite Explotion;

    public GameObject Obstacle1;
    public GameObject Obstacle2;

    private int P1Obstacle = 0;
    private int P2Obstacle = 0;

    private bool P1Fail = false;
    private bool P2Fail = false;

    public Obstacle[] ObstaclesSprites;

    [Header("Rhythm")]
    public int BPM = 120;
    public RhythmNote[] Notes;
    public RythmObject[] Sequence;

    public float NoteForgiveness = 0.2f;

    public float CurrentTimer = 0f;

    private int p1HitNotes = 0;
    private int p2HitNotes = 0;

    private bool ActiveGame = true;

    [Header("Players")]
    private int P1CurrentNote = 0;
    private int P1Score = 0;

    private int P2CurrentNote = 0;
    private int P2Score = 0;

    private bool P1Winner = false;
    private bool P2Winner = false;

    private int MaxScore = 10;


    void Start()
    {
        InitMiniGame();
    }


    public override void InitMiniGame()
    {
        MaxScore = Notes.Length;

        CurrentTimer = 0f;

        P1CurrentNote = 0;
        P2CurrentNote = 0;

        P1Score = 0;
        P2Score = 0;

        p1HitNotes = 0;
        p2HitNotes = 0;

        ActiveGame = true;
    }


    void Update()
    {
        if (!ActiveGame)
        {
            return;
        }

        CurrentTimer += Time.deltaTime;

        if (P1Winner && P2Winner)
        {
            P1Fail = true;
            P1Winner = false;
        }

        if (P1Fail || P2Fail)
        {
            if (P1Fail)
            {
                Obstacle1.GetComponent<SpriteRenderer>().sprite = Explotion;
                Obstacle2.GetComponent<SpriteRenderer>().sprite = Winner;
            }

            else if (P2Fail)
            {
                Obstacle2.GetComponent<SpriteRenderer>().sprite = Explotion;
                Obstacle1.GetComponent<SpriteRenderer>().sprite = Winner;
            }

            if (P1Fail && P2Fail)
            {
                P2Fail = false;
            }
        }

        else
        {
            UpdateObstacles();
        }

        CheckPlayer1();
        CheckPlayer2();

        if (CurrentTimer >= TimeLimit)
        {
            if (P1Score > P2Score)
            {
                NotifyWinner(1);
            } else
            {
                NotifyWinner(2);
            }
            ActiveGame = false;
                EndMiniGame();
        }
    }

    void UpdateObstacles()
    {
        RythmObject currentObject = null;

        for (int i = 0; i < Sequence.Length; i++)
        {
            if (Sequence[i].Time < CurrentTimer)
            {
                currentObject = Sequence[i];
            }
        }

        if (currentObject == null)
        {
            return;
        }

        switch (currentObject.Type)
        {
            case RythmObjectType.READY:

                if (!P1Fail)
                {
                    Obstacle1.GetComponent<SpriteRenderer>().sprite =
                        ObstaclesSprites[P1Obstacle].Ready;
                }

                if (!P2Fail)
                {
                    Obstacle2.GetComponent<SpriteRenderer>().sprite =
                        ObstaclesSprites[P2Obstacle].Ready;
                }

                break;


            case RythmObjectType.SET:

                if (!P1Fail)
                {
                    Obstacle1.GetComponent<SpriteRenderer>().sprite =
                        ObstaclesSprites[P1Obstacle].Set;
                }

                if (!P2Fail)
                {
                    Obstacle2.GetComponent<SpriteRenderer>().sprite =
                        ObstaclesSprites[P2Obstacle].Set;
                }

                break;


            case RythmObjectType.JUMP:

                if (!P1Fail)
                {
                    Obstacle1.GetComponent<SpriteRenderer>().sprite =
                        ObstaclesSprites[P1Obstacle].Jump;
                }

                if (!P2Fail)
                {
                    Obstacle2.GetComponent<SpriteRenderer>().sprite =
                        ObstaclesSprites[P2Obstacle].Jump;
                }

                break;
        }
    }
    void CheckPlayer1()
    {
        if (P1CurrentNote >= Notes.Length)
        {
            return;
        }

        if (P1Fail)
        {
            return;
        }

        RhythmNote note = Notes[P1CurrentNote];

        if (Keyboard.current[note.KeyP1].wasPressedThisFrame)
        {
            float difference = Mathf.Abs(CurrentTimer - note.HitTime);

            if (difference <= NoteForgiveness)
            {
                P1Score++;
                p1HitNotes++;

                P1Winner = true;

                P1CurrentNote++;
            }
            else
            {
                P1Fail = true;
                Obstacle1.GetComponent<SpriteRenderer>().sprite = Explotion;
                P1Score--;
                P1CurrentNote++;
                print("P1 FAILED");
            }
        }

        if (CurrentTimer > note.HitTime + NoteForgiveness)
        {
            P1Fail = true;
            Obstacle1.GetComponent<SpriteRenderer>().sprite = Explotion;
            print("P1 MISS!");
            P1Score--;
            P1CurrentNote++;
        }
    }


    void CheckPlayer2()
    {
        if (P2CurrentNote >= Notes.Length)
        {
            return;
        }

        if (P2Fail)
        {
            return;
        }

        RhythmNote note = Notes[P2CurrentNote];

        if (Keyboard.current[note.KeyP2].wasPressedThisFrame)
        {
            float difference = Mathf.Abs(CurrentTimer - note.HitTime);

            if (difference <= NoteForgiveness)
            {
                P2Score++;
                p2HitNotes++;

                P2Winner = true;

                P2CurrentNote++;
            }
            else
            {
                P2Fail = true;
                Obstacle2.GetComponent<SpriteRenderer>().sprite = Explotion;
                P2Score--;
                P2CurrentNote++;
                print("P2 FAILED");
            }
        }

        if (CurrentTimer > note.HitTime + NoteForgiveness)
        {
            P2Fail = true;
            Obstacle2.GetComponent<SpriteRenderer>().sprite = Explotion;
            print("P2 MISS!");
            P1Score--;
            P2CurrentNote++;
        }
    }


    public override void EndMiniGame()
    {
        Obstacle1.SetActive(false);
    }
}