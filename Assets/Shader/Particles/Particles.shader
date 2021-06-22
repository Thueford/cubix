Shader "Custom/Particles"
{
    Properties {
        _MainTex("Texture", 2D) = "white" {}
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
            
            Blend One OneMinusSrcAlpha
            // Cull Back
            Lighting Off
            ZWrite Off // necessary major slowdown (1e6:7/20, 5e6:17/94) for transparency
            
            CGPROGRAM
            #pragma target 5.0
            #include "Particles.cginc"
            ENDCG
        }
    }
}
