using System;
using UnityEngine;

public static class Noise
{

    /// <summary>summary
    /// <param name="octaves">how many noise maps are added</param>
    /// <param name="persistence"></param>
    /// <param name="lacunarity"> </param>
    /// <returns>float[width,height] noise map</returns>
    /// </summary>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset)
    {

        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Random.InitState(seed); // InitState can only be called from the main thread.
        System.Random prgn = new System.Random(seed);
        Vector2[] octavesOffset = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prgn.Next(-100000, 100000) + offset.x;
            float offsetY = prgn.Next(-100000, 100000) + offset.y;
            octavesOffset[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0)
        {
            scale = 1f;
        }

        float maxNoiseH = float.MinValue;
        float minNoiseH = float.MaxValue;

        float halfW = mapWidth / 2f;
        float halfH = mapHeight / 2f;

        for (int j = 0; j < mapHeight; j++)
        {
            for (int i = 0; i < mapWidth; i++)

            {
                float amplitude = 1f;
                float frequency = 1f;
                float noiseHeight = 0f;

                for (int k = 0; k < octaves; k++)
                {
                    float sampleX = (i - halfW) / scale * frequency + octavesOffset[k].x;
                    float sampleY = (j - halfH) / scale * frequency + octavesOffset[k].y;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;

                }
                maxNoiseH = Mathf.Max(maxNoiseH, noiseHeight);
                minNoiseH = Mathf.Min(minNoiseH, noiseHeight);

                noiseMap[i, j] = noiseHeight;
            }
        }

        // normalize noiseMap
        for (int j = 0; j < mapHeight; j++)
        {
            for (int i = 0; i < mapWidth; i++)
            {
                noiseMap[i, j] = Mathf.InverseLerp(minNoiseH, maxNoiseH, noiseMap[i, j]);
            }
        }

        return noiseMap;

    }

}
