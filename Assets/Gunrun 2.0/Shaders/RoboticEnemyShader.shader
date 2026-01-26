Shader "Custom/RoboticEnemyShader"
{
    Properties
    {
        [Header(Main Textures)]
        _MainTex ("Albedo Map", 2D) = "white" {}
        _MetallicMap ("Metallic Map", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _CircuitMask ("Circuit Mask", 2D) = "black" {}
        
        [Header(Material Properties)]
        _Metallic ("Metallic Strength", Range(0, 1)) = 0.8
        _Roughness ("Roughness", Range(0, 1)) = 0.2
        
        [Header(Neon Glow)]
        [HDR] _NeonColor ("Neon Color", Color) = (0, 1, 1, 1)
        _NeonIntensity ("Neon Intensity", Range(0, 10)) = 2.0
        _FresnelPower ("Fresnel Power", Range(0.1, 10)) = 2.0
        
        [Header(LED Circuit)]
        [HDR] _CircuitColor ("Circuit Color", Color) = (1, 0.5, 0, 1)
        _CircuitSpeed ("Circuit Speed", Range(0, 5)) = 1.0
        _CircuitIntensity ("Circuit Intensity", Range(0, 10)) = 3.0
        _CircuitTiling ("Circuit Tiling", Vector) = (1, 1, 0, 0)
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ _STEREO_INSTANCING_ON
            #pragma multi_compile _ _STEREO_MULTIVIEW_ON
            
            // URP gerekli include'ları
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 tangentWS : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float3 viewDirWS : TEXCOORD5;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            // Texture tanımları
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MetallicMap);
            SAMPLER(sampler_MetallicMap);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            TEXTURE2D(_CircuitMask);
            SAMPLER(sampler_CircuitMask);
            
            // Material property buffer
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _NeonColor;
                float4 _CircuitColor;
                float4 _CircuitTiling;
                float _Metallic;
                float _Roughness;
                float _NeonIntensity;
                float _FresnelPower;
                float _CircuitSpeed;
                float _CircuitIntensity;
            CBUFFER_END
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                // URP vertex transformation
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionHCS = positionInputs.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.tangentWS = normalInputs.tangentWS;
                output.bitangentWS = normalInputs.bitangentWS;
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(positionInputs.positionWS);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Texture sampling
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 metallicMap = SAMPLE_TEXTURE2D(_MetallicMap, sampler_MetallicMap, input.uv);
                half3 normalMap = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, input.uv));
                half circuitMask = SAMPLE_TEXTURE2D(_CircuitMask, sampler_CircuitMask, input.uv).r;
                
                // World space normal calculation
                half3x3 tangentToWorld = half3x3(input.tangentWS, input.bitangentWS, input.normalWS);
                half3 worldNormal = normalize(mul(normalMap, tangentToWorld));
                
                // Material properties
                half metallic = metallicMap.r * _Metallic;
                half smoothness = 1.0 - _Roughness;
                
                // Fresnel neon glow
                half fresnel = 1.0 - saturate(dot(worldNormal, input.viewDirWS));
                fresnel = pow(fresnel, _FresnelPower);
                half3 neonGlow = _NeonColor.rgb * fresnel * _NeonIntensity;
                
                // LED circuit animation
                float2 circuitUV = input.uv * _CircuitTiling.xy + _CircuitTiling.zw;
                float circuitTime = _Time.y * _CircuitSpeed;
                
                // Circuit pattern
                float circuitPattern = sin(circuitUV.x * 10.0 + circuitTime) * sin(circuitUV.y * 10.0 + circuitTime * 0.7);
                circuitPattern = saturate(circuitPattern * 0.5 + 0.5);
                
                // Flowing effect
                float2 flowUV = circuitUV + float2(circuitTime * 0.1, circuitTime * 0.05);
                float flow = sin(flowUV.x * 20.0 + circuitTime * 2.0) * sin(flowUV.y * 15.0 + circuitTime * 1.5);
                flow = saturate(flow * 0.5 + 0.5);
                
                half3 circuitGlow = _CircuitColor.rgb * circuitMask * circuitPattern * flow * _CircuitIntensity;
                
                // URP lighting
                Light mainLight = GetMainLight();
                half3 lightColor = mainLight.color;
                half3 lightDir = mainLight.direction;
                
                // Lighting calculations
                half NdotL = saturate(dot(worldNormal, lightDir));
                half3 diffuse = albedo.rgb * lightColor * NdotL;
                
                // Specular (Blinn-Phong)
                half3 halfDir = normalize(lightDir + input.viewDirWS);
                half NdotH = saturate(dot(worldNormal, halfDir));
                half specPower = exp2(10 * smoothness + 1);
                half3 specular = lightColor * pow(NdotH, specPower) * metallic;
                
                // Simple environment reflection
                half3 reflectDir = reflect(-input.viewDirWS, worldNormal);
                half3 envReflection = SampleSH(reflectDir) * metallic * smoothness;
                
                // Final color combination
                half3 finalColor = diffuse + specular + envReflection;
                finalColor += neonGlow;
                finalColor += circuitGlow;
                
                // Ambient lighting
                finalColor += albedo.rgb * 0.1;
                
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma target 3.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ _STEREO_INSTANCING_ON
            #pragma multi_compile _ _STEREO_MULTIVIEW_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            float3 _LightDirection;
            float3 _LightPosition;
            
            float4 GetShadowPositionHClip(Attributes input)
            {
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 lightDirectionWS = normalize(_LightPosition - positionWS);
                #else
                    float3 lightDirectionWS = _LightDirection;
                #endif
                
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
                
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                
                return positionCS;
            }
            
            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.positionCS = GetShadowPositionHClip(input);
                return output;
            }
            
            half4 ShadowPassFragment(Varyings input) : SV_TARGET
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                return 0;
            }
            ENDHLSL
        }
    }
    
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
} 