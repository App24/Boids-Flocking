using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class BoidBody : MonoBehaviour
    {
        public BoidSettings boidSettings;

        private Vector3 velocity;
        private Vector3 acceleration;

        public event System.Action onDestroy;

        private void Start()
        {
            BoidsManager.Instance.AddBoid(this);

            velocity = transform.forward * ((boidSettings.maxSpeed + boidSettings.minSpeed) / 2f);
        }

        private void OnDestroy()
        {
            onDestroy?.Invoke();
        }

        public BoidData ToBoidData()
        {
            return new BoidData()
            {
                position = transform.position,
                velocity = velocity,
                acceleration = acceleration,
                dir = transform.forward,
                minSpeed = boidSettings.minSpeed,
                maxSpeed = boidSettings.maxSpeed,
                viewRadius = boidSettings.viewRadius,
                maxSteerForce = boidSettings.maxSteerForce,
                alignWeight = boidSettings.alignWeight,
                cohesionWeight = boidSettings.cohesionWeight,
                seperateWeight = boidSettings.seperateWeight,
                avoidanceRadius = boidSettings.avoidanceRadius
            };
        }

        public void FromBoidData(BoidData boidData)
        {
            transform.position = boidData.position;
            velocity = boidData.velocity;
            acceleration = boidData.acceleration;
            transform.forward = boidData.dir;
        }
    }

    public struct BoidData
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;
        public Vector3 dir;
        public int listIndex;

        public float minSpeed;
        public float maxSpeed;
        public float maxSteerForce;
        public float viewRadius;

        public float alignWeight;
        public float cohesionWeight;
        public float seperateWeight;
        public float avoidanceRadius;

        public static int GetSize()
        {
            return sizeof(float) * 20 + sizeof(int) * 1;
        }
    }
}
