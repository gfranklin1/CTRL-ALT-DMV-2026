Shader "Custom/Shader_VertexLit"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _EnvMap("Environment Map", Cube) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS     : NORMAL;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half3  diffuse      : TEXCOORD1;   // Gouraud: lighting computed per-vertex
                half3  envColor     : TEXCOORD2;   // Cubemap reflection, pre-sampled via reflect dir
                half4  vertexColor  : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            TEXTURECUBE(_EnvMap);
            SAMPLER(sampler_EnvMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
            CBUFFER_END

            half3 ComputeGouraudDiffuse(float3 positionWS, float3 normalWS)
            {
                half3 lighting = half3(0, 0, 0);
                lighting += SampleSH(normalWS);

                Light mainLight = GetMainLight(TransformWorldToShadowCoord(positionWS));
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                lighting += mainLight.color * (NdotL * mainLight.distanceAttenuation * mainLight.shadowAttenuation);

                return lighting;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   normInputs = GetVertexNormalInputs(IN.normalOS);

                float3 positionWS = posInputs.positionWS;
                half3  normalWS   = normInputs.normalWS;

                OUT.diffuse = ComputeGouraudDiffuse(positionWS, normalWS);
                OUT.vertexColor  = IN.color - 0.25;

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                color *= IN.vertexColor;
                color.rgb *= IN.diffuse;
                return color;
            }
            ENDHLSL
        }
    }
}
