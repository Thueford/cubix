Shader "Hidden/Blurr"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _StarburstTex ("Texture", 2D) = "white" {}
        _horizontal ("Horizontal", int) = 1
        _vignetteAmount ("Amount", float) = 1.0
        _vignetteWidth ("Width", float) = 0.1
        _CAAmount ("CAAmount", float) = 0.001
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            static const float weight[3] = { 0.2270270270, 0.3162162162, 0.0702702703 };
            static const float offset[3] = { 0.0, 1.3846153846, 3.2307692308 };

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
            float4 _MainTex_TexelSize;
            int _horizontal;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * weight[0];
                float2 texcoord = i.uv;

                if (_horizontal == 1)
                {
                    for (int i = 1; i < 3; ++i)
                    {
                        float xOffset = offset[i] * _MainTex_TexelSize.x;
                        col += tex2D(_MainTex, texcoord + float2(xOffset, 0)) * weight[i];
                        col += tex2D(_MainTex, texcoord - float2(xOffset, 0)) * weight[i];
                    }
                }
                else
                {
                    for (int i = 1; i < 3; ++i)
                    {
                        float yOffset = offset[i] * _MainTex_TexelSize.y;
                        col += tex2D(_MainTex, texcoord + float2(0, yOffset)) * weight[i];
                        col += tex2D(_MainTex, texcoord - float2(0, yOffset)) * weight[i];
                    }
                }
                return col;
            }
            ENDCG
        }

        Pass
        {
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _vignetteAmount;
            float _vignetteWidth;

            fixed4 getVignetteCol(float2 uv) 
            {
                //store original color
                fixed4 col = tex2D(_MainTex, uv);

                // uv from -1 to 1
                float2 texcoord = uv * 2 - 1;

                // Scanlines are more prominent at the corners
                float scanLineIntensity = smoothstep(.8, 1.41422, length(texcoord));
                if (scanLineIntensity > 0)
                    col.rgb *= 1-(sin(texcoord.y * 6.28*100) / 2 + .5) * scanLineIntensity;

                texcoord = abs(texcoord);

                fixed2 u = texcoord * _vignetteWidth;
                u = 1 - (pow(smoothstep(u, 0, 1 - texcoord), 4) * _vignetteAmount);

                col.rgb *= u.x * u.y;
                return col;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // uv from -1 to 1
                float2 uv = i.uv * 2 - 1;

                // Warp Coordinates at the corners
                uv.x *= 1 + pow(abs(uv.y) / 8, 2);
                uv.y *= 1 + pow(abs(uv.x) / 8, 2);

                // uv from 0 to 1
                uv = uv / 2 + .5;

                fixed4 col = fixed4(0, 0, 0, 1);

                // pixels outside the main texture (at the corners) remain black
                if (uv.x <= 1 && uv.x >= 0 && uv.y <= 1 && uv.y >= 0)
                    col = getVignetteCol(uv);

                return col;
            }
            ENDCG
        }

        Pass
        {
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            sampler2D _StarburstTex;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float angle = acos(dot(normalize(i.uv-.5), float2(1, 0))) / 3.1416 / 2 + 1;
                col += max(0, tex2D(_StarburstTex, float2(angle, .5)-.2)) * .3;

                return col;
            }
            ENDCG
        }

        Pass
        {
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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float _CAAmount;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = fixed4(0, 0, 0, 1);
                col.r = tex2D(_MainTex, i.uv + float2(_CAAmount, 0)).r;
                col.g = tex2D(_MainTex, i.uv).g;
                col.b = tex2D(_MainTex, i.uv - float2(_CAAmount, 0)).b;

                return col;
            }
            ENDCG
        }
    }
}
