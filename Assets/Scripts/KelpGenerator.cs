using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class KelpGenerator : MonoBehaviour
    {
        [SerializeField]
        private Vector3 bounds;

        [SerializeField]
        private int kelpCount;

        [SerializeField]
        private float minKelpHeight;

        [SerializeField]
        private float maxKelpHeight;

        [SerializeField]
        private Material kelpMaterial;

        [SerializeField]
        private Mesh kelpMesh;

        private Material boidInstancedMaterial;
        private uint[] args;
        private GraphicsBuffer argsBuffer;

        private ComputeBuffer boidsBuffer;

        private List<KelpData> kelpDatas = new List<KelpData>();

        private void Awake()
        {
            boidInstancedMaterial = new Material(kelpMaterial);

            args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = (uint)kelpMesh.GetIndexCount(0);
            args[1] = (uint)0;
            args[2] = (uint)kelpMesh.GetIndexStart(0);
            args[3] = (uint)kelpMesh.GetBaseVertex(0);
        }

        private void OnEnable()
        {
            RecreateBoidsBuffer();
        }

        private void OnDisable()
        {
            if (boidsBuffer != null)
            {
                boidsBuffer.Release();
                boidsBuffer = null;
            }

            if (argsBuffer != null)
            {
                argsBuffer.Release();
                argsBuffer = null;
            }
        }

        private void RecreateBoidsBuffer()
        {
            if (boidsBuffer != null)
            {
                boidsBuffer.Release();
            }

            if (argsBuffer != null)
                argsBuffer.Release();

            kelpDatas.Clear();

            var halfBounds = bounds / 2f;

            for (int i = 0; i < kelpCount; i++)
            {
                var position = new Vector3(Random.Range(-1f, 1), 0, Random.Range(-1f, 1));
                position.x *= halfBounds.x;
                position.z *= halfBounds.z;

                float height = Random.Range(minKelpHeight, maxKelpHeight);

                kelpDatas.Add(new KelpData()
                {
                    position = position,
                    height = height,
                });
            }

            boidsBuffer = new ComputeBuffer(kelpDatas.Count, KelpData.Size);
            boidsBuffer.SetData(kelpDatas);
            boidInstancedMaterial.SetBuffer("kelpData", boidsBuffer);

            argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, 5 * sizeof(uint));
            args[1] = (uint)kelpDatas.Count;
            argsBuffer.SetData(args);
        }

        private void Update()
        {
            Graphics.DrawMeshInstancedIndirect(kelpMesh, 0, boidInstancedMaterial, new Bounds(transform.position, bounds), argsBuffer);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireCube(transform.position, bounds);
        }

        private struct KelpData
        {
            public Vector3 position;
            public float height;

            public static int Size => sizeof(float) * 4;
        }
    }
}
