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
}
