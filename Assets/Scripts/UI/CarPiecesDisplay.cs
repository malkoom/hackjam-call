using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Muestra el inventario de piezas de cada jugador como sprites del mundo junto a su coche.
/// </summary>
public class CarPiecesDisplay : MonoBehaviour
{
    [Header("Anclas junto a cada coche")]
    [SerializeField] private Transform player1Anchor;
    [SerializeField] private Transform player2Anchor;

    [Header("Aspecto")]
    [SerializeField] private Vector3 firstIconOffset = new Vector3(0.8f, 0.4f, 0f);
    [SerializeField] private Vector2 iconSize = new Vector2(0.45f, 0.45f);
    [SerializeField] private float verticalSpacing = 0.1f;
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 10;

    private readonly List<GameObject> player1Icons = new List<GameObject>();
    private readonly List<GameObject> player2Icons = new List<GameObject>();
    private Player subscribedPlayer1;
    private Player subscribedPlayer2;

    private void OnEnable()
    {
        Subscribe();
        RefreshAll();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        GameManager manager = GameManager.Singleton;
        if (manager == null)
            return;

        if (subscribedPlayer1 != manager.player1)
        {
            if (subscribedPlayer1 != null)
                subscribedPlayer1.PieceAdded -= OnPlayer1PieceAdded;

            subscribedPlayer1 = manager.player1;
            if (subscribedPlayer1 != null)
                subscribedPlayer1.PieceAdded += OnPlayer1PieceAdded;
        }

        if (subscribedPlayer2 != manager.player2)
        {
            if (subscribedPlayer2 != null)
                subscribedPlayer2.PieceAdded -= OnPlayer2PieceAdded;

            subscribedPlayer2 = manager.player2;
            if (subscribedPlayer2 != null)
                subscribedPlayer2.PieceAdded += OnPlayer2PieceAdded;
        }
    }

    private void OnPlayer1PieceAdded(Piece _) =>
        RefreshPlayer(subscribedPlayer1, player1Anchor, player1Icons);

    private void OnPlayer2PieceAdded(Piece _) =>
        RefreshPlayer(subscribedPlayer2, player2Anchor, player2Icons);

    public void RefreshAll()
    {
        Subscribe();
        RefreshPlayer(subscribedPlayer1, player1Anchor, player1Icons);
        RefreshPlayer(subscribedPlayer2, player2Anchor, player2Icons);
    }

    private void RefreshPlayer(Player player, Transform anchor, List<GameObject> icons)
    {
        Clear(icons);
        if (player == null || anchor == null || player.Pieces == null)
            return;

        int index = 0;
        foreach (Piece piece in player.Pieces)
        {
            if (piece == null || piece.PieceTexture == null)
                continue;

            GameObject icon = new GameObject($"Piece - {piece.PieceName}", typeof(SpriteRenderer));
            icon.transform.SetParent(anchor, false);
            icon.transform.localPosition = firstIconOffset
                + Vector3.down * index * (iconSize.y + verticalSpacing);
            icon.transform.localScale = new Vector3(iconSize.x, iconSize.y, 1f);

            SpriteRenderer renderer = icon.GetComponent<SpriteRenderer>();
            renderer.sprite = piece.PieceTexture;
            renderer.sortingLayerName = sortingLayerName;
            renderer.sortingOrder = sortingOrder + index;

            icons.Add(icon);
            index++;
        }
    }

    private static void Clear(List<GameObject> icons)
    {
        foreach (GameObject icon in icons)
        {
            if (icon != null)
                Destroy(icon);
        }
        icons.Clear();
    }

    private void Unsubscribe()
    {
        if (subscribedPlayer1 != null)
            subscribedPlayer1.PieceAdded -= OnPlayer1PieceAdded;
        if (subscribedPlayer2 != null)
            subscribedPlayer2.PieceAdded -= OnPlayer2PieceAdded;

        subscribedPlayer1 = null;
        subscribedPlayer2 = null;
    }
}
