using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

/// <summary>
/// Hex mesh component used to render a single mesh, composed of hexagonals, on the grid.
/// Lists are used to determine the required amount of vertices and triangles.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    private Mesh hexMesh;
    private List<Vector3> vertices;
    private List<int> triangles;

    private MeshCollider meshCollider; // Used to interact with (colour) the hexagons.

    private List<Color> colors;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "Hex Mesh";
        vertices = new List<Vector3>();
        triangles = new List<int>();
        colors = new List<Color>();
    }

    /// <summary>
    /// Clear old data.
    /// Loop through all the cells, triangulate them individually.
    /// Assign vertices, triangles, and colors to the mesh, and recalculate mesh normals.
    /// Assign collider to the mesh.
    /// </summary>
    /// <param name="cells"></param>
    public void Triangulate(HexCell[] cells)
    {
        hexMesh.Clear();
        vertices.Clear();
        triangles.Clear();
        colors.Clear();

        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }

        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.colors = colors.ToArray();
        hexMesh.RecalculateNormals();
        meshCollider.sharedMesh = hexMesh;
    }

    /// <summary>
    /// (Instead of looping through the 6 hexagon corners, loop through the six directions)
    /// Using the directions enumerator, identify the cell parts and triangulate them.
    /// </summary>
    /// <param name="cell"></param>
    private void Triangulate(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    /// <summary>
    /// Define center, first & second solid corner of the hexagon's triangle.
    /// Draw and colour the triangle.
    /// If the direction value is smaller and/or equal to SE (2):
    /// Triangulate connections to other hexagons.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="cell"></param>
    private void Triangulate(HexDirection direction, HexCell cell)
    {
        // Based on the current direction, take the center of the vertex, then the first corner, then the second corner.
        Vector3 center = cell.transform.localPosition;
        Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction);
        Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction);
        
        // Draw triangle and colour it.
        AddTriangle(center, v1, v2);
        AddTriangleColor(cell.color);

        if (direction <= HexDirection.SE)
        {
            TriangulateConnection(direction, cell, v1, v2);
        }
    }

    /// <summary>
    /// Retrieve the cell's neighbours, if there are any.
    /// Determine bridge vectors to neighbour(s).
    /// Draw the rectangular bridge(s) to the neighbour(s) and colour them.
    /// Retrieve the next neighbour of the hexagon.
    /// If the direction value is smaller and/or equal than E (1) AND there's a neighbour present:
    /// Add a triangle to fill in the gap and colour it.
    /// Implement elevation by overriding the Y axis.
    /// Add terraces if the connection type is a slope, otherwise add a quad as normal.
    /// Add terraces by using interpolation between point a and b.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="cell"></param>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    private void TriangulateConnection(
        HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
    {
        HexCell neighbour = cell.GetNeighbour(direction);
        if (neighbour == null)
            return;

        // Determine v3 and v4 vectors by adding the bridge factor to v1 and v2.
        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;

        // In case of edge connections, override the height of the other end of the bridge.
        v3.y = v4.y = neighbour.Elevation * HexMetrics.elevationStep;

        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
        {
            TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbour);
        }
        else
        {
            // Create square quad with these vectors.
            // Bridge quad only needs two colours.
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(cell.color, neighbour.color);
        }

        HexCell nextNeighbour = cell.GetNeighbour(direction.Next());
        if (direction <= HexDirection.E && nextNeighbour != null)
        {
            // In case of corner connections, override the height of the other end of the triangle.
            Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbour.Elevation * HexMetrics.elevationStep;

            /*
             * Check whether the cell being triangulated is lower than its neighbours, or tied for lowest.
             * If yes, use it as the bottom cell.
             * If the innermost if-statement fails, it means that the next neighbour is the lowest cell.
             * Rotate the triangle counterclockwise to keep it correctly oriented.
             * If the first if-statement already failed, it becomes a contest between the two neighbouring cells.
             * If the edge neighbour is the lowest, then rotate clockwise, otherwise counterclockwise.
             */

            if (cell.Elevation <= neighbour.Elevation)
            {
                if (cell.Elevation <= nextNeighbour.Elevation)
                {
                    TriangulateCorner(v2, cell, v4, neighbour, v5, nextNeighbour);
                }
                else
                {
                    TriangulateCorner(v5, nextNeighbour, v2, cell, v4, neighbour);
                }
            }
            else if (neighbour.Elevation <= nextNeighbour.Elevation)
            {
                TriangulateCorner(v4, neighbour, v5, nextNeighbour, v2, cell);
            }
            else
            {
                TriangulateCorner(v5, nextNeighbour, v2, cell, v4, neighbour);
            }
        }
    }

    /// <summary>
    /// Use the interpolation method to create terraces.
    /// Begin with creating a shorter, steeper slope.
    /// Using a for-loop, add the steps in between the beginning and the end.
    /// Each step, the previous last two vertices become the new first two.
    /// Compute the new vertices and colours and create a new quad.
    /// Lastly, finish the slope by creating a normal sloped quad.
    /// </summary>
    /// <param name="beginLeft"></param>
    /// <param name="beginRight"></param>
    /// <param name="beginCell"></param>
    /// <param name="endLeft"></param>
    /// <param name="endRight"></param>
    /// <param name="endCell"></param>
    private void TriangulateEdgeTerraces(
        Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
        Vector3 endLeft, Vector3 endRight, HexCell endCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, 1);

        // Create square quad with these vectors.
        // Bridge quad only needs two colours.
        AddQuad(beginLeft, beginRight, v3, v4);
        AddQuadColor(beginCell.color, c2);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c2;
            v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
            v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
            c2 = HexMetrics.TerraceLerp(beginCell.color, endCell.color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2);
        }

        AddQuad(v3, v4, endLeft, endRight);
        AddQuadColor(c2, endCell.color);
    }

    /// <summary>
    /// Triangulate corner connections between hexagons.
    /// Start from the bottom, then left, then right and add triangles.
    /// </summary>
    /// <param name="bottom"></param>
    /// <param name="bottomCell"></param>
    /// <param name="left"></param>
    /// <param name="leftCell"></param>
    /// <param name="right"></param>
    /// <param name="rightCell"></param>
    private void TriangulateCorner(
        Vector3 bottom, HexCell bottomCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        HexEdgeType lefEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        /*
         * If both edges are slopes, then there are terraces on both the left and right side.
         * Because the bottom cell is the lowest, those slopes will go up.
         * This means the left and right cell have the same elevation, therefore the top connection is flat.
         * This is a case of slope-slope-flat (SSF)
         *
         * If the right edge is flat, then begin terrace from the left instead of the bottom.
         *
         * If the left edge is flat, then begin terrace from the right instead of the bottom.
         */
        if (lefEdgeType == HexEdgeType.Slope)
        {
            if (rightEdgeType == HexEdgeType.Slope)
            {
                TriangulateCornerTerraces(
                    bottom, bottomCell, left, leftCell, right, rightCell);
                return;
            }

            if (rightEdgeType == HexEdgeType.Flat)
            {
                TriangulateCornerTerraces(
                    left, leftCell, right, rightCell, bottom, bottomCell);
                return;
            }
            TriangulateCornerTerracesCliff(
                bottom, bottomCell, left, leftCell, right, rightCell);
            return;
        }

        if (rightEdgeType == HexEdgeType.Slope)
        {
            if (lefEdgeType == HexEdgeType.Flat)
            {
                TriangulateCorner(
                    right, rightCell, bottom, bottomCell, left, leftCell);
                return;
            }
            TriangulateCornerCliffTerraces(
                bottom, bottomCell, left, leftCell, right, rightCell);
            return;
        }

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            if (leftCell.Elevation < rightCell.Elevation)
            {
                TriangulateCornerCliffTerraces(
                    right, rightCell, bottom, bottomCell, left, leftCell);
            }
            else
            {
                TriangulateCornerTerracesCliff(
                    left, leftCell, right, rightCell, bottom, bottomCell);
            }

            return;
        }

        AddTriangle(bottom, left, right);
        AddTriangleColor(bottomCell.color, leftCell.color, rightCell.color);
    }

    /// <summary>
    /// Use the interpolation method to create terraces.
    /// Begin with creating a shorter, steeper slope.
    /// Using a for-loop, add the steps in between the beginning and the end.
    /// Each step, the previous last two vertices become the new first two.
    /// Compute the new vertices and colours and create a new triangle.
    /// Lastly, finish the slope by creating a normal sloped triangle.
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="beginCell"></param>
    /// <param name="left"></param>
    /// <param name="leftCell"></param>
    /// <param name="right"></param>
    /// <param name="rightCell"></param>
    private void TriangulateCornerTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
        Color c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);
        Color c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, 1);

        AddTriangle(begin, v3, v4);
        AddTriangleColor(beginCell.color, c3, c4);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;
            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            c4 = HexMetrics.TerraceLerp(beginCell.color, rightCell.color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2, c3, c4);
        }

        AddQuad(v3, v4, left, right);
        AddQuadColor(c3, c4, leftCell.color, rightCell.color);
    }

    /// <summary>
    /// Method to merge slopes and cliffs together.
    /// Split into two parts:
    /// Bottom part: collapse terraces at a boundary point that lies along the cliff.
    /// Place the boundary point one elevation level above the bottom cell.
    /// Find this point by interpolating based on elevation difference.
    /// Top part: if there's a slope, add a rotated boundary triangle.
    /// Otherwise a simple triangle suffices.
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="beginCell"></param>
    /// <param name="left"></param>
    /// <param name="leftCell"></param>
    /// <param name="right"></param>
    /// <param name="rightCell"></param>
    private void TriangulateCornerTerracesCliff(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        if (b < 0)
            b = -b;

        Vector3 boundary = Vector3.Lerp(begin, right, b);
        Color boundaryColor = Color.Lerp(beginCell.color, rightCell.color, b);

        TriangulateBoundaryTriangle(
            begin, beginCell, left, leftCell, boundary, boundaryColor);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(
                left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else
        {
            AddTriangle(left, right, boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
        }
    }

    /// <summary>
    /// Mirrored version of TriangulateCornerTerracesCliff.
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="beginCell"></param>
    /// <param name="left"></param>
    /// <param name="leftCell"></param>
    /// <param name="right"></param>
    /// <param name="rightCell"></param>
    private void TriangulateCornerCliffTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell)
    {
        float b = 1f / (leftCell.Elevation - beginCell.Elevation);
        if (b < 0)
            b = -b;

        Vector3 boundary = Vector3.Lerp(begin, left, b);
        Color boundaryColor = Color.Lerp(beginCell.color, leftCell.color, b);

        TriangulateBoundaryTriangle(
            right, rightCell, begin, beginCell, boundary, boundaryColor);

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(
                left, leftCell, right, rightCell, boundary, boundaryColor);
        }
        else
        {
            AddTriangle(left, right, boundary);
            AddTriangleColor(leftCell.color, rightCell.color, boundaryColor);
        }
    }

    /// <summary>
    /// Connect terraces to a cliff.
    /// Collapse each step towards a boundary point that lies along a cliff.
    /// 
    /// </summary>
    /// <param name="begin"></param>
    /// <param name="beginCell"></param>
    /// <param name="left"></param>
    /// <param name="leftCell"></param>
    /// <param name="boundary"></param>
    /// <param name="boundaryColor"></param>
    private void TriangulateBoundaryTriangle(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 boundary, Color boundaryColor)
    {
        Vector3 v2 = HexMetrics.TerraceLerp(begin, left, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, 1);

        // First collapsing step
        AddTriangle(begin, v2, boundary);
        AddTriangleColor(beginCell.color, c2, boundaryColor);

        // Collapsing steps in between
        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = HexMetrics.TerraceLerp(begin, left, i);
            c2 = HexMetrics.TerraceLerp(beginCell.color, leftCell.color, i);
            AddTriangle(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
        }

        // Last collapsing step
        AddTriangle(v2, left, boundary);
        AddTriangleColor(c2, leftCell.color, boundaryColor);
    }

    /// <summary>
    /// Add color data for each triangle when triangulating.
    /// </summary>
    /// <param name="color"></param>
    private void AddTriangleColor(Color color)
    {
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }

    /// <summary>
    /// Allows for adding separate color data for each vertex.
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    /// <param name="c3"></param>
    private void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }

    /// <summary>
    /// Method to add a triangle, given three vertex positions.
    /// Add vertices in order.
    /// Add the indices of the vertices to form a triangle.
    /// Remember before adding vertices: index of first vertex is equal to the length of the vertex list before adding new vertices to it.
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    private void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    /// <summary>
    /// Add a trapezoid shape to fill in the gap between hexagons.
    /// This shape contains 3 triangle shapes, so 6 triangles need to be added.
    /// </summary>
    /// <param name="v1"></param>
    /// <param name="v2"></param>
    /// <param name="v3"></param>
    /// <param name="v4"></param>
    private void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    /// <summary>
    /// Give each vertex of the trapezoid quad a colour.
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    /// <param name="c3"></param>
    /// <param name="c4"></param>
    private void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }

    /// <summary>
    /// Give each vertex of the rectangular quad a colour.
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    private void AddQuadColor(Color c1, Color c2)
    {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);
    }
}
