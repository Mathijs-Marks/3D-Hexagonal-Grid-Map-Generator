using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapGenerator : MonoBehaviour
{
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;

    public bool autoUpdate;

    public void GenerateMap()
    {
        float[,] noiseMap = PerlinNoise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale);

        NoiseMapDisplay display = FindObjectOfType<NoiseMapDisplay>();
        display.DrawNoiseMap(noiseMap);
    }
}
