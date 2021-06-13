using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {
    public static MeshData GenerateTerrainMesh(float[,] heightMap) {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1f) / -2f;
        float topLeftZ = (height - 1f) / 2f;

        MeshData meshData = new MeshData(width, height);
        int vertexIndex = 0;

        for (int j = 0; j < height; j++) {
            for (int i = 0; i < width; i++) {
                // it will create non-discrete faces; so the light will bend on edges;
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + i, heightMap[i, j], topLeftZ - j);
                meshData.uvs[vertexIndex] = new Vector2((float)i / (float)width, (float)j / (float)height);

                if (j < height - 1 && i < width - 1) {
                    meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData {
    public Vector3[] vertices;
    public int[] triangles;

    public Vector2[] uvs;

    int triangleIndex = 0;

    public MeshData(int meshWidth, int meshHeight) {
        triangleIndex = 0;
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c) {
        triangles[triangleIndex++] = a;
        triangles[triangleIndex++] = b;
        triangles[triangleIndex++] = c;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }

}