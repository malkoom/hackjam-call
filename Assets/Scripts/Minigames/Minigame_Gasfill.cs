using FMODUnity;
using UnityEngine;
using UnityEngine.InputSystem;


[System.Serializable] public struct PlayerTank
{
    public GameObject Fill, Hand, Tank;
}
public class Minigame_Gasfill : AMiniGame
{
    [Header("Sound")]
    public EventReference Pour;

    private FMOD.Studio.EventInstance p1PourInstance;
    private FMOD.Studio.EventInstance p2PourInstance;

    private float ZBegginerPos;

    public PlayerTank P1Tank;
    public PlayerTank P2Tank;

    public Vector2 MinNMaxFill;

    public float Objective;

    public float[] AllowedPercentage = {10,20,50,70,90};
    public float Forgiveness = 5f;

    private bool ActiveGame = true;

    private bool P1isReady = false;
    private float P1Score = 0.0f;

    private bool P2isReady = false;
    private float P2Score = 0.0f;

    private int firstPlayer = 0;

    void Start()
    {
        InitMiniGame();
    }
    public override void InitMiniGame()
    {
        Objective = GetPercentage(100.0f);
        setHandPose();

    }

    void Update()
    {
        if (!ActiveGame)
        {
            return;
        }

        if (Keyboard.current.wKey.isPressed && !P1isReady)
        {
            Vector3 p1Pos = P1Tank.Fill.transform.position;
            p1Pos.y += 0.01f;

            if (p1Pos.y < MinNMaxFill.y)
            {
                P1Tank.Fill.transform.position = p1Pos;
            }
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            P1Tank.Tank.transform.rotation = Quaternion.Euler(0, 0, -50);

            p1PourInstance = RuntimeManager.CreateInstance(Pour);
            RuntimeManager.AttachInstanceToGameObject(p1PourInstance, P1Tank.Tank);
            p1PourInstance.start();
        }

        if (Keyboard.current.wKey.wasReleasedThisFrame)
        {
            if (firstPlayer == 0)
            {
                firstPlayer = 1;
            }

            P1isReady = true;
            P1Score = P1Tank.Fill.transform.position.y;
            P1Tank.Tank.transform.rotation = Quaternion.Euler(0, 0, 0);

            p1PourInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            p1PourInstance.release();
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            P2Tank.Tank.transform.rotation = Quaternion.Euler(0,0,50);

            p2PourInstance = RuntimeManager.CreateInstance(Pour);
            RuntimeManager.AttachInstanceToGameObject(p2PourInstance, P2Tank.Tank);
            p2PourInstance.start();
        }

        if (Keyboard.current.upArrowKey.isPressed && !P2isReady)
        {
            Vector3 p2Pos = P2Tank.Fill.transform.position;
            p2Pos.y += 0.01f;

            if (p2Pos.y < MinNMaxFill.y)
            {
                P2Tank.Fill.transform.position = p2Pos;
            }
        }

        if (Keyboard.current.upArrowKey.wasReleasedThisFrame)
        {
            if (firstPlayer == 0)
            {
                firstPlayer = 2;
            }

            P2isReady = true;
            P2Score = P2Tank.Fill.transform.position.y;
            P2Tank.Tank.transform.rotation = Quaternion.Euler(0, 0, 0);

            p2PourInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            p2PourInstance.release();
        }

        if (P2isReady && P1isReady)
        {
            VerifyWinner();
        }
    }

    void VerifyWinner()
    {
        ActiveGame = false;
        float P1Distance = Mathf.Abs(Objective - P1Score);
        float P2Distance = Mathf.Abs(Objective - P2Score);

        if (P1Distance < P2Distance)
        {
            print("Winner P1");
            NotifyWinner(1);
        } else if(P1Distance > P2Distance)
        {
            print("Winner P2");
            NotifyWinner(2);
        } else
        {
            NotifyWinner(firstPlayer);
        }

            EndMiniGame();

    }
    void setHandPose()
    {
        Vector3 p1Pos = P1Tank.Hand.transform.position;
        p1Pos.y = Objective;

        Vector3 p2Pos = P2Tank.Hand.transform.position;
        p2Pos.y = Objective;

        P1Tank.Hand.transform.position = p1Pos;
        P2Tank.Hand.transform.position = p2Pos;
    }

    float GetPercentage(float percentage)
    {
        return Mathf.Lerp(MinNMaxFill.x, MinNMaxFill.y, percentage / 100f);
    }
    public override void EndMiniGame()
    {
        ReturnToMiddleScene();
    }
}
