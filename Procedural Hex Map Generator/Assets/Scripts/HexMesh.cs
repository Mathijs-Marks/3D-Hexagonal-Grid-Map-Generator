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

    private MeshCollider meshCollider;

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
    /// Start with first triangle:
    /// First vertex is center of hex, other two vertices are the first and second corners, relative to its center.
    /// Repeat this 5 more times to form a hexagon shape.
    /// Add color when triangulating.
    /// </summary>
    /// <param name="cell"></param>
    private void Triangulate(HexCell cell)
    {
        for (int i = 0; i < 6; i++)
        {
            Vector3 center = cell.transform.localPosition;
            AddTriangle(
                center,
                center + HexMetrics.corners[i],
                center + HexMetrics.corners[i + 1] // Tries to grab a seventh corner, which gives an error. Duplicate first corner to prevent going out of bounds.
            );
            AddTriangleColor(cell.color);
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
}
