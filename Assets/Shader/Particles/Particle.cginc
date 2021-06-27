#pragma once

#ifndef _PARTICLE_H_
#define _PARTICLE_H_

#define PI 3.1415926535
#define B(X) (1<<X)

#define P_PREWARM B(0)
#define P_CLRGRAD B(1)
#define P_CLRVARY B(2)

#define S_DOT    0
#define S_CIRCLE 1
#define S_RECT   2
#define S_SPHERE 3
#define S_CUBE   4

#define F(X) (_Flags & X)

struct Particle
{
    float3 pos, vel, force;
    float4 color, size;
    float rand;
};

#endif