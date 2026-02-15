using UnityEngine;

public class Piece
{
    public PieceType type;
    public Team team;

    public int x;
    public int y;

    public float coolTime;
    public float currentCoolTime;

    public GameObject pieceObject; // ⭐追加
    public float reCastTime;        // RCの最大値
    public float currentReCastTime; // RC残り時間
    public GameObject markerObject;


    public Piece(PieceType type, Team team, int x, int y, float ct)
    {
        this.type = type;
        this.team = team;
        this.x = x;
        this.y = y;

        this.coolTime = ct;
        this.currentCoolTime = 0;
    }

    public bool CanMove()
    {
        return currentCoolTime <= 0f;
    }

    public bool CanRecast()
    {
        return currentReCastTime <= 0f;
    }
    public bool CanMoveTo(int targetX, int targetY)
    {
        int dx = targetX - x;
        int dy = targetY - y;

        switch (type)
        {
            case PieceType.Pawn:
                return PawnMove(dx, dy);

            case PieceType.Rook:
                return RookMove(dx, dy);

            case PieceType.Bishop:
                return BishopMove(dx, dy);

            case PieceType.King:
                return KingMove(dx, dy);
        }

        return false;
    }
    bool PawnMove(int dx, int dy)
    {
        int dir = team == Team.Blue ? 1 : -1;
        return dx == 0 && dy == dir;
    }

    bool RookMove(int dx, int dy)
    {
        return Mathf.Abs(dx) + Mathf.Abs(dy) == 1;
    }

    bool BishopMove(int dx, int dy)
    {
        return Mathf.Abs(dx) == 1 && Mathf.Abs(dy) == 1;
    }

    bool KingMove(int dx, int dy)
    {
        return Mathf.Abs(dx) <= 1 && Mathf.Abs(dy) <= 1;
    }



}
