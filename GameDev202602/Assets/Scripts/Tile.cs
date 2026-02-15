using UnityEngine;

public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    void OnMouseDown()
    {
        FindObjectOfType<BoardManager>().OnTileClicked(this);
    }
    Renderer rend;
    Color defaultColor;

    void Start()
    {
        rend = GetComponent<Renderer>();
        defaultColor = rend.material.color;
    }

    public void Highlight(Color color)
    {
        rend.material.color = color;
    }

    public void ResetColor()
    {
        rend.material.color = defaultColor;
    }

}
