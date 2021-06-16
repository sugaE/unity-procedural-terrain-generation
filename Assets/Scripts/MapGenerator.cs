using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public enum DrawMode
{
    Mesh,
    NoiseMap,
    ColorMap
}
public class MapGenerator : MonoBehaviour {
    public DrawMode drawMode;
    public const int mapChuckSize = 241;//240 can be divided by 1,2,4,6,8,10,12
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

    Queue<MapthreadInfo<MapData>> mapDataThreadInfoQ = new Queue<MapthreadInfo<MapData>>();
    Queue<MapthreadInfo<MeshData>> meshDataThreadInfoQ = new Queue<MapthreadInfo<MeshData>>();

    public void DrawMapInEditor() {
        MapData mapData = GenerateMapData();

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
        } else if (drawMode == DrawMode.ColorMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChuckSize, mapChuckSize));

        } else if (drawMode == DrawMode.Mesh) {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColorMap(mapData.colorMap, mapChuckSize, mapChuckSize));

        }
    }

    public void RequestMapData(Action<MapData> callback) {
        ThreadStart threadStart = delegate {
            MapDataThread(callback);
        };

        new Thread(threadStart).Start();
    }
    public void RequestMeshData(MapData mapData, Action<MeshData> callback) {
        ThreadStart threadStart = delegate {
            MeshDataThread(mapData, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, Action<MeshData> callback) {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail);
        lock (meshDataThreadInfoQ) {
            meshDataThreadInfoQ.Enqueue(new MapthreadInfo<MeshData>(callback, meshData));
        }
    }
    void MapDataThread(Action<MapData> callback) {
        MapData mapData = GenerateMapData();
        lock (mapDataThreadInfoQ) {
            mapDataThreadInfoQ.Enqueue(new MapthreadInfo<MapData>(callback, mapData));
        }
    }

    private void Update() {
        if (mapDataThreadInfoQ.Count > 0) {
            for (int i = 0; i < mapDataThreadInfoQ.Count; i++) {
                MapthreadInfo<MapData> threadInfo = mapDataThreadInfoQ.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
        if (meshDataThreadInfoQ.Count > 0) {
            for (int i = 0; i < meshDataThreadInfoQ.Count; i++) {
                MapthreadInfo<MeshData> threadInfo = meshDataThreadInfoQ.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    public MapData GenerateMapData()
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
        return new MapData(noiseMap, colorMap);

    }

    private void OnValidate()
    {
        // if (mapChuckSize < 1) mapChuckSize = 1;
        // if (mapChuckSize < 1) mapChuckSize = 1;
        if (lacunarity < 1) lacunarity = 1;
        if (octaves < 0) octaves = 0;

    }

    struct MapthreadInfo<T> {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapthreadInfo(Action<T> callback, T parameter) {
            this.callback = callback;
            this.parameter = parameter;
        }
    }

}

[Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

public struct MapData {

    public readonly float[,] heightMap;
    public readonly Color[] colorMap;

    public MapData(float[,] heightMap, Color[] colorMap) {
        this.heightMap = heightMap;
        this.colorMap = colorMap;
    }

}