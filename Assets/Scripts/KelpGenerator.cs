using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class KelpGenerator : GPUGenerator<KelpGenerator.KelpData>
    {

        [SerializeField]
        private float minKelpHeight;

        [SerializeField]
        private float maxKelpHeight;

        [SerializeField]
        private Color baseColor;

        [SerializeField]
        private Color tipColor;

        [SerializeField]
        private ModulationData xModulation = new ModulationData() { speed = .5f, frequency = 2, amplitude = 1 };

        [SerializeField]
        private ModulationData zModulation = new ModulationData() { speed = 1, frequency = .2f, amplitude = 1 };

        public override int DataSize => KelpData.Size;

        protected override void SpawnObjects()
        {
            var halfBounds = bounds / 2f;

            for (int i = 0; i < spawnCount; i++)
            {
                var position = new Vector2(Random.Range(-1f, 1), Random.Range(-1f, 1));
                position.x *= halfBounds.x;
                position.y *= halfBounds.z;

                float height = Random.Range(minKelpHeight, maxKelpHeight);

                var color = baseColor;
                var tipColor = this.tipColor;

                storedData.Add(new KelpData()
                {
                    position = position,
                    height = height,
                    color = new Vector3(color.r, color.g, color.b),
                    tipColor = new Vector3(tipColor.r, tipColor.g, tipColor.b)
                });
            }

            instancedMaterial.SetFloat("_SpeedX", xModulation.speed);
            instancedMaterial.SetFloat("_FrequencyX", xModulation.frequency);
            instancedMaterial.SetFloat("_AmplitudeX", xModulation.amplitude);

            instancedMaterial.SetFloat("_SpeedZ", zModulation.speed);
            instancedMaterial.SetFloat("_FrequencyZ", zModulation.frequency);
            instancedMaterial.SetFloat("_AmplitudeZ", zModulation.amplitude);
        }

        private void OnValidate()
        {
            if (!instancedMaterial) return;

            instancedMaterial.SetFloat("_SpeedX", xModulation.speed);
            instancedMaterial.SetFloat("_FrequencyX", xModulation.frequency);
            instancedMaterial.SetFloat("_AmplitudeX", xModulation.amplitude);

            instancedMaterial.SetFloat("_SpeedZ", zModulation.speed);
            instancedMaterial.SetFloat("_FrequencyZ", zModulation.frequency);
            instancedMaterial.SetFloat("_AmplitudeZ", zModulation.amplitude);
        }

        public struct KelpData
        {
            public Vector2 position;
            public float height;
            public Vector3 color;
            public Vector3 tipColor;

            public static int Size => sizeof(float) * 9;
        }

        [System.Serializable]
        private struct ModulationData
        {
            public float speed, frequency, amplitude;
        }
    }
}
