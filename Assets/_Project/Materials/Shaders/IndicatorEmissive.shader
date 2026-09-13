// Minimal unlit emissive shader for small indicator lights, screens, buttons, etc.
// VR-performance notes:
//  - Unlit: no per-pixel lighting, no shadow receiving.
//  - No ShadowCaster pass: this material never casts shadows (skip the extra draw).
//  - SRP Batcher compatible (properties live in the UnityPerMaterial CBUFFER).
//  - Output can go above 1.0 (HDR) so URP Bloom picks it up as a glow.
Shader "Custom/IndicatorEmissive"
{
    Properties
    {
        // Named _BaseColor (not _Color) to match the URP convention that XR Interaction
        // Toolkit's Affordance System (button hover/press color feedback) expects by default.
        [HDR] _BaseColor("Color", Color) = (1, 1, 1, 1)
        _Intensity("Intensity", Range(0, 50)) = 1
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "Geometry" }
        LOD 100

        Pass
        {
            Name "Unlit"
            Tags { "LightMode" = "UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Intensity;
            CBUFFER_END

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                return half4(_BaseColor.rgb * _Intensity, 1.0);
            }
            ENDHLSL
        }
    }
}
