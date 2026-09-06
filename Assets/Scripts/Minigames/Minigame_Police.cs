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

    public float CurrentTimer = 0f;

    private bool ActiveGame = true;


    void Start()
    {
        InitMiniGame();
    }


    public override void InitMiniGame()
    {
        CurrentTimer = 0f;
        P1Fail = false;
        P2Fail = false;
        ActiveGame = true;
    }


    void Update()
    {
        if (!ActiveGame)
        {
            return;
        }

        CurrentTimer += Time.deltaTime;
        UpdateObstacles();
        CheckReactionInput();
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
    private void CheckReactionInput()
    {
        if (Notes == null || Notes.Length == 0 || Keyboard.current == null)
            return;

        RhythmNote reactionNote = Notes[0];
        bool p1Pressed = Keyboard.current[reactionNote.KeyP1].wasPressedThisFrame;
        bool p2Pressed = Keyboard.current[reactionNote.KeyP2].wasPressedThisFrame;

        if (!p1Pressed && !p2Pressed)
            return;

        // Si ambos inputs llegan en el mismo frame, Input System no proporciona
        // un orden entre ellos. Se escoge al azar para no favorecer a P1.
        int pressingPlayer = p1Pressed && p2Pressed ? Random.Range(1, 3) : p1Pressed ? 1 : 2;

        if (CurrentTimer < reactionNote.HitTime)
        {
            FinishGame(pressingPlayer == 1 ? 2 : 1);
            return;
        }

        FinishGame(pressingPlayer);
    }

    private void FinishGame(int winner)
    {
        ActiveGame = false;

        bool player1Wins = winner == 1;
        P1Fail = !player1Wins;
        P2Fail = player1Wins;

        Obstacle1.GetComponent<SpriteRenderer>().sprite = player1Wins ? Winner : Explotion;
        Obstacle2.GetComponent<SpriteRenderer>().sprite = player1Wins ? Explotion : Winner;

        NotifyWinner(winner);
        EndMiniGame();
    }


    public override void EndMiniGame()
    {
        ReturnToMiddleScene();
    }
}
