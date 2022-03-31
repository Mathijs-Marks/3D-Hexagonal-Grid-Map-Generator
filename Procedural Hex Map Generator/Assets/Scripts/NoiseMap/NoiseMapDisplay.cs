using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Texture2D texture = new Texture2D(width, height);

        Color[] colourMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                /*
                 * ColourMap is an 1-dimensional array, while the noiseMap is a 2-dimensional array.
                 * To get the right index, first multiply Y by the width of the map.
                 * This gives the index of the current row.
                 * To get the column, add the X value.
                 */
                colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        texture.SetPixels(colourMap);
        texture.Apply();

        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(width, 1, height);
    }
}
