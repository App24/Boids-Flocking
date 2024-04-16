using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class KelpGenerator : MonoBehaviour
    {
        [SerializeField]
        private Vector3 bounds;

        [SerializeField]
        private GameObject kelpPrefab;

        [SerializeField]
        private float kelpCount;

        [SerializeField]
        private float minKelpHeight;

        [SerializeField]
        private float maxKelpHeight;

        private void Awake()
        {
            var halfBounds = bounds / 2f;
            for (int i = 0; i < kelpCount; i++)
            {
                var position = new Vector3(Random.Range(-1f, 1), 0, Random.Range(-1f, 1));
                position.x *= halfBounds.x;
                position.z *= halfBounds.z;

                GameObject kelp = Instantiate(kelpPrefab);
                kelp.transform.localScale = new Vector3(1, Random.Range(minKelpHeight, maxKelpHeight), 1);
                kelp.transform.SetParent(transform, false);
                kelp.transform.localPosition = position;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireCube(transform.position, bounds);
        }
    }
}
