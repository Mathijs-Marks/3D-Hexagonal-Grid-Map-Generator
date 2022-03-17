using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static class for easy access to:
/// - Outer radius of the hexagon, which is 10 units.
/// - Inner radius of the hexagon, which is equal to (root of 3 divided by 2) times the outer radius.
/// - Definition of the corners relative to the cell's center.
///   Start with a corner at the top, then add the rest in clockwise order.
///   Place them in the XZ plane, so the hexagons will be aligned to the ground.
/// </summary>
public static class HexMetrics
{
    public const float outerRadius = 10f;
    public const float innerRadius = (outerRadius * 0.866025404f);
    public const float solidFactor = 0.75f;
    public const float blendFactor = 1f - solidFactor;
    public const float elevationStep = 5f;
    public const int terracesPerSlope = 2;
    public const int terraceSteps = terracesPerSlope * 2 + 1;
    public const float horizontalTerraceStepSize = 1f / terraceSteps;
    public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

    private static Vector3[] corners =
    {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius)
    };

    /// <summary>
    /// Grab the first solid (v1) corner of the triangle, based on current direction.
    /// Used to draw the inner hexagon triangle.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static Vector3 GetFirstCorner(HexDirection direction)
    {
        return corners[(int) direction];
    }

    /// <summary>
    /// Grab the second solid (v2) corner of the triangle, based on current direction.
    /// Used to draw the inner hexagon triangle.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static Vector3 GetSecondCorner(HexDirection direction)
    {
        return corners[(int) direction + 1];
    }

    /// <summary>
    /// Grab the first non-solid corner (v3) of the triangle, based on the current direction.
    /// Used, in unison with v1 and v2, to draw the outer hexagon quad.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    {
        return corners[(int) direction] * solidFactor;
    }

    /// <summary>
    /// Grab the second non-solid corner (v4) of the triangle, based on the current direction.
    /// Used, in unison with v1 and v2, to draw the outer hexagon quad.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {
        return corners[(int) direction + 1] * solidFactor;
    }

    /// <summary>
    /// Add the two non-solid corners (v3, v4) based on direction, multiply by 0.5, multiply by blend factor.
    /// Result is bridge offset to determine the new v3 and v4 vectors for the bridge rectangle quad.
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    public static Vector3 GetBridge(HexDirection direction)
    {
        return (corners[(int) direction] + corners[(int) direction + 1]) * 
               blendFactor;
    }

    /// <summary>
    /// Interpolate each step along a slope.
    /// For horizontal, interpolate between point a and b and multiply it with the horizontal step size.
    /// For vertical, interpolate between point a and b and multiply it with the vertical step size.
    /// The vertical step size is determined by adding 1 to a step and multiplying it by two.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
    {
        float horizontal = step * horizontalTerraceStepSize;
        a.x += (b.x - a.x) * horizontal;
        a.z += (b.z - a.z) * horizontal;
        float vertical = ((step + 1) / 2) * verticalTerraceStepSize;
        a.y += (b.y - a.y) * vertical;
        return a;
    }

    /// <summary>
    /// Interpolate colour as if the connection is flat on the Y axis.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="step"></param>
    /// <returns></returns>
    public static Color TerraceLerp(Color a, Color b, int step)
    {
        float h = step * horizontalTerraceStepSize;
        return Color.Lerp(a, b, h);
    }

    /// <summary>
    /// Check which type of connection is present between cells.
    /// If the elevation between cells is the same, then the connection is flat.
    /// If the elevation between cells is either 1 or -1, then the connection is a slope.
    /// In any other case, the connection is a cliff.
    /// </summary>
    /// <param name="elevation1"></param>
    /// <param name="elevation2"></param>
    /// <returns></returns>
    public static HexEdgeType GetEdgeType(int elevation1, int elevation2)
    {
        if (elevation1 == elevation2)
        {
            return HexEdgeType.Flat;
        }

        int delta = elevation2 - elevation1;
        if (delta == 1 || delta == -1)
        {
            return HexEdgeType.Slope;
        }

        return HexEdgeType.Cliff;
    }
}
