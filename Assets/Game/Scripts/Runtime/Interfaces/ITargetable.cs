using UnityEngine;

public interface ITargetable
{
    bool IsTargetable { get; }
    Vector2 Position { get; }
}
