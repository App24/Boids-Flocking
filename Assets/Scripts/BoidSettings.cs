using System;
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

        public BoidSettingsData ToData()
        {
            return new BoidSettingsData()
            {
                minSpeed = minSpeed,
                alignWeight = alignWeight,
                avoidanceRadius = avoidanceRadius,
                cohesionWeight = cohesionWeight,
                maxSpeed = maxSpeed,
                maxSteerForce = maxSteerForce,
                seperateWeight = seperateWeight,
                viewRadius = viewRadius
            };
        }
    }

    public struct BoidSettingsData
    {
        public float minSpeed;
        public float maxSpeed;
        public float maxSteerForce;
        public float viewRadius;
        public float avoidanceRadius;

        public float alignWeight;
        public float cohesionWeight;
        public float seperateWeight;

        public static int GetSize()
        {
            return sizeof(float) * 8;
        }
    }
}
