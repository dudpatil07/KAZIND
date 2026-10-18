Shader "DroneSimulator/Showcase"
{
    Properties { _BaseColor ("Color", Color) = (0.5,0.5,0.5,1) }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END
            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 normalWS : TEXCOORD0; float3 positionWS : TEXCOORD1; };
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                return output;
            }
            half4 Frag(Varyings input) : SV_Target
            {
                float3 normal = normalize(input.normalWS);
                float3 view = normalize(_WorldSpaceCameraPos - input.positionWS);
                float key = saturate(dot(normal, normalize(float3(-0.5, 0.8, -0.6))));
                float rim = pow(1 - saturate(dot(normal, view)), 3);
                float specular = pow(saturate(dot(normal, normalize(view + normalize(float3(-0.5, 0.8, -0.6))))), 48);
                return half4(_BaseColor.rgb * (0.32 + 0.8 * key) + half3(0.2, 0.38, 0.42) * rim + specular * 0.22, 1);
            }
            ENDHLSL
        }
    }
}
