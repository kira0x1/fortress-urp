using UnityEngine;

namespace Kira
{
    [CreateAssetMenu(menuName = "Kira/Noise")]
    public class NoiseData : UpdatableData
    {
        public Noise.NormalizeMode normalizeMode;

        public float noiseScale = 28;
        public int octaves = 4;

        [Range(0, 1)]
        public float persistance = 0.5f;
        public float lacunarity = 2f;

        public int seed;
        public Vector2 offset;

        protected override void _OnValidate()
        {
            if (lacunarity < 1) lacunarity = 1;
            if (octaves < 0) octaves = 0;

            base._OnValidate();
        }
    }
}