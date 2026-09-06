using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;

public class MiniGame1 : AMiniGame
{
    [Header("Sounds")]
    public EventReference Bong;
    public EventReference Punch;
    public EventReference TapSFX;

    [Header("Players")]
    public GameObject CarPrefab;
    public GameObject CarPrefab2;

    private GameObject CarObject;
    private GameObject CarObject2;

    [Header("Sumos")]
    public GameObject SumoPrefab;

    private GameObject SumoObject1;
    private GameObject SumoObject2;

    public Sprite HappySumo;
    public Sprite SadSumo;

    [Header("Buttons")]
    public Transform P1Button;
    public Transform P2Button;

    public float ButtonIdleSpeed = 4f;
    public float ButtonIdleAmount = 0.05f;

    public float ButtonSquash = 0.7f;
    public float ButtonPunch = 1.3f;
    public float ButtonPressDuration = 0.1f;
    public float ButtonRotation = 8f;

    private Vector3 p1ButtonScale;
    private Vector3 p2ButtonScale;

    private Quaternion p1ButtonRotation;
    private Quaternion p2ButtonRotation;

    private Coroutine p1ButtonCoroutine;
    private Coroutine p2ButtonCoroutine;

    [Header("Time")]
    public float Timer = 3f;
    public float WinnerPause = 0.5f;
    public float EndDelay = 0.6f;

    private float currentTimer = 0f;

    [Header("Push Animation")]
    public float PushDistance = 0.15f;
    public float PushDuration = 0.08f;

    public float PushScaleX = 1.15f;
    public float PushScaleY = 0.85f;

    public float PushRotation = 4f;

    [Header("Throw Animation")]
    public float ThrowDistance = 6f;
    public float ThrowHeight = 2f;
    public float ThrowDuration = 0.8f;
    public float ThrowRotation = 720f;

    [Header("Final Impact")]
    public float HitStopDuration = 0.08f;

    public float FinalShakeDuration = 0.3f;
    public float FinalShakeStrength = 0.2f;

    [Header("Particles")]
    public ParticleSystem P1PushParticles;
    public ParticleSystem P2PushParticles;

    [Header("Score Advantage Juice")]
    public Transform CenterMarker;

    public float MarkerMaxDistance = 0.75f;
    public float MarkerSmoothSpeed = 8f;

    private Vector3 markerStartPosition;

    [Header("Camera")]
    private Camera mainCamera;

    public Player Player;

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

    public override void InitMiniGame()
    {
        RuntimeManager.PlayOneShot(
            Bong,
            transform.position
        );

        ActiveGame = true;

        currentTimer = 0f;

        P1Score = 0;
        P2Score = 0;

        mainCamera = Camera.main;

        // -------------------------
        // BUTTON SETUP
        // -------------------------

        if (P1Button != null)
        {
            p1ButtonScale =
                P1Button.localScale;

            p1ButtonRotation =
                P1Button.localRotation;
        }

        if (P2Button != null)
        {
            p2ButtonScale =
                P2Button.localScale;

            p2ButtonRotation =
                P2Button.localRotation;
        }

        // -------------------------
        // MARKER
        // -------------------------

        if (CenterMarker != null)
        {
            markerStartPosition =
                CenterMarker.localPosition;
        }

        // -------------------------
        // CREATE CARS
        // -------------------------

        CarObject =
            Instantiate(
                CarPrefab,
                transform
            );

        CarObject2 =
            Instantiate(
                CarPrefab2,
                transform
            );

        // -------------------------
        // CREATE SUMOS
        // -------------------------

        SumoObject1 =
            Instantiate(
                SumoPrefab,
                transform
            );

        SumoObject2 =
            Instantiate(
                SumoPrefab,
                transform
            );

        // -------------------------
        // POSITIONS
        // -------------------------

        CarObject.transform.position =
            new Vector3(
                -1.5f,
                1f - 0.159f,
                -8f
            );

        CarObject2.transform.position =
            new Vector3(
                1.5f,
                1f - 0.159f,
                -8f
            );

        CarObject
            .GetComponent<SpriteRenderer>()
            .sprite =
            GameManager.Singleton
                .player1
                .Skin;

        CarObject2
            .GetComponent<SpriteRenderer>()
            .sprite =
            GameManager.Singleton
                .player2
                .Skin;

        SumoObject1.transform.position =
            new Vector3(
                -1f,
                1f,
                -8f
            );

        SumoObject2.transform.position =
            new Vector3(
                1f,
                1f,
                -8f
            );

        SumoObject2.transform.localScale =
            new Vector3(
                -1f,
                1f,
                1f
            );
    }

    private void Update()
    {
        if (!ActiveGame)
        {
            return;
        }

        currentTimer +=
            Time.deltaTime;

        HandleButtonIdleJuice();
        UpdateCenterMarker();

        if (currentTimer >= Timer)
        {
            ActiveGame = false;

            StartCoroutine(
                ShowWinner()
            );

            return;
        }

        // -------------------------
        // PLAYER 1
        // -------------------------

        if (
            Keyboard.current
                .wKey
                .wasPressedThisFrame
        )
        {
            P1Score++;

            if (!TapSFX.IsNull)
            {
                RuntimeManager.PlayOneShot(
                    TapSFX,
                    CarObject.transform.position
                );
            }

            if (P1PushParticles != null)
            {
                P1PushParticles.Play();
            }

            PressButton(1);

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
        }

        // -------------------------
        // PLAYER 2
        // -------------------------

        if (
            Keyboard.current
                .upArrowKey
                .wasPressedThisFrame
        )
        {
            P2Score++;

            if (!TapSFX.IsNull)
            {
                RuntimeManager.PlayOneShot(
                    TapSFX,
                    CarObject2.transform.position
                );
            }

            if (P2PushParticles != null)
            {
                P2PushParticles.Play();
            }

            PressButton(2);

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
        }
    }

    // =====================================================
    // BUTTON IDLE
    // =====================================================

    private void HandleButtonIdleJuice()
    {
        if (
            P1Button != null &&
            p1ButtonCoroutine == null
        )
        {
            float pulse =
                1f +
                Mathf.Sin(
                    Time.time *
                    ButtonIdleSpeed
                ) *
                ButtonIdleAmount;

            P1Button.localScale =
                Vector3.Lerp(
                    P1Button.localScale,
                    p1ButtonScale * pulse,
                    Time.deltaTime * 10f
                );
        }

        if (
            P2Button != null &&
            p2ButtonCoroutine == null
        )
        {
            float pulse =
                1f +
                Mathf.Sin(
                    Time.time *
                    ButtonIdleSpeed +
                    1.5f
                ) *
                ButtonIdleAmount;

            P2Button.localScale =
                Vector3.Lerp(
                    P2Button.localScale,
                    p2ButtonScale * pulse,
                    Time.deltaTime * 10f
                );
        }
    }

    // =====================================================
    // BUTTON PRESS
    // =====================================================

    private void PressButton(
        int player
    )
    {
        if (player == 1)
        {
            if (P1Button == null)
                return;

            if (p1ButtonCoroutine != null)
            {
                StopCoroutine(
                    p1ButtonCoroutine
                );
            }

            p1ButtonCoroutine =
                StartCoroutine(
                    ButtonPressAnimation(
                        P1Button,
                        p1ButtonScale,
                        p1ButtonRotation,
                        1
                    )
                );
        }
        else
        {
            if (P2Button == null)
                return;

            if (p2ButtonCoroutine != null)
            {
                StopCoroutine(
                    p2ButtonCoroutine
                );
            }

            p2ButtonCoroutine =
                StartCoroutine(
                    ButtonPressAnimation(
                        P2Button,
                        p2ButtonScale,
                        p2ButtonRotation,
                        2
                    )
                );
        }
    }

    private IEnumerator ButtonPressAnimation(
        Transform button,
        Vector3 baseScale,
        Quaternion baseRotation,
        int player
    )
    {
        Vector3 squash =
            new Vector3(
                baseScale.x * 1.15f,
                baseScale.y * ButtonSquash,
                baseScale.z
            );

        Vector3 punch =
            baseScale *
            ButtonPunch;

        float randomRotation =
            Random.value > 0.5f
                ? ButtonRotation
                : -ButtonRotation;

        Quaternion rotated =
            baseRotation *
            Quaternion.Euler(
                0f,
                0f,
                randomRotation
            );

        float partTime =
            ButtonPressDuration /
            3f;

        float time = 0f;

        // -------------------------
        // SQUASH
        // -------------------------

        while (
            time < partTime
        )
        {
            time +=
                Time.deltaTime;

            float t =
                time /
                partTime;

            button.localScale =
                Vector3.Lerp(
                    baseScale,
                    squash,
                    t
                );

            yield return null;
        }

        // -------------------------
        // POP
        // -------------------------

        time = 0f;

        while (
            time < partTime
        )
        {
            time +=
                Time.deltaTime;

            float t =
                time /
                partTime;

            float smooth =
                Mathf.Sin(
                    t *
                    Mathf.PI *
                    0.5f
                );

            button.localScale =
                Vector3.Lerp(
                    squash,
                    punch,
                    smooth
                );

            button.localRotation =
                Quaternion.Lerp(
                    baseRotation,
                    rotated,
                    smooth
                );

            yield return null;
        }

        // -------------------------
        // RETURN
        // -------------------------

        time = 0f;

        while (
            time < partTime
        )
        {
            time +=
                Time.deltaTime;

            float t =
                time /
                partTime;

            button.localScale =
                Vector3.Lerp(
                    punch,
                    baseScale,
                    t
                );

            button.localRotation =
                Quaternion.Lerp(
                    rotated,
                    baseRotation,
                    t
                );

            yield return null;
        }

        button.localScale =
            baseScale;

        button.localRotation =
            baseRotation;

        if (player == 1)
        {
            p1ButtonCoroutine = null;
        }
        else
        {
            p2ButtonCoroutine = null;
        }
    }

    // =====================================================
    // CENTER MARKER
    // =====================================================

    private void UpdateCenterMarker()
    {
        if (CenterMarker == null)
            return;

        float difference =
            P1Score - P2Score;

        float normalized =
            Mathf.Clamp(
                difference / 20f,
                -1f,
                1f
            );

        Vector3 target =
            markerStartPosition +
            Vector3.right *
            normalized *
            MarkerMaxDistance;

        CenterMarker.localPosition =
            Vector3.Lerp(
                CenterMarker.localPosition,
                target,
                Time.deltaTime *
                MarkerSmoothSpeed
            );
    }

    // =====================================================
    // WINNER
    // =====================================================

    private IEnumerator ShowWinner()
    {
        P1Animating = false;
        P2Animating = false;

        yield return
            StartCoroutine(
                FinalHitStop()
            );

        if (P1Score > P2Score)
        {
            NotifyWinner(1);

            SumoObject1
                .GetComponent<SpriteRenderer>()
                .sprite =
                HappySumo;

            SumoObject2
                .GetComponent<SpriteRenderer>()
                .sprite =
                SadSumo;

            yield return
                new WaitForSeconds(
                    WinnerPause
                );

            StartCoroutine(
                CameraShake(
                    FinalShakeDuration,
                    FinalShakeStrength
                )
            );

            yield return
                StartCoroutine(
                    ThrowObjects(
                        CarObject2,
                        SumoObject1,
                        Vector3.right
                    )
                );
        }
        else if (
            P2Score > P1Score
        )
        {
            NotifyWinner(2);

            SumoObject1
                .GetComponent<SpriteRenderer>()
                .sprite =
                SadSumo;

            SumoObject2
                .GetComponent<SpriteRenderer>()
                .sprite =
                HappySumo;

            yield return
                new WaitForSeconds(
                    WinnerPause
                );

            StartCoroutine(
                CameraShake(
                    FinalShakeDuration,
                    FinalShakeStrength
                )
            );

            yield return
                StartCoroutine(
                    ThrowObjects(
                        CarObject,
                        SumoObject2,
                        Vector3.left
                    )
                );
        }
        else
        {
            NotifyWinner(1);

            SumoObject1
                .GetComponent<SpriteRenderer>()
                .sprite =
                SadSumo;

            SumoObject2
                .GetComponent<SpriteRenderer>()
                .sprite =
                SadSumo;

            yield return
                new WaitForSeconds(
                    WinnerPause
                );
        }

        yield return
            new WaitForSeconds(
                EndDelay
            );

        EndMiniGame();
    }

    // =====================================================
    // HITSTOP
    // =====================================================

    private IEnumerator FinalHitStop()
    {
        float oldScale =
            Time.timeScale;

        Time.timeScale = 0f;

        yield return
            new WaitForSecondsRealtime(
                HitStopDuration
            );

        Time.timeScale =
            oldScale;
    }

    // =====================================================
    // THROW
    // =====================================================

    private IEnumerator ThrowObjects(
        GameObject loserCar,
        GameObject winnerSumo,
        Vector3 direction
    )
    {
        RuntimeManager.PlayOneShot(
            Punch,
            transform.position
        );

        Vector3 carStart =
            loserCar.transform.position;

        Vector3 sumoStart =
            winnerSumo.transform.position;

        Quaternion carStartRotation =
            loserCar.transform.rotation;

        Quaternion sumoStartRotation =
            winnerSumo.transform.rotation;

        Vector3 carEnd =
            carStart +
            direction *
            ThrowDistance;

        Vector3 sumoEnd =
            sumoStart +
            direction *
            ThrowDistance;

        float time = 0f;

        while (
            time < ThrowDuration
        )
        {
            time +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    time /
                    ThrowDuration
                );

            float ease =
                1f -
                Mathf.Pow(
                    1f - t,
                    3f
                );

            Vector3 carPosition =
                Vector3.Lerp(
                    carStart,
                    carEnd,
                    ease
                );

            Vector3 sumoPosition =
                Vector3.Lerp(
                    sumoStart,
                    sumoEnd,
                    ease
                );

            float arc =
                Mathf.Sin(
                    t *
                    Mathf.PI
                ) *
                ThrowHeight;

            carPosition.y += arc;
            sumoPosition.y += arc;

            loserCar.transform.position =
                carPosition;

            winnerSumo.transform.position =
                sumoPosition;

            float rotation =
                ThrowRotation *
                t;

            float rotationDirection =
                direction.x;

            loserCar.transform.rotation =
                carStartRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    rotation *
                    rotationDirection
                );

            winnerSumo.transform.rotation =
                sumoStartRotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    rotation *
                    rotationDirection
                );

            yield return null;
        }

        loserCar.transform.position =
            carEnd;

        winnerSumo.transform.position =
            sumoEnd;
    }

    // =====================================================
    // PUSH
    // =====================================================

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

        Vector3 carScaleStart =
            car.transform.localScale;

        Vector3 sumoScaleStart =
            sumo.transform.localScale;

        Quaternion carRotationStart =
            car.transform.localRotation;

        Quaternion sumoRotationStart =
            sumo.transform.localRotation;

        Vector3 carPush =
            carStart +
            direction *
            PushDistance;

        Vector3 sumoPush =
            sumoStart +
            direction *
            PushDistance;

        Vector3 carSquash =
            new Vector3(
                carScaleStart.x *
                PushScaleX,

                carScaleStart.y *
                PushScaleY,

                carScaleStart.z
            );

        Vector3 sumoSquash =
            new Vector3(
                sumoScaleStart.x *
                PushScaleX,

                sumoScaleStart.y *
                PushScaleY,

                sumoScaleStart.z
            );

        Quaternion carPushRotation =
            carRotationStart *
            Quaternion.Euler(
                0f,
                0f,
                -PushRotation *
                direction.x
            );

        Quaternion sumoPushRotation =
            sumoRotationStart *
            Quaternion.Euler(
                0f,
                0f,
                PushRotation *
                direction.x
            );

        float time = 0f;

        // -------------------------
        // PUSH
        // -------------------------

        while (
            time < PushDuration &&
            ActiveGame
        )
        {
            time +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    time /
                    PushDuration
                );

            float smooth =
                Mathf.Sin(
                    t *
                    Mathf.PI *
                    0.5f
                );

            car.transform.position =
                Vector3.Lerp(
                    carStart,
                    carPush,
                    smooth
                );

            sumo.transform.position =
                Vector3.Lerp(
                    sumoStart,
                    sumoPush,
                    smooth
                );

            car.transform.localScale =
                Vector3.Lerp(
                    carScaleStart,
                    carSquash,
                    smooth
                );

            sumo.transform.localScale =
                Vector3.Lerp(
                    sumoScaleStart,
                    sumoSquash,
                    smooth
                );

            car.transform.localRotation =
                Quaternion.Lerp(
                    carRotationStart,
                    carPushRotation,
                    smooth
                );

            sumo.transform.localRotation =
                Quaternion.Lerp(
                    sumoRotationStart,
                    sumoPushRotation,
                    smooth
                );

            yield return null;
        }

        if (!ActiveGame)
        {
            ResetPushTransforms(
                car,
                sumo,
                carStart,
                sumoStart,
                carScaleStart,
                sumoScaleStart,
                carRotationStart,
                sumoRotationStart
            );

            yield break;
        }

        // -------------------------
        // RETURN
        // -------------------------

        time = 0f;

        while (
            time < PushDuration &&
            ActiveGame
        )
        {
            time +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    time /
                    PushDuration
                );

            float smooth =
                1f -
                Mathf.Pow(
                    1f - t,
                    3f
                );

            car.transform.position =
                Vector3.Lerp(
                    carPush,
                    carStart,
                    smooth
                );

            sumo.transform.position =
                Vector3.Lerp(
                    sumoPush,
                    sumoStart,
                    smooth
                );

            car.transform.localScale =
                Vector3.Lerp(
                    carSquash,
                    carScaleStart,
                    smooth
                );

            sumo.transform.localScale =
                Vector3.Lerp(
                    sumoSquash,
                    sumoScaleStart,
                    smooth
                );

            car.transform.localRotation =
                Quaternion.Lerp(
                    carPushRotation,
                    carRotationStart,
                    smooth
                );

            sumo.transform.localRotation =
                Quaternion.Lerp(
                    sumoPushRotation,
                    sumoRotationStart,
                    smooth
                );

            yield return null;
        }

        ResetPushTransforms(
            car,
            sumo,
            carStart,
            sumoStart,
            carScaleStart,
            sumoScaleStart,
            carRotationStart,
            sumoRotationStart
        );

        if (player == 1)
        {
            P1Animating = false;
        }
        else
        {
            P2Animating = false;
        }
    }

    private void ResetPushTransforms(
        GameObject car,
        GameObject sumo,
        Vector3 carPosition,
        Vector3 sumoPosition,
        Vector3 carScale,
        Vector3 sumoScale,
        Quaternion carRotation,
        Quaternion sumoRotation
    )
    {
        car.transform.position =
            carPosition;

        sumo.transform.position =
            sumoPosition;

        car.transform.localScale =
            carScale;

        sumo.transform.localScale =
            sumoScale;

        car.transform.localRotation =
            carRotation;

        sumo.transform.localRotation =
            sumoRotation;
    }

    // =====================================================
    // CAMERA SHAKE
    // =====================================================

    private IEnumerator CameraShake(
        float duration,
        float strength
    )
    {
        if (mainCamera == null)
            yield break;

        Transform cameraTransform =
            mainCamera.transform;

        Vector3 originalPosition =
            cameraTransform.localPosition;

        float time = 0f;

        while (
            time < duration
        )
        {
            time +=
                Time.deltaTime;

            float currentStrength =
                Mathf.Lerp(
                    strength,
                    0f,
                    time / duration
                );

            Vector2 shake =
                Random.insideUnitCircle *
                currentStrength;

            cameraTransform.localPosition =
                originalPosition +
                new Vector3(
                    shake.x,
                    shake.y,
                    0f
                );

            yield return null;
        }

        cameraTransform.localPosition =
            originalPosition;
    }

    // =====================================================
    // END
    // =====================================================

    public override void EndMiniGame()
    {
        print("Fin");

        StopAllCoroutines();

        Destroy(CarObject);
        Destroy(CarObject2);

        Destroy(SumoObject1);
        Destroy(SumoObject2);

        ReturnToMiddleScene();
    }
}