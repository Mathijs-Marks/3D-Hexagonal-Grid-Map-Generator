using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Struct used to convert to a different coordinate system.
/// Serializable so Unity can store it.
/// Coordinates are immutable.
/// </summary>
[System.Serializable]
public struct HexCoordinates
{
    [SerializeField] private int x, z;

    public int X
    {
        get { return x; }
    }
    public int Z
    {
        get { return z; }
    }

    public int Y
    {
        get { return -X - Z; }
    }

    public HexCoordinates(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    /// <summary>
    /// Method to create a set of coordinates using regular offset coordinates.
    /// Fix X coordinates so that they are aligned along a straight axis.
    /// This is done by undoing the horizontal shift implemented in HexGrid.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public static HexCoordinates FromOffsetCoordinates(int x, int z)
    {
        return new HexCoordinates(x - z / 2, z);
    }

    /// <summary>
    /// Default ToString returns the struct's type name.
    /// This method overrides ToString to return the coordinates on a single line.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
    }

    /// <summary>
    /// Put the returned coordinates on separate lines.
    /// </summary>
    /// <returns></returns>
    public string ToStringOnSeparateLines()
    {
        return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
    }
}
