using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator {
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail) {
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);
        int meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        int borderedSize = heightMap.GetLength(0);
        int meshSize = borderedSize - 2 * meshSimplificationIncrement;
        int meshSizeUnsimplified = borderedSize - 2;

        float topLeftX = (meshSizeUnsimplified - 1f) / -2f;
        float topLeftZ = (meshSizeUnsimplified - 1f) / 2f;

        int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

        MeshData meshData = new MeshData(verticesPerLine);

        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
        int meshVertexIndex = 0;
        int borderedVertexIndex = -1;

        for (int j = 0; j < borderedSize; j += meshSimplificationIncrement) {
            for (int i = 0; i < borderedSize; i += meshSimplificationIncrement) {
                bool isBorderVertex = i == 0 || i == borderedSize - 1 || j == 0 || j == borderedSize - 1;

                if (isBorderVertex) {
                    vertexIndicesMap[i, j] = borderedVertexIndex--;
                } else {
                    vertexIndicesMap[i, j] = meshVertexIndex++;
                }
            }
        }

        for (int j = 0; j < borderedSize; j += meshSimplificationIncrement) {
            for (int i = 0; i < borderedSize; i += meshSimplificationIncrement) {
                int vertexIndex = vertexIndicesMap[i, j];
                // lock (heightCurve) {
                // meshData.uvs[vertexIndex]
                Vector2 percent = new Vector2((float)(i - meshSimplificationIncrement) / (float)meshSize, (float)(j - meshSimplificationIncrement) / (float)meshSize);
                // it will create non-discrete faces; so the light will bend on edges;
                // meshData.vertices[vertexIndex]
                float height = heightCurve.Evaluate(heightMap[i, j]) * heightMultiplier;
                Vector3 vertexPos = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);
                // }

                meshData.AddVertex(vertexPos, percent, vertexIndex);

                if (j < borderedSize - 1 && i < borderedSize - 1) {
                    int a = vertexIndicesMap[i, j];
                    int b = vertexIndicesMap[i + meshSimplificationIncrement, j];
                    int c = vertexIndicesMap[i, j + meshSimplificationIncrement];
                    int d = vertexIndicesMap[i + meshSimplificationIncrement, j + meshSimplificationIncrement];
                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }
}

public class MeshData {
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    Vector3[] borderVertices;
    int[] borderTriangles;

    int triangleIndex = 0;
    int borderTriangleIndex = 0;

    public MeshData(int verticesPerLine) {
        // triangleIndex = 0;
        vertices = new Vector3[verticesPerLine * verticesPerLine];
        uvs = new Vector2[verticesPerLine * verticesPerLine];
        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];

        borderVertices = new Vector3[verticesPerLine * 4 + 4];
        borderTriangles = new int[24 * verticesPerLine];
    }

    public void AddVertex(Vector3 vertexPos, Vector2 uv, int vertexInd) {
        if (vertexInd < 0) {
            borderVertices[-vertexInd - 1] = vertexPos;
        } else {
            vertices[vertexInd] = vertexPos;
            uvs[vertexInd] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c) {
        if (a < 0 || b < 0 || c < 0) {
            borderTriangles[borderTriangleIndex++] = a;
            borderTriangles[borderTriangleIndex++] = b;
            borderTriangles[borderTriangleIndex++] = c;
        } else {
            triangles[triangleIndex++] = a;
            triangles[triangleIndex++] = b;
            triangles[triangleIndex++] = c;
        }
    }

    Vector3[] CalculateNormals() {
        Vector3[] vertexNormals = new Vector3[vertices.Length];
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++) {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = borderTriangles.Length / 3;
        for (int i = 0; i < borderTriangleCount; i++) {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = borderTriangles[normalTriangleIndex];
            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0) vertexNormals[vertexIndexA] += triangleNormal;
            if (vertexIndexB >= 0) vertexNormals[vertexIndexB] += triangleNormal;
            if (vertexIndexC >= 0) vertexNormals[vertexIndexC] += triangleNormal;
        }

        for (int i = 0; i < vertexNormals.Length; i++) {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indA, int indB, int indC) {
        Vector3 pointA = indA < 0 ? borderVertices[-indA - 1] : vertices[indA];
        Vector3 pointB = indB < 0 ? borderVertices[-indB - 1] : vertices[indB];
        Vector3 pointC = indC < 0 ? borderVertices[-indC - 1] : vertices[indC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        // mesh.RecalculateNormals();
        mesh.normals = CalculateNormals();
        return mesh;
    }

}