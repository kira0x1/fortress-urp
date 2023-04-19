using UnityEngine;

namespace Kira.CustomNoise
{
    public class TextureCreator : MonoBehaviour
    {
        [Range(1, 3)] public int dimensions = 3;
        [Range(2, 512)] public int resolution = 256;
        public float frequency = 2f;

        private Texture2D texture;
        private MeshRenderer meshRenderer;

        private void OnEnable()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (texture == null)
                InitTexture();

            FillTexture();
        }

        private void InitTexture()
        {
            texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
            texture.name = "Procedural Texture";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point;
            // texture.anisoLevel = 9;
            meshRenderer.material.mainTexture = texture;
        }

        public void RefreshTexture()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            InitTexture();
            FillTexture();
        }

        private void Update()
        {
            if (!transform.hasChanged) return;
            transform.hasChanged = false;
            FillTexture();
        }

        public void FillTexture()
        {
            Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f, -0.5f));
            Vector3 point10 = transform.TransformPoint(new Vector3(0.5f, -0.5f));
            Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
            Vector3 point11 = transform.TransformPoint(new Vector3(0.5f, 0.5f));

            if (texture.width != resolution)
            {
                texture.Reinitialize(resolution, resolution);
            }

            NoiseMethod method = Noise.valueMethods[dimensions - 1];
            float stepSize = 1f / resolution;

            for (int y = 0; y < resolution; y++)
            {
                Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
                Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);

                for (int x = 0; x < resolution; x++)
                {
                    Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                    texture.SetPixel(x, y, Color.white * method(point, frequency));
                }
            }

            texture.Apply();
        }
    }
}