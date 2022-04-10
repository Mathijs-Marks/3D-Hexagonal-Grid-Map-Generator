using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexMapGenerator : MonoBehaviour
{
    [Serializable]
    public struct TerrainType
    {
        public string name;
        public float height;
        public Color colour;
    }

    public HexGrid grid;

    public int seed;
    private int mapWidth;
    private int mapHeight;
    public float noiseScale;
    public int octaves;
    [Range(0, 1)] public float persistence;// Always a value between 0 and 1!
    public float lacunarity;
    public Vector2 offset;
    public float heightMultiplier;

    public bool autoUpdate;

    public TerrainType[] regions;

    public void Awake()
    {
        mapWidth = grid.width;
        mapHeight = grid.height;
    }

    public void GenerateMap()
    {
        // Get the noise map from PerlinNoise.
        float[,] noiseMap = PerlinNoise.GenerateNoiseMap(
            seed, mapWidth, mapHeight, noiseScale, octaves, persistence, lacunarity, offset);

        Color[] colourMap = new Color[mapWidth * mapHeight];

        float topLeftX = (grid.width -1 / 2f);
        float topLeftZ = (grid.height -1 / -2f);

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
                        continue;
                    }
                }
            }
        }

        NoiseMapRenderer renderer = FindObjectOfType<NoiseMapRenderer>();
        renderer.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));

        for (int y = 0, index = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                Vector3 noiseMapPosition = new Vector3((topLeftX + x * 10), noiseMap[x,y], (topLeftZ + y * 10));
                HexCell cell = grid.GetCell(noiseMapPosition);

                for (int i = 0; i < regions.Length; i++)
                {
                    if (cell.Elevation <= regions[i].height)
                    {
                        cell.color = regions[i].colour;
                        cell.Elevation = (int) regions[i].height * (int) heightMultiplier;
                        break;
                    }
                }
                index++;
            }
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
