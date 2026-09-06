using System;
using System.Collections.Generic;
using UnityEngine;

public enum BoostType //Que modifica la pieza
{
    NONE,
    SPEED,
    ACCELERATION,
}

public enum BodyPart //Partes del Coche
{
    WHEELS,
    TURBO,
    AILERON,
    BODYWORK,
    GAS,
    EXHAUST_PIPE,
    LIGHTS,
}

public class Player : MonoBehaviour
{
    public Sprite Skin;

    public int id = 0; //P1, P2, etc.

    public int speed = 10;
    public int acceleration = 2;

    [SerializeField]
    public Dictionary<BodyPart, Piece> PlayerBody = new Dictionary<BodyPart, Piece>();

    // Inventario ordenado: cada premio se conserva para poder representarlo en el garaje.
    public List<Piece> Pieces = new List<Piece>();

    public event Action<Piece> PieceAdded;

    void Awake()
    {
        GameManager gameManager = GameManager.Singleton;
        if (gameManager == null)
            return;

        if (this != gameManager.player1 && this != gameManager.player2)
            Destroy(gameObject);
    }

    public Player()
    {
        print("Debes asignarle un numero");
        return;
    }

    public Player(int n)
    {
        id = n;

        PlayerBody.Add(BodyPart.AILERON, new Piece());
        PlayerBody.Add(BodyPart.TURBO, new Piece());
        PlayerBody.Add(BodyPart.WHEELS, new Piece());
        PlayerBody.Add(BodyPart.BODYWORK, new Piece());
        PlayerBody.Add(BodyPart.GAS, new Piece());
        PlayerBody.Add(BodyPart.EXHAUST_PIPE, new Piece());
        PlayerBody.Add(BodyPart.LIGHTS, new Piece());
    }

    public void AddPiece(Piece piece)
    {
        if (piece == null)
            return;

        switch (piece.BoostType)
        {
            case BoostType.ACCELERATION:
                acceleration += piece.Value;
                break;
            case BoostType.SPEED:
                speed += piece.Value;
                break;
            default:
                Debug.Log("BoosType no implementado");
                break;
        }

        Pieces.Add(piece);
        PieceAdded?.Invoke(piece);
    }

    public void add_piece(Piece NewPiece)
    {
        if (!PlayerBody.ContainsKey(NewPiece.BodyPart))
        {
            return;
        }

        switch (NewPiece.BoostType)
        {
            case BoostType.SPEED:
                speed += NewPiece.Value;
                break;
            case BoostType.ACCELERATION:
                acceleration += NewPiece.Value;
                break;
            default:
                return;
        }

        PlayerBody[NewPiece.BodyPart] = NewPiece;
    }
}

[Serializable]
public class Piece
{
    public Sprite PieceTexture;

    public string PieceName;

    public BoostType BoostType;

    public BodyPart BodyPart;

    public int Value;

    public Piece()
    {
        PieceName = "Null Piece";
        Value = 0;
    }

    public Piece(Sprite texture, string name, BoostType type, BodyPart part, int value)
    {
        PieceTexture = texture;
        PieceName = name;
        BodyPart = part;
        Value = value;
    }
}
