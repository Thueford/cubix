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
        public RenderSettings renderSettings;

        [Header("Emission")]
        [Tooltip("xy: offset, z: timefac")]
        public Capsule<Vector3> size = new Capsule<Vector3>(Vector3.one);
        public DynamicEffect pos;
        public DynamicEffect vel;
        public DynamicEffect force;
        public DynamicEffect posFac;

        [Header("Other")]
        //public Vector2 size = new Vector2(0.2f, 0.2f);
        public Color color = Color.yellow;

        #region Flags
        private static int F(bool v, int p) { return v ? 1 << p : 0; }
        private static int F(int v, int p) { return v * 1 << p; }

        int GetFlags()
        {
            return 
                F(pos.shape == Shape.SPHERE, 0) +
                F(vel.shape == Shape.SPHERE, 1) +
                F(force.shape == Shape.SPHERE, 2) +
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
            ShaderSetup();
        }

        private void OnDrawGizmosSelected()
        {
            if (isEditorPaused()) { ReleaseBuffers(); return; }
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

            //update maxParts
            if (curMaxParts != properties.maxParts) {
                curMaxParts = properties.maxParts;
                stats.reset = true;
            }
            if (stats.reset) ReleaseBuffers();
            if (!stats.initialized) Awake();

            DispatchUpdate();

            // spawn particles
            timePerPart = 1 / properties.emissionRate;
            if (partTimer == 0) partTimer = -10*Time.deltaTime;
            partTimer += Time.deltaTime;
            partTimer -= timePerPart * DispatchEmit((int)(partTimer / timePerPart));
        }

        void OnRenderObject()
        {
            if (isEditorPaused()) return;
            if (!stats.initialized) Awake();

            // set uniforms
            assets.mat.SetBuffer("_Particles", particlesBuf);
            assets.mat.SetBuffer("_QuadVert", quadVertBuf);
            renderSettings.Uniform(assets.mat);

            // assets.mat.SetFloat("_Time", Time.time);
            assets.mat.SetPass(0);

            Graphics.DrawProceduralNow(MeshTopology.Triangles, meshVerts.Length, deadBuf.count);
        }

        #endregion

        #region Shader
        public Assets assets;
        private ComputeBuffer particlesBuf, deadBuf;
        private ComputeBuffer counterBuf, quadVertBuf;

        private int kernelInit, kernelEmit, kernelUpdate;

        #region Shader Setup
        private void ShaderSetup()
        {
            kernelInit = assets.compute.FindKernel("Init");
            kernelEmit = assets.compute.FindKernel("EmitOne");
            kernelUpdate = assets.compute.FindKernel("Update");
            assets.compute.GetKernelThreadGroupSizes(kernelInit, out uint threads, out _, out _);

            curMaxParts = properties.maxParts;
            stats.groupCount = Mathf.CeilToInt((float)curMaxParts / threads);
            stats.bufferSize = stats.groupCount * (int)threads;

            if (curMaxParts < (1 << 14))
            {
                stats.bufferSize = stats.groupCount = curMaxParts;
                kernelInit = assets.compute.FindKernel("InitOne");
                kernelUpdate = assets.compute.FindKernel("UpdateOne");
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
            assets.compute.SetBuffer(kernelInit, "_Particles", particlesBuf);
            assets.compute.SetBuffer(kernelInit, "_Dead", deadBuf);
            assets.compute.Dispatch(kernelInit, stats.groupCount, 1, 1);
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
            count = Mathf.Min(count, 1<<15, curMaxParts - (stats.bufferSize - stats.dead));
            stats.ppf = count;
            stats.pps = Mathf.RoundToInt(count / Time.deltaTime);
            if (count <= 0) return 0;

            Vector3 velocity = (transform.position - lastPos) / Time.deltaTime;
            lastPos = transform.position;

            assets.compute.SetBuffer(kernelEmit, "_Particles", particlesBuf);
            assets.compute.SetBuffer(kernelEmit, "_Alive", deadBuf);

            assets.compute.SetInt("_Flags", GetFlags());
            assets.compute.SetVector("_PosParent", transform.position);
            assets.compute.SetVector("_SpdParent", velocity);
            assets.compute.SetVector("_Seeds", new Vector4(Random.value, Random.value, Random.value, Random.value));
            assets.compute.SetFloat("_Lifetime", properties.lifetime);

            pos.Uniform(assets.compute, "_Pos");
            vel.Uniform(assets.compute, "_Spd");
            force.Uniform(assets.compute, "_Force");
            posFac.Uniform(assets.compute, "_PosFac");

            assets.compute.SetVector("_Size", size.val);
            assets.compute.SetVector("_Color", color);

            assets.compute.Dispatch(kernelEmit, count, 1, 1);
            return count;
        }

        private void DispatchUpdate()
        {
            assets.compute.SetFloat("_SizeVel", size.val.z);
            assets.compute.SetFloat("_DeltaTime", Time.deltaTime);
            assets.compute.SetBuffer(kernelUpdate, "_Particles", particlesBuf);
            assets.compute.SetBuffer(kernelUpdate, "_Dead", deadBuf);

            assets.compute.Dispatch(kernelUpdate, stats.groupCount, 1, 1);
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