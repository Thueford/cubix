Shader "Unlit/PartUnlitShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
        // Tags { "RenderType"="Opaque" }
        Blend One OneMinusSrcAlpha
        ZWrite Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);


                /* o.vertex = mul(UNITY_MATRIX_P,
                    mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0)) +
                    float4(v.vertex.x, v.vertex.y, 0.0, 0.0)
                ); */


                /*
                float4x4 mv = UNITY_MATRIX_MV;
                mv._m00 = 1.0f; mv._m10 = 0.0f; mv._m20 = 0.0f;
                mv._m01 = 0.0f; mv._m11 = 1.0f; mv._m21 = 0.0f;
                mv._m02 = 0.0f; mv._m12 = 0.0f; mv._m22 = 1.0f;
                o.vertex = mul(UNITY_MATRIX_P, mul(mv, v.vertex));
                */

                
                // Billboard: https://gist.github.com/kaiware007/8ebad2d28638ff83b6b74970a4f70c9a
                /*
                float3 vpos = mul((float3x3)unity_ObjectToWorld, v.vertex.xyz);
                float4 worldCoord = float4(unity_ObjectToWorld._m03, unity_ObjectToWorld._m13, unity_ObjectToWorld._m23, 1);
                float4 viewPos = mul(UNITY_MATRIX_V, worldCoord) + float4(vpos, 0);
                o.vertex = mul(UNITY_MATRIX_P, viewPos);
                // */
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
}
