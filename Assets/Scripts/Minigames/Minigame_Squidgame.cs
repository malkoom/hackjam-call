using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;

public class Minigame_Squidgame : AMiniGame
{
    [Header("SFX")]
    public EventReference PoliceScream;

    [Header("Game")]
    public float StartingX;
    public float ObjectiveX;

    public GameObject Police;

    private bool isLooking = false;
    private bool ActiveGame = true;

    public GameObject Alert;

    [Header("Timer")]
    public float alertTime = 0.5f;
    private float currentTimer = 0.0f;

    public float Timer = 1.0f;
    public float LookTimer = 0.5f;

    public Vector2 MinNMaxCoolDown = new Vector2(0.5f, 1f);

    [Header("Players")]
    public GameObject Player1;
    public GameObject Player2;

    private bool P1Death = false;
    private bool P2Death = false;

    public Sprite Explotion;

    [Header("Movement")]
    public float StepSpeed = 0.5f;

    [Header("Step Juice")]
    public float StepSquash = 0.15f;
    public float StepRotation = 8f;
    public float StepAnimationTime = 0.12f;

    [Header("Alert Juice")]
    public float AlertPulseSpeed = 12f;
    public float AlertPulseAmount = 0.2f;

    private Vector3 alertOriginalScale;

    [Header("Police Juice")]
    public float PoliceSquashAmount = 0.25f;
    public float PoliceTurnTime = 0.12f;

    private Vector3 policeOriginalScale;

    [Header("Death Juice")]
    public float HitStopDuration = 0.08f;

    public float DeathScale = 1.8f;
    public float DeathRotation = 180f;
    public float DeathAnimationTime = 0.25f;

    [Header("Camera Shake")]
    public float ShakeDuration = 0.15f;
    public float ShakeStrength = 0.15f;

    private Camera mainCamera;

    void Start()
    {
        InitMiniGame();
    }

    public override void InitMiniGame()
    {
        Alert.SetActive(false);

        alertOriginalScale = Alert.transform.localScale;
        policeOriginalScale = Police.transform.localScale;

        mainCamera = Camera.main;

        GameObject[] PlayerArray =
        {
            Player1,
            Player2
        };

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
            return;

        HandleTimer();

        HandleInputs();

        CheckWinner();
    }

    void HandleTimer()
    {
        currentTimer += Time.deltaTime;

        // -------------------------
        // WARNING
        // -------------------------

        if (currentTimer >= Timer - alertTime && !isLooking)
        {
            if (!Alert.activeSelf)
            {
                Alert.SetActive(true);
            }

            JuiceAlert();
        }

        // -------------------------
        // POLICE LOOKS
        // -------------------------

        if (currentTimer >= Timer && !isLooking)
        {
            RuntimeManager.PlayOneShot(
                PoliceScream,
                transform.position
            );

            Alert.SetActive(false);

            Alert.transform.localScale =
                alertOriginalScale;

            currentTimer = 0.0f;

            isLooking = true;

            StartCoroutine(
                PoliceTurnAnimation(-1)
            );

            Timer = Random.Range(
                MinNMaxCoolDown.x,
                MinNMaxCoolDown.y
            );
        }

        // -------------------------
        // POLICE LOOKS AWAY
        // -------------------------

        if (currentTimer >= LookTimer && isLooking)
        {
            currentTimer = 0.0f;

            isLooking = false;

            StartCoroutine(
                PoliceTurnAnimation(1)
            );

            LookTimer = Random.Range(
                MinNMaxCoolDown.x,
                MinNMaxCoolDown.y
            );
        }
    }

    void HandleInputs()
    {
        // -------------------------
        // PLAYER 1
        // -------------------------

        if (
            Keyboard.current.wKey.wasPressedThisFrame
            && !P1Death
        )
        {
            MovePlayer(Player1);

            StartCoroutine(
                StepAnimation(Player1)
            );

            if (isLooking)
            {
                P1Death = true;

                StartCoroutine(
                    KillPlayer(Player1)
                );
            }
        }

        // -------------------------
        // PLAYER 2
        // -------------------------

        if (
            Keyboard.current.upArrowKey.wasPressedThisFrame
            && !P2Death
        )
        {
            MovePlayer(Player2);

            StartCoroutine(
                StepAnimation(Player2)
            );

            if (isLooking)
            {
                P2Death = true;

                StartCoroutine(
                    KillPlayer(Player2)
                );
            }
        }
    }

    void MovePlayer(GameObject player)
    {
        Vector3 tempPos =
            player.transform.position;

        tempPos.x += StepSpeed;

        player.transform.position =
            tempPos;
    }

    IEnumerator StepAnimation(
        GameObject player
    )
    {
        Transform playerTransform =
            player.transform;

        Vector3 originalScale =
            playerTransform.localScale;

        Quaternion originalRotation =
            playerTransform.localRotation;

        Vector3 squashScale =
            new Vector3(
                originalScale.x * (1f + StepSquash),
                originalScale.y * (1f - StepSquash),
                originalScale.z
            );

        float randomRotation =
            Random.Range(
                -StepRotation,
                StepRotation
            );

        Quaternion targetRotation =
            Quaternion.Euler(
                0,
                0,
                randomRotation
            );

        float timer = 0f;

        float halfTime =
            StepAnimationTime / 2f;

        // Squash
        while (timer < halfTime)
        {
            timer += Time.deltaTime;

            float t =
                timer / halfTime;

            playerTransform.localScale =
                Vector3.Lerp(
                    originalScale,
                    squashScale,
                    t
                );

            playerTransform.localRotation =
                Quaternion.Lerp(
                    originalRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        timer = 0f;

        // Return
        while (timer < halfTime)
        {
            timer += Time.deltaTime;

            float t =
                timer / halfTime;

            playerTransform.localScale =
                Vector3.Lerp(
                    squashScale,
                    originalScale,
                    t
                );

            playerTransform.localRotation =
                Quaternion.Lerp(
                    targetRotation,
                    originalRotation,
                    t
                );

            yield return null;
        }

        playerTransform.localScale =
            originalScale;

        playerTransform.localRotation =
            originalRotation;
    }

    void JuiceAlert()
    {
        float pulse =
            1f +
            Mathf.Sin(
                Time.time * AlertPulseSpeed
            )
            * AlertPulseAmount;

        Alert.transform.localScale =
            alertOriginalScale * pulse;
    }

    IEnumerator PoliceTurnAnimation(
        float direction
    )
    {
        Transform police =
            Police.transform;

        Vector3 targetScale =
            policeOriginalScale;

        targetScale.x =
            Mathf.Abs(policeOriginalScale.x)
            * direction;

        Vector3 squashScale =
            new Vector3(
                policeOriginalScale.x
                    * (1f + PoliceSquashAmount),
                policeOriginalScale.y
                    * (1f - PoliceSquashAmount),
                policeOriginalScale.z
            );

        float timer = 0f;

        float half =
            PoliceTurnTime / 2f;

        Vector3 startScale =
            police.localScale;

        // Squash before turning
        while (timer < half)
        {
            timer += Time.deltaTime;

            float t =
                timer / half;

            police.localScale =
                Vector3.Lerp(
                    startScale,
                    squashScale,
                    t
                );

            yield return null;
        }

        timer = 0f;

        // Turn + recover
        while (timer < half)
        {
            timer += Time.deltaTime;

            float t =
                timer / half;

            police.localScale =
                Vector3.Lerp(
                    squashScale,
                    targetScale,
                    t
                );

            yield return null;
        }

        police.localScale =
            targetScale;
    }

    IEnumerator KillPlayer(
        GameObject player
    )
    {
        // -------------------------
        // HIT STOP
        // -------------------------

        float oldTimeScale =
            Time.timeScale;

        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(
            HitStopDuration
        );

        Time.timeScale =
            oldTimeScale;

        // -------------------------
        // EXPLOSION
        // -------------------------

        SpriteRenderer renderer =
            player.GetComponent<SpriteRenderer>();

        renderer.sprite =
            Explotion;

        StartCoroutine(
            CameraShake()
        );

        Transform playerTransform =
            player.transform;

        Vector3 startScale =
            playerTransform.localScale;

        Vector3 targetScale =
            startScale * DeathScale;

        Quaternion startRotation =
            playerTransform.rotation;

        Quaternion targetRotation =
            startRotation
            * Quaternion.Euler(
                0,
                0,
                DeathRotation
            );

        float timer = 0f;

        while (
            timer < DeathAnimationTime
        )
        {
            timer += Time.deltaTime;

            float t =
                timer / DeathAnimationTime;

            // EaseOut
            float smoothT =
                1f -
                Mathf.Pow(
                    1f - t,
                    3f
                );

            playerTransform.localScale =
                Vector3.Lerp(
                    startScale,
                    targetScale,
                    smoothT
                );

            playerTransform.rotation =
                Quaternion.Lerp(
                    startRotation,
                    targetRotation,
                    smoothT
                );

            yield return null;
        }
    }

    IEnumerator CameraShake()
    {
        if (mainCamera == null)
            yield break;

        Transform cameraTransform =
            mainCamera.transform;

        Vector3 originalPosition =
            cameraTransform.localPosition;

        float timer = 0f;

        while (
            timer < ShakeDuration
        )
        {
            timer += Time.deltaTime;

            float strength =
                Mathf.Lerp(
                    ShakeStrength,
                    0f,
                    timer / ShakeDuration
                );

            Vector2 shake =
                Random.insideUnitCircle
                * strength;

            cameraTransform.localPosition =
                originalPosition
                + new Vector3(
                    shake.x,
                    shake.y,
                    0
                );

            yield return null;
        }

        cameraTransform.localPosition =
            originalPosition;
    }

    void CheckWinner()
    {
        // -------------------------
        // REACHED FINISH
        // -------------------------

        if (
            Player1.transform.position.x >= ObjectiveX
            ||
            Player2.transform.position.x >= ObjectiveX
        )
        {
            ActiveGame = false;

            if (
                Player2.transform.position.x
                >= ObjectiveX
            )
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

        // -------------------------
        // PLAYER DIED
        // -------------------------

        if (P1Death || P2Death)
        {
            ActiveGame = false;

            if (P1Death)
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
    }

    public override void EndMiniGame()
    {
        Debug.Log("End");

        ReturnToMiddleScene();
    }
}