using UnityEngine;

namespace Particles
{
    public enum Shape { CUBE, SPHERE };

    struct Particle
    {
        public Vector3 pos, vel;
        public Color col;
        public Vector2 life;
    }

    struct Vertex2D
    {
        public Vector2 pos;
        public Vector2 uv;
    }
}