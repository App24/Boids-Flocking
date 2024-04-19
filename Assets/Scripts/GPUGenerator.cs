using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public abstract class GPUGenerator<Data> : MonoBehaviour where Data : struct
    {
        [SerializeField]
        protected Vector3 bounds;

        [SerializeField]
        protected int spawnCount;

        [SerializeField]
        protected Material material;

        [SerializeField]
        protected Mesh mesh;

        protected Material instancedMaterial;
        private uint[] args;
        private GraphicsBuffer argsBuffer;

        private ComputeBuffer boidsBuffer;

        protected List<Data> storedData = new List<Data>();

        public abstract int DataSize { get; }

        protected virtual void Awake()
        {
            instancedMaterial = new Material(material);

            args = new uint[5] { 0, 0, 0, 0, 0 };
            args[0] = (uint)mesh.GetIndexCount(0);
            args[1] = (uint)0;
            args[2] = (uint)mesh.GetIndexStart(0);
            args[3] = (uint)mesh.GetBaseVertex(0);
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

            storedData.Clear();

            SpawnObjects();

            boidsBuffer = new ComputeBuffer(storedData.Count, DataSize);
            boidsBuffer.SetData(storedData);
            instancedMaterial.SetBuffer("storedData", boidsBuffer);

            argsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, 5 * sizeof(uint));
            args[1] = (uint)storedData.Count;
            argsBuffer.SetData(args);
        }

        protected virtual void SpawnObjects()
        {

        }

        private void Update()
        {
            Graphics.DrawMeshInstancedIndirect(mesh, 0, instancedMaterial, new Bounds(transform.position, bounds), argsBuffer);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireCube(transform.position, bounds);
        }
    }
}
