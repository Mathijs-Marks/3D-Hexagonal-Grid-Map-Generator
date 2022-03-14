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
    /// Convert Vector3 position to Hex coordinates.
    /// Divide X by the horizontal width of a hexagon.
    /// Because Y is the mirror of X, the negative of X gives Y.
    /// Every two rows, shift an entire unit to the left to take the Z axis into account.
    /// Derive Z and round the values to integers.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static HexCoordinates FromPosition(Vector3 position)
    {
        float x = position.x / (HexMetrics.innerRadius * 2f);
        float y = -x;
        float offset = position.z / (HexMetrics.outerRadius * 3f);

        x -= offset;
        y -= offset;

        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x -y);

        /*
         * Rounding near the edges produce wrong numbers that mess up the cube coordinates.
         * Solution: discard the coordinate with the largest rounding delta.
         * Reconstruct the discarded coordinate from the other two.
         * Only X and Z required, so no need to reconstruct Y.
         */
        if (iX + iY + iZ != 0)
        {
            //Debug.LogWarning("rounding error!");
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x -y - iZ);

            if (dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            }
            else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return new HexCoordinates(iX, iZ);
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
