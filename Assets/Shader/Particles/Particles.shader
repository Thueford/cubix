Shader "Custom/Particles"
{
    Properties
    {
        [MainTexture] 
        _MainTex ("Main Texture", 2D) = "white" {}
        [HideInInspector] _BlendSrc ("Source BlendMode", int) = 0
        [HideInInspector] _BlendDst ("Dest BlendMode", int) = 0
        [HideInInspector] _BlendOp ("BlendOp", int) = 0
    }

    SubShader
    {
        Pass
        {
            Tags {
                "Queue" = "Transparent" 
                "IgnoreProjector" = "True" 
                "RenderType" = "Transparent"
            }

            BlendOp [_BlendOp]
            Blend [_BlendSrc] [_BlendDst]

            // Cull Back
            Lighting Off
            ZWrite Off // necessary major slowdown (1e6:7/20, 5e6:17/94) for transparency
            
            CGPROGRAM
            #pragma target 4.5 % Compute Shaders
            #include "Particles.cginc"
            ENDCG
        }
    }
}
