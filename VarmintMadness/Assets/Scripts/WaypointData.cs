using UnityEngine;

public class WaypointData
{
    public Vector2 Position;
    public string Tag;
    public string Name;

    public WaypointData(Vector2 position, string tag, string name)
    {
        Position = position;
        Tag = tag;
        Name = name;
    }
}