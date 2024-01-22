using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class BoidBody : MonoBehaviour
    {
        private Vector3 velocity;
        [SerializeField]
        private float turnSpeed;

        public event System.Action onDestroy;

        private void Start()
        {
            BoidsManager.Instance.AddBoid(this);
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
                turnSpeed = turnSpeed
            };
        }

        public void FromBoidData(BoidData boidData)
        {
            transform.position = boidData.position;
            velocity = boidData.velocity;
            //transform.forward = velocity.normalized;
            transform.LookAt(velocity.normalized + transform.position);
        }
    }

    public struct BoidData
    {
        public Vector3 position;
        public Vector3 velocity;
        public float turnSpeed;
        public int listIndex;

        public static int GetSize()
        {
            return sizeof(float) * 7 + sizeof(int);
        }
    }
}
