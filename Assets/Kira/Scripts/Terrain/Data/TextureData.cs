using UnityEngine;

namespace Kira
{
    [CreateAssetMenu(menuName = "Kira/Texture")]
    public class TextureData : UpdatableData
    {
        private float savedMinHeight;
        private float savedMaxHeight;

        private static readonly int MinHeightProp = Shader.PropertyToID("_MinHeight");
        private static readonly int MaxHeightProp = Shader.PropertyToID("_MaxHeight");

        public void ApplyToMaterial(Material material)
        { 
            UpdateMeshHeight(material, savedMinHeight, savedMaxHeight);
        }

        public void UpdateMeshHeight(Material material, float minHeight, float maxHeight)
        {
            savedMinHeight = minHeight;
            savedMaxHeight = maxHeight;

            material.SetFloat(MinHeightProp, minHeight);
            material.SetFloat(MaxHeightProp, maxHeight);
        }
    }
}