Shader "Custom/M_Cartoonish"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color", Color) = (0.2,0.2,0.2,1)
        _HighlightColor ("Highlight Color", Color) = (1,1,1,1)
        _ShadowThreshold ("Shadow Threshold", Range(0,1)) = 0.5
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Range(0.0, 0.05)) = 0.02
        _Opacity ("Opacity", Range(0.0, 1.0)) = 0.85
        [Header(Jelly Effect)]
        _MutationSpeed ("Mutation Speed", Float) = 3.0
        _MutationScale ("Mutation Scale", Float) = 5.0
        _MutationStrength ("Mutation Strength", Range(0.0, 0.2)) = 0.08
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 200
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Name "ToonLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _STEREO_INSTANCING_ON
            #pragma multi_compile _ _STEREO_MULTIVIEW_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _ShadowColor;
                float4 _HighlightColor;
                float4 _OutlineColor;
                float _ShadowThreshold;
                float _OutlineThickness;
                float _Opacity;
                float _MutationSpeed, _MutationScale, _MutationStrength;
            CBUFFER_END
            float hash(float n) { return frac(sin(n) * 43758.5453123); }
            float noise(float3 x) {
                float3 p = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                float n = p.x + p.y * 57.0 + 113.0 * p.z;
                return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
                            lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
                        lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
                            lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
            }
            Varyings vert(Attributes input) {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float n = noise(input.positionOS.xyz * _MutationScale + _Time.y * _MutationSpeed);
                input.positionOS.xyz += input.normalOS * n * _MutationStrength;
                VertexPositionInputs vpos = GetVertexPositionInputs(input.positionOS.xyz);
                o.positionHCS = vpos.positionCS;
                o.uv = input.uv;
                o.normalWS = TransformObjectToWorldNormal(input.normalOS);
                o.viewDirWS = GetCameraPositionWS() - vpos.positionWS;
                return o;
            }
            float4 frag(Varyings i) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                float3 normal = normalize(i.normalWS);
                float3 viewDir = normalize(i.viewDirWS);
                float3 lightDir = normalize(_MainLightPosition.xyz);
                float NdotL = dot(normal, lightDir);
                float shadowStep = step(_ShadowThreshold, NdotL);
                float3 shadowCol = lerp(_ShadowColor.rgb, _HighlightColor.rgb, shadowStep);
                float4 texSample = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                float3 texCol = texSample.rgb * _Color.rgb;
                float3 finalCol = texCol * shadowCol;
                float alpha = texSample.a * _Color.a * _Opacity;
                return float4(finalCol, alpha);
            }
            ENDHLSL
        }
        // Outline pass
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }
            Cull Front
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _STEREO_INSTANCING_ON
            #pragma multi_compile _ _STEREO_MULTIVIEW_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings {
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineThickness;
                float _Opacity;
                float _MutationSpeed, _MutationScale, _MutationStrength;
            CBUFFER_END
            float hash(float n) { return frac(sin(n) * 43758.5453123); }
            float noise(float3 x) {
                float3 p = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                float n = p.x + p.y * 57.0 + 113.0 * p.z;
                return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
                            lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
                        lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
                            lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
            }
            Varyings vert(Attributes input) {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                float n = noise(input.positionOS.xyz * _MutationScale + _Time.y * _MutationSpeed);
                input.positionOS.xyz += input.normalOS * n * _MutationStrength;
                float3 pos = input.positionOS.xyz + input.normalOS * _OutlineThickness;
                o.positionHCS = TransformObjectToHClip(pos);
                return o;
            }
            float4 frag(Varyings i) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                return float4(_OutlineColor.rgb, _OutlineColor.a * _Opacity);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
