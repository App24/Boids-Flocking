using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    [CreateAssetMenu]
    public class BoidSettings : ScriptableObject
    {
        public float minSpeed;
        public float maxSpeed;
        public float maxSteerForce;
        public float viewRadius = 2;
        public float avoidanceRadius = 1;

        public float alignWeight = 1;
        public float cohesionWeight = 1;
        public float seperateWeight = 1;
    }
}
