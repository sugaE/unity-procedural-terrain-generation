using System;
using Unity.Mathematics;
using UnityEngine;

public static class Noise
{
    public enum NormalizeMode { Local, Global }

    /// <summary>summary
    /// <param name="octaves">how many noise maps are added</param>
    /// <param name="persistence"></param>
    /// <param name="lacunarity"> </param>
    /// <returns>float[width,height] noise map</returns>
    /// </summary>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
    {

        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Random.InitState(seed); // InitState can only be called from the main thread.
        System.Random prgn = new System.Random(seed);
        Vector2[] octavesOffset = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1f;
        float frequency = 1f;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prgn.Next(-100000, 100000) + offset.x;
            float offsetY = prgn.Next(-100000, 100000) - offset.y;
            octavesOffset[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }

        if (scale <= 0)
        {
            scale = 1f;
        }

        float maxNoiseHLocal = float.MinValue;
        float minNoiseHLocal = float.MaxValue;

        float halfW = mapWidth / 2f;
        float halfH = mapHeight / 2f;

        for (int j = 0; j < mapHeight; j++)
        {
            for (int i = 0; i < mapWidth; i++)
            {
                amplitude = 1f;
                frequency = 1f;
                float noiseHeight = 0f;

                for (int k = 0; k < octaves; k++)
                {
                    float sampleX = (i - halfW + octavesOffset[k].x) / scale * frequency;
                    float sampleY = (j - halfH + octavesOffset[k].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;

                }
                maxNoiseHLocal = Mathf.Max(maxNoiseHLocal, noiseHeight);
                minNoiseHLocal = Mathf.Min(minNoiseHLocal, noiseHeight);

                noiseMap[i, j] = noiseHeight;
            }
        }

        // normalize noiseMap
        for (int j = 0; j < mapHeight; j++)
        {
            for (int i = 0; i < mapWidth; i++)
            {
                if (normalizeMode == NormalizeMode.Local) {
                    noiseMap[i, j] = Mathf.InverseLerp(minNoiseHLocal, maxNoiseHLocal, noiseMap[i, j]);
                } else {
                    //todo how to best estimate the maxPossibleHeight; the 0.7 is approximate;
                    float normalizedHeight = (noiseMap[i, j] + 1) / (maxPossibleHeight / 0.8f);
                    noiseMap[i, j] = Mathf.Clamp(normalizedHeight, 0, float.MaxValue);
                }
            }
        }

        return noiseMap;

    }

}
