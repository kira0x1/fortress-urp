using UnityEngine;

// ReSharper disable Unity.InefficientMultidimensionalArrayUsage

namespace Kira
{
    public static class MeshGenerator
    {
        public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail, bool useFlatShading)
        {
            AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

            int meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;

            int borderedSize = heightMap.GetLength(0);
            int meshSize = borderedSize - 2 * meshSimplificationIncrement;
            int meshSizeUnsimplified = borderedSize - 2;

            float topLeftX = (meshSizeUnsimplified - 1) / -2f;
            float topLeftZ = (meshSizeUnsimplified - 1) / 2f;

            int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

            MeshData meshData = new MeshData(verticesPerLine, useFlatShading);

            int[,] vertexIndicesMap = new int[borderedSize, borderedSize];
            int meshVertexIndex = 0;
            int borderVertexIndex = -1;

            for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
            {
                for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
                {
                    bool isBorderVertex = y == 0 || y == borderedSize - 1 || x == 0 || x == borderedSize - 1;

                    if (isBorderVertex)
                    {
                        vertexIndicesMap[x, y] = borderVertexIndex;
                        borderVertexIndex--;
                    }
                    else
                    {
                        vertexIndicesMap[x, y] = meshVertexIndex;
                        meshVertexIndex++;
                    }
                }
            }

            for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
            {
                for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
                {
                    int vertexIndex = vertexIndicesMap[x, y];

                    Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);
                    float height = heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;
                    Vector3 vertexPosition = new Vector3(topLeftX + percent.x * meshSizeUnsimplified, height, topLeftZ - percent.y * meshSizeUnsimplified);

                    meshData.AddVertex(vertexPosition, percent, vertexIndex);

                    // if not the right or bottom edges of the map
                    if (x < borderedSize - 1 && y < borderedSize - 1)
                    {
                        int a = vertexIndicesMap[x, y];
                        int b = vertexIndicesMap[x + meshSimplificationIncrement, y];
                        int c = vertexIndicesMap[x, y + meshSimplificationIncrement];
                        int d = vertexIndicesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];

                        meshData.AddTriangle(a, d, c);
                        meshData.AddTriangle(d, a, b);
                    }

                    vertexIndex++;
                }
            }

            meshData.ProcessMesh();

            return meshData;
        }
    }
}