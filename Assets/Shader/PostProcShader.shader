Shader "Hidden/PostProcShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        //_BufferTex("Texture", 2D) = "cyan" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "Pass1"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            //sampler2D _BufferTex;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // just invert the colors
                if (dot(col.rgb, fixed3(0.2126, 0.7152, 0.0722)) < 0.8) {
                    col = fixed4(0.0, 0.0, 0.0, 1.0);
                }
                return col;
            }
            ENDCG
        }
        /*
        Pass
        {
            Name "Pass2"
            CGPROGRAM
            #pragma fragment frag
            
            #include "UnityCG.cginc"

            sampler2D _MainTex;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                // just invert the colors
                col.rgb = 1 - col.rgb;
                return col;
            }
            ENDCG
        }
        */
    }
}
