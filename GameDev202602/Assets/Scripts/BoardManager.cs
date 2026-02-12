using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Piece Prefabs")]
    public GameObject pawnPrefab;
    public GameObject rookPrefab;
    public GameObject bishopPrefab;
    public GameObject kingPrefab;


    public int width = 3;
    public int height = 5;

    public float tileSize = 1.2f;
    public GameObject tilePrefab;

    private GameObject[,] tiles;
    private Piece[,] boardPieces;
    private Piece selectedPiece = null;



    void Start()
    {
        GenerateBoard();
        GeneratePieceData();
        SpawnAllPieces(); // ⭐追加
    }

    void Update()
    {
        UpdateCoolTimes();
    }



    void GenerateBoard()
    {
        tiles = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * tileSize, 0, y * tileSize);

                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);
                tile.name = $"Tile_{x}_{y}";
                tile.transform.parent = transform;

                // ⭐ ここ追加
                Tile tileData = tile.GetComponent<Tile>();
                if (tileData == null)
                {
                    tileData = tile.AddComponent<Tile>();
                }

                tileData.x = x;
                tileData.y = y;

                tiles[x, y] = tile;
            }
        }
    }
    void GeneratePieceData()
    {
        boardPieces = new Piece[width, height];

        // ===== Bottom =====
        boardPieces[0, 0] = new Piece(PieceType.Rook, Team.Bottom, 0, 0, 3f);
        boardPieces[1, 0] = new Piece(PieceType.King, Team.Bottom, 1, 0, 4f);
        boardPieces[2, 0] = new Piece(PieceType.Bishop, Team.Bottom, 2, 0, 3f);

        boardPieces[1, 1] = new Piece(PieceType.Pawn, Team.Bottom, 1, 1, 2f);

        // ===== Top =====
        boardPieces[0, 4] = new Piece(PieceType.Bishop, Team.Top, 0, 4, 3f);
        boardPieces[1, 4] = new Piece(PieceType.King, Team.Top, 1, 4, 4f);
        boardPieces[2, 4] = new Piece(PieceType.Rook, Team.Top, 2, 4, 3f);

        boardPieces[1, 3] = new Piece(PieceType.Pawn, Team.Top, 1, 3, 2f);
    }


    void SpawnAllPieces()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Piece piece = boardPieces[x, y];
                if (piece == null) continue;

                GameObject prefab = GetPrefab(piece.type);

                Vector3 pos = new Vector3(x * tileSize, 0.5f, y * tileSize);

                GameObject obj = Instantiate(prefab, pos, Quaternion.identity);

                // チーム色
                Renderer r = obj.GetComponent<Renderer>();
                if (r != null)
                {
                    r.material.color = piece.team == Team.Bottom ? Color.blue : Color.red;
                }

                piece.pieceObject = obj;
            }
        }
    }

    void TrySelectPiece(Tile tile)
    {
        Piece piece = boardPieces[tile.x, tile.y];

        if (piece == null) return;

        if (!piece.CanMove())
        {
            Debug.Log("CT中");
            return;
        }

        selectedPiece = piece;
        Debug.Log("Selected " + piece.type);
    }

    void TryMovePiece(Tile tile)
    {
        if (boardPieces[tile.x, tile.y] != null)
        {
            Debug.Log("そこは埋まってる");
            selectedPiece = null;
            return;
        }

        MovePiece(selectedPiece, tile.x, tile.y);
        selectedPiece = null;
    }

    void MovePiece(Piece piece, int newX, int newY)
    {
        boardPieces[piece.x, piece.y] = null;

        piece.x = newX;
        piece.y = newY;

        boardPieces[newX, newY] = piece;

        Vector3 newPos = new Vector3(newX * tileSize, 0.5f, newY * tileSize);
        piece.pieceObject.transform.position = newPos;

        // ⭐ CT開始
        piece.currentCoolTime = piece.coolTime;

        Debug.Log("Moved / CT Start");
    }



    public void OnTileClicked(Tile tile)
    {
        if (selectedPiece == null)
        {
            TrySelectPiece(tile);
        }
        else
        {
            TryMovePiece(tile);
        }
    }

    void UpdateCoolTimes()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Piece piece = boardPieces[x, y];
                if (piece == null) continue;

                if (piece.currentCoolTime > 0f)
                {
                    piece.currentCoolTime -= Time.deltaTime;
                }
            }
        }
    }

    GameObject GetPrefab(PieceType type)
    {
        switch (type)
        {
            case PieceType.Pawn: return pawnPrefab;
            case PieceType.Rook: return rookPrefab;
            case PieceType.Bishop: return bishopPrefab;
            case PieceType.King: return kingPrefab;
        }

        return null;
    }

}
