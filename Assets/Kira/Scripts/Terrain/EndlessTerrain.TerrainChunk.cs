using UnityEngine;

namespace Kira
{
    public partial class EndlessTerrain
    {
        public class TerrainChunk
        {
            private GameObject meshObject;
            private Vector2 position;
            private Bounds bounds;

            private MeshRenderer meshRenderer;
            private MeshFilter meshFilter;
            private MeshCollider meshCollider;

            private LODInfo[] detailLevels;
            private LODMesh[] lodMeshes;
            private LODMesh collisionLODMesh;

            private MapData mapData;
            private bool mapDataRecieved;

            private int previousLODIndex = -1;

            public TerrainChunk(Vector2 coord, int size, LODInfo[] detailLevels, Transform parent, Material material)
            {
                this.detailLevels = detailLevels;

                position = coord * size;
                bounds = new Bounds(position, Vector2.one * size);
                Vector3 positionV3 = new Vector3(position.x, 0, position.y);

                meshObject = new GameObject("Terrain Chunk");
                meshRenderer = meshObject.AddComponent<MeshRenderer>();
                meshFilter = meshObject.AddComponent<MeshFilter>();
                meshCollider = meshObject.AddComponent<MeshCollider>();
                meshRenderer.material = material;

                meshObject.transform.position = positionV3 * mapGenerator.terrainData.uniformScale;
                meshObject.transform.SetParent(parent);
                meshObject.transform.localScale = Vector3.one * mapGenerator.terrainData.uniformScale;
                SetVisible(false);

                lodMeshes = new LODMesh[detailLevels.Length];

                for (int i = 0; i < detailLevels.Length; i++)
                {
                    lodMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateChunk);
                    if (detailLevels[i].useForCollider)
                    {
                        collisionLODMesh = lodMeshes[i];
                    }
                }

                mapGenerator.RequestMapData(position, OnMapDataRecieved);
            }

            private void OnMapDataRecieved(MapData mapData)
            {
                this.mapData = mapData;
                mapDataRecieved = true;

                Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.MapChunkSize, MapGenerator.MapChunkSize);
                meshRenderer.material.mainTexture = texture;

                UpdateChunk();
            }

            public void UpdateChunk()
            {
                if (!mapDataRecieved) return;

                float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistFromNearestEdge <= maxViewDist;

                if (visible)
                {
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDistFromNearestEdge > detailLevels[i].visibleDistThreshhold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh lodMesh = lodMeshes[lodIndex];
                        if (lodMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = lodMesh.mesh;
                            meshCollider.sharedMesh = lodMesh.mesh;
                        }
                        else if (!lodMesh.hasRequestedMesh)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }

                    if (lodIndex == 0)
                    {
                        if (collisionLODMesh.hasMesh)
                        {
                            meshCollider.sharedMesh = collisionLODMesh.mesh;
                        }
                        else if (!collisionLODMesh.hasRequestedMesh)
                        {
                            collisionLODMesh.RequestMesh(mapData);
                        }
                    }

                    terrainChunksVisibleLastUpdate.Add(this);
                }

                SetVisible(visible);
            }

            public void SetVisible(bool visible)
            {
                meshObject.SetActive(visible);
            }

            public bool IsVisible()
            {
                return meshObject.activeSelf;
            }
        }
    }
}