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


    void Start()
    {
        GenerateBoard();
        GeneratePieceData();
        SpawnAllPieces(); // ⭐追加
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

        // 仮：テスト配置
        boardPieces[1, 0] = new Piece(PieceType.King, Team.Bottom, 1, 0, 4f);
        boardPieces[1, 4] = new Piece(PieceType.King, Team.Top, 1, 4, 4f);

        Debug.Log("Piece Data Generated");
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
