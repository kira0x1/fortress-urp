using UnityEngine;

namespace Kira
{
    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        [Range(0f, 1f)]
        public float height;
        public Color color;
    }
}