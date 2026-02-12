using UnityEngine;

public class Piece
{
    public PieceType type;
    public Team team;

    public int x;
    public int y;

    public float coolTime;
    public float currentCoolTime;

    public Piece(PieceType type, Team team, int x, int y, float ct)
    {
        this.type = type;
        this.team = team;
        this.x = x;
        this.y = y;

        this.coolTime = ct;
        this.currentCoolTime = 0;
    }
}
