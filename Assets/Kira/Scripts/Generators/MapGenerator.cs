using System;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

namespace Kira
{
    public class MapGenerator : MonoBehaviour
    {
        public enum DrawMode
        {
            NoiseMap,
            ColorMap,
            Mesh,
            FalloffMap
        }

        public TerrainData terrainData;
        public NoiseData noiseData;

        public DrawMode drawMode;

        [Range(0, 6)]
        public int editorLevelOfDetail;
        public bool autoUpdate = true;

        public TerrainType[] regions;
        private float[,] falloffMap;
        private static MapGenerator Instance;

        private Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
        private Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

        public static int MapChunkSize
        {
            get
            {
                if (Instance == null) Instance = FindObjectOfType<MapGenerator>();
                return Instance.terrainData.useFlatShading ? 95 : 239;
            }
        }

        private void Awake()
        {
            falloffMap = FalloffGenerator.GenerateFalloffMap(MapChunkSize);
        }

        private void OnValuesUpdated()
        {
            if (!Application.isPlaying)
            {
                DrawMapInEditor();
            }
        }

        public void DrawMapInEditor()
        {
            MapData mapData = GenerateMapData(Vector2.zero);

            MapDisplay display = FindObjectOfType<MapDisplay>();

            if (drawMode == DrawMode.NoiseMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heightMap));
            }
            else if (drawMode == DrawMode.ColorMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromColorMap(mapData.colorMap, MapChunkSize, MapChunkSize));
            }
            else if (drawMode == DrawMode.Mesh)
            {
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, editorLevelOfDetail, terrainData.useFlatShading), TextureGenerator.TextureFromColorMap(mapData.colorMap, MapChunkSize, MapChunkSize));
            }
            else if (drawMode == DrawMode.FalloffMap)
            {
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(MapChunkSize)));
            }
        }

        public void RequestMapData(Vector2 center, Action<MapData> callback)
        {
            void ThreadStart()
            {
                MapDataThread(center, callback);
            }

            new Thread(ThreadStart).Start();
        }

        public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
        {
            void ThreadStart()
            {
                MeshDataThread(mapData, lod, callback);
            }

            new Thread(ThreadStart).Start();
        }

        private void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
        {
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, terrainData.meshHeightMultiplier, terrainData.meshHeightCurve, lod, terrainData.useFlatShading);

            lock (meshDataThreadInfoQueue)
            {
                meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
            }
        }

        private void MapDataThread(Vector2 center, Action<MapData> callback)
        {
            MapData mapData = GenerateMapData(center);

            lock (mapDataThreadInfoQueue)
            {
                mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
            }
        }

        private void Update()
        {
            if (mapDataThreadInfoQueue.Count > 0)
            {
                for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
                {
                    MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.paramater);
                }
            }

            if (meshDataThreadInfoQueue.Count > 0)
            {
                for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
                {
                    MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.paramater);
                }
            }
        }

        private MapData GenerateMapData(Vector2 center)
        {
            // + 2 to compensate for chunk border
            float[,] noiseMap = Noise.GenerateNoiseMap(MapChunkSize + 2, MapChunkSize + 2, noiseData.seed, noiseData.noiseScale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, center + noiseData.offset, noiseData.normalizeMode);


            Color[] colorMap = new Color[MapChunkSize * MapChunkSize];

            for (int y = 0; y < MapChunkSize; y++)
            {
                for (int x = 0; x < MapChunkSize; x++)
                {
                    if (terrainData.useFalloff)
                    {
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);
                    }

                    float currentHeight = noiseMap[x, y];
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (currentHeight >= regions[i].height)
                        {
                            colorMap[y * MapChunkSize + x] = regions[i].color;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return new MapData(noiseMap, colorMap);
        }

        private void OnValidate()
        {
            if (terrainData != null)
            {
                terrainData.OnValuesUpdated -= OnValuesUpdated;
                terrainData.OnValuesUpdated += OnValuesUpdated;
            }

            if (noiseData != null)
            {
                noiseData.OnValuesUpdated -= OnValuesUpdated;
                noiseData.OnValuesUpdated += OnValuesUpdated;
            }

            falloffMap = FalloffGenerator.GenerateFalloffMap(MapChunkSize);
        }

        private struct MapThreadInfo<T>
        {
            public readonly Action<T> callback;
            public readonly T paramater;

            public MapThreadInfo(Action<T> callback, T paramater)
            {
                this.callback = callback;
                this.paramater = paramater;
            }
        }
    }
}