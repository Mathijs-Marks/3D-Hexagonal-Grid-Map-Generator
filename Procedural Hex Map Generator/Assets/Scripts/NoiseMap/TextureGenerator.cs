using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generate the texture of the colour map.
/// </summary>
public static class TextureGenerator
{
    /// <summary>
    /// Create a texture the size of the noise map, add a colour map to it.
    /// Return the texture. The actual colour map is created in TextureFromHeightMap.
    /// </summary>
    /// <param name="colourMap"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <returns></returns>
    public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colourMap);
        texture.Apply();
        return texture;
    }

    /// <summary>
    /// Determine the texture size by taking the size of the noise map.
    /// Apply colours between black and white, based on each value of the noise map.
    /// Use this noise map in TextureFromColourMap to return a completed colour map.
    /// </summary>
    /// <param name="heightMap"></param>
    /// <returns></returns>
    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        // Determine the size of the texture by retrieving the size of the X and Y dimensions of the height map.
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        // Create a colour map, using the determined size of the texture.
        Color[] colourMap = new Color[width * height];
        
        // Loop through all of the values in the noise map and set a colour for each value.
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                /*
                 * ColourMap is an 1-dimensional array, while the heightMap is a 2-dimensional array.
                 * To get the right index, first multiply Y by the width of the map.
                 * This gives the index of the current row.
                 * To get the column, add the X value.
                 * Set the colour to a value between black and white for each value on the noise map.
                 */
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }

        return TextureFromColourMap(colourMap, width, height);
    }
}
