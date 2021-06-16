using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class EndlessTerrain : MonoBehaviour {
    public const float maxViewDst = 450f;
    public Transform viewer;

    public static Vector2 viewerPosition;
    static MapGenerator mapGenerator;
    public Material mapMaterial;
    int chunckSize;
    int chunksVisibleInviewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    // Start is called before the first frame update
    void Start() {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunckSize = MapGenerator.mapChuckSize - 1;
        chunksVisibleInviewDst = Mathf.RoundToInt(maxViewDst / chunckSize);

    }
    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibelChunks();
    }

    void UpdateVisibelChunks() {
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunckSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunckSize);

        for (int yOffset = -chunksVisibleInviewDst; yOffset <= chunksVisibleInviewDst; yOffset++) {
            for (int xOffset = -chunksVisibleInviewDst; xOffset <= chunksVisibleInviewDst; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDict.ContainsKey(viewedChunkCoord)) {
                    terrainChunkDict[viewedChunkCoord].UpdateTerrainChunck();
                    if (terrainChunkDict[viewedChunkCoord].IsVisible()) {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDict[viewedChunkCoord]);
                    }
                } else {
                    terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunckSize, transform, mapMaterial));
                }
            }

        }

    }

    public class TerrainChunk {
        GameObject meshObj;
        Vector2 position;

        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        public TerrainChunk(Vector2 coord, int size, Transform parent, Material material) {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            // meshObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObj = new GameObject("Terrain Chunk");

            meshRenderer = meshObj.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshFilter = meshObj.AddComponent<MeshFilter>();

            meshObj.transform.position = positionV3;
            // meshObj.transform.localScale = Vector3.one * size / 10f;
            meshObj.transform.parent = parent;
            SetVisible(false);

            mapGenerator.RequestMapData(onMapDataReceived);
        }

        void onMapDataReceived(MapData mapData) {
            mapGenerator.RequestMeshData(mapData, onMeshDataReceived);

        }
        void onMeshDataReceived(MeshData meshData) {
            meshFilter.mesh = meshData.CreateMesh();

        }

        public void UpdateTerrainChunck() {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDst;
            SetVisible(visible);

        }

        public void SetVisible(bool visible) {
            meshObj.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObj.activeSelf;
        }
    }
}
