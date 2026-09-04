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

    public Sprite Car2Sprite;


    [Header("Sumos")]
    public GameObject SumoPrefab;

    private GameObject SumoObject1;
    private GameObject SumoObject2;

    public Sprite HappySumo;
    public Sprite SadSumo;


    [Header("Time")]
    public float Timer = 3f;

    // Tiempo que se ven las caras antes de salir volando
    public float WinnerPause = 0.5f;

    // Tiempo después de la animación antes de terminar
    public float EndDelay = 1f;

    private float currentTimer = 0f;


    [Header("Push Animation")]
    public float PushDistance = 0.15f;
    public float PushDuration = 0.08f;


    [Header("Throw Animation")]
    public float ThrowDistance = 6f;
    public float ThrowHeight = 2f;
    public float ThrowDuration = 0.8f;
    public float ThrowRotation = 720f;


    public Player Player;

    public bool ActiveGame = true;

    private int P1Score = 0;
    private int P2Score = 0;

    private bool P1Animating = false;
    private bool P2Animating = false;


    private void Start()
    {
        InitMiniGame();
    }

    public override void InitMiniGame()
    {
        ActiveGame = true;

        currentTimer = 0f;

        P1Score = 0;
        P2Score = 0;


        // -------------------------
        // CREATE PLAYERS
        // -------------------------

        CarObject = Instantiate(CarPrefab);
        CarObject2 = Instantiate(CarPrefab2);


        // -------------------------
        // CREATE SUMOS
        // -------------------------

        SumoObject1 = Instantiate(SumoPrefab);
        SumoObject2 = Instantiate(SumoPrefab);


        // -------------------------
        // POSITIONS
        // -------------------------

        CarObject.transform.position =
            new Vector3(-1.5f, 1f - 0.159f, -8f);

        CarObject2.transform.position =
            new Vector3(1.5f, 1f - 0.159f, -8f);


        // Player 2 sprite
        CarObject2.GetComponent<SpriteRenderer>().sprite =
            Car2Sprite;


        SumoObject1.transform.position =
            new Vector3(-1f, 1f, -8f);

        SumoObject2.transform.position =
            new Vector3(1f, 1f, -8f);


        // Flip Sumo 2
        SumoObject2.transform.localScale =
            new Vector3(-1f, 1f, 1f);
    }


    private void Update()
    {
        if (!ActiveGame)
        {
            return;
        }


        // -------------------------
        // TIMER
        // -------------------------

        currentTimer += Time.deltaTime;

        if (currentTimer >= Timer)
        {
            ActiveGame = false;

            StartCoroutine(ShowWinner());

            return;
        }


        // -------------------------
        // PLAYER 1
        // -------------------------

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


        // -------------------------
        // PLAYER 2
        // -------------------------

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


    private IEnumerator ShowWinner()
    {
        P1Animating = false;
        P2Animating = false;


        // -------------------------
        // P1 GANA
        // -------------------------

        if (P1Score > P2Score)
        {
            print("P1 Gana");


            // Sumo ganador feliz
            SumoObject1
                .GetComponent<SpriteRenderer>()
                .sprite = HappySumo;


            // Sumo perdedor triste
            SumoObject2
                .GetComponent<SpriteRenderer>()
                .sprite = SadSumo;


            // Mostrar resultado
            yield return new WaitForSeconds(WinnerPause);


            /*
             * P1 ganó.
             *
             * El perdedor es:
             * CarObject2
             *
             * El sumo ganador es:
             * SumoObject1
             *
             * Ambos salen hacia la DERECHA.
             */

            yield return StartCoroutine(
                ThrowObjects(
                    CarObject2,
                    SumoObject1,
                    Vector3.right
                )
            );
        }


        // -------------------------
        // P2 GANA
        // -------------------------

        else if (P2Score > P1Score)
        {
            print("P2 Gana");


            // Sumo perdedor triste
            SumoObject1
                .GetComponent<SpriteRenderer>()
                .sprite = SadSumo;


            // Sumo ganador feliz
            SumoObject2
                .GetComponent<SpriteRenderer>()
                .sprite = HappySumo;


            yield return new WaitForSeconds(WinnerPause);


            /*
             * P2 ganó.
             *
             * El perdedor es:
             * CarObject
             *
             * El sumo ganador es:
             * SumoObject2
             *
             * Ambos salen hacia la IZQUIERDA.
             */

            yield return StartCoroutine(
                ThrowObjects(
                    CarObject,
                    SumoObject2,
                    Vector3.left
                )
            );
        }


        // -------------------------
        // EMPATE
        // -------------------------

        else
        {
            print("Empate");


            SumoObject1
                .GetComponent<SpriteRenderer>()
                .sprite = SadSumo;

            SumoObject2
                .GetComponent<SpriteRenderer>()
                .sprite = SadSumo;


            yield return new WaitForSeconds(WinnerPause);
        }


        // Esperar después de la animación
        yield return new WaitForSeconds(EndDelay);


        EndMiniGame();
    }


    private IEnumerator ThrowObjects(
        GameObject loserCar,
        GameObject winnerSumo,
        Vector3 direction
    )
    {
        // -------------------------
        // START POSITIONS
        // -------------------------

        Vector3 carStart =
            loserCar.transform.position;

        Vector3 sumoStart =
            winnerSumo.transform.position;


        // -------------------------
        // END POSITIONS
        // -------------------------

        Vector3 carEnd =
            carStart + direction * ThrowDistance;

        Vector3 sumoEnd =
            sumoStart + direction * ThrowDistance;


        Quaternion carStartRotation =
            loserCar.transform.rotation;

        Quaternion sumoStartRotation =
            winnerSumo.transform.rotation;


        float time = 0f;


        while (time < ThrowDuration)
        {
            time += Time.deltaTime;

            float t =
                Mathf.Clamp01(time / ThrowDuration);


            // -------------------------
            // HORIZONTAL MOVEMENT
            // -------------------------

            Vector3 carPosition =
                Vector3.Lerp(
                    carStart,
                    carEnd,
                    t
                );

            Vector3 sumoPosition =
                Vector3.Lerp(
                    sumoStart,
                    sumoEnd,
                    t
                );


            // -------------------------
            // ARC
            // -------------------------

            float arc =
                Mathf.Sin(t * Mathf.PI)
                * ThrowHeight;


            carPosition.y += arc;
            sumoPosition.y += arc;


            loserCar.transform.position =
                carPosition;

            winnerSumo.transform.position =
                sumoPosition;


            // -------------------------
            // ROTATION
            // -------------------------

            float rotation =
                ThrowRotation * t;


            // Hace que giren según hacia qué lado salen
            float rotationDirection =
                direction.x;


            loserCar.transform.rotation =
                carStartRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    rotation * rotationDirection
                );


            winnerSumo.transform.rotation =
                sumoStartRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    rotation * rotationDirection
                );


            yield return null;
        }


        // Asegurar posición final
        loserCar.transform.position = carEnd;
        winnerSumo.transform.position = sumoEnd;
    }


    private IEnumerator PushAnimation(
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


        Vector3 carStart =
            car.transform.position;

        Vector3 sumoStart =
            sumo.transform.position;


        Vector3 carPush =
            carStart + direction * PushDistance;

        Vector3 sumoPush =
            sumoStart + direction * PushDistance;


        // -------------------------
        // PUSH
        // -------------------------

        float time = 0f;


        while (time < PushDuration && ActiveGame)
        {
            time += Time.deltaTime;

            float t =
                Mathf.Clamp01(time / PushDuration);


            car.transform.position =
                Vector3.Lerp(
                    carStart,
                    carPush,
                    t
                );


            sumo.transform.position =
                Vector3.Lerp(
                    sumoStart,
                    sumoPush,
                    t
                );


            yield return null;
        }


        // El minijuego terminó mientras empujaba
        if (!ActiveGame)
        {
            car.transform.position =
                carStart;

            sumo.transform.position =
                sumoStart;

            yield break;
        }


        // -------------------------
        // RETURN
        // -------------------------

        time = 0f;


        while (time < PushDuration && ActiveGame)
        {
            time += Time.deltaTime;

            float t =
                Mathf.Clamp01(time / PushDuration);


            car.transform.position =
                Vector3.Lerp(
                    carPush,
                    carStart,
                    t
                );


            sumo.transform.position =
                Vector3.Lerp(
                    sumoPush,
                    sumoStart,
                    t
                );


            yield return null;
        }


        car.transform.position =
            carStart;

        sumo.transform.position =
            sumoStart;


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