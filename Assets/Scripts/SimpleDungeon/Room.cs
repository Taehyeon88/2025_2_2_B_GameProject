using UnityEngine;

public class Room
{
    public Vector2Int centor;
    public int size;
    public RoomType type;

    public Room(Vector2Int _centor, int _size, RoomType _type)
    {
        this.centor = _centor;
        this.size = _size;
        this.type = _type;
    }

    public Color GetColor()
    {
        switch (type)
        {
            case RoomType.Start:
                return Color.green;
            case RoomType.Treasure:
                return Color.yellow;
            case RoomType.Boss:
                return Color.red;
            default:
                return Color.white;
        }
    }
}
