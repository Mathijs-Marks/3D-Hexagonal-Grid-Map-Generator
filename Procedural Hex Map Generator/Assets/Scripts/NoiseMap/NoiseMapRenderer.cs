using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Take the noise map from NoiseMapGenerator and draw it as a texture on a plane.
/// </summary>
public class NoiseMapRenderer : MonoBehaviour
{
    // References for rendering texture on a mesh.
    public Renderer textureRenderer;

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
}
