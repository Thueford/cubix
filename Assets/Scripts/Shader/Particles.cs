using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEditor;

namespace Particles {
    [ExecuteAlways]
    public class Particles : MonoBehaviour
    {
        #region General
        [Header("General")]
        public Stats stats;
        public GeneralProps properties = GeneralProps.dflt;
        public RenderSettings renderSettings = RenderSettings.Default;

        [Header("Emission")]
        [Tooltip("xy: offset, z: timefac")]
        public Capsule<Vector3> size = new Capsule<Vector3>(new Vector3(1,1,0));
        public DynamicEffect pos;
        public DynamicEffect vel;
        public DynamicEffect force;
        public DynamicEffect posFac;

        [Header("Other")]
        public Colors color = Colors.dflt;

        #region Flags
        private static int F(bool v, int p) => v ? 1 << p : 0;
        private static int F(int v, int p) => v * 1 << p;

        int GetFlags()
        {
            return 
                F(pos.shape == Shape.SPHERE, 0) +
                F(vel.shape == Shape.SPHERE, 1) +
                F(force.shape == Shape.SPHERE, 2) +
                F(color.useGradient, 3) +
                F(color.useVariation, 4) +
                0;
        }
        #endregion

        #endregion

        #region Privates
        private float partTimer = 0, timePerPart; // for spawning new particles
        private Vector3 lastPos;

        private int curMaxParts = 1;
        private Vector2[] meshVerts;

        #endregion

        #region Unity

        void Awake()
        {
            if (tex == null && mat.mainTexture != null) tex = mat.mainTexture;
            stats = new Stats();
            ShaderSetup();
        }

        private void OnDrawGizmosSelected()
        {
            if (isEditorPaused()) { ReleaseBuffers(); return; }

            //update maxParts
            if (curMaxParts != properties.maxParts) {
                curMaxParts = properties.maxParts;
                ReleaseBuffers(); ShaderSetup();
            }

            if (stats.reset) ReleaseBuffers();
            renderSettings.setPreset();

            if (isEditorPlaying(EditorDrawMode.FAST)) {
                EditorApplication.QueuePlayerLoopUpdate();
                SceneView.RepaintAll();
            }
        }

        private bool isEditorPlaying(EditorDrawMode edm) { return isEditorPlaying() && stats.editorDrawMode == edm; }
        private bool isEditorPlaying() { return !Application.isPlaying && !EditorApplication.isPaused; }
        private bool isEditorPaused() { return !Application.isPlaying && (EditorApplication.isPaused || stats.editorDrawMode == 0); }

        // Update is called once per frame
        void Update()
        {
            if (isEditorPlaying(EditorDrawMode.SLOW))
                EditorApplication.delayCall += EditorApplication.QueuePlayerLoopUpdate;
            if (isEditorPaused()) return;

            DispatchUpdate();

            if (Application.isPlaying && !properties.repeat && stats.emitted >= curMaxParts && stats.alive == 0)
            {
                Destroy(gameObject);
                return;
            }

            // spawn particles
            if (properties.emissionRate > 1e-2)
            {
                timePerPart = 1 / properties.emissionRate;
                if (partTimer == 0) partTimer = -10*Time.deltaTime;
                partTimer += Time.deltaTime;
                partTimer -= timePerPart * DispatchEmit((int)(partTimer / timePerPart));
            }
        }

        void OnRenderObject()
        {
            if (isEditorPaused()) return;
            if (!stats.initialized) ShaderSetup();
            mat.mainTexture = tex;

            // set uniforms
            mat.SetBuffer("_Particles", particlesBuf);
            mat.SetBuffer("_QuadVert", quadVertBuf);
            renderSettings.Uniform(mat);

            // mat.SetFloat("_Time", Time.time);
            mat.SetPass(0);

            if (deadBuf == null) Debug.LogWarning("... " + stats.initialized);
            else Graphics.DrawProceduralNow(MeshTopology.Triangles, meshVerts.Length, deadBuf.count);
        }

        #endregion

        #region User
        [Range(-10, 10)]
        public float velocityFactor = 0;
        //public Vector3 posOffset;
        //public Quaternion rotation;
        #endregion

        #region Shader
        // public Assets assets; // not visible in script inspector :(
        [NotNull] public Texture tex;
        [NotNull] public Material mat;
        [NotNull] public ComputeShader compute;
        private ComputeBuffer particlesBuf, deadBuf;
        private ComputeBuffer counterBuf, quadVertBuf;

        private int kernelInit, kernelEmit, kernelUpdate;

        #region Shader Setup
        private void ShaderSetup()
        {
            kernelInit = compute.FindKernel("Init");
            kernelEmit = compute.FindKernel("EmitOne");
            kernelUpdate = compute.FindKernel("Update");
            compute.GetKernelThreadGroupSizes(kernelInit, out uint threads, out _, out _);

            curMaxParts = properties.maxParts;
            stats.groupCount = Mathf.CeilToInt((float)curMaxParts / threads);
            stats.bufferSize = stats.groupCount * (int)threads;

            if (curMaxParts < (1 << 14))
            {
                stats.bufferSize = stats.groupCount = curMaxParts;
                kernelInit = compute.FindKernel("InitOne");
                kernelUpdate = compute.FindKernel("UpdateOne");
            }

            meshVerts = new[] {
                new Vector2(-0.5f,0.5f),
                new Vector2(0.5f,0.5f),
                new Vector2(0.5f,-0.5f),
                new Vector2(0.5f,-0.5f),
                new Vector2(-0.5f,-0.5f),
                new Vector2(-0.5f,0.5f),
            };

            InitializePartBuffer();
            DispatchInit();

            stats.initialized = true;
        }

        private void InitializePartBuffer()
        {
            particlesBuf = new ComputeBuffer(stats.bufferSize, Marshal.SizeOf<Particle>());

            deadBuf = new ComputeBuffer(stats.bufferSize, sizeof(int), ComputeBufferType.Append);
            deadBuf.SetCounterValue(0);

            counterBuf = new ComputeBuffer(4, sizeof(int), ComputeBufferType.IndirectArguments);
            counterArray = new int[] { 0, 1, 0, 0 };

            quadVertBuf = new ComputeBuffer(meshVerts.Length, Marshal.SizeOf<Vector2>());
            quadVertBuf.SetData(meshVerts);
        }


        private void DispatchInit()
        {
            compute.SetBuffer(kernelInit, "_Particles", particlesBuf);
            compute.SetBuffer(kernelInit, "_Dead", deadBuf);
            compute.Dispatch(kernelInit, stats.groupCount, 1, 1);
        }

        private void ReleaseBuffers()
        {
            if (!stats.initialized) return;
            EditorDrawMode edm = stats.editorDrawMode;
            stats = new Stats();
            stats.editorDrawMode = edm;

            if (particlesBuf != null) particlesBuf.Release();
            if (quadVertBuf != null) quadVertBuf.Release();
            if (counterBuf != null) counterBuf.Release();
            if (deadBuf != null) deadBuf.Release();
        }

        #endregion

        #region Shader Loop

        public int DispatchEmit(int count)
        {
            count = Mathf.Min(count, 1<<15, curMaxParts - stats.alive);
            if (!properties.repeat && stats.emitted + count > properties.maxParts)
                count = properties.maxParts - stats.emitted;

            if (count <= 0) count = 0;
            stats.ppf = count;
            stats.pps = Mathf.RoundToInt(count / Time.deltaTime);
            if (count <= 0) return 0;

            stats.emitted += count;

            Vector3 velocity = (transform.position - lastPos); // / Time.deltaTime;
            lastPos = transform.position;

            compute.SetBuffer(kernelEmit, "_Particles", particlesBuf);
            compute.SetBuffer(kernelEmit, "_Alive", deadBuf);

            compute.SetVector("_PosParent", transform.position); // + posOffset
            // compute.SetMatrix("_Rotation", Matrix4x4.Rotate(rotation));
            compute.SetVector("_SpdParent", velocityFactor * velocity);
            compute.SetVector("_Seeds", new Vector4(Random.value, Random.value, Random.value, Random.value));
            compute.SetFloat("_Lifetime", properties.lifetime);

            pos.Uniform(compute, "_Pos");
            vel.Uniform(compute, "_Spd");
            force.Uniform(compute, "_Force");
            posFac.Uniform(compute, "_PosFac");

            compute.SetVector("_Size", size.val);
            color.UniformEmit(compute, "_Color");

            compute.Dispatch(kernelEmit, count, 1, 1);
            return count;
        }

        private void DispatchUpdate()
        {
            if (!stats.initialized || particlesBuf == null) ShaderSetup();

            compute.SetInt("_Flags", GetFlags());
            compute.SetFloat("_SizeVel", size.val.z);
            compute.SetFloat("_DeltaTime", Time.deltaTime);
            compute.SetBuffer(kernelUpdate, "_Particles", particlesBuf);
            compute.SetBuffer(kernelUpdate, "_Dead", deadBuf);
            color.Uniform(compute, kernelUpdate, "_Color");

            compute.Dispatch(kernelUpdate, stats.groupCount, 1, 1);
            ReadDeadCount();
        }

        private int[] counterArray;
        private void ReadDeadCount()
        {
            if (deadBuf == null || counterBuf == null || counterArray == null)
            {
                stats.dead = stats.bufferSize;
                return;
            }
            counterBuf.SetData(counterArray);
            ComputeBuffer.CopyCount(deadBuf, counterBuf, 0);
            counterBuf.GetData(counterArray);
            stats.dead = counterArray[0];
            stats.alive = curMaxParts - stats.dead;
        }

        #endregion

        private void OnDisable() {
            ReleaseBuffers();
        }
        private void OnDestroy() {
            ReleaseBuffers();
        }

        #endregion
    }
}