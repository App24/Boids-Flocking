using System.Collections;
using System.Collections.Generic;
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

        private List<ComputeBuffer> buffersToDispose = new List<ComputeBuffer>();

        private void Awake()
        {
            instance = this;
        }

        public void AddBoid(BoidBody boid)
        {
            boids.Add(boid);
            boid.onDestroy += () => RemoveBoid(boid);
        }

        private void RemoveBoid(BoidBody boid)
        {
            boids.Remove(boid);
        }

        private void Update()
        {
            DoSimulation();
        }

        private void DoSimulation()
        {
            buffersToDispose.Clear();

            var boidsBuffer = SetUpBoidsData(out var boidsCount);

            boidComputeShader.SetFloat("deltaTime", Time.deltaTime);

            int numThreadsX = Mathf.CeilToInt(boidsCount / 256f);

            boidComputeShader.Dispatch(0, numThreadsX, 1, 1);

            var boidsData = new BoidData[boidsCount];
            boidsBuffer.GetData(boidsData);

            for(int i = 0; i < boidsCount; i++)
            {
                var outBoid = boidsData[i];
                boids[outBoid.listIndex].FromBoidData(outBoid);
            }

            foreach (var buffer in buffersToDispose)
            {
                buffer.Release();
            }
        }

        private ComputeBuffer SetUpBoidsData(out int boidsCount)
        {
            var boidDatas = new List<BoidData>();

            for (int i = 0; i < boids.Count; i++)
            {
                var boid = boids[i];
                if (!boid.enabled) continue;

                var boidData = boid.ToBoidData();
                boidData.listIndex = i;

                boidDatas.Add(boidData);
            }

            ComputeBuffer boidsBuffer = new ComputeBuffer(boidDatas.Count, BoidData.GetSize());
            boidsBuffer.SetData(boidDatas);
            boidComputeShader.SetBuffer(0, "boids", boidsBuffer);
            boidComputeShader.SetInt("numBoids", boidDatas.Count);

            buffersToDispose.Add(boidsBuffer);

            boidsCount = boidDatas.Count;

            return boidsBuffer;
        }
    }
}
