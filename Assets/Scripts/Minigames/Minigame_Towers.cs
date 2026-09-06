using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;

public class Minigame_Towers : AMiniGame
{
    [Header("SFX")]
    public EventReference GunShot;
    public EventReference Death;

    [Header("Towers")]
    public GameObject[] Towers = new GameObject[2];

    [Header("Players")]
    public GameObject[] Players = new GameObject[2];

    public GameObject[] Guns = new GameObject[2];

    public Sprite DeathP1;
    public Sprite DeathP2;

    [Header("Ball")]
    public GameObject Ball;

    public float BallSpeed = 10f;
    public float BallSpawnDistance = 1.5f;
    public float BallUpForce = 2f;
    public float ThrowCooldown = 1.5f;

    [Header("Balance")]
    public float BalanceForce = 20f;
    public float MaxAngularSpeed = 150f;

    [Header("Death")]
    public GameObject DeathZone;

    private float P1ThrowTimer = 0f;
    private float P2ThrowTimer = 0f;

    private bool ActiveGame = true;

    private Collider2D deathZoneCollider;

    void Start()
    {
        InitMiniGame();
    }

    public override void InitMiniGame()
    {
        ActiveGame = true;

        P1ThrowTimer = 0f;
        P2ThrowTimer = 0f;

        deathZoneCollider = DeathZone.GetComponent<Collider2D>();

        // Los jugadores deben poder rotar
        foreach (GameObject player in Players)
        {
            if (player == null)
                continue;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.freezeRotation = false;
            }
        }
    }

    void Update()
    {
        if (!ActiveGame)
            return;

        CheckDeathZone();

        if (P1ThrowTimer > 0f)
            P1ThrowTimer -= Time.deltaTime;

        if (P2ThrowTimer > 0f)
            P2ThrowTimer -= Time.deltaTime;


        // =========================
        // PLAYER 1
        // W = disparar
        // =========================

        if (Keyboard.current.wKey.wasPressedThisFrame &&
            P1ThrowTimer <= 0f)
        {
            ThrowBall(0, 1);

            P1ThrowTimer = ThrowCooldown;
        }


        // =========================
        // PLAYER 2
        // UP = disparar
        // =========================

        if (Keyboard.current.upArrowKey.wasPressedThisFrame &&
            P2ThrowTimer <= 0f)
        {
            ThrowBall(1, 0);

            P2ThrowTimer = ThrowCooldown;
        }
    }

    void FixedUpdate()
    {
        if (!ActiveGame)
            return;

        BalancePlayers();
    }

    void BalancePlayers()
    {
        Rigidbody2D P1RB = Players[0].GetComponent<Rigidbody2D>();
        Rigidbody2D P2RB = Players[1].GetComponent<Rigidbody2D>();


        // =========================
        // PLAYER 1
        // A = girar izquierda
        // D = girar derecha
        // =========================

        float P1Balance = 0f;

        if (Keyboard.current.aKey.isPressed)
            P1Balance += 1f;

        if (Keyboard.current.dKey.isPressed)
            P1Balance -= 1f;

        if (P1RB != null)
        {
            P1RB.AddTorque(
                P1Balance * BalanceForce,
                ForceMode2D.Force
            );

            P1RB.angularVelocity = Mathf.Clamp(
                P1RB.angularVelocity,
                -MaxAngularSpeed,
                MaxAngularSpeed
            );
        }


        // =========================
        // PLAYER 2
        // LEFT = girar izquierda
        // RIGHT = girar derecha
        // =========================

        float P2Balance = 0f;

        if (Keyboard.current.leftArrowKey.isPressed)
            P2Balance += 1f;

        if (Keyboard.current.rightArrowKey.isPressed)
            P2Balance -= 1f;

        if (P2RB != null)
        {
            P2RB.AddTorque(
                P2Balance * BalanceForce,
                ForceMode2D.Force
            );

            P2RB.angularVelocity = Mathf.Clamp(
                P2RB.angularVelocity,
                -MaxAngularSpeed,
                MaxAngularSpeed
            );
        }
    }

    void CheckDeathZone()
    {
        if (deathZoneCollider == null)
            return;

        Collider2D player1Collider = Players[0].GetComponent<Collider2D>();
        Collider2D player2Collider = Players[1].GetComponent<Collider2D>();

        if (player1Collider != null &&
            deathZoneCollider.IsTouching(player1Collider))
        {
            RuntimeManager.PlayOneShot(Death, transform.position);
            ActiveGame = false;
            Players[0].GetComponent<SpriteRenderer>().sprite = DeathP1;
            // P1 perdió
            NotifyWinner(2);
            EndMiniGame();
            return;
        }

        if (player2Collider != null &&
            deathZoneCollider.IsTouching(player2Collider))
        {
            RuntimeManager.PlayOneShot(Death, transform.position);
            Players[1].GetComponent<SpriteRenderer>().sprite = DeathP2;
            ActiveGame = false;

            // P2 perdió
            NotifyWinner(1);
            EndMiniGame();
            return;
        }
    }

    void ThrowBall(int shooterIndex, int targetIndex)
    {
        GameObject shooter = Guns[shooterIndex];
        GameObject target = Players[targetIndex];

        if (shooter == null || target == null)
            return;

        RuntimeManager.PlayOneShot(GunShot, shooter.transform.position);

        float directionX = Mathf.Sign(
            target.transform.position.x -
            shooter.transform.position.x
        );

        Vector2 direction = new Vector2(directionX, 0f);

        Vector2 spawnPosition =
            (Vector2)shooter.transform.position +
            direction * BallSpawnDistance;

        GameObject newBall = Instantiate(
            Ball,
            spawnPosition,
            Quaternion.identity
        );

        Rigidbody2D rb = newBall.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Destroy(newBall);
            return;
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.simulated = true;

        rb.linearVelocity = new Vector2(
            directionX * BallSpeed,
            BallUpForce
        );
    }

    public override void EndMiniGame()
    {
        ActiveGame = false;

        ReturnToMiddleScene();
    }
}