using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public int width = 3;
    public int height = 5;

    public float tileSize = 1.2f;
    public GameObject tilePrefab;

    private GameObject[,] tiles;

    void Start()
    {
        GenerateBoard();
    }

    void GenerateBoard()
    {
        tiles = new GameObject[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * tileSize, 0, y * tileSize);

                GameObject tile = Instantiate(tilePrefab, position, Quaternion.identity);//ƒ|ƒWƒVƒ‡ƒ“‚©‚ç”Õ‚ð¶¬
                tile.name = $"Tile_{x}_{y}";
                tile.transform.parent = transform;

                tiles[x, y] = tile;
            }
        }
    }
}
