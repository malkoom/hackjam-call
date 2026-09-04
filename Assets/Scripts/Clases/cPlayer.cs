using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;
public enum BoostType //Que modifica la pieza
{
    NONE,
    SPEED,
    ACCELERATION
}
public enum BodyPart //Partes del Coche
{
    WHEELS,
    TURBO,
    AILERON,
    CHASIS
}
public class Player: MonoBehaviour
{
    public Sprite Skin;

    public int id = 0; //P1, P2, etc.

    public int speed = 10;
    public int acceleration = 2;

    [SerializeField] public Dictionary<BodyPart, Piece> PlayerBody = new Dictionary<BodyPart, Piece>();

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
    }
    public void add_piece(Piece NewPiece)
    {
        if (!PlayerBody.ContainsKey(NewPiece.BodyPart))
        {
            return;
        }

        switch(NewPiece.BoostType)
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

[Serializable] public class Piece
{
    public Texture2D PieceTexture;

    public string PieceName;

    public BoostType BoostType;

    public BodyPart BodyPart;

    public int Value;

    public Piece()
    {
        PieceName = "Null Piece";
        Value = 0;
    }

    public Piece(Texture2D texture, string name, BoostType type, BodyPart part, int value)
    {
        PieceTexture = texture;
        PieceName = name;
        BodyPart = part;
        Value = value;
    }
}