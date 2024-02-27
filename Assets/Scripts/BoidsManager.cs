using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UIElements;

namespace Boids
{
    public class BoidsManager : MonoBehaviour
    {
        private static BoidsManager instance;
        public static BoidsManager Instance => instance;

        List<BoidBody> boids = new List<BoidBody>();

        [SerializeField]
        private ComputeShader boidComputeShader;

        private ComputeBuffer boidsBuffer;
        private ComputeBuffer boidSettingsBufffer;
        private ComputeBuffer boidCollisionBuffer;

        List<BoidCollisionData> boidCollisionDatas = new List<BoidCollisionData>();

        [SerializeField]
        private Material boidMaterial;

        private Material boidInstancedMaterial;

        private void Awake()
        {
            instance = this;
            boidInstancedMaterial = new Material(boidMaterial);
        }

        private void OnEnable()
        {
            RecreateBoidsBuffer();
        }

        private void OnDisable()
        {
            boidsBuffer.Release();
            boidsBuffer = null;

            boidSettingsBufffer.Release();
            boidSettingsBufffer = null;

            boidCollisionBuffer.Release();
            boidCollisionBuffer = null;
        }

        public void RecreateBoidsBuffer()
        {
            if(boidsBuffer != null)
            {
                boidsBuffer.Release();
            }

            if(boidCollisionBuffer != null)
            {
                boidCollisionBuffer.Release();
            }

            var boidDatas = new List<BoidData>();
            boidCollisionDatas.Clear();

            for (int i = 0; i < boids.Count; i++)
            {
                var boid = boids[i];

                var boidData = boid.ToBoidData();
                boidData.listIndex = (uint)i;

                boidDatas.Add(boidData);

                var headingCollision = IsHeadingForCollision(boid);
                var dir = Vector3.zero;
                if (headingCollision)
                {
                    dir = ObstacleRays(boid);
                }

                boidCollisionDatas.Add(new BoidCollisionData()
                {
                    collisionAvoidDir = dir,
                    headingForCollision = (uint)(headingCollision ? 1 : 0)
                });
            }

            RecreateBoidSettingsBuffer(boidDatas);

            if (boidDatas.Count > 0)
            {
                boidsBuffer = new ComputeBuffer(boidDatas.Count, BoidData.GetSize());
                boidsBuffer.SetData(boidDatas);
                boidComputeShader.SetBuffer(0, "boids", boidsBuffer);

                boidCollisionBuffer = new ComputeBuffer(boidCollisionDatas.Count, BoidCollisionData.GetSize());
                boidCollisionBuffer.SetData(boidCollisionDatas);
                boidComputeShader.SetBuffer(0, "boidCollisionData", boidCollisionBuffer);
            }
            boidComputeShader.SetInt("numBoids", boidDatas.Count);
            boidInstancedMaterial.SetInt("numBoids", boidDatas.Count);
        }

        private void RecreateBoidSettingsBuffer(List<BoidData> boidDatas)
        {
            if (boidSettingsBufffer != null)
            {
                boidSettingsBufffer.Release();
            }

            var boidSettingsDict = new Dictionary<BoidSettings, BoidSettingsData>();
            var boidSettingsData = new List<BoidSettingsData>();

            for (int i = 0; i < boids.Count; i++)
            {
                var boid = boids[i];

                if (!boidSettingsDict.TryGetValue(boid.boidSettings, out var boidSettings))
                {
                    boidSettings = boid.boidSettings.ToData();
                    boidSettingsDict.Add(boid.boidSettings, boidSettings);
                    boidSettingsData.Add(boidSettings);
                }

                var boidData = boidDatas[i];
                boidData.boidSettingIndex = (uint)boidSettingsData.IndexOf(boidSettings);
                boidDatas[i] = boidData;
            }

            if (boidSettingsData.Count > 0)
            {
                boidSettingsBufffer = new ComputeBuffer(boidSettingsData.Count, BoidSettingsData.GetSize());
                boidSettingsBufffer.SetData(boidSettingsData);
                boidComputeShader.SetBuffer(0, "boidSettings", boidSettingsBufffer);
                boidInstancedMaterial.SetBuffer("boidSettings", boidSettingsBufffer);
            }
        }

        public void AddBoid(BoidBody boid)
        {
            boids.Add(boid);
            RecreateBoidsBuffer();
        }

        public void RemoveBoid(BoidBody boid)
        {
            boids.Remove(boid);
            RecreateBoidsBuffer();
        }

        private void Update()
        {
            DoSimulation();
        }

        private void DoSimulation()
        {
            boidComputeShader.SetFloat("deltaTime", Time.deltaTime);

            var boidsCount = boids.Count;

            int numThreadsX = Mathf.CeilToInt(boidsCount / 256f);

            if (boidSettingsBufffer != null && boidsBuffer != null)
            {
                boidComputeShader.Dispatch(0, numThreadsX, 1, 1);

                var boidsData = new BoidData[boidsCount];
                boidsBuffer.GetData(boidsData);

                for (int i = 0; i < boidsCount; i++)
                {
                    var outBoid = boidsData[i];
                    var boid = boids[(int)outBoid.listIndex];
                    boid.FromBoidData(outBoid);
                    var headingForCollision = IsHeadingForCollision(boid);
                    var collisionData = boidCollisionDatas[(int)outBoid.listIndex];
                    collisionData.headingForCollision = (uint)(headingForCollision ? 1 : 0);
                    if (headingForCollision)
                    {
                        collisionData.collisionAvoidDir = ObstacleRays(boid);
                    }
                    else
                    {
                        collisionData.collisionAvoidDir = Vector3.zero;
                    }
                    boidCollisionDatas[(int)outBoid.listIndex] = collisionData;
                }

                boidCollisionBuffer.SetData(boidCollisionDatas);
                boidComputeShader.SetBuffer(0, "boidCollisionData", boidCollisionBuffer);

                boidInstancedMaterial.SetBuffer("boids", boidsBuffer);
            }
        }

        private bool IsHeadingForCollision(BoidBody boid)
        {
            RaycastHit hit;
            if (Physics.SphereCast(boid.transform.position, boid.boidSettings.boundsRadius, boid.transform.forward, out hit, boid.boidSettings.collisionAvoidDst, boid.boidSettings.collisonMask))
                return true;
            return false;
        }

        Vector3 ObstacleRays(BoidBody boid)
        {
            Vector3[] rayDirections = BoidHelper.directions;

            for (int i = 0; i < rayDirections.Length; i++)
            {
                Vector3 dir = boid.transform.TransformDirection(rayDirections[i]);
                Ray ray = new Ray(boid.transform.position, dir);
                if (!Physics.SphereCast(ray, boid.boidSettings.boundsRadius, boid.boidSettings.collisionAvoidDst, boid.boidSettings.collisonMask))
                {
                    return dir;
                }
            }

            return boid.transform.forward;
        }

        /*private ComputeBuffer SetUpBoidsData(out int boidsCount)
        {
            var boidDatas = new List<BoidData>();
            var boidSettingsDict = new Dictionary<BoidSettings, BoidSettingsData>();
            var boidSettingsData = new List<BoidSettingsData>();

            for (int i = 0; i < boids.Count; i++)
            {
                var boid = boids[i];
                if (!boid.enabled) continue;

                if(!boidSettingsDict.TryGetValue(boid.boidSettings, out var boidSettings))
                {
                    boidSettings = boid.boidSettings.ToData();
                    boidSettingsDict.Add(boid.boidSettings, boidSettings);
                    boidSettingsData.Add(boidSettings);
                }

                var boidData = boid.ToBoidData();
                boidData.listIndex = (uint)i;
                boidData.boidSettingIndex = (uint)boidSettingsData.IndexOf(boidSettings);

                boidDatas.Add(boidData);
            }

            ComputeBuffer boidsBuffer = new ComputeBuffer(boidDatas.Count, BoidData.GetSize());
            boidsBuffer.SetData(boidDatas);
            boidComputeShader.SetBuffer(0, "boids", boidsBuffer);
            boidComputeShader.SetInt("numBoids", boidDatas.Count);

            buffersToDispose.Add(boidsBuffer);

                ComputeBuffer settingsBuffer = new ComputeBuffer(boidSettingsData.Count, BoidSettingsData.GetSize());
                settingsBuffer.SetData(boidSettingsData);
                boidComputeShader.SetBuffer(0, "boidSettings", settingsBuffer);

                buffersToDispose.Add(settingsBuffer);

            boidsCount = boidDatas.Count;

            return boidsBuffer;
        }*/
    }

    public static class BoidHelper
    {

        const int numViewDirections = 300;
        public static readonly Vector3[] directions;

        static BoidHelper()
        {
            directions = new Vector3[BoidHelper.numViewDirections];

            float goldenRatio = (1 + Mathf.Sqrt(5)) / 2;
            float angleIncrement = Mathf.PI * 2 * goldenRatio;

            for (int i = 0; i < numViewDirections; i++)
            {
                float t = (float)i / numViewDirections;
                float inclination = Mathf.Acos(1 - 2 * t);
                float azimuth = angleIncrement * i;

                float x = Mathf.Sin(inclination) * Mathf.Cos(azimuth);
                float y = Mathf.Sin(inclination) * Mathf.Sin(azimuth);
                float z = Mathf.Cos(inclination);
                directions[i] = new Vector3(x, y, z);
            }
        }

    }

    public struct BoidCollisionData
    {
        public uint headingForCollision;
        public Vector3 collisionAvoidDir;

        public static int GetSize()
        {
            return sizeof(float) * 3 + sizeof(uint);
        }
    }
}
