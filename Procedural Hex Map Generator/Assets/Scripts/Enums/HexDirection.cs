using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enumerator depicting all of the six cell neighbours by compass direction.
/// </summary>
public enum HexDirection
{
    NE, E, SE, SW, W, NW
}

/// <summary>
/// Add extension method to support getting the opposite direction.
/// </summary>
public static class HexDirectionExtensions
{
    /// <summary>
    /// Add three to the direction to get one opposite.
    /// Subtract three from the direction to get the other opposite.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static HexDirection Opposite(this HexDirection direction)
    {
        // One-liner if-statement.
        return (int) direction < 3 ? (direction + 3) : (direction - 3);
    }
}
