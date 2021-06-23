using UnityEngine;
using UnityEngine.Rendering;

namespace Particles
{
    public enum Shape { SPHERE, CUBE };
    public enum EditorDrawMode { OFF, SLOW, FAST };

    public struct Particle
    {
        public Vector3 pos, vel, force;
        public Vector4 col, size; // xy: size, zw: life
    }

    [System.Serializable]
    public struct Stats
    {
        public EditorDrawMode editorDrawMode;
        public bool reset;
        [HideInInspector]
        public bool initialized;
        [ReadOnly]
        public int alive, dead;
        [HideInInspector] 
        public int bufferSize, groupCount;
    }

    [System.Serializable]
    public struct GeneralProps
    {
        public int maxParts;
        public float lifetime;

        public float emissionRate;
        public float startDelay;

        public static GeneralProps dflt =
            new GeneralProps((int)1e6, 2, (float)4e5);

        public GeneralProps(
            int maxParts = 1, float lifetime = 3, 
            float emissionRate = 5, float startDelay = 0)
        {
            this.maxParts = maxParts;
            this.lifetime = lifetime;
            this.emissionRate = emissionRate;
            this.startDelay = startDelay;
        }
    }

    [System.Serializable]
    public struct DynamicEffect
    {
        public Vector3 offset;
        public Vector3 scale;
        public Shape shape;

        public DynamicEffect(Vector3 offset, Vector3 scale, Shape shape = Shape.SPHERE)
        {
            this.offset = offset;
            this.scale = scale;
            this.shape = shape;
        }

        public void Uniform(ComputeShader sh, string name)
        {
            sh.SetVector(name + "Offset", offset);
            sh.SetVector(name + "Scale", scale);
            // sh.SetInt(name + "Shape", (int)shape);
        }
    }

    [System.Serializable]
    public struct Capsule<T>
    {
        public T val;
        public Capsule(T val) { this.val = val; }
    }


    [System.Serializable]
    public struct RenderSettigns
    {
        // public BlendOp blendOp;
        public BlendMode srcBlend;
        public BlendMode dstBlend;

        public static RenderSettigns dflt =
            new RenderSettigns((BlendMode)1, (BlendMode)10);

        public RenderSettigns(BlendMode srcBlend, BlendMode dstBlend)
        {
            this.srcBlend = srcBlend;
            this.dstBlend = dstBlend;
        }

        public void Uniform(Material mat)
        {
            mat.SetInt("_BlendSrc", (int)srcBlend);
            mat.SetInt("_BlendDst", (int)dstBlend);
        }
    }
}