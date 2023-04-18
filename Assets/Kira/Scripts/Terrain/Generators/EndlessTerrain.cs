using System.Collections.Generic;
using UnityEngine;

namespace Kira
{
    public partial class EndlessTerrain : MonoBehaviour
    {
        private const float viewerMoveThresholdForChunkUpdate = 25f;
        private const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

        public LODInfo[] detailLevels;
        public static float maxViewDist;

        public Transform viewer;
        public Material mapMaterial;

        public static Vector2 viewerPosition;
        private Vector2 viewerPositionOld;
        private static MapGenerator mapGenerator;

        private int chunkSize;
        private int chunkVisibleInViewDist;

        private Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
        private static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

        private void Start()
        {
            mapGenerator = FindObjectOfType<MapGenerator>();

            maxViewDist = detailLevels[^1].visibleDistThreshhold;
            chunkSize = mapGenerator.mapChunkSize - 1;
            chunkVisibleInViewDist = Mathf.RoundToInt(maxViewDist / chunkSize);

            UpdateVisibleChunks();
        }

        private void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapGenerator.terrainData.uniformScale;

            if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
            {
                viewerPositionOld = viewerPosition;
                UpdateVisibleChunks();
            }
        }

        private void UpdateVisibleChunks()
        {
            foreach (TerrainChunk chunk in terrainChunksVisibleLastUpdate)
            {
                chunk.SetVisible(false);
            }

            terrainChunksVisibleLastUpdate.Clear();

            int curChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
            int curChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

            for (int yOffset = -chunkVisibleInViewDist; yOffset <= chunkVisibleInViewDist; yOffset++)
            {
                for (int xOffset = -chunkVisibleInViewDist; xOffset <= chunkVisibleInViewDist; xOffset++)
                {
                    Vector2 viewedChunkCoord = new Vector2(curChunkCoordX + xOffset, curChunkCoordY + yOffset);

                    if (terrainChunkDict.TryGetValue(viewedChunkCoord, out TerrainChunk chunk))
                    {
                        chunk.UpdateChunk();
                    }
                    else
                    {
                        terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
                    }
                }
            }
        }
    }
}