Shader "Custom/Particles"
{
    Properties {
        _MainTex("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Pass
        {
            Name "TransparentParticles"
            Tags {
                "Queue" = "Transparent" 
                "IgnoreProjector" = "True" 
                "RenderType" = "Transparent"
            }
            
            Blend One OneMinusSrcAlpha
            ZWrite Off // necessary major slowdown (1e6:7/20, 5e6:17/94) for transparency
            
            CGPROGRAM
            #include "Particles.cginc"
            #pragma vertex vert
            #pragma fragment frag
            ENDCG
        }
    }
}
