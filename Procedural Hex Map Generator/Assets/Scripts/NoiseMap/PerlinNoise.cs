using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generate a 2D grid of values between 0 and 1, also known as a noise map.
/// </summary>
public static class PerlinNoise
{
    /// <summary>
    /// Method responsible for creating a 2D array of floats, needed for the grid.
    /// Array values are influenced based on modifiers, such as octaves and lacunarity.
    /// See Perlin Noise at https://kihontekina.dev/posts/perlin_noise_part_one/
    /// </summary>
    /// <param name="seed"></param>
    /// <param name="mapWidth"></param>
    /// <param name="mapHeight"></param>
    /// <param name="scale"></param>
    /// <param name="octaves"></param>
    /// <param name="persistence"></param>
    /// <param name="lacunarity"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static float[,] GenerateNoiseMap(
        int seed, int mapWidth, int mapHeight, 
        float scale, int octaves, float persistence, 
        float lacunarity, Vector2 offset)
    {
        /*
         * Create a pseudo-random number and assign it to the seed.
         * This will produce a different noise map for each different seed.
         * To sample each octave from a different location, create an array of offsets for the amount of octaves.
         */
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        // Loop through all of the octaves (iterations)
        for (int i = 0; i < octaves; i++)
        {
            /*
             * Take a random number as the offset value for X and Y and assign them to the octaveOffsets index.
             * Use own provided offset value to be able to scroll through the noise
             */
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Instantiate the 2D float array, giving it the size of the mapWidth and mapHeight.
        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Create two extreme values for the maximum and minimum noise height. Used for normalizing.
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        // Used to zoom in on the noise map from the middle, instead of the top-right
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        // Prevent division by 0 errors.
        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        // Loop through the X and Y values of the noise map.
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // Create amplitude, frequency, and height values, which will be changed during each octave.
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                // Depending on the amount of octaves, do the following:
                for (int i = 0; i < octaves; i++)
                {
                    /*
                     * Sample an X and Y coordinate and apply Perlin Noise to it.
                     * Change the height of that coordinate, according to the Perlin noise value.
                     * The higher the frequency, the further apart the sample points will be, meaning that the height values will change more rapidly.
                     * Add the octave offsets for sampling from different locations.
                     * Subtract the halfWidth and halfHeight in order to zoom from the center.
                     */
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffsets[i].y;
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // Multiply by 2 and subtract 1 to allow for negative Perlin noise values.
                    
                    /*
                     * Increase the noise height by multiplying the amplitude with the perlinValue.
                     * Multiply the current amplitude with the persistence value, decreasing the amplitude of each octave.
                     * Multiply the current frequency with the lacunarity value, increasing the frequency of each octave.
                     */
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                // Normalize noise values
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if(noiseHeight < minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }

                // Apply the noise height to the noise map values.
                noiseMap[x, y] = noiseHeight;
            }
        }

        /*
         * Loop through all of the noise map values again.
         * For each value in the noise map, do an Inverse Lerp.
         * So if the noise map value is equal to the minNoiseHeight, return zero.
         * If the noise map value is equal to the maxNoiseHeight, return one.
         * If it's somewhere in between (for instance 40%), return the inbetween value (0.4).
         */
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
