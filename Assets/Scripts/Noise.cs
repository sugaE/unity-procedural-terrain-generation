using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale)
    {

        float[,] noiseMap = new float[mapWidth, mapHeight];

        if (scale <= 0)
        {
            scale = 1f;
        }

        for (int j = 0; j < mapHeight; j++)
        {
            for (int i = 0; i < mapWidth; i++)

            {
                float sampleX = i / scale;
                float sampleY = j / scale;

                float noise = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[i, j] = noise;
            }
        }
        return noiseMap;

    }

}
