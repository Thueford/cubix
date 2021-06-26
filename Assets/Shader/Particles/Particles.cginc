
#pragma vertex vert
#pragma fragment frag

#include <UnityCG.cginc>
#include "./Particle.cginc"

struct v2f
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
};


StructuredBuffer<Particle> _Particles;
StructuredBuffer<float2> _QuadVert;

sampler2D _MainTex;
float4 _MainTex_ST;

v2f vert(uint vid : SV_VertexID, uint iid : SV_INSTANCEID)
{
    v2f o = { 0,0,0,0,0,0,0,0,0,0 };
    Particle p = _Particles[iid];
    if (p.size.w == 0 || p.size.x < 0 || p.size.y < 0) return o;
    
    float3 vpos = float3(p.size.xy * _QuadVert[vid], 0);
    float4 ppos = float4(p.pos, 1);
    //float4 wpos = mul(unity_ObjectToWorld, vpos);
    
    // Billboard
    o.pos = mul(UNITY_MATRIX_P, float4(vpos, 1) + mul(UNITY_MATRIX_V, ppos));
    // o.pos = UnityObjectToClipPos(ppos + vpos);
    
    o.uv = _QuadVert[vid] + 0.5;  // TRANSFORM_TEX((_QuadVert[vid] + 0.5), _MainTex);
    o.color = p.color;
    return o;
}

float4 frag(v2f i) : SV_Target
{
    return i.color * tex2D(_MainTex, i.uv);
}
