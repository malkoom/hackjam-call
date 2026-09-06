using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class SplineCarController : MonoBehaviour
{
    public enum Team
    {
        Blue_Arrows,
        Red_WASD,
    }

    [Header("Configuración del Jugador")]
    public Team team = Team.Blue_Arrows;
    public bool autoSplitScreen = true;

    [Header("Referencia al Spline")]
    public SplineContainer splineContainer;

    [Header("Modelo Visual (Hijo)")]
    [Tooltip(
        "Arrastra aquí el GameObject hijo donde está la malla real del coche. Evita que reaparezca la malla del objeto padre (ej. cubo viejo)."
    )]
    public GameObject visualModel;

    [Header("Checkpoints (Por Knots del Spline)")]
    public List<int> checkpointKnots = new List<int> { 0 };

    [Header("Reaparición y Parpadeo")]
    public float respawnDelay = 2.5f;
    public float blinkDuration = 1.5f;
    public float blinkInterval = 0.12f;

    [Header("Velocidad y Físicas")]
    [Range(0.1f, 3f)]
    public float globalSpeedMultiplier = 1f;
    public float maxSpeed = 2f;
    public float acceleration = 1.2f;
    public float brakeStrength = 3.5f;
    public float friction = 0.5f;

    [Header("Aviso Visual: Inclinación")]
    public float maxTiltAngle = 28f;

    [Range(0.1f, 0.9f)]
    public float tiltWarningThreshold = 0.35f;
    public float tiltSmoothSpeed = 8f;
    public bool invertTiltDirection = false;

    [Header("Agarre y Descarrilamiento")]
    public float maxCorneringGrip = 2.5f;
    public float derailReactionTime = 0.35f;
    public float lookAheadDistance = 1.5f;
    public float derailOutwardForce = 0.4f;
    public float derailUpwardForce = 1.0f;
    public float derailSpinSpeed = 3.5f;

    [Header("Cámara")]
    public Camera carCamera;

    [Header("Comportamiento General")]
    public bool isLoop = true;
    public float rotationSmoothSpeed = 12f;
    public Vector3 modelRotationOffset = Vector3.zero;

    // Estado interno
    private float currentSpeed = 0f;
    private float progress = 0f;
    private float splineLength = 0f;
    private bool isDerailed = false;
    private bool isFinished = false;
    private int currentLap = 1;
    private float currentTilt = 0f;
    private float overGripTimer = 0f;

    private Rigidbody rb;

    private struct CheckpointInfo
    {
        public int knotIndex;
        public float progress;
    }

    private List<CheckpointInfo> cachedCheckpoints = new List<CheckpointInfo>();
    private float lastCheckpointProgress = 0f;

    // Renderers exclusivos de la malla hija
    private Renderer[] carRenderers;

    private Transform originalCameraParent;
    private Vector3 originalCameraLocalPos;
    private Quaternion originalCameraLocalRot;
    private Vector3 cameraWorldOffsetOnDerail;
    private float frozenCameraX = 0f;
    private float frozenCameraY = 0f;
    private float frozenCameraZ = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        // Comprobación segura: no da error si se inicia la escena de forma aislada
        if (GameManager.Singleton != null)
        {
            var p1 = GameManager.Singleton.player1;
            var p2 = GameManager.Singleton.player2;
            if (team == Team.Red_WASD && p1 != null)
            {
                maxSpeed += p1.speed;
                acceleration += p1.acceleration;
            }
            else if (team == Team.Blue_Arrows && p2 != null)
            {
                maxSpeed += p2.speed;
                acceleration += p2.acceleration;
            }
        }
        SetupVisualRenderers();
        SetupCamera();
        InitCheckpoints();
        if (splineContainer != null)
        {
            splineLength = splineContainer.CalculateLength();
            progress = lastCheckpointProgress;
            // ALINEACIÓN EXACTA Y PERFECTA EN LA SALIDA (sin interpolación lenta)
            UpdateTransformOnSpline(snapImmediate: true);
        }
        else
        {
            Debug.LogError($"¡[{gameObject.name}] Falta asignar el SplineContainer!", this);
        }
    }

    private void SetupVisualRenderers()
    {
        // Si el usuario asignó el hijo donde está el modelo real
        if (visualModel != null)
        {
            carRenderers = visualModel.GetComponentsInChildren<Renderer>(true);
        }
        else
        {
            // Auto-filtro: Tomar solo los renderers hijos y excluir el del objeto padre
            Renderer rootRenderer = GetComponent<Renderer>();
            List<Renderer> validRenderers = new List<Renderer>();

            foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            {
                if (r != rootRenderer) // Ignorar la malla del padre donde está puesto el script
                {
                    validRenderers.Add(r);
                }
            }

            if (validRenderers.Count > 0)
                carRenderers = validRenderers.ToArray();
            else
                carRenderers = GetComponentsInChildren<Renderer>(true);
        }
    }

    private void SetupCamera()
    {
        if (carCamera == null)
            carCamera = GetComponentInChildren<Camera>(true);

        if (carCamera != null)
        {
            originalCameraParent = carCamera.transform.parent;
            originalCameraLocalPos = carCamera.transform.localPosition;
            originalCameraLocalRot = carCamera.transform.localRotation;

            if (autoSplitScreen)
            {
                if (team == Team.Red_WASD)
                {
                    carCamera.rect = new Rect(0f, 0f, 0.5f, 1f);
                }
                else
                {
                    carCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
                    AudioListener listener = carCamera.GetComponent<AudioListener>();
                    if (listener != null)
                        listener.enabled = false;
                }
            }
        }
    }

    private void InitCheckpoints()
    {
        cachedCheckpoints.Clear();
        if (splineContainer == null || splineContainer.Spline == null)
            return;

        int totalKnots = splineContainer.Spline.Count;
        if (checkpointKnots == null || checkpointKnots.Count == 0)
        {
            checkpointKnots = new List<int> { 0 };
        }

        foreach (int knotIndex in checkpointKnots)
        {
            if (knotIndex >= 0 && knotIndex < totalKnots)
            {
                float normT = splineContainer.Spline.ConvertIndexUnit(
                    knotIndex,
                    PathIndexUnit.Knot,
                    PathIndexUnit.Normalized
                );
                cachedCheckpoints.Add(
                    new CheckpointInfo { knotIndex = knotIndex, progress = normT }
                );
            }
        }

        cachedCheckpoints.Sort((a, b) => a.progress.CompareTo(b.progress));

        if (cachedCheckpoints.Count > 0)
        {
            lastCheckpointProgress = cachedCheckpoints[0].progress;
        }
    }

    void Update()
    {
        if (isDerailed || isFinished || splineContainer == null || splineLength <= 0f)
            return;

        if (RaceManager.isRaceOver)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, friction * 2f * Time.deltaTime);
            AdvanceProgress();
            UpdateTransformOnSpline(snapImmediate: false);
            return;
        }

        // Antes y durante la cuenta atrás: fijar posición y orientación perfecta
        if (!RaceManager.isRaceStarted)
        {
            currentSpeed = 0f;
            UpdateTransformOnSpline(snapImmediate: true);
            return;
        }

        HandleInput();
        CheckCurveAndTilt();
        AdvanceProgress();
        UpdateCheckpoints();
        UpdateTransformOnSpline(snapImmediate: false);
    }

    private void HandleInput()
    {
        if (Keyboard.current == null)
            return;

        bool isAccelerating = false;
        bool isBraking = false;

        if (team == Team.Blue_Arrows)
        {
            isAccelerating =
                Keyboard.current.rightArrowKey.isPressed || Keyboard.current.upArrowKey.isPressed;
            isBraking =
                Keyboard.current.leftArrowKey.isPressed || Keyboard.current.downArrowKey.isPressed;
        }
        else
        {
            isAccelerating = Keyboard.current.dKey.isPressed || Keyboard.current.wKey.isPressed;
            isBraking = Keyboard.current.aKey.isPressed || Keyboard.current.sKey.isPressed;
        }

        float targetMaxSpeed = maxSpeed * globalSpeedMultiplier;

        if (isAccelerating)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetMaxSpeed,
                acceleration * globalSpeedMultiplier * Time.deltaTime
            );
        }
        else if (isBraking)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeStrength * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, friction * Time.deltaTime);
        }
    }

    private void CheckCurveAndTilt()
    {
        if (currentSpeed < 0.2f)
        {
            currentTilt = Mathf.Lerp(currentTilt, 0f, Time.deltaTime * tiltSmoothSpeed);
            overGripTimer = 0f;
            return;
        }

        splineContainer.Evaluate(
            progress,
            out _,
            out float3 currentTangentF3,
            out float3 worldUpF3
        );
        Vector3 currentTangent = (Vector3)currentTangentF3;
        Vector3 up = (Vector3)worldUpF3;

        float forwardDelta = lookAheadDistance / splineLength;
        float nextProgress = progress + forwardDelta;
        if (isLoop && nextProgress >= 1f)
            nextProgress -= 1f;

        splineContainer.Evaluate(
            Mathf.Clamp01(nextProgress),
            out _,
            out float3 futureTangentF3,
            out _
        );
        Vector3 futureTangent = (Vector3)futureTangentF3;

        float turnAngle = Vector3.Angle(currentTangent, futureTangent);
        float turnAngleRad = turnAngle * Mathf.Deg2Rad;

        float lateralAcceleration =
            (currentSpeed * currentSpeed) * (turnAngleRad / lookAheadDistance);
        float turnSign = Vector3.SignedAngle(currentTangent, futureTangent, up);

        float gripRatio = lateralAcceleration / maxCorneringGrip;
        float targetTilt = 0f;

        if (gripRatio > tiltWarningThreshold && Mathf.Abs(turnSign) > 0.5f)
        {
            float intensity = Mathf.InverseLerp(tiltWarningThreshold, 1f, gripRatio);
            float sign = (invertTiltDirection ? -1f : 1f) * Mathf.Sign(turnSign);
            targetTilt = sign * (intensity * maxTiltAngle);

            if (intensity > 0.8f)
            {
                targetTilt += Mathf.Sin(Time.time * 30f) * 2f;
            }
        }

        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmoothSpeed);

        if (lateralAcceleration > maxCorneringGrip)
        {
            overGripTimer += Time.deltaTime;
            if (overGripTimer >= derailReactionTime)
            {
                TriggerDerail(currentTangent, futureTangent, up, turnSign);
            }
        }
        else
        {
            overGripTimer = Mathf.MoveTowards(overGripTimer, 0f, Time.deltaTime * 2f);
        }
    }

    private void UpdateCheckpoints()
    {
        for (int i = 0; i < cachedCheckpoints.Count; i++)
        {
            if (progress >= cachedCheckpoints[i].progress)
            {
                lastCheckpointProgress = cachedCheckpoints[i].progress;
            }
        }
    }

    private void TriggerDerail(
        Vector3 currentTangent,
        Vector3 futureTangent,
        Vector3 up,
        float turnSign
    )
    {
        isDerailed = true;

        if (carCamera != null)
        {
            cameraWorldOffsetOnDerail = carCamera.transform.position - transform.position;
            frozenCameraX = carCamera.transform.eulerAngles.x;
            frozenCameraY = carCamera.transform.eulerAngles.y;
            frozenCameraZ = 0f;
            carCamera.transform.SetParent(null);
        }

        rb.isKinematic = false;

        Vector3 lateralDir = Vector3.Cross(up, currentTangent).normalized;
        Vector3 outwardDir = (turnSign > 0 ? -lateralDir : lateralDir);

        Vector3 ejectVelocity =
            (currentTangent.normalized * (currentSpeed * 0.7f))
            + (outwardDir * (currentSpeed * derailOutwardForce))
            + (Vector3.up * derailUpwardForce);

        rb.linearVelocity = ejectVelocity;

        Vector3 tumbleAxis = (currentTangent.normalized * 0.6f + outwardDir * 0.4f).normalized;
        rb.angularVelocity = tumbleAxis * (-Mathf.Sign(turnSign) * derailSpinSpeed);

        StartCoroutine(RespawnSequence());
    }

    private IEnumerator RespawnSequence()
    {
        yield return new WaitForSeconds(respawnDelay);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        currentSpeed = 0f;
        currentTilt = 0f;
        overGripTimer = 0f;
        progress = lastCheckpointProgress;

        if (carCamera != null)
        {
            carCamera.transform.SetParent(originalCameraParent);
            carCamera.transform.localPosition = originalCameraLocalPos;
            carCamera.transform.localRotation = originalCameraLocalRot;
        }

        // Snap exacto en el checkpoint
        UpdateTransformOnSpline(snapImmediate: true);
        isDerailed = false;

        // Efecto de parpadeo solo sobre las mallas del hijo
        float elapsed = 0f;
        bool isVisible = true;
        while (elapsed < blinkDuration)
        {
            isVisible = !isVisible;
            SetRenderersVisible(isVisible);
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        SetRenderersVisible(true);
    }

    private void SetRenderersVisible(bool visible)
    {
        if (carRenderers == null)
            return;
        for (int i = 0; i < carRenderers.Length; i++)
        {
            if (carRenderers[i] != null)
                carRenderers[i].enabled = visible;
        }
    }

    private void AdvanceProgress()
    {
        if (currentSpeed <= 0f)
            return;

        float deltaProgress = (currentSpeed * Time.deltaTime) / splineLength;
        progress += deltaProgress;

        int requiredLaps = RaceManager.Instance != null ? RaceManager.Instance.totalLaps : 1;

        if (isLoop)
        {
            if (progress >= 1f)
            {
                progress -= 1f;

                if (currentLap >= requiredLaps)
                {
                    isFinished = true;
                    if (RaceManager.Instance != null)
                        RaceManager.Instance.CarFinished(team);
                }
                else
                {
                    currentLap++;
                    if (cachedCheckpoints.Count > 0)
                        lastCheckpointProgress = cachedCheckpoints[0].progress;
                }
            }
        }
        else
        {
            if (progress >= 1f)
            {
                progress = 1f;
                currentSpeed = 0f;
                isFinished = true;
                if (RaceManager.Instance != null)
                    RaceManager.Instance.CarFinished(team);
            }
        }
    }

    private void UpdateTransformOnSpline(bool snapImmediate = false)
    {
        splineContainer.Evaluate(
            progress,
            out float3 worldPos,
            out float3 worldTangent,
            out float3 worldUp
        );

        transform.position = (Vector3)worldPos;

        Vector3 tangent = (Vector3)worldTangent;
        Vector3 up = (Vector3)worldUp;

        if (tangent != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(tangent, up);
            targetRotation *= Quaternion.Euler(modelRotationOffset);
            targetRotation *= Quaternion.Euler(0f, 0f, currentTilt);

            if (snapImmediate)
            {
                // En la salida o reaparición: rotación instantánea y perfecta sin girar despacio
                transform.rotation = targetRotation;
            }
            else
            {
                // Conduciendo en carrera: suavizado normal
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSmoothSpeed * Time.deltaTime
                );
            }
        }
    }

    void LateUpdate()
    {
        if (isDerailed && carCamera != null)
        {
            carCamera.transform.position = transform.position + cameraWorldOffsetOnDerail;
            carCamera.transform.rotation = Quaternion.Euler(
                frozenCameraX,
                frozenCameraY,
                frozenCameraZ
            );
        }
    }
}
