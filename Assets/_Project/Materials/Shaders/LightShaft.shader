Shader "MeowStudio/FX/LightShaft"
{
    Properties
    {
        _Color ("Shaft Color", Color) = (0.55, 0.68, 1, 0.12)
        _Intensity ("Intensity", Float) = 1
        _EdgeSoftness ("Edge Softness", Range(0.5, 8)) = 2.5
        _LengthFade ("Length Fade", Range(0.1, 6)) = 1.6
        _AxisFade ("End-On Fade", Range(0.1, 6)) = 1.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "LightShaft"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha One
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Intensity;
                float _EdgeSoftness;
                float _LengthFade;
                float _AxisFade;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float3 viewDirWS  : TEXCOORD1;
                float3 axisWS     : TEXCOORD2;
                float2 uv         : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = positionInputs.positionCS;
                OUT.normalWS = normalInputs.normalWS;
                OUT.viewDirWS = GetWorldSpaceViewDir(positionInputs.positionWS);
                // the mesh is built along its local +Y, so that axis is the beam direction
                OUT.axisWS = TransformObjectToWorldDir(float3(0, 1, 0));
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

                float3 normalWS = normalize(IN.normalWS);
                float3 viewDirWS = normalize(IN.viewDirWS);
                float3 axisWS = normalize(IN.axisWS);

                // stands in for how much of the volume the eye looks through:
                // full at the centre of the silhouette, zero at its edges
                float body = pow(saturate(dot(normalWS, viewDirWS)), _EdgeSoftness);

                // dissolve along the beam so it never ends on a visible edge
                float lengthFade = pow(saturate(1.0 - IN.uv.y), _LengthFade);

                // looking straight down the beam would show a bright disc; fade that away
                float endOn = saturate(1.0 - abs(dot(axisWS, viewDirWS)));
                float axisFade = pow(endOn, _AxisFade);

                float alpha = _Color.a * body * lengthFade * axisFade;
                return half4(_Color.rgb * _Intensity, alpha);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
