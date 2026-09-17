Shader "MeowStudio/FX/InteractableOutline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0.35, 0.85, 1, 1)
        _OutlineWidth ("Outline Width (m)", Range(0, 0.02)) = 0.0025
    }

    // Classic inverted-hull outline: the mesh is redrawn pushed outward, with only its
    // back faces visible (Cull Front). Where that shell isn't hidden behind the object's
    // own front-facing surface, a thin rim shows around the silhouette. One extra unlit,
    // textureless draw per renderer - about as cheap as an outline gets.
    //
    // Extrusion uses TEXCOORD2, a per-position-averaged "smooth normal" baked at runtime
    // by InteractableOutline (see EnsureSmoothNormals) - NOT the mesh's real NORMAL. Our
    // props are flat-shaded/faceted low-poly, so every hard edge has duplicate vertices
    // with different normals; extruding along the real (per-face) normal pushes adjacent
    // faces apart at those edges and the shell tears open there, which is exactly the
    // "near side isn't outlined" symptom this was built to fix. Averaging by position
    // first makes every vertex that shares a location agree on one extrusion direction,
    // so the shell stays sealed at every edge regardless of shading.
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
                float4 positionOS     : POSITION;
                float3 smoothNormalOS : TEXCOORD2;
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
                // A mesh with no baked smooth normal yet (smoothNormalOS all zero) simply
                // doesn't extrude - the outline degrades to invisible, not to garbage.
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.smoothNormalOS);
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
