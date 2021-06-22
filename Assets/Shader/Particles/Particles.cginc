
#include <UnityCG.cginc>

struct appdata_t
{
    float4 vertex : POSITION;
    float2 uv : TEXCOORD0;
};

struct v2f
{
    float4 vertex : SV_POSITION;
    float2 uv : TEXCOORD0;
    float4 color : COLOR;
};

struct Particle
{
    float3 pos, vel;
    float4 color;
    float2 life;
};

StructuredBuffer<Particle> _Particles;

sampler2D _MainTex;
float4 _MainTex_ST;

v2f vert(appdata_t i, uint iid : SV_INSTANCEID)
{
    v2f o;

    float4 ppos = float4(_Particles[iid].pos, 1);
    float4 vpos = mul(unity_ObjectToWorld, i.vertex.xyz);
    
    // Billboard
    o.vertex = mul(UNITY_MATRIX_P, i.vertex + mul(UNITY_MATRIX_V, ppos));
    // o.vertex = UnityObjectToClipPos(ppos + i.vertex);
    
    o.uv = TRANSFORM_TEX(i.uv, _MainTex);
    o.color = _Particles[iid].color;
    return o;
}

float4 frag(v2f i) : SV_Target
{
    return i.color * tex2D(_MainTex, i.uv);

}
