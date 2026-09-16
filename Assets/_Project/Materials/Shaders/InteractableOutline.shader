Shader "MeowStudio/FX/InteractableOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0.35, 0.85, 1, 1)
        _OutlineWidth ("Outline Width (m)", Range(0, 0.02)) = 0.0025
    }

    // Classic inverted-hull outline: the mesh is redrawn pushed outward along its
    // world-space normals, with only its back faces visible (Cull Front). Where that
    // shell isn't hidden behind the object's own front-facing surface, a thin rim
    // shows around the silhouette. One extra unlit, textureless draw per renderer -
    // about as cheap as an outline gets, and it works on any mesh unmodified.
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry+1"
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "UniversalForward" }

            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                // World-space extrusion keeps a constant outline thickness even if
                // the object is non-uniformly scaled, unlike extruding in object space.
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                positionWS += normalWS * _OutlineWidth;

                OUT.positionCS = TransformWorldToHClip(positionWS);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                return _OutlineColor;
            }
            ENDHLSL
        }
    }

    Fallback Off
}
