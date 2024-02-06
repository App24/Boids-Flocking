using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor.Build.Content;
using UnityEngine;

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

        private void Awake()
        {
            instance = this;
        }

        private void OnEnable()
        {
            RecreateBoidsBuffer();
        }

        private void OnDisable()
        {
            boidsBuffer.Release();
            boidsBuffer = null;
        }

        private void RecreateBoidsBuffer()
        {
            if(boidsBuffer != null)
            {
                boidsBuffer.Release();
            }

            var boidDatas = new List<BoidData>();

            for (int i = 0; i < boids.Count; i++)
            {
                var boid = boids[i];

                var boidData = boid.ToBoidData();
                boidData.listIndex = (uint)i;

                boidDatas.Add(boidData);
            }

            RecreateBoidSettingsBuffer(boidDatas);

            if (boidDatas.Count > 0)
            {
                boidsBuffer = new ComputeBuffer(boidDatas.Count, BoidData.GetSize());
                boidsBuffer.SetData(boidDatas);
                boidComputeShader.SetBuffer(0, "boids", boidsBuffer);
            }
            boidComputeShader.SetInt("numBoids", boidDatas.Count);
        }

        private void RecreateBoidSettingsBuffer(List<BoidData> boidDatas)
        {
            if (boidSettingsBufffer != null)
            {
                boidSettingsBufffer.Release();
                boidSettingsBufffer = null;
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
                    boids[(int)outBoid.listIndex].FromBoidData(outBoid);
                }
            }
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
}
