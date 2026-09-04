using UnityEngine;
using UnityEngine.SceneManagement;

public struct PiecePlaceholder { };

public struct PlayerPlaceholder { };

public abstract class AMiniGame : MonoBehaviour
{
    public string Name;
    public PiecePlaceholder Piece;
    public static uint SceneID;

    private PlayerPlaceholder player1;
    private PlayerPlaceholder player2;

    public abstract void NotifyWinner(PlayerPlaceholder winnerPlayer);

    public abstract void InitMiniGame();

    public abstract void EndMiniGame();

    public void ReturnToMiddleScene()
    {
        //Id de la escena intermedia puesta en 1. se puede cambiar
        SceneManager.LoadScene(1);
    }
}
