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

        private void Start()
        {
            velocity = transform.forward * ((boidSettings.maxSpeed + boidSettings.minSpeed) / 2f);
        }

        private void OnEnable()
        {
            BoidsManager.Instance.AddBoid(this);
        }

        private void OnDisable()
        {
            BoidsManager.Instance.RemoveBoid(this);
        }

        public BoidData ToBoidData()
        {
            return new BoidData()
            {
                position = transform.position,
                velocity = velocity,
                acceleration = acceleration,
                dir = transform.forward
            };
        }

        public void FromBoidData(BoidData boidData)
        {
            transform.position = boidData.position;
            velocity = boidData.velocity;
            acceleration = boidData.acceleration;
            transform.forward = boidData.dir;
        }

        private void OnDrawGizmosSelected()
        {
            if (!boidSettings) return;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, boidSettings.boundsRadius);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, boidSettings.collisionAvoidDst);
        }
    }

    public struct BoidData
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;
        public Vector3 dir;
        public uint listIndex;
        public uint boidSettingIndex;
        /*public uint headingForCollision;
        public Vector3 collisionAvoidDir;*/

        public static int GetSize()
        {
            return sizeof(float) * 12 + sizeof(uint) * 2;
        }
    }
}
