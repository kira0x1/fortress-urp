using UnityEngine;

namespace Kira
{
    [CreateAssetMenu(menuName = "Kira/Terrain")]
    public class TerrainData : UpdatableData
    {
        public float uniformScale = 2.5f;
        public bool useFlatShading;
        public bool useFalloff;
        public float meshHeightMultiplier = 10f;
        public AnimationCurve meshHeightCurve;
    }
}