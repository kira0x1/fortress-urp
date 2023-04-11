﻿using UnityEngine;

namespace Kira
{
    public struct MeshData
    {
        private readonly Vector3[] vertices;
        private readonly int[] triangles;
        private readonly Vector2[] uvs;

        private Vector3[] borderVertices;
        private int[] borderTriangles;

        private int triangleIndex;
        private int borderTriangleIndex;

        public MeshData(int verticesPerLine)
        {
            vertices = new Vector3[verticesPerLine * verticesPerLine];
            triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];
            triangleIndex = 0;
            uvs = new Vector2[verticesPerLine * verticesPerLine];

            borderVertices = new Vector3[verticesPerLine * 4 + 4];
            borderTriangles = new int[24 * verticesPerLine];
            borderTriangleIndex = 0;
        }

        public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
        {
            if (vertexIndex < 0)
            {
                borderVertices[-vertexIndex - 1] = vertexPosition;
            }
            else
            {
                vertices[vertexIndex] = vertexPosition;
                uvs[vertexIndex] = uv;
            }
        }

        public void AddTriangle(int a, int b, int c)
        {
            // Check if a border triangle
            if (a < 0 || b < 0 || c < 0)
            {
                borderTriangles[borderTriangleIndex] = a;
                borderTriangles[borderTriangleIndex + 1] = b;
                borderTriangles[borderTriangleIndex + 2] = c;
                borderTriangleIndex += 3;
            }
            else
            {
                triangles[triangleIndex] = a;
                triangles[triangleIndex + 1] = b;
                triangles[triangleIndex + 2] = c;
                triangleIndex += 3;
            }
        }

        private Vector3[] CalculateNormals()
        {
            Vector3[] vertexNormals = new Vector3[vertices.Length];
            int triangleCount = triangles.Length / 3;

            for (int i = 0; i < triangleCount; i++)
            {
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

            for (int i = 0; i < borderTriangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = borderTriangles[normalTriangleIndex];
                int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
                int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

                if (vertexIndexA >= 0)
                    vertexNormals[vertexIndexA] += triangleNormal;

                if (vertexIndexB >= 0)
                    vertexNormals[vertexIndexB] += triangleNormal;

                if (vertexIndexC >= 0)
                    vertexNormals[vertexIndexC] += triangleNormal;
            }

            for (var i = 0; i < vertexNormals.Length; i++)
            {
                vertexNormals[i].Normalize();
            }

            return vertexNormals;
        }

        private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
        {
            Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];
            Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];
            Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];

            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;

            return Vector3.Cross(sideAB, sideAC).normalized;
        }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.normals = CalculateNormals();

            return mesh;
        }
    }
}