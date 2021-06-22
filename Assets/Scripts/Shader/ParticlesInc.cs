using UnityEngine;

namespace Particles
{
    public enum Shape { SPHERE, CUBE };

    public struct Particle
    {
        public Vector3 pos, vel, force;
        public Color col;
        public Vector2 life;
    }

    public struct Vertex2D
    {
        public Vector2 pos;
        public Vector2 uv;
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
    public struct DynamicEffects
    {
        public DynamicEffect pos;
        public DynamicEffect vel;
        public DynamicEffect force;
    }
}