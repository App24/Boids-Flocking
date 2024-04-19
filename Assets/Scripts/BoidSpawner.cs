using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

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

        [SerializeField]
        [FormerlySerializedAs("ignoreOtherBoids")]
        private bool ignoreOtherBoidGroups = true;

        [SerializeField]
        private bool goToTarget = false;

        [SerializeField]
        private Transform targetPosition;

        private List<BoidBody> boids = new List<BoidBody>();

        private static ushort boidGroup;

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
                boid.boidGroup = boidGroup;
                boid.ignoreOtherBoids = ignoreOtherBoidGroups;
                boid.goToTarget = goToTarget;
                if(targetPosition)
                boid.targetPosition = targetPosition.position;
                boids.Add(boid);
            }
            BoidsManager.Instance.AddBoids(boids);
            boidGroup++;
        }

        private void OnDisable()
        {
            BoidsManager.Instance.RemoveBoids(boids);
        }

        private void OnEnable()
        {
            if (boids.Count == 0) return;

            BoidsManager.Instance.AddBoids(boids);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireCube(transform.position, boundsBox);
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;
            RecreateBoids();
        }

        public void ChangeTargetPosition(Transform transform)
        {
            this.targetPosition = transform;
            RecreateBoids();
        }

        private void RecreateBoids()
        {
            foreach (var boid in boids)
            {
                boid.color = boidColor;
                boid.ignoreOtherBoids = ignoreOtherBoidGroups;
                boid.goToTarget = goToTarget;
                if (targetPosition)
                    boid.targetPosition = targetPosition.position;
            }
            if (BoidsManager.Instance)
                BoidsManager.Instance.SetBoidsBufferToRecreate();
        }
    }
}
