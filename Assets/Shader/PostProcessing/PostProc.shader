Shader "Hidden/PostProc"
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
            Name "Blur"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // Weight of Pixels depending on their offset
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

                if (_horizontal == 1)   // horizontal blur pass
                {
                    for (int i = 1; i < 3; ++i)
                    {
                        float xOffset = offset[i] * _MainTex_TexelSize.x;   // offset normalized to texel size
                        col += tex2D(_MainTex, texcoord + float2(xOffset, 0)) * weight[i];
                        col += tex2D(_MainTex, texcoord - float2(xOffset, 0)) * weight[i];
                    }
                }
                else    // vertical blur pass
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
            Name "CRTEffect"

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

            fixed4 getCRTCol(float2 uv) 
            {
                // store original color
                fixed4 col = tex2D(_MainTex, uv);

                // uv from [0, 1] to [-1, 1]
                float2 texcoord = uv * 2 - 1;

                // Scanlines are more prominent at the corners
                float scanLineIntensity = smoothstep(.8, 1.41422, length(texcoord));

                // Apply Scanlines (unsing the sin of the y position of the pixel)
                if (scanLineIntensity > 0)
                    col.rgb *= (1 - (sin(texcoord.y * 6.28 * 100) / 2 + .5) * scanLineIntensity) * 0.8 + 0.2;

                // Vignette needs absolute texcoords
                texcoord = abs(texcoord);

                float vignetteStrength = smoothstep(1 - _vignetteWidth, 1, max(texcoord.x, texcoord.y));
                float vignette = 1 - (pow(vignetteStrength, 4) * _vignetteAmount);
                col.rgb *= vignette;

                return col;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // uv from [0, 1] to [-1, 1]
                float2 uv = i.uv * 2 - 1;

                // Warp Coordinates at the corners
                uv.x *= 1 + pow(abs(uv.y) / 8, 2);
                uv.y *= 1 + pow(abs(uv.x) / 8, 2);

                // uv from [-1, 1] to [0, 1]
                uv = uv / 2 + .5;

                fixed4 col = fixed4(0, 0, 0, 1);

                // only pixels inside the main texture get color (with vignette and scanlines applied)
                if (uv.x <= 1 && uv.x >= 0 && uv.y <= 1 && uv.y >= 0)
                    col = getCRTCol(uv);

                return col;
            }
            ENDCG
        }

        Pass
        {
            Name "GenLensTex"

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
                // MainTex is the LensDirt texture, StarburstTex represents a 1D barcode texture
                fixed4 col = tex2D(_MainTex, i.uv);

                // StarburstTex is accessed via the angle of the texcoord vector (uv)
                float angle = acos(normalize(i.uv - .5).x) / 3.1416 / 2 + 1;
                // Starburst is additively applied to Lens Dirt
                col += max(0, tex2D(_StarburstTex, float2(angle, .5)-.2)) * .3;

                return col;
            }
            ENDCG
        }

        Pass
        {
            Name "ChromaticAberration"

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
                // r and b channels are accessed at an offset (CAAmount)
                col.r = tex2D(_MainTex, i.uv + float2(_CAAmount, 0)).r;
                col.g = tex2D(_MainTex, i.uv).g;
                col.b = tex2D(_MainTex, i.uv - float2(_CAAmount, 0)).b;

                return col;
            }
            ENDCG
        }
    }
}
