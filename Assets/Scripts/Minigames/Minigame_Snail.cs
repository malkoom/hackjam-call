
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class Minigame_Snail : AMiniGame
{

    [Header("Snail")]

    public Sprite Snail_Sprite1;
    public Sprite Snail_Sprite2;

    public Vector3[] InitialPos = new Vector3[2];
    public float FinalPosX;

    public GameObject Snail;
    public GameObject Snail2;

    private bool ActiveGame = true;


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
    }

    void Update()
    {
        
        if (!ActiveGame)
        {
            return;
        }

        if(Keyboard.current.wKey.wasPressedThisFrame)
        {
            Snail.transform.position += new Vector3(0.3f, 0, 0);
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            Snail2.transform.position += new Vector3(0.3f, 0, 0);
        }

        if (Snail.transform.position.x >= FinalPosX || Snail2.transform.position.x >= FinalPosX)
        {
            ActiveGame = false;

            if (Snail.transform.position.x >= FinalPosX)
            {
                NotifyWinner(1);
            }
            else if (Snail2.transform.position.x >= FinalPosX)
            {
                NotifyWinner(2);
            }
            else
            {
                NotifyWinner(1);
            }

            EndMiniGame();
        }
    }

    public override void EndMiniGame()
    {
        ReturnToMiddleScene();
    }
}
