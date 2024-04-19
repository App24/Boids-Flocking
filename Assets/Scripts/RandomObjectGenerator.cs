using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class RandomObjectGenerator : MonoBehaviour
    {
        public GameObject[] prefabs;

        [SerializeField]
        private int count;

        [SerializeField]
        private Vector3 bounds;

        public void Generate()
        {
            var halfBounds = bounds / 2f;

            while(transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }

            for (int i = 0; i < count; i++)
            {
                var prefab = prefabs[Random.Range(0, prefabs.Length)];
                var position = new Vector3(Random.Range(-1, 1f), 0, Random.Range(-1, 1f));
                position.x *= halfBounds.x;
                position.z *= halfBounds.z;

                position.y = Random.Range(-1f, 1f);

                GameObject spawned = Instantiate(prefab, transform);
                spawned.transform.localPosition = position;
                spawned.transform.localScale = Vector3.one * 2;

                spawned.transform.rotation = Random.rotation;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;

            Gizmos.DrawWireCube(transform.position, bounds);
        }
    }
}
