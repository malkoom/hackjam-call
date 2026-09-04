using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class AMiniGame : MonoBehaviour
{
    public string Name;
    public Piece Piece;
    public static uint SceneID;

    public void NotifyWinner(int player)
    {
        GameManager.Singleton.SetWinner(player, Piece);
    }

    public abstract void InitMiniGame();

    public abstract void EndMiniGame();

    public void ReturnToMiddleScene()
    {
        GameManager.Singleton.StartFeedbackSequence();
    }
}
