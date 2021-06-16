using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class EndlessTerrain : MonoBehaviour {
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    public const float maxViewDst = 450f;
    public Transform viewer;

    public static Vector2 viewerPosition;
    public static Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    public Material mapMaterial;

    public LODInfo[] lODInfo;
    int chunckSize;
    int chunksVisibleInviewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    // Start is called before the first frame update
    void Start() {
        mapGenerator = FindObjectOfType<MapGenerator>();
        chunckSize = MapGenerator.mapChuckSize - 1;
        chunksVisibleInviewDst = Mathf.RoundToInt(maxViewDst / chunckSize);

        UpdateVisibelChunks();

    }
    private void Update() {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate) {
            viewerPositionOld = viewerPosition;
            UpdateVisibelChunks();
        }
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
                    terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunckSize, lODInfo, transform, mapMaterial));
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

        LODInfo[] detailLevels;
        LODMesh[] lODMeshes;

        MapData mapData;
        bool mapDataReceived;
        int preLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material) {
            this.detailLevels = detailLevels;

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

            lODMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++) {
                lODMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunck);
            }

            mapGenerator.RequestMapData(position, onMapDataReceived);
        }

        void onMapDataReceived(MapData mapData) {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, mapData.heightMap.GetLength(0), mapData.heightMap.GetLength(1));
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunck();

        }

        public void UpdateTerrainChunck() {
            if (mapDataReceived) {
                float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDstFromNearestEdge <= maxViewDst;

                if (visible) {
                    int lodInd = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++) {
                        if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold) {
                            lodInd = i + 1;
                        } else {
                            break;
                        }
                    }

                    if (lodInd != preLODIndex) {
                        // update mesh lod
                        LODMesh lodMesh = lODMeshes[lodInd];
                        if (lodMesh.hasMesh) {
                            meshFilter.mesh = lodMesh.mesh;
                            preLODIndex = lodInd;
                        } else if (!lodMesh.hasRequestedMesh) {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                }

                SetVisible(visible);

            }

        }

        public void SetVisible(bool visible) {
            meshObj.SetActive(visible);
        }

        public bool IsVisible() {
            return meshObj.activeSelf;
        }
    }

    public class LODMesh {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        Action updateCb;

        public LODMesh(int lod, Action updateCb) {
            this.lod = lod;
            this.updateCb = updateCb;
        }

        public void OnMeshDataReceived(MeshData meshData) {
            mesh = meshData.CreateMesh();
            hasMesh = true;
            updateCb();
        }

        public void RequestMesh(MapData mapData) {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }

    }

    [Serializable]
    public struct LODInfo {
        public int lod;
        public float visibleDstThreshold;
    }
}
