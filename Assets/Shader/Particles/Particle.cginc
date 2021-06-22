#pragma once

#ifndef _PARTICLE_H_
#define _PARTICLE_H_

struct Particle
{
    float3 pos, vel;
    float4 color;
    float2 life;
};

#endif