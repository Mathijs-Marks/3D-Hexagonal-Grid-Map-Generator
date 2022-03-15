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

    /// <summary>
    /// If the direction is NW, return direction - 1, making it the previous direction.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static HexDirection Previous(this HexDirection direction)
    {
        return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
    }

    /// <summary>
    /// If the direction is NE, return direction + 1, making it the next direction.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static HexDirection Next(this HexDirection direction)
    {
        return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
    }
}
