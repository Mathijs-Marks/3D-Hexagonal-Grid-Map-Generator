using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class responsible for generating the noise map.
/// </summary>
public class NoiseMapGenerator : MonoBehaviour
{
    /// <summary>
    /// Enumerator to switch between different draw modes in the scene.
    /// </summary>
    public enum DrawMode
    {
        NoiseMap,
        ColourMap,
        Mesh
    };

    /// <summary>
    /// struct defining the contents of a type of terrain.
    /// </summary>
    [Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color colour;
    }

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public int octaves;
    [Range(0, 1)] public float persistence; // Always a value between 0 and 1!
    public float lacunarity;

    public int seed;
    public Vector2 offset;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public DrawMode drawMode;
    public TerrainType[] regions;

    /// <summary>
    /// Call on the GenerateNoiseMap method in PerlinNoise to create a noise map as a texture.
    /// </summary>
    public void GenerateMap()
    {
        // Get the noise map from PerlinNoise.
        float[,] noiseMap = PerlinNoise.GenerateNoiseMap(
            seed, mapWidth, mapHeight, noiseScale, octaves, persistence, lacunarity, offset);

        Color[] colourMap = new Color[mapWidth * mapHeight];

        /*
         * Determine the current height at the position of the noise map.
         * Check whether the current height matches with any region
         */
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        // Assign the colour corresponding to the region, to the position.
                        colourMap[y * mapWidth + x] = regions[i].colour;
                        break;
                    }
                }
            }
        }

        /*
         * Get a reference to the NoiseMapDisplay class.
         * If the DrawMode enum in the editor is equal to noise map, draw the noise map.
         * If the DrawMode enum in the editor is equal to the colour map, draw the colour map.
         * If the DrawMode enum in the editor is equal to the mesh, draw the mesh.
         */
        NoiseMapDisplay display = FindObjectOfType<NoiseMapDisplay>();
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve),
                TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
        }
    }

    /// <summary>
    /// Make sure that the mapWidth and Mapheight are always 1 or higher.
    /// Make sure that the lacunarity is always 1 or higher.
    /// Make sure that the octaves are never negative.
    /// </summary>
    private void OnValidate()
    {
        if (mapWidth < 1)
        {
            mapWidth = 1;
        }

        if (mapHeight < 1)
        {
            mapHeight = 1;
        }

        if (lacunarity < 1)
        {
            lacunarity = 1;
        }

        if (octaves < 0)
        {
            octaves = 0;
        }
    }
}
