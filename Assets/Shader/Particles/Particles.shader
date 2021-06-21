Shader "Custom/Particles" {
    Properties {
        _MainTex("Texture", 2D) = "white" {}
    }

    SubShader {
        // Tags { "RenderType" = "Transparent" }

        Pass
        {
            Tags {"Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent"}
            Blend One OneMinusSrcAlpha
            // Cull Off // draw backfaces
            ZWrite Off
            // LOD 100

            CGPROGRAM
            #include "Particles.cginc"
            ENDCG
        }
    }
}
