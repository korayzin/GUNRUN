Shader "Custom/UltimateAbyssalPearl"
{
    Properties
    {
        [Header(Surface Iridescence)]
        _ColorA("Iridescent Tone A", Color) = (0.1, 0.4, 0.8, 1)
        _ColorB("Iridescent Tone B", Color) = (0.5, 0.0, 0.5, 1)
        _RimPower("Rim Sharpness", Range(0.5, 10.0)) = 4.0

        [Header(Internal Core Depth)]
        [NoScaleOffset] _CoreTex ("Internal Noise/Core", 2D) = "white" {}
        _ParallaxStrength("Core Depth Strength", Range(0.0, 0.2)) = 0.05
        _CoreScale("Core Tiling", Float) = 2.0
        _CoreSpeed("Core Rotation Speed", Float) = 1.5

        [Header(Organic Mutation)]
        _MutationSpeed("Mutation Speed", Float) = 3.0
        _MutationScale("Mutation Scale", Float) = 5.0
        _MutationStrength("Mutation Strength", Range(0.0, 0.2)) = 0.08
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 300
        ZWrite Off
        Blend SrcAlpha One
        Cull Back

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_CoreTex);
            SAMPLER(sampler_CoreTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _ColorA, _ColorB;
                float _RimPower, _ParallaxStrength, _CoreScale, _CoreSpeed;
                float _MutationSpeed, _MutationScale, _MutationStrength;
            CBUFFER_END

            // Basit bir 3D Noise fonksiyonu (Vertex mutasyonu için)
            float hash(float n) { return frac(sin(n) * 43758.5453123); }
            float noise(float3 x)
            {
                float3 p = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                float n = p.x + p.y * 57.0 + 113.0 * p.z;
                return lerp(lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
                            lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y),
                        lerp(lerp(hash(n + 113.0), hash(n + 114.0), f.x),
                            lerp(hash(n + 170.0), hash(n + 171.0), f.x), f.y), f.z);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                
                // --- Organik Mutasyon (Vertex Displacement) ---
                float n = noise(input.positionOS.xyz * _MutationScale + _Time.y * _MutationSpeed);
                input.positionOS.xyz += input.normalOS * n * _MutationStrength;

                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetCameraPositionWS() - output.positionWS;
                output.uv = input.uv;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float3 viewDir = normalize(input.viewDirWS);
                float3 normal = normalize(input.normalWS);
                float NdotV = saturate(dot(normal, viewDir));

                // --- 1. Iridescence & Rim ---
                float fresnel = pow(1.0 - NdotV, _RimPower);
                half3 surfaceColor = lerp(_ColorA.rgb, _ColorB.rgb, fresnel);

                // --- 2. Parallax Internal Core ---
                // Bakış açısına göre UV'yi kaydırarak derinlik hissi yaratıyoruz
                float2 parallaxOffset = viewDir.xy * _ParallaxStrength;
                float2 coreUV = (input.uv + parallaxOffset) * _CoreScale;
                
                // İç çekirdek rotasyonu
                float s = sin(_Time.y * _CoreSpeed);
                float c = cos(_Time.y * _CoreSpeed);
                coreUV = mul(float2x2(c, -s, s, c), coreUV - 0.5) + 0.5;

                half4 coreColor = SAMPLE_TEXTURE2D(_CoreTex, sampler_CoreTex, coreUV);
                
                // --- 3. Final Composition ---
                // İçerideki parlama ile dış yüzeyi birleştir
                half3 finalRGB = surfaceColor + (coreColor.rgb * _ColorA.rgb * 2.0);
                
                // Kenarları ışıkla patlat (Glow)
                finalRGB += surfaceColor * fresnel * 3.0;

                // Opasite: İnci gibi, içi dolu ama ışık geçiren bir yapı
                float alpha = saturate(fresnel + 0.5);

                return half4(finalRGB, alpha);
            }
            ENDHLSL
        }
    }
}