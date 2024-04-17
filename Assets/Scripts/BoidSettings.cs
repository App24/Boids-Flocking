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

        public float targetWeight;

        public float alignWeight = 1;
        public float cohesionWeight = 1;
        public float seperateWeight = 1;

        public float avoidCollisionWeight = 1;

        public float boundsRadius;
        public float collisionAvoidDst;

        public LayerMask collisonMask = Physics.AllLayers;

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
                viewRadius = viewRadius,
                avoidCollisionWeight = avoidCollisionWeight,
                targetWeight = targetWeight
            };
        }

        private void OnValidate()
        {
            if (!Application.isPlaying) return;
            BoidsManager.Instance.SetBoidsBufferToRecreate();
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

        public float avoidCollisionWeight;

        public float targetWeight;

        public static int GetSize()
        {
            return sizeof(float) * 10;
        }
    }
}
