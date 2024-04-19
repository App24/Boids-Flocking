using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class BoidBody
    {
        public BoidSettings boidSettings;

        private Vector3 velocity;
        private Vector3 acceleration;
        public Vector3 position;
        public Vector3 forward;
        public Quaternion rotation;
        public Color32 color;
        public ushort boidGroup;
        public bool ignoreOtherBoids;
        public bool goToTarget;
        public Vector3 targetPosition;

        public BoidBody(Vector3 position, Vector3 forward, BoidSettings boidSettings)
        {
            this.position = position;
            this.forward = forward;
            this.boidSettings = boidSettings;
            velocity = forward * ((boidSettings.maxSpeed + boidSettings.minSpeed) / 2f);
            rotation = Quaternion.LookRotation(forward);
        }

        public BoidData ToBoidData()
        {
            ushort flags = 0;
            if (ignoreOtherBoids) flags |= 1;
            if (goToTarget) flags |= 2;
            uint extraData = (uint)((flags << 16) + boidGroup);
            uint color = (uint)((0xff << 24) + (this.color.b << 16) + (this.color.g << 8) + this.color.r);
            return new BoidData()
            {
                position = position,
                velocity = velocity,
                acceleration = acceleration,
                dir = forward,
                color = color,
                extraData = extraData,
                targetPosition = targetPosition
            };
        }

        public void FromBoidData(BoidData boidData)
        {
            position = boidData.position;
            velocity = boidData.velocity;
            acceleration = boidData.acceleration;
            forward = boidData.dir;
            rotation = Quaternion.LookRotation(forward);
        }
    }

    public struct BoidData
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;
        public Vector3 dir;
        public uint color;
        public uint listIndex;
        public uint boidSettingIndex;
        public uint extraData; // flags on top half, boid group on bottom half
        public Vector3 targetPosition;

        public static int GetSize()
        {
            return sizeof(float) * 15 + sizeof(uint) * 4;
        }
    }
}
