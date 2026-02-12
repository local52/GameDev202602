using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    void OnMouseDown()
    {
        FindObjectOfType<BoardManager>().OnTileClicked(this);
    }

}
