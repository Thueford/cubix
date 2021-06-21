using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawMovedMeshIndirect : MonoBehaviour
{
    public int population;
    public float range;

    public Material material;
    public ComputeShader compute;
    public Transform pusher;

    private ComputeBuffer meshPropertiesBuffer;
    private ComputeBuffer argsBuffer;

    public Mesh mesh;
    private Bounds bounds;

    private int kernel;
    private uint tgx, tgy, tgz;

    // Mesh Properties struct to be read from the GPU.
    // Size() is a convenience funciton which returns the stride of the struct.
    private struct MeshProperties
    {
        public Matrix4x4 mat;
        public Vector4 color;

        public static int Size()
        {
            return
                sizeof(float) * 4 * 4 + // matrix;
                sizeof(float) * 4;      // color;
        }
    }

    private void Setup()
    {
        // Boundary surrounding the meshes we will be drawing.  Used for occlusion.
        bounds = new Bounds(transform.position, Vector3.one * (range + 1));
        kernel = compute.FindKernel("CSMain");
        compute.GetKernelThreadGroupSizes(kernel, out tgx, out tgy, out tgz);
        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
        // Argument buffer used by DrawMeshInstancedIndirect.
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        // Arguments for drawing mesh.
        args[0] = mesh.GetIndexCount(0); // number of triangle indices
        args[1] = (uint)population;      // population
        args[2] = mesh.GetIndexStart(0); // submesh start
        args[3] = mesh.GetBaseVertex(0); // submesh bv
        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        argsBuffer.SetData(args);

        // Initialize buffer with the given population.
        MeshProperties[] properties = new MeshProperties[population];
        for (int i = 0; i < population; i++)
        {
            MeshProperties props = new MeshProperties();
            Vector3 position = new Vector3(Random.Range(-range, range), Random.Range(-range, range), Random.Range(-range, range));
            Quaternion rotation = Quaternion.identity; // Quaternion.Euler(Random.Range(-180, 180), Random.Range(-180, 180), Random.Range(-180, 180));
            Vector3 scale = Vector3.one;

            props.mat = Matrix4x4.TRS(position, rotation, scale);
            props.color = Color.Lerp(Color.red, Color.blue, Random.value);

            properties[i] = props;
        }

        meshPropertiesBuffer = new ComputeBuffer(population, MeshProperties.Size());
        meshPropertiesBuffer.SetData(properties);
        compute.SetBuffer(kernel, "_Properties", meshPropertiesBuffer);
        material.SetBuffer("_Properties", meshPropertiesBuffer);
    }

    private void Start()
    {
        Setup();
    }

    private void Update()
    {
        bounds.center = transform.position;
        compute.SetVector("_PusherPosition", pusher.position);
        compute.SetVector("_ParentPosition", bounds.center);
        compute.SetFloat("_DeltaTime", Time.deltaTime);
        compute.SetMatrix("_Rotate", Matrix4x4.Rotate(Quaternion.Euler(0, 90*Time.deltaTime, 0)));

        compute.Dispatch(kernel, Mathf.CeilToInt(population / (float)tgx), 1, 1);
        Graphics.DrawMeshInstancedIndirect(mesh, 0, material, bounds, argsBuffer);
    }

    private void OnDisable()
    {
        if (meshPropertiesBuffer != null)
            meshPropertiesBuffer.Release();
        meshPropertiesBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;
    }
}