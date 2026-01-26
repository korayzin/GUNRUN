Shader "Custom/M_RealEye_Advanced"
{
    Properties
    {
        [Header(Eye Textures)]
        _IrisTexture ("Iris Texture", 2D) = "white" {}
        _ScleraTexture ("Sclera Texture", 2D) = "white" {}
        _IrisNormal ("Iris Normal Map", 2D) = "bump" {}
        _ScleraNormal ("Sclera Normal Map", 2D) = "bump" {}
        _CausticsTexture ("Caustics Texture", 2D) = "black" {}
        
        [Header(Eye Anatomy)]
        _IrisRadius ("Iris Radius", Range(0.1, 0.5)) = 0.25
        _PupilSize ("Pupil Size", Range(0.05, 0.3)) = 0.12
        _PupilOffset ("Pupil Position Offset", Vector) = (-0.05, 0.05, 0, 0)
        _LimbusWidth ("Limbus Dark Ring", Range(0.01, 0.05)) = 0.02
        _CorneaBulge ("Cornea Bulge", Range(0, 0.1)) = 0.03
        _IrisDepth ("Iris Depth", Range(0, 0.05)) = 0.02
        _PupilAsymmetry ("Pupil Asymmetry", Range(0, 0.1)) = 0.02
        _PupilIrregularity ("Pupil Irregularity", Range(0, 0.05)) = 0.01
        
        [Header(Eye Colors)]
        _IrisColor ("Iris Color", Color) = (0.2, 0.4, 0.8, 1)
        _ScleraColor ("Sclera Color", Color) = (1, 0.95, 0.9, 1)
        _PupilColor ("Pupil Color", Color) = (0, 0, 0, 1)
        _LimbusColor ("Limbus Color", Color) = (0.1, 0.1, 0.1, 1)
        _VeinColor ("Vein Color", Color) = (1, 0.5, 0.5, 1)
        
        [Header(Shine and Highlights)]
        _Wetness ("Eye Wetness", Range(0, 1)) = 0.98
        _Glossiness ("Cornea Glossiness", Range(0, 1)) = 0.99
        _SpecularPower ("Specular Power", Range(8, 512)) = 256
        _SpecularIntensity ("Specular Intensity", Range(0, 5)) = 3.0
        _ReflectionStrength ("Reflection Strength", Range(0, 3)) = 2.5
        _FresnelPower ("Fresnel Power", Range(0.1, 8)) = 3.0
        _ShineBoost ("Overall Shine Boost", Range(1, 3)) = 2.2
        
        [Header(Pupil Reflections)]
        _PupilReflectionStrength ("Pupil Reflection Strength", Range(0, 5)) = 2.0
        _PupilSparkleSize ("Pupil Sparkle Size", Range(0.01, 0.2)) = 0.08
        _PupilSparkleIntensity ("Pupil Sparkle Intensity", Range(0, 10)) = 5.0
        _PupilSparkleOffset ("Pupil Sparkle Offset", Vector) = (0.08, 0.08, 0, 0)
        _PupilGlintSharpness ("Pupil Glint Sharpness", Range(1, 50)) = 15
        
        [Header(Advanced Pupil Effects)]
        _PupilReflectionColor ("Pupil Reflection Color", Color) = (0.8, 0.9, 1.0, 1.0)
        _ColoredReflectionSize ("Colored Reflection Size", Range(0.02, 0.15)) = 0.06
        _ColoredReflectionIntensity ("Colored Reflection Intensity", Range(0, 8)) = 4.0
        _ColoredReflectionOffset ("Colored Reflection Offset", Vector) = (-0.06, 0.08, 0, 0)
        _ColoredReflectionSoftness ("Colored Reflection Softness", Range(1, 20)) = 8
        
        [Header(Cool Pupil Features)]
        _PupilGlow ("Pupil Inner Glow", Range(0, 2)) = 0.5
        _PupilGlowColor ("Pupil Glow Color", Color) = (0.1, 0.3, 0.8, 1.0)
        _PupilRingEffect ("Pupil Ring Effect", Range(0, 1)) = 0.3
        _PupilDepthEffect ("Pupil Depth Effect", Range(0, 0.05)) = 0.02
        _HolographicEffect ("Holographic Shimmer", Range(0, 1)) = 0.4
        _PupilLightReaction ("Light Reaction", Range(0, 2)) = 1.0
        _PupilContrast ("Pupil Contrast", Range(0.5, 3)) = 1.5
        
        [Header(Multiple Reflections)]
        _PrimaryReflection ("Primary Reflection", Range(0, 2)) = 1.8
        _SecondaryReflection ("Secondary Reflection", Range(0, 1.5)) = 0.8
        _HighlightSize ("Highlight Size", Range(0.1, 2)) = 0.4
        _HighlightSharpness ("Highlight Sharpness", Range(0.1, 10)) = 4.0
        _RimIntensity ("Rim Light Intensity", Range(0, 2)) = 1.2
        _RefractiveIndex ("Refractive Index", Range(1.0, 2.0)) = 1.376
        
        [Header(Subsurface Scattering)]
        _SubsurfaceColor ("Subsurface Color", Color) = (1, 0.3, 0.2, 1)
        _SubsurfaceStrength ("Subsurface Strength", Range(0, 2)) = 0.6
        _TranslucencyPower ("Translucency Power", Range(0.1, 5)) = 2.0
        
        [Header(Movement and Animation)]
        _SquishyStrength ("Squishy Movement", Range(0, 1)) = 0.3
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1.0
        _MicroMovement ("Micro Movement", Range(0, 0.02)) = 0.005
        _BlinkFrequency ("Blink Frequency", Range(0, 2)) = 0.1
        
        [Header(Eye Sway Movement)]
        _SwayStrength ("Sway Strength", Range(0, 0.1)) = 0.02
        _SwaySpeed ("Sway Speed", Range(0, 3)) = 0.8
        _SwayDirectionNE ("Northeast Sway", Range(0, 1)) = 0.7
        _SwayDirectionSW ("Southwest Sway", Range(0, 1)) = 0.5
        _SwayRandomness ("Sway Randomness", Range(0, 1)) = 0.3
        
        [Header(Caustics and Effects)]
        _CausticsStrength ("Caustics Strength", Range(0, 1)) = 0.4
        _CausticsSpeed ("Caustics Speed", Range(0, 3)) = 1.2
        _IrisPattern ("Iris Pattern Intensity", Range(0.5, 2)) = 1.3
        _VeinStrength ("Sclera Veins", Range(0, 1)) = 0.6
        _Brightness ("Overall Brightness", Range(0.5, 2)) = 1.4
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 300
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_instancing
            #pragma multi_compile _ _STEREO_INSTANCING_ON
            #pragma multi_compile _ _STEREO_MULTIVIEW_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
            };
            
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                float3 tangentWS : TEXCOORD4;
                float3 bitangentWS : TEXCOORD5;
                float4 shadowCoord : TEXCOORD6;
                float fogCoord : TEXCOORD7;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_IrisTexture);
            SAMPLER(sampler_IrisTexture);
            TEXTURE2D(_ScleraTexture);
            SAMPLER(sampler_ScleraTexture);
            TEXTURE2D(_IrisNormal);
            SAMPLER(sampler_IrisNormal);
            TEXTURE2D(_ScleraNormal);
            SAMPLER(sampler_ScleraNormal);
            TEXTURE2D(_CausticsTexture);
            SAMPLER(sampler_CausticsTexture);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _IrisTexture_ST;
                float4 _ScleraTexture_ST;
                float4 _CausticsTexture_ST;
                half _IrisRadius;
                half _PupilSize;
                half4 _PupilOffset;
                half _LimbusWidth;
                half _PupilAsymmetry;
                half _PupilIrregularity;
                half _CorneaBulge;
                half _IrisDepth;
                half4 _IrisColor;
                half4 _ScleraColor;
                half4 _PupilColor;
                half4 _LimbusColor;
                half4 _VeinColor;
                half _Wetness;
        half _Glossiness;
                half _SpecularPower;
                half _SpecularIntensity;
                half _ReflectionStrength;
                half _FresnelPower;
                half _ShineBoost;
                half _PrimaryReflection;
                half _SecondaryReflection;
                half _HighlightSize;
                half _HighlightSharpness;
                half _RimIntensity;
                half _RefractiveIndex;
                half4 _SubsurfaceColor;
                half _SubsurfaceStrength;
                half _TranslucencyPower;
                half _SquishyStrength;
                half _PulseSpeed;
                half _MicroMovement;
                half _BlinkFrequency;
                half _SwayStrength;
                half _SwaySpeed;
                half _SwayDirectionNE;
                half _SwayDirectionSW;
                half _SwayRandomness;
                half _CausticsStrength;
                half _CausticsSpeed;
                half _IrisPattern;
                half _VeinStrength;
                half _Brightness;
                half _PupilReflectionStrength;
                half _PupilSparkleSize;
                half _PupilSparkleIntensity;
                half4 _PupilSparkleOffset;
                half _PupilGlintSharpness;
                half4 _PupilReflectionColor;
                half _ColoredReflectionSize;
                half _ColoredReflectionIntensity;
                half4 _ColoredReflectionOffset;
                half _ColoredReflectionSoftness;
                half _PupilGlow;
                half4 _PupilGlowColor;
                half _PupilRingEffect;
                half _PupilDepthEffect;
                half _HolographicEffect;
                half _PupilLightReaction;
                half _PupilContrast;
            CBUFFER_END
            
            // Enhanced circular mask with soft edges
            half SmoothCircleMask(float2 uv, float2 center, half radius, half smoothness)
            {
                half dist = distance(uv, center);
                return 1.0 - smoothstep(radius - smoothness, radius + smoothness, dist);
            }
            
            // Realistic pupil shape with irregularities
            half RealisticPupilMask(float2 uv, float2 center, half radius, half asymmetry, half irregularity, float time)
            {
                float2 toCenter = uv - center;
                float angle = atan2(toCenter.y, toCenter.x);
                
                // Pupil asimetrisi - gerçek pupiller mükemmel yuvarlak değil
                half asymmetricRadius = radius * (1.0 + sin(angle * 2.0) * asymmetry);
                
                // Pupil düzensizliği - kenar dalgalanması
                half irregularRadius = asymmetricRadius * (1.0 + sin(angle * 8.0 + time * 0.1) * irregularity);
                
                half dist = length(toCenter);
                return smoothstep(irregularRadius + 0.005, irregularRadius - 0.005, dist);
            }
            
            // Parallax mapping for depth effect
            float2 ParallaxMapping(float2 uv, float3 viewDir, half depth, half2 center)
            {
                float2 toCenter = uv - center;
                float2 offset = viewDir.xy / (viewDir.z + 0.42) * depth;
                return uv - offset * length(toCenter);
            }
            
            // Advanced noise function for organic movement
            float Noise(float2 uv, float time)
            {
                return sin(uv.x * 12.9898 + uv.y * 78.233 + time) * 43758.5453;
            }
            
            // Squishy animation function
            float2 SquishyMovement(float2 uv, float time)
            {
                float2 center = float2(0.5, 0.5);
                float2 toCenter = uv - center;
                float dist = length(toCenter);
                
                // Breathing/pulsing effect
                float pulse = sin(time * _PulseSpeed) * 0.5 + 0.5;
                float squish = pulse * _SquishyStrength * (1.0 - dist);
                
                // Micro movements
                float2 microMove = float2(
                    sin(time * 3.7 + uv.x * 20) * _MicroMovement,
                    cos(time * 2.3 + uv.y * 15) * _MicroMovement
                );
                
                return uv + toCenter * squish + microMove;
            }
            
            // Enhanced specular calculation with multiple highlights
            half3 CalculateSpecular(float3 lightDir, float3 viewDir, float3 normal, half3 lightColor)
            {
                float3 halfVector = normalize(lightDir + viewDir);
                half NdotH = saturate(dot(normal, halfVector));
                
                // Primary sharp highlight
                half primarySpec = pow(NdotH, _SpecularPower * _HighlightSharpness);
                half3 primarySpecular = lightColor * primarySpec * _SpecularIntensity * _PrimaryReflection;
                
                // Secondary broader highlight
                half secondarySpec = pow(NdotH, _SpecularPower * 0.3);
                half3 secondarySpecular = lightColor * secondarySpec * _SecondaryReflection * _HighlightSize;
                
                // Concentrated shine spot
                half shineSpot = pow(NdotH, _SpecularPower * 2.0);
                half3 concentratedShine = lightColor * shineSpot * _ShineBoost;
                
                return primarySpecular + secondarySpecular + concentratedShine;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                // Cornea bulge effect
                float2 center = float2(0.5, 0.5);
                float2 toCenter = input.uv - center;
                float dist = length(toCenter);
                float bulge = exp(-dist * 8.0) * _CorneaBulge;
                
                float3 positionOS = input.positionOS.xyz;
                positionOS += input.normalOS * bulge;
                
                // Eye sway movement - kuzeydoğu ve güneybatı sallanma
                float time = _Time.y * _SwaySpeed;
                
                // Kuzeydoğu yönü (Northeast: +X, +Z)
                float swayNE = sin(time + input.positionOS.x * 2.0) * _SwayDirectionNE;
                // Güneybatı yönü (Southwest: -X, -Z) 
                float swaySW = cos(time * 1.3 + input.positionOS.z * 1.5) * _SwayDirectionSW;
                
                // Rastgele varyasyon ekle
                float randomFactor = sin(time * 0.7 + input.positionOS.y * 3.0) * _SwayRandomness;
                
                // Sallanma vektörü hesapla
                float3 swayVector = float3(
                    (swayNE - swaySW + randomFactor * 0.5) * _SwayStrength,
                    sin(time * 2.1) * _SwayStrength * 0.3, // Hafif Y ekseni hareketi
                    (swayNE - swaySW - randomFactor * 0.3) * _SwayStrength
                );
                
                // Sallanmayı pozisyona ekle
                positionOS += swayVector;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionHCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.tangentWS = normalInput.tangentWS;
                output.bitangentWS = normalInput.bitangentWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _IrisTexture);
                output.shadowCoord = GetShadowCoord(vertexInput);
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float time = _Time.y;
                
                // Apply squishy movement
                float2 animatedUV = SquishyMovement(input.uv, time);
                float2 centerUV = float2(0.5, 0.5);
                
                // Gerçekçi pupil pozisyonu - iris'in sol üst kısmında
                float2 pupilCenter = centerUV + _PupilOffset.xy;
                
                // Pupil'in iris sınırları içinde kalmasını sağla
                float2 toPupilCenter = pupilCenter - centerUV;
                float maxPupilDistance = _IrisRadius - _PupilSize - 0.02; // Güvenlik mesafesi
                if (length(toPupilCenter) > maxPupilDistance) {
                    pupilCenter = centerUV + normalize(toPupilCenter) * maxPupilDistance;
                }
                
                // Create anatomical masks with realistic pupil
                half irisMask = SmoothCircleMask(animatedUV, centerUV, _IrisRadius, 0.02);
                half pupilMask = RealisticPupilMask(animatedUV, pupilCenter, _PupilSize, _PupilAsymmetry, _PupilIrregularity, time);
                
                // Pupil'in iris dışına taşmasını engelle
                pupilMask *= irisMask;
                
                half limbusMask = SmoothCircleMask(animatedUV, centerUV, _IrisRadius + _LimbusWidth, 0.01) - irisMask;
                half scleraMask = 1.0 - irisMask - limbusMask;
                
                // Parallax mapping for iris depth
                float3 viewDirTS = normalize(float3(
                    dot(input.viewDirWS, input.tangentWS),
                    dot(input.viewDirWS, input.bitangentWS),
                    dot(input.viewDirWS, input.normalWS)
                ));
                float2 parallaxUV = ParallaxMapping(animatedUV, viewDirTS, _IrisDepth, centerUV);
                
                // Sample textures with enhanced patterns
                half4 irisAlbedo = SAMPLE_TEXTURE2D(_IrisTexture, sampler_IrisTexture, parallaxUV);
                irisAlbedo.rgb = lerp(half3(0.5, 0.5, 0.5), irisAlbedo.rgb, _IrisPattern);
                irisAlbedo *= _IrisColor;
                
                half4 scleraAlbedo = SAMPLE_TEXTURE2D(_ScleraTexture, sampler_ScleraTexture, animatedUV);
                scleraAlbedo = lerp(scleraAlbedo, _VeinColor, scleraAlbedo.r * _VeinStrength);
                scleraAlbedo *= _ScleraColor;
                
                // Sample normal maps
                float3 irisNormal = UnpackNormal(SAMPLE_TEXTURE2D(_IrisNormal, sampler_IrisNormal, parallaxUV));
                float3 scleraNormal = UnpackNormal(SAMPLE_TEXTURE2D(_ScleraNormal, sampler_ScleraNormal, animatedUV));
                
                // Combine normals based on masks
                float3 finalNormal = lerp(scleraNormal, irisNormal, irisMask);
                finalNormal = normalize(mul(finalNormal, float3x3(input.tangentWS, input.bitangentWS, input.normalWS)));
                
                // Caustics effect
                float2 causticsUV = animatedUV * _CausticsTexture_ST.xy + _Time.y * _CausticsSpeed * 0.1;
                half4 caustics = SAMPLE_TEXTURE2D(_CausticsTexture, sampler_CausticsTexture, causticsUV);
                
                // Combine colors based on anatomical regions
                half4 finalColor = lerp(scleraAlbedo, irisAlbedo, irisMask);
                finalColor = lerp(finalColor, _PupilColor, pupilMask);
                finalColor = lerp(finalColor, _LimbusColor, limbusMask);
                
                // Add caustics to iris
                finalColor.rgb += caustics.rgb * _CausticsStrength * irisMask;
                
                // Enhanced lighting calculation
                Light mainLight = GetMainLight(input.shadowCoord);
                half3 lightColor = mainLight.color;
                half3 lightDir = mainLight.direction;
                half3 viewDir = normalize(input.viewDirWS);
                
                float NdotL = saturate(dot(finalNormal, lightDir));
                float NdotV = saturate(dot(finalNormal, viewDir));
                float VdotL = saturate(dot(viewDir, lightDir));
                
                // Subsurface scattering
                half subsurface = pow(saturate(VdotL), _TranslucencyPower) * _SubsurfaceStrength;
                finalColor.rgb = lerp(finalColor.rgb, _SubsurfaceColor.rgb, subsurface * scleraMask);
                
                // Enhanced diffuse lighting
                half3 diffuse = lightColor * NdotL * mainLight.shadowAttenuation;
                
                // Multiple specular highlights for maximum shine
                half3 specular = CalculateSpecular(lightDir, viewDir, finalNormal, lightColor);
                specular *= _Wetness * _Glossiness;
                
                // Enhanced Fresnel reflection
                half fresnel = pow(1.0 - NdotV, _FresnelPower);
                half3 fresnelReflection = lightColor * fresnel * _ReflectionStrength * _Wetness;
                
                // Multiple rim lighting layers
                half rim = 1.0 - NdotV;
                half3 rimLight1 = lightColor * pow(rim, 2.0) * _RimIntensity * _Wetness;
                half3 rimLight2 = lightColor * pow(rim, 4.0) * _RimIntensity * 0.5;
                half3 totalRimLight = rimLight1 + rimLight2;
                
                // Cornea ultra shine effect
                half distFromCenter = distance(animatedUV, centerUV);
                half corneaShine = exp(-distFromCenter * 3.0) * fresnel * _ShineBoost;
                half3 corneaHighlight = lightColor * corneaShine * _Wetness;
                
                // Eye wetness sparkle
                float sparklePattern = sin(animatedUV.x * 50 + time) * sin(animatedUV.y * 50 + time * 1.3);
                half sparkle = saturate(sparklePattern) * fresnel * _Wetness * 0.3;
                half3 sparkleLight = lightColor * sparkle;
                
                // Pupil reflections - beyaz minik parıltı efekti (yeni pupil pozisyonuna göre)
                half distFromPupilCenter = distance(animatedUV, pupilCenter);
                half pupilArea = pupilMask; // Gerçekçi pupil şeklini kullan
                
                // Ana pupil parıltısı - beyaz minik nokta
                half2 sparkleCenter = pupilCenter + _PupilSparkleOffset.xy;
                half sparkleDistance = distance(animatedUV, sparkleCenter);
                half pupilSparkle = 1.0 - smoothstep(0.0, _PupilSparkleSize, sparkleDistance);
                pupilSparkle = pow(pupilSparkle, _PupilGlintSharpness);
                pupilSparkle *= pupilArea; // Sadece pupil içinde
                half3 pupilSparkleLight = half3(1, 1, 1) * pupilSparkle * _PupilSparkleIntensity;
                
                // İkincil parıltı - daha küçük
                half2 secondarySparkleCenter = pupilCenter + _PupilSparkleOffset.xy * 0.7 + half2(-0.03, 0.04);
                half secondarySparkleDistance = distance(animatedUV, secondarySparkleCenter);
                half secondarySparkle = 1.0 - smoothstep(0.0, _PupilSparkleSize * 0.6, secondarySparkleDistance);
                secondarySparkle = pow(secondarySparkle, _PupilGlintSharpness * 1.5);
                secondarySparkle *= pupilArea;
                half3 secondarySparkleLight = half3(1, 1, 1) * secondarySparkle * _PupilSparkleIntensity * 0.6;
                
                // Kornea genel yansıması
                half3 worldViewDir = normalize(_WorldSpaceCameraPos - input.positionWS);
                half corneaReflection = pow(saturate(dot(finalNormal, worldViewDir)), 2.0);
                half3 pupilCorneaReflection = lightColor * corneaReflection * _PupilReflectionStrength * pupilArea;
                
                // Renkli pupil yansıması - pupil'in sol üst kısmında
                half2 coloredReflectionCenter = pupilCenter + _ColoredReflectionOffset.xy;
                half coloredReflectionDistance = distance(animatedUV, coloredReflectionCenter);
                half coloredReflection = 1.0 - smoothstep(0.0, _ColoredReflectionSize, coloredReflectionDistance);
                coloredReflection = pow(coloredReflection, _ColoredReflectionSoftness);
                coloredReflection *= pupilArea; // Sadece pupil içinde
                half3 coloredReflectionLight = _PupilReflectionColor.rgb * coloredReflection * _ColoredReflectionIntensity;
                
                // Renkli yansımanın kenar efekti - daha yumuşak geçiş
                half coloredReflectionRim = 1.0 - smoothstep(_ColoredReflectionSize * 0.7, _ColoredReflectionSize * 1.2, coloredReflectionDistance);
                coloredReflectionRim = pow(coloredReflectionRim, _ColoredReflectionSoftness * 0.5);
                coloredReflectionRim *= pupilArea;
                half3 coloredReflectionRimLight = _PupilReflectionColor.rgb * coloredReflectionRim * _ColoredReflectionIntensity * 0.3;
                
                // === HAVALİ PUPİL EFEKTLERİ ===
                
                // 1. Pupil iç parlaması - derinlik hissi
                half pupilGlowMask = smoothstep(_PupilSize - 0.02, _PupilSize - 0.005, distFromPupilCenter);
                half3 pupilInnerGlow = _PupilGlowColor.rgb * pupilGlowMask * _PupilGlow;
                
                // 2. Pupil kenar halkası - gelişmiş sınır efekti
                half pupilRingDistance = abs(distFromPupilCenter - _PupilSize);
                half pupilRing = exp(-pupilRingDistance * 100.0) * _PupilRingEffect;
                half3 pupilRingLight = half3(0.8, 0.9, 1.0) * pupilRing;
                
                // 3. Holografik parıltı - gökkuşağı efekti
                half2 hologramUV = animatedUV * 15.0 + time * 0.5;
                half hologramPattern = sin(hologramUV.x + hologramUV.y) * 0.5 + 0.5;
                half3 hologramColors = half3(
                    sin(hologramPattern * 6.28 + time) * 0.5 + 0.5,
                    sin(hologramPattern * 6.28 + time + 2.09) * 0.5 + 0.5,
                    sin(hologramPattern * 6.28 + time + 4.18) * 0.5 + 0.5
                );
                half3 holographicShimmer = hologramColors * _HolographicEffect * pupilArea * 0.3;
                
                // 4. Işığa tepki - dinamik pupil boyutu efekti
                half lightIntensity = dot(lightColor, half3(0.299, 0.587, 0.114)); // Luminance
                half lightReaction = lerp(1.0, 0.7, lightIntensity * _PupilLightReaction);
                half dynamicPupilMask = RealisticPupilMask(animatedUV, pupilCenter, _PupilSize * lightReaction, _PupilAsymmetry, _PupilIrregularity, time);
                
                // 5. Pupil derinlik efekti - 3D görünüm (yeni pupil pozisyonuna göre)
                half2 depthOffset = (animatedUV - pupilCenter) * _PupilDepthEffect;
                half2 depthUV = animatedUV + depthOffset;
                half depthMask = smoothstep(_PupilSize + 0.01, _PupilSize - 0.02, distance(depthUV, pupilCenter));
                half3 depthShadow = half3(0, 0, 0) * depthMask * 0.3;
                
                // 6. Gelişmiş pupil kontrast
                half contrastMask = pupilArea;
                half3 contrastEffect = lerp(half3(0.5, 0.5, 0.5), half3(0, 0, 0), contrastMask * _PupilContrast);
                
                // Final lighting with enhanced brightness
                half3 ambient = half3(0.1, 0.1, 0.1);
                half3 lighting = (diffuse + specular + fresnelReflection + totalRimLight + corneaHighlight + sparkleLight + 
                                pupilCorneaReflection + ambient) * _Brightness;
                finalColor.rgb *= lighting;
                
                // Pupil parıltılarını ayrı olarak ekle - daha belirgin olması için
                finalColor.rgb += pupilSparkleLight * _Brightness;
                finalColor.rgb += secondarySparkleLight * _Brightness;
                
                // Renkli pupil yansımasını ekle
                finalColor.rgb += coloredReflectionLight * _Brightness;
                finalColor.rgb += coloredReflectionRimLight * _Brightness;
                
                // Havalı pupil efektlerini ekle
                finalColor.rgb += pupilInnerGlow * _Brightness;
                finalColor.rgb += pupilRingLight * _Brightness;
                finalColor.rgb += holographicShimmer * _Brightness;
                finalColor.rgb -= depthShadow; // Derinlik gölgesi
                
                // Pupil kontrastını uygula
                finalColor.rgb = lerp(finalColor.rgb, contrastEffect, dynamicPupilMask * 0.3);
                
                // Overall shine boost
                finalColor.rgb *= _ShineBoost;
                
                // Apply fog
                finalColor.rgb = MixFog(finalColor.rgb, input.fogCoord);
                
                return finalColor;
            }
            ENDHLSL
        }
        
        // Shadow caster pass for proper shadows
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode" = "ShadowCaster"}

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
    
    Fallback "Universal Render Pipeline/Lit"
}
