using System;
using System.Collections;
using System.Collections.Generic;
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

        // Create square quad with these vectors.
        // Bridge quad only needs two colours.
        AddQuad(v1, v2, v3, v4);
        AddQuadColor(cell.color, neighbour.color);

        HexCell nextNeighbour = cell.GetNeighbour(direction.Next());
        if (direction <= HexDirection.E && nextNeighbour != null)
        {
            AddTriangle(v2, v4, v2 + HexMetrics.GetBridge(direction.Next()));
            AddTriangleColor(cell.color, neighbour.color, nextNeighbour.color);
        }
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
