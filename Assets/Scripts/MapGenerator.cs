using System;
using UnityEngine;

public enum DrawMode
{
    Mesh,
    NoiseMap,
    ColorMap
}
public class MapGenerator : MonoBehaviour {
    public DrawMode drawMode;
    const int mapChuckSize = 241;//240 can be divided by 1,2,4,6,8,10,12
    [Range(0, 6)]
    public int levelOfDetail;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistence;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public TerrainType[] regions;
    public bool autoUpdate;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChuckSize, mapChuckSize, seed, noiseScale, octaves, persistence, lacunarity, offset);

        Color[] colorMap = new Color[mapChuckSize * mapChuckSize];

        for (int j = 0; j < mapChuckSize; j++) {
            for (int i = 0; i < mapChuckSize; i++) {
                float currentH = noiseMap[i, j];
                for (int k = 0; k < regions.Length; k++) {
                    if (currentH <= regions[k].height) {
                        colorMap[j * mapChuckSize + i] = regions[k].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        } else if (drawMode == DrawMode.ColorMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapChuckSize, mapChuckSize));

        } else if (drawMode == DrawMode.Mesh) {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colorMap, mapChuckSize, mapChuckSize));

        }
    }

    private void OnValidate()
    {
        // if (mapChuckSize < 1) mapChuckSize = 1;
        // if (mapChuckSize < 1) mapChuckSize = 1;
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;

    }

}

[Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}