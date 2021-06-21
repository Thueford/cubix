#pragma vertex vert
#pragma fragment frag

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
    float life;
};

StructuredBuffer<Particle> _Particles;

sampler2D _MainTex;
float4 _MainTex_ST;

v2f vert(appdata_t i, uint instanceID : SV_InstanceID)
{
    v2f o;

    float4 ppos = float4(_Particles[instanceID].pos, 0);
    // o.vertex = UnityObjectToClipPos(ppos + i.vertex);
    
    
    // Billboard: https://gist.github.com/kaiware007/8ebad2d28638ff83b6b74970a4f70c9a
    float4 vpos = mul(unity_ObjectToWorld, i.vertex.xyz);
    // worldCoord = PaticlePos + matO2W.translate
    float4 worldCoord = ppos + float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
    float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + vpos;
    o.vertex = mul(UNITY_MATRIX_P, viewPos);
    
    
    o.uv = TRANSFORM_TEX(i.uv, _MainTex);
    o.color = _Particles[instanceID].color;
    return o;
}

float4 frag(v2f i) : SV_Target
{
    return i.color * tex2D(_MainTex, i.uv);

}
