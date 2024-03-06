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
        private int minAmount;

        [SerializeField]
        private int maxAmount;

        [SerializeField]
        private BoidSettings boidSettings;

        [SerializeField]
        [ColorUsage(false, false)]
        private Color boidColor;

        private List<BoidBody> boids = new List<BoidBody>();

        private void Start()
        {
            var amount = Random.Range(minAmount, maxAmount);
            var halfSize = boundsBox / 2f;
            for (int i = 0; i < amount; i++)
            {
                var position = new Vector3(Random.Range(-halfSize.x, halfSize.x), Random.Range(-halfSize.y, halfSize.y), Random.Range(-halfSize.z, halfSize.z));

                /*GameObject gameObject = Instantiate(prefab);
                gameObject.transform.SetParent(transform, false);
                gameObject.transform.position = transform.position + position;
                gameObject.transform.rotation = Random.rotation;
                gameObject.GetComponent<MeshRenderer>().enabled = false;*/
                var boid = new BoidBody(transform.position + position, Random.rotation * Vector3.forward, boidSettings);
                boid.color = boidColor;
                boids.Add(boid);
                BoidsManager.Instance.AddBoid(boid);
            }
        }

        private void OnDisable()
        {
            foreach (var boid in boids)
            {
                BoidsManager.Instance.RemoveBoid(boid);
            }
        }

        private void OnEnable()
        {
            if (boids.Count == 0) return;

            foreach(var boid in boids)
            {
                BoidsManager.Instance.AddBoid(boid);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position, boundsBox);
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;
            foreach (var boid in boids)
            {
                boid.color = boidColor;
            }
            BoidsManager.Instance.RecreateBoidsBuffer();
        }
    }
}
