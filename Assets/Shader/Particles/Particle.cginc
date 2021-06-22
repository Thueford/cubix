#pragma once

#ifndef _PARTICLE_H_
#define _PARTICLE_H_

#define PI 3.1415926535
#define B(X) (1<<X)

#define P_POSSHP B(0)
#define P_SPDSHP B(1)
#define P_FORCESHP B(2)


struct Particle
{
    float3 pos, vel, force;
    float4 color;
    float2 life;
};

#endif