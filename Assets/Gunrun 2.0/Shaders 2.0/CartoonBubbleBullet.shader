Shader "Custom/CartoonBubbleBullet_Advanced"
{
    Properties
    {
        [Header(Base Settings)]
        _BaseColor("Base Color", Color) = (0.2, 0.7, 1, 1)
        [HDR] _GlowColor("Glow Color (Rim)", Color) = (1, 1, 1, 1)
        _RimPower("Rim Sharpness", Range(0.5, 8.0)) = 3.0
        _Opacity("Overall Opacity", Range(0.0, 1.0)) = 0.8

        [Header(Wobble Animation)]
        _WobbleSpeed("Wobble Speed", Float) = 2.0
        _WobbleStrength("Wobble Strength", Float) = 0.05

        [Header(Surface Detail)]
        [NoScaleOffset] _NoiseTex ("Noise Texture (Grayscale)", 2D) = "white" {}
        _NoiseScale("Noise Tiling", Float) = 1.0
        _NoiseScrollSpeed("Noise Scroll Speed (X,Y)", Vector) = (0.5, 0.5, 0, 0)
        _NoiseIntensity("Noise Intensity", Range(0.0, 1.0)) = 0.5
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
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
                UNITY_VERTEX_INPUT_INSTANCE_ID // VR Stereo Support
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO // VR Stereo Support
            };

            // Texture ve Sampler Tanımları
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _GlowColor;
                float _RimPower;
                float _Opacity;
                float _WobbleSpeed;
                float _WobbleStrength;
                float _NoiseScale;
                float2 _NoiseScrollSpeed;
                float _NoiseIntensity;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                // Vertex Wobble (Jöle efekti - World Space bazlı)
                float3 worldPosInitial = TransformObjectToWorld(input.positionOS.xyz);
                // Zamanı ve pozisyonu kullanarak dalgalanma oluştur
                float wobble = sin(_Time.y * _WobbleSpeed + worldPosInitial.y * 5.0 + worldPosInitial.x * 5.0) * _WobbleStrength;
                
                // Normal yönünde vertexi şişir/indir
                input.positionOS.xyz += input.normalOS * wobble;

                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float3 viewDir = normalize(GetCameraPositionWS() - input.positionWS);
                float3 normal = normalize(input.normalWS);

                // --- Noise (Doku) Hesaplama ---
                // UV yerine World Position kullanarak küre üzerinde dikiş izi olmadan kaplama yapıyoruz.
                // Mermi hareket ettikçe doku da kayacak.
                float2 noiseUV = input.positionWS.xy * _NoiseScale;
                noiseUV += _Time.y * _NoiseScrollSpeed;
                half noiseValue = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, noiseUV).r;
                
                // Noise'u daha kontrastlı hale getir (Cartoon etki için)
                noiseValue = smoothstep(0.2, 0.8, noiseValue);


                // --- Fresnel (Kenar Parlaması) ---
                float NdotV = dot(normal, viewDir);
                float fresnel = 1.0 - saturate(NdotV);
                float rim = pow(fresnel, _RimPower);

                // --- Renkleri Birleştirme ---
                // Taban rengi ile Glow rengini kenar etkisine göre karıştır
                half3 finalRGB = lerp(_BaseColor.rgb, _GlowColor.rgb, rim);
                
                // Noise'u renge hafifçe ekle (isteğe bağlı, şu an sadece alfayı etkiliyor)
                // finalRGB += noiseValue * _GlowColor.rgb * 0.2; 

                // --- Opasite (Saydamlık) Hesaplama ---
                // Kenarlar daha opak, merkez daha saydam. Noise değeri de yer yer saydamlık katıyor.
                float alpha = saturate(rim + 0.3); // Temel kenar bazlı alfa
                
                // Noise'u alfaya yedir: Beyaz kısımlar daha opak, siyah kısımlar daha saydam
                float noiseAlphaEffect = lerp(1.0, noiseValue, _NoiseIntensity);
                alpha *= noiseAlphaEffect;
               
                // Genel opasite çarpanı
                alpha *= _Opacity;

                return half4(finalRGB, alpha);
            }
            ENDHLSL
        }
    }
}