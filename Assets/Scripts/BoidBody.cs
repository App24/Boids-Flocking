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
        public Color color;
        public uint boidGroup;
        public bool ignoreOtherBoids;

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
            return new BoidData()
            {
                position = position,
                velocity = velocity,
                acceleration = acceleration,
                dir = forward,
                color = new Vector3(color.r, color.g, color.b),
                boidGroup = boidGroup,
                ignoreOtherBoids = (uint)(ignoreOtherBoids ? 1 : 0)
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
        public Vector3 color;
        public uint listIndex;
        public uint boidSettingIndex;
        public uint boidGroup;
        public uint ignoreOtherBoids;
        /*public uint headingForCollision;
        public Vector3 collisionAvoidDir;*/

        public static int GetSize()
        {
            return sizeof(float) * 15 + sizeof(uint) * 4;
        }
    }
}
