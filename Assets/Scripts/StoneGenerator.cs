using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class StoneGenerator : GPUGenerator<StoneGenerator.StoneData>
    {
        public override int DataSize => StoneData.Size;

        [SerializeField]
        private float minScale;

        [SerializeField]
        private float maxScale;

        [SerializeField]
        private float yOffset;

        protected override void SpawnObjects()
        {
            var halfBounds = bounds / 2f;

            for (int i = 0; i<spawnCount; i++)
            {
                var position = new Vector2(Random.Range(-1f, 1), Random.Range(-1f, 1));
                position.x *= halfBounds.x;
                position.y *= halfBounds.z;

                storedData.Add(new StoneData()
                {
                    rotation = Random.value * 360f,
                    position = position,
                    scale = Random.Range(minScale, maxScale),
                });
            }

            instancedMaterial.SetFloat("yOffset", yOffset);
        }

        public struct StoneData
        {
            public float rotation;
            public Vector2 position;
            public float scale;

            public static int Size => sizeof(float) * 4;
        }
    }
}
