using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FMODUnity;
public class Minigame_Snail : AMiniGame
{
    [Header("Sound")]
    public EventReference Slime;

    [Header("Snail")]
    public GameObject Baba;
    public Sprite[] BabaTypes = new Sprite[3];

    public Sprite Snail_Sprite1;
    public Sprite Snail_Sprite2;

    public Sprite Snail_Back_Sprite1;
    public Sprite Snail_Back_Sprite2;

    public Vector3[] InitialPos = new Vector3[2];
    public float FinalPosX;

    public GameObject Snail;
    public GameObject Snail2;

    private float TiltP1 = 7f;
    private float TiltP2 = 7f;

    private bool ActiveGame = true;

    [Header("Baba")]
    public float BabaOffsetX = -0.15f;
    public float BabaLifetime = 3f;
    public float BabaScale = 1f;

    [Header("Squash & Stretch")]
    public float SquashAmount = 0.15f;
    public float StretchAmount = 0.15f;
    public float SquashDuration = 0.05f;
    public float StretchDuration = 0.05f;

    private Vector3 Snail1OriginalScale;
    private Vector3 Snail2OriginalScale;

    private Coroutine P1SquashCoroutine;
    private Coroutine P2SquashCoroutine;

    [Header("Keys")]
    private Key[] player1KeySequence = { Key.W, Key.D, Key.S, Key.A };

    private int playerKeyIndex = 0;

    private Key[] player2KeySequence =
    {
        Key.UpArrow,
        Key.RightArrow,
        Key.DownArrow,
        Key.LeftArrow,
    };

    private int player2KeyIndex = 0;

    [Header("UI")]
    public GameObject key;
    public GameObject key2;

    [SerializeField]
    public Dictionary<Key, Sprite> p1KeysDictionary = new Dictionary<Key, Sprite>();

    [SerializeField]
    public Dictionary<Key, Sprite> p2KeysDictionary = new Dictionary<Key, Sprite>();

    void Start()
    {
        InitMiniGame();
    }

    public override void InitMiniGame()
    {
        Snail.GetComponent<SpriteRenderer>().sprite = Snail_Sprite1;
        Snail2.GetComponent<SpriteRenderer>().sprite = Snail_Sprite2;

        Snail.transform.position = InitialPos[0];
        Snail2.transform.position = InitialPos[1];

        Snail1OriginalScale = Snail.transform.localScale;
        Snail2OriginalScale = Snail2.transform.localScale;

        ActiveGame = true;
    }

    void Update()
    {
        if (!ActiveGame)
        {
            return;
        }

        // PLAYER 1
        if (Keyboard.current[player1KeySequence[playerKeyIndex]].wasPressedThisFrame)
        {
            RuntimeManager.PlayOneShot(Slime, Snail.transform.position);
            Snail.transform.position += new Vector3(0.3f, 0, 0);

            Snail.transform.rotation = Quaternion.Euler(0, 0, TiltP1);

            int randomIndex = UnityEngine.Random.Range(0, BabaTypes.Length);
            TiltP1 *= -1;
            GameObject newBaba = Instantiate(Baba);
            newBaba.GetComponent<SpriteRenderer>().sprite = BabaTypes[randomIndex];
            newBaba.transform.position = Snail.transform.position + new Vector3(-0.5f, -0.4f, 0);
            PlaySquashP1();

            playerKeyIndex++;

            if (playerKeyIndex >= player1KeySequence.Length)
            {
                playerKeyIndex = 0;
            }

            key.GetComponent<SpriteRenderer>().sprite = p1KeysDictionary[
                player1KeySequence[playerKeyIndex]
            ];
        }

        // PLAYER 2
        if (Keyboard.current[player2KeySequence[player2KeyIndex]].wasPressedThisFrame)
        {
            RuntimeManager.PlayOneShot(Slime, Snail2.transform.position);
            Snail2.transform.position += new Vector3(0.3f, 0, 0);

            Snail2.transform.rotation = Quaternion.Euler(0, 0, TiltP2);

            int randomIndex = UnityEngine.Random.Range(0, BabaTypes.Length);
            TiltP2 *= -1;

            GameObject newBaba = Instantiate(Baba);
            newBaba.GetComponent<SpriteRenderer>().sprite = BabaTypes[randomIndex];
            newBaba.transform.position = Snail2.transform.position + new Vector3(-0.5f, -0.4f, 0);

            PlaySquashP2();

            player2KeyIndex++;

            if (player2KeyIndex >= player2KeySequence.Length)
            {
                player2KeyIndex = 0;
            }

            key2.GetComponent<SpriteRenderer>().sprite = p2KeysDictionary[
                player2KeySequence[player2KeyIndex]
            ];
        }

        // WIN
        if (Snail.transform.position.x >= FinalPosX || Snail2.transform.position.x >= FinalPosX)
        {
            ActiveGame = false;

            if (Snail.transform.position.x >= FinalPosX)
            {
                NotifyWinner(1);
            }
            else
            {
                NotifyWinner(2);
            }

            EndMiniGame();
        }
    }

    void PlaySquashP1()
    {
        if (P1SquashCoroutine != null)
        {
            StopCoroutine(P1SquashCoroutine);
        }

        P1SquashCoroutine = StartCoroutine(SquashAndStretch(Snail.transform, Snail1OriginalScale));
    }

    void PlaySquashP2()
    {
        if (P2SquashCoroutine != null)
        {
            StopCoroutine(P2SquashCoroutine);
        }

        P2SquashCoroutine = StartCoroutine(SquashAndStretch(Snail2.transform, Snail2OriginalScale));
    }

    IEnumerator SquashAndStretch(Transform target, Vector3 originalScale)
    {
        Vector3 squashScale = new Vector3(
            originalScale.x * (1f + SquashAmount),
            originalScale.y * (1f - SquashAmount),
            originalScale.z
        );

        Vector3 stretchScale = new Vector3(
            originalScale.x * (1f - StretchAmount),
            originalScale.y * (1f + StretchAmount),
            originalScale.z
        );

        // SQUASH
        yield return StartCoroutine(
            ScaleTo(target, target.localScale, squashScale, SquashDuration)
        );

        // STRETCH
        yield return StartCoroutine(ScaleTo(target, squashScale, stretchScale, StretchDuration));

        // NORMAL
        yield return StartCoroutine(ScaleTo(target, stretchScale, originalScale, SquashDuration));

        target.localScale = originalScale;
    }

    IEnumerator ScaleTo(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t = timer / duration;

            target.localScale = Vector3.Lerp(from, to, t);

            yield return null;
        }

        target.localScale = to;
    }

    public override void EndMiniGame()
    {
        StopAllCoroutines();
        Destroy(Snail);
        Destroy(Snail2);

        ReturnToMiddleScene();
    }
}
