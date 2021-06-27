using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace ParticleProps
{
    public enum Shape { SPHERE, CUBE };
    public enum EditorDrawMode { OFF, SLOW, FAST };

    public struct Particle
    {
        public Vector3 pos, vel, force;
        public Vector4 col, size; // xy: size, zw: life
        public float rand;
    }

    [System.Serializable]
    public struct Stats
    {
        public EditorDrawMode editorDrawMode;
        public bool reset;
        [HideInInspector]
        public bool initialized, prewarmed;
        [ReadOnly]
        public int alive, dead, emitted, ppf, pps;
        [HideInInspector] 
        public int bufferSize, groupCount;
    }

    [System.Serializable]
    public struct GeneralProps
    {
        public bool enabled;
        public bool repeat;
        public bool prewarm;
        public bool destroyOnFinished;

        public int maxParts;
        public float lifetime;

        public float emissionRate;
        public float startDelay;

        public static GeneralProps dflt =
            new GeneralProps((int)1e3, 2, (float)4e2);

        public GeneralProps(
            int maxParts = 1, float lifetime = 3, 
            float emissionRate = 5, float startDelay = 0)
        {
            enabled = true;
            repeat = true;
            prewarm = false;
            destroyOnFinished = false;

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
    public struct Assets
    {
        [NotNull] public Material mat;
        [NotNull] public ComputeShader compute;
    }

    [System.Serializable]
    public struct Colors
    {
        public static Colors dflt = new Colors(Color.white);
        public Color color, color2;
        public bool useVariation;
        public bool useGradient;
        [Range(2, 256)]
        public int steps;
        public Gradient gradient, gradient2;
        private static Texture2D dfltTex;

        public Colors(Color clr)
        {
            useVariation = false;
            useGradient = false;
            color = color2 = clr;
            steps = 16;
            gradient = gradient2 = null;
            tex = null;
        }

        private Texture2D tex;
        private Texture2D getTexture()
        {
            int height = useVariation ? 2 : 1;
            if (tex == null || tex.height != height || tex.width != steps)
                tex = new Texture2D(steps, height);

            if (gradient != null)
            {
                for (int i = 0; i < steps; i++) {
                    tex.SetPixel(i, 0, gradient.Evaluate(i / (float)steps));
                    if(useVariation) tex.SetPixel(i, 1, gradient2.Evaluate(i / (float)steps));
                }
                tex.Apply();
            }
            else Debug.LogWarning("Colors.gradient is null");

            return tex;
        }

        public void Uniform(ComputeShader compute, int kernel, string name)
        {
            // if (!useGradient) return;
            if (dfltTex == null) dfltTex = new Texture2D(0, 0);
            compute.SetFloat(name + "Steps", useGradient ? steps : 0);
            compute.SetTexture(kernel, name + "Grad", useGradient ? getTexture() : dfltTex);
        }

        public void UniformEmit(ComputeShader compute, string name)
        {
            compute.SetVector(name, color);
            if(useVariation) compute.SetVector(name + "2", color2);
        }
    }

    [System.Serializable]
    public struct Capsule<T>
    {
        public T val;
        public Capsule(T val) { this.val = val; }
    }

    [System.Serializable]
    public struct RenderSettings
    {
        public Preset preset;
        private Preset prev;
        public BlendOp blendOp;
        public BlendMode srcBlend;
        public BlendMode dstBlend;

        public static RenderSettings
            Default = new RenderSettings(0, BlendMode.One, BlendMode.OneMinusSrcAlpha),
            Darken = new RenderSettings(BlendOp.Min, BlendMode.One, BlendMode.One),
            Lighten = new RenderSettings(BlendOp.Max, BlendMode.One, BlendMode.One),
            LinearBurn = new RenderSettings(BlendOp.ReverseSubtract, BlendMode.One, BlendMode.One),
            LinearDodge = new RenderSettings(0, BlendMode.One, BlendMode.One),
            Multiply = new RenderSettings(0, BlendMode.DstColor, BlendMode.OneMinusSrcAlpha);

        public enum Preset { Default, Custom, Darken, Lighten, LinearBurn, LinearDodge, Multiply }

        public RenderSettings(BlendOp blendOp, BlendMode srcBlend, BlendMode dstBlend)
        {
            preset = prev = 0;
            this.blendOp = blendOp;
            this.srcBlend = srcBlend;
            this.dstBlend = dstBlend;
        }

        public void Uniform(Material mat)
        {
            mat.SetInt("_BlendOp", (int)blendOp);
            mat.SetInt("_BlendSrc", (int)srcBlend);
            mat.SetInt("_BlendDst", (int)dstBlend);
        }

        public void setPreset()
        {
            if (preset == prev) {
                prev = preset = Preset.Custom;
                return;
            }

            Preset p = preset;
            switch (preset)
            {
                case Preset.Default: this = Default; break;
                case Preset.Darken: this = Darken; break;
                case Preset.Lighten: this = Lighten; break;
                case Preset.LinearBurn: this = LinearBurn; break;
                case Preset.LinearDodge: this = LinearDodge; break;
                case Preset.Multiply: this = Multiply; break;
            }
            preset = prev = p;
        }
    }
}