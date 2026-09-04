using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MiniGame1 : AMiniGame
{
    [Header("Players")]
    public GameObject CarPrefab;
    public GameObject CarPrefab2;

    private GameObject CarObject;
    private GameObject CarObject2;

    [Header("Sumos")]
    public GameObject SumoPrefab;

    private GameObject SumoObject1;
    private GameObject SumoObject2;

    [Header("Time")]
    public float Timer = 3f;
    private float currentTimer = 0f;

    [Header("Push Animation")]
    public float PushDistance = 0.15f;
    public float PushDuration = 0.08f;

    public Player Player; // Se obtiene del GameManager.

    public bool ActiveGame = true;

    public int MaxScore = 80;

    private int P1Score = 0;
    private int P2Score = 0;

    private bool P1Animating = false;
    private bool P2Animating = false;


    private void Start()
    {
        InitMiniGame();
    }


    public override void NotifyWinner(PlayerPlaceholder winnerPlayer)
    {
        print("Hola");
    }


    public override void InitMiniGame()
    {
        CarObject = Instantiate(CarPrefab);
        CarObject2 = Instantiate(CarPrefab2);

        SumoObject1 = Instantiate(SumoPrefab);
        SumoObject2 = Instantiate(SumoPrefab);

        CarObject.transform.position = new Vector3(-1.5f, 1, -8);
        CarObject2.transform.position = new Vector3(1.5f, 1, -8);

        SumoObject1.transform.position = new Vector3(-1, 1, -8);
        SumoObject2.transform.position = new Vector3(1, 1, -8);
    }


    void Update()
    {
        if (!ActiveGame)
        {
            return;
        }


        // TIMER
        currentTimer += Time.deltaTime;

        if (currentTimer >= Timer)
        {
            ActiveGame = false;

            if (P1Score > P2Score)
            {
                print("P1 Gana");
            }
            else if (P1Score < P2Score)
            {
                print("P2 Gana");
            }
            else
            {
                print("Empate");
            }

            EndMiniGame();
            return;
        }


        // PLAYER 1
        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            P1Score++;

            if (!P1Animating)
            {
                StartCoroutine(
                    PushAnimation(
                        CarObject,
                        SumoObject1,
                        Vector3.right,
                        1
                    )
                );
            }

            print(P1Score + " / " + P2Score);
        }


        // PLAYER 2
        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            P2Score++;

            if (!P2Animating)
            {
                StartCoroutine(
                    PushAnimation(
                        CarObject2,
                        SumoObject2,
                        Vector3.left,
                        2
                    )
                );
            }

            print(P1Score + " / " + P2Score);
        }
    }


    IEnumerator PushAnimation(
        GameObject car,
        GameObject sumo,
        Vector3 direction,
        int player
    )
    {
        if (player == 1)
        {
            P1Animating = true;
        }
        else
        {
            P2Animating = true;
        }


        // Posiciones originales
        Vector3 carStart = car.transform.position;
        Vector3 sumoStart = sumo.transform.position;

        // Posiciones del pequeño empujón
        Vector3 carPush = carStart + direction * PushDistance;
        Vector3 sumoPush = sumoStart + direction * PushDistance;


        // -------------------------
        // IR HACIA DELANTE
        // -------------------------

        float time = 0f;

        while (time < PushDuration)
        {
            time += Time.deltaTime;

            float t = time / PushDuration;

            car.transform.position =
                Vector3.Lerp(carStart, carPush, t);

            sumo.transform.position =
                Vector3.Lerp(sumoStart, sumoPush, t);

            yield return null;
        }


        // -------------------------
        // REGRESAR
        // -------------------------

        time = 0f;

        while (time < PushDuration)
        {
            time += Time.deltaTime;

            float t = time / PushDuration;

            car.transform.position =
                Vector3.Lerp(carPush, carStart, t);

            sumo.transform.position =
                Vector3.Lerp(sumoPush, sumoStart, t);

            yield return null;
        }


        // Asegurar posición original
        car.transform.position = carStart;
        sumo.transform.position = sumoStart;


        if (player == 1)
        {
            P1Animating = false;
        }
        else
        {
            P2Animating = false;
        }
    }


    public override void EndMiniGame()
    {
        print("Fin");

        StopAllCoroutines();

        NotifyWinner(new PlayerPlaceholder());

        Destroy(CarObject);
        Destroy(CarObject2);

        Destroy(SumoObject1);
        Destroy(SumoObject2);

        ReturnToMiddleScene();
    }
}