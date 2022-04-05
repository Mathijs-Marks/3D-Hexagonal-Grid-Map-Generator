using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Take the noise map from NoiseMapGenerator and draw it as a texture on a plane.
/// </summary>
public class NoiseMapDisplay : MonoBehaviour
{
    // References for rendering texture on a mesh.
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    /// <summary>
    /// Apply the generated texture to the texture renderer in Edit mode.
    /// Set the plane to the same size of the texture map.
    /// </summary>
    /// <param name="texture"></param>
    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    /// <summary>
    /// Apply the mesh and the texture and draw the mesh on the screen.
    /// </summary>
    /// <param name="meshData"></param>
    /// <param name="texture"></param>
    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }
}
