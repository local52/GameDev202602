using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

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

    private List<StockPiece> bottomStock = new List<StockPiece>();
    private List<StockPiece> topStock = new List<StockPiece>();
    List<Tile> highlightedTiles = new List<Tile>();

    public GameObject teamMarkerPrefab;
    bool[,] keyPrevState = new bool[5, 3];

    StockPiece selectedStockPiece = null;
    bool isStockMode = false;

    public CanvasGroup fadePanel;
    public float endFadeDuration = 2f;
    public string resultSceneName = "ResultScene";

    public GameObject selectionEffectPrefab;
    private GameObject currentSelectionEffect = null;

    void Start()
    {
        GenerateBoard();
        GeneratePieceData();
        SpawnAllPieces();
        StartCoroutine(StartSequence());
    }

    void Update()
    {
        if (gameEnded) return;

        UpdateCoolTimes();
        UpdateReCast(bottomStock);
        UpdateReCast(topStock);

        HandleKeyboardInput();
        HandleStockInput();
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

                Tile tileData = tile.GetComponent<Tile>();
                if (tileData == null)
                    tileData = tile.AddComponent<Tile>();

                tileData.x = x;
                tileData.y = y;

                tiles[x, y] = tile;
            }
        }
    }

    void GeneratePieceData()
    {
        boardPieces = new Piece[width, height];

        boardPieces[0, 0] = new Piece(PieceType.Rook, Team.Blue, 0, 0, 3f);
        boardPieces[1, 0] = new Piece(PieceType.King, Team.Blue, 1, 0, 4f);
        boardPieces[2, 0] = new Piece(PieceType.Bishop, Team.Blue, 2, 0, 3f);
        boardPieces[1, 1] = new Piece(PieceType.Pawn, Team.Blue, 1, 1, 2f);

        boardPieces[0, 4] = new Piece(PieceType.Bishop, Team.Red, 0, 4, 3f);
        boardPieces[1, 4] = new Piece(PieceType.King, Team.Red, 1, 4, 4f);
        boardPieces[2, 4] = new Piece(PieceType.Rook, Team.Red, 2, 4, 3f);
        boardPieces[1, 3] = new Piece(PieceType.Pawn, Team.Red, 1, 3, 2f);
    }

    void SpawnAllPieces()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Piece piece = boardPieces[x, y];
                if (piece == null) continue;

                GameObject prefab = GetPrefab(piece.type);
                Vector3 pos = new Vector3(x * tileSize, 0.5f, y * tileSize);
                Quaternion rot = piece.team == Team.Red ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;

                GameObject obj = Instantiate(prefab, pos, rot);
                piece.pieceObject = obj;

                GameObject marker = Instantiate(teamMarkerPrefab, obj.transform);
                marker.transform.localPosition = new Vector3(0, 1.2f, 0);

                Renderer mr = marker.GetComponent<Renderer>();
                if (mr != null)
                    mr.material.color = piece.team == Team.Blue ? Color.blue : Color.red;

                piece.markerObject = marker;
            }
    }

    void TrySelectPiece(Tile tile)
    {
        Piece piece = boardPieces[tile.x, tile.y];
        if (piece == null || !piece.CanMove()) return;

        selectedPiece = piece;
        ShowMoveTiles(piece);
        ShowSelectionEffect(piece);
    }

    void ShowSelectionEffect(Piece piece)
    {
        if (currentSelectionEffect != null)
            Destroy(currentSelectionEffect);

        currentSelectionEffect = Instantiate(selectionEffectPrefab);
        currentSelectionEffect.transform.position = piece.pieceObject.transform.position + Vector3.up * 0.05f;
        currentSelectionEffect.transform.parent = piece.pieceObject.transform;
    }

    void ClearSelectionEffect()
    {
        if (currentSelectionEffect != null)
        {
            Destroy(currentSelectionEffect);
            currentSelectionEffect = null;
        }
    }

    void TryMovePiece(Tile tile)
    {
        if (!selectedPiece.CanMoveTo(tile.x, tile.y))
        {
            ClearSelectionEffect();
            selectedPiece = null;
            return;
        }

        Piece target = boardPieces[tile.x, tile.y];
        if (target != null && target.team == selectedPiece.team) return;

        MovePiece(selectedPiece, tile.x, tile.y);

        ClearSelectionEffect();
        selectedPiece = null;
        ClearHighlights();
    }

    void MovePiece(Piece piece, int newX, int newY)
    {
        Piece target = boardPieces[newX, newY];

        boardPieces[piece.x, piece.y] = null;
        piece.x = newX;
        piece.y = newY;
        boardPieces[newX, newY] = piece;

        piece.pieceObject.transform.position =
            new Vector3(newX * tileSize, 0.5f, newY * tileSize);

        piece.currentCoolTime = piece.coolTime;

        if (target != null && target.team != piece.team)
        {
            if (target.type == PieceType.King)
            {
                GameEnd(piece.team);
                return;
            }

            CapturePiece(piece.team, target);
            Destroy(target.pieceObject);
        }

        if (piece.type == PieceType.King)
        {
            if (piece.team == Team.Blue && newY == height - 1) GameEnd(Team.Blue);
            if (piece.team == Team.Red && newY == 0) GameEnd(Team.Red);
        }
    }

    void PressTile(int x, int y)
    {
        Tile tile = GetTile(x, y);
        OnTileClicked(tile);
    }

    public void OnTileClicked(Tile tile)
    {
        if (isStockMode)
        {
            TryPlaceFromStock(tile);
            return;
        }

        if (selectedPiece == null)
            TrySelectPiece(tile);
        else
            TryMovePiece(tile);
    }

    public KeyCode[,] keys = new KeyCode[5, 3]
    {
        { KeyCode.E, KeyCode.D, KeyCode.C },
        { KeyCode.R, KeyCode.F, KeyCode.V },
        { KeyCode.T, KeyCode.G, KeyCode.B },
        { KeyCode.Y, KeyCode.H, KeyCode.N },
        { KeyCode.U, KeyCode.J, KeyCode.M }
    };

    void HandleKeyboardInput()
    {
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                bool current = Input.GetKey(keys[y, x]);
                if (current && !keyPrevState[y, x])
                    PressTile(x, y);

                keyPrevState[y, x] = current;
            }
    }

    void UpdateCoolTimes()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                Piece piece = boardPieces[x, y];
                if (piece == null) continue;

                if (piece.currentCoolTime > 0f)
                    piece.currentCoolTime -= Time.deltaTime;
            }
    }

    void UpdateReCast(List<StockPiece> stock)
    {
        foreach (var sp in stock)
            if (sp.currentReCastTime > 0f)
                sp.currentReCastTime -= Time.deltaTime;
    }

    void CapturePiece(Team capturer, Piece target)
    {
        float rc = GetReCastTime(target.type);
        StockPiece sp = new StockPiece(target.type, capturer, rc);

        if (capturer == Team.Blue) bottomStock.Add(sp);
        else topStock.Add(sp);
    }

    void ShowMoveTiles(Piece piece)
    {
        ClearHighlights();

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (piece.CanMoveTo(x, y))
                {
                    Tile tile = GetTile(x, y);
                    tile.Highlight(Color.green);
                    highlightedTiles.Add(tile);
                }
            }
    }

    void ClearHighlights()
    {
        foreach (var tile in highlightedTiles)
            tile.ResetColor();

        highlightedTiles.Clear();
    }

    Dictionary<KeyCode, (Team team, int stockIndex)> stockKeyMap = new()
    {
        { KeyCode.Alpha1, (Team.Blue, 0) },
        { KeyCode.Alpha2, (Team.Blue, 1) },
        { KeyCode.Alpha3, (Team.Blue, 2) },
        { KeyCode.Alpha4, (Team.Blue, 3) },
        { KeyCode.Alpha5, (Team.Blue, 4) },

        { KeyCode.Alpha0, (Team.Red, 0) },
        { KeyCode.Alpha9, (Team.Red, 1) },
        { KeyCode.Alpha8, (Team.Red, 2) },
        { KeyCode.Alpha7, (Team.Red, 3) },
        { KeyCode.Alpha6, (Team.Red, 4) },
    };

    void HandleStockInput()
    {
        foreach (var pair in stockKeyMap)
        {
            if (Input.GetKeyDown(pair.Key))
            {
                List<StockPiece> stockList =
                    pair.Value.team == Team.Blue ? bottomStock : topStock;

                if (pair.Value.stockIndex >= stockList.Count) continue;

                StockPiece sp = stockList[pair.Value.stockIndex];
                if (!sp.CanRecast()) continue;

                selectedStockPiece = sp;
                isStockMode = true;
                return;
            }
        }
    }

    void TryPlaceFromStock(Tile tile)
    {
        if (!isStockMode || selectedStockPiece == null) return;
        if (boardPieces[tile.x, tile.y] != null) return;

        Piece newPiece = new Piece(
            selectedStockPiece.type,
            selectedStockPiece.team,
            tile.x,
            tile.y,
            GetCoolTime(selectedStockPiece.type));

        boardPieces[tile.x, tile.y] = newPiece;

        GameObject prefab = GetPrefab(selectedStockPiece.type);
        Vector3 pos = new Vector3(tile.x * tileSize, 0.5f, tile.y * tileSize);
        Quaternion rot = selectedStockPiece.team == Team.Red ?
            Quaternion.Euler(0, 180, 0) : Quaternion.identity;

        GameObject obj = Instantiate(prefab, pos, rot);
        newPiece.pieceObject = obj;

        GameObject marker = Instantiate(teamMarkerPrefab, obj.transform);
        marker.transform.localPosition = new Vector3(0, 1.2f, 0);

        Renderer mr = marker.GetComponent<Renderer>();
        if (mr != null)
            mr.material.color =
                selectedStockPiece.team == Team.Blue ? Color.blue : Color.red;

        newPiece.markerObject = marker;

        if (selectedStockPiece.team == Team.Blue)
            bottomStock.Remove(selectedStockPiece);
        else
            topStock.Remove(selectedStockPiece);

        selectedStockPiece = null;
        isStockMode = false;
    }

    float GetCoolTime(PieceType type)
    {
        return type switch
        {
            PieceType.Pawn => 2f,
            PieceType.Rook => 3f,
            PieceType.Bishop => 3f,
            PieceType.King => 4f,
            _ => 2f
        };
    }

    GameObject GetPrefab(PieceType type)
    {
        return type switch
        {
            PieceType.Pawn => pawnPrefab,
            PieceType.Rook => rookPrefab,
            PieceType.Bishop => bishopPrefab,
            PieceType.King => kingPrefab,
            _ => null
        };
    }

    float GetReCastTime(PieceType type)
    {
        return type switch
        {
            PieceType.Pawn => 3f,
            PieceType.Rook => 5f,
            PieceType.Bishop => 5f,
            PieceType.King => 0f,
            _ => 0f
        };
    }

    Tile GetTile(int x, int y)
    {
        return tiles[x, y].GetComponent<Tile>();
    }

    bool gameEnded = false;

    void GameEnd(Team winner)
    {
        if (gameEnded) return;
        gameEnded = true;
        StartCoroutine(FadeOutAndLoadResult(winner));
    }

    IEnumerator FadeOutAndLoadResult(Team winner)
    {
        float t = 0f;

        if (fadePanel == null)
        {
            SceneManager.LoadScene(resultSceneName);
            yield break;
        }

        while (t < endFadeDuration)
        {
            t += Time.deltaTime;
            fadePanel.alpha = Mathf.Clamp01(t / endFadeDuration);
            yield return null;
        }

        WinnerInfo.Winner = winner;
        SceneManager.LoadScene(resultSceneName);
    }

    public Text startText;
    public CanvasGroup backgroundFade;
    public float fadeDuration = 0.5f;
    public float readyTime = 1.5f;
    public float fightTime = 1f;

    IEnumerator StartSequence()
    {
        startText.gameObject.SetActive(true);

        backgroundFade.alpha = 0;
        yield return StartCoroutine(FadeCanvasGroup(backgroundFade, true, fadeDuration));

        startText.text = "Ready?";
        yield return new WaitForSeconds(readyTime);

        startText.text = "Fight!";
        yield return new WaitForSeconds(fightTime);

        yield return StartCoroutine(FadeCanvasGroup(backgroundFade, false, fadeDuration));

        startText.gameObject.SetActive(false);
        EnablePlayerInput(true);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, bool fadeIn, float duration)
    {
        float t = 0f;
        float start = fadeIn ? 0f : 1f;
        float end = fadeIn ? 0.3f : 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / duration);
            yield return null;
        }

        cg.alpha = end;
    }

    void EnablePlayerInput(bool enable) { }
}
