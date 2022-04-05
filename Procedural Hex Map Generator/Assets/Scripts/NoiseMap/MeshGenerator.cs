using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
/// Responsible for generating the terrain mesh on the screen.
/// </summary>
public static class MeshGenerator
{
    /// <summary>
    /// Generate the terrain mesh, based on the height map.
    /// </summary>
    /// <param name="heightMap"></param>
    /// <param name="heightMultiplier"></param>
    /// <param name="heightCurve"></param>
    /// <returns></returns>
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve heightCurve)
    {
        // Get the width and height from the height map by using the array lengths.
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        
        // Used to center the positions of the vertex.
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        /*
         * Loop through the height map and create the vertices for each position.
         */
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                /*
                 * Create a vector3 for each vertex.
                 * The x value is equal to the centered position of height map x value.
                 * The y value is equal to the height map position times the height multiplier.
                 * the z value is equal to the centered position of the height map y value.
                 */
                meshData.vertices[vertexIndex] =
                    new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                // Ignore the right and bottom vertices
                if (x < width - 1 && y < height - 1)
                {
                    /*
                     * Draw the first and second set of triangles in this order:
                     *
                     * (i), (i + w + 1), (i + w)
                     * (i + w + 1), (i), (i + 1)
                     */
                    meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}


/// <summary>
/// Class containing all the necessary data to create mesh shapes.
/// </summary>
public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;

    private int triangleIndex;

    /// <summary>
    /// Initialize the vertices, triangles, and uvs.
    /// The amount of vertices is equal to the width and the height of the mesh.
    /// The amount of triangles is equal to the width -1 and height -1 of the mesh, times 6.
    /// The amount of uvs is equal to the width and the height of the mesh.
    /// </summary>
    /// <param name="meshWidth"></param>
    /// <param name="meshHeight"></param>
    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
        uvs = new Vector2[meshWidth * meshHeight];
    }

    /// <summary>
    /// Add a triangle for each of the three vertices in a triangle.
    /// Increment the index by three to switch to the next triangle position.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    /// <summary>
    /// Create the mesh using the determined values above.
    /// </summary>
    /// <returns></returns>
    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}