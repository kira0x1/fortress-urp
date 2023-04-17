using UnityEngine;

namespace Kira
{
    [CreateAssetMenu(menuName = "Kira/Terrain")]
    public class TerrainData : UpdatableData
    {
        public float uniformScale = 2.5f;
        public float meshHeightMultiplier = 10f;
        public bool useFlatShading;
        public bool useFalloff;
        public AnimationCurve meshHeightCurve;

        public float MinHeight => uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(0);
        public float MaxHeight => uniformScale * meshHeightMultiplier * meshHeightCurve.Evaluate(1);
    }
}