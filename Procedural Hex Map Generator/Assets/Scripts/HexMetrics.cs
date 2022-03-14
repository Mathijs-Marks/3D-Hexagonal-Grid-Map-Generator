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

    public static Vector3[] corners =
    {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius)
    };
}
