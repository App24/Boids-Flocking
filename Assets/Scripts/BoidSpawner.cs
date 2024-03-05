using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class BoidSpawner : MonoBehaviour
    {
        [SerializeField]
        private Vector3 boundsBox;

        [SerializeField]
        private GameObject prefab;

        [SerializeField]
        private int minAmount;

        [SerializeField]
        private int maxAmount;

        [SerializeField]
        private BoidSettings boidSettings;

        private void Start()
        {
            var amount = Random.Range(minAmount, maxAmount);
            var halfSize = boundsBox / 2f;
            for (int i = 0; i < amount; i++)
            {
                var position = new Vector3(Random.Range(-halfSize.x, halfSize.x), Random.Range(-halfSize.y, halfSize.y), Random.Range(-halfSize.z, halfSize.z));

                GameObject gameObject = Instantiate(prefab);
                gameObject.transform.SetParent(transform, false);
                gameObject.transform.position = transform.position + position;
                gameObject.transform.rotation = Random.rotation;
                gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position, boundsBox);
        }
    }
}
