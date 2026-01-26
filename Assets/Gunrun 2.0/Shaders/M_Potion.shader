Shader "Custom/M_Potion"
{
    Properties
    {
        [Header(Algae Pool Base)]
        _PotionColor ("Base Pool Color", Color) = (0.3, 0.8, 0.6, 0.7)
        _AlgaeColor ("Algae Color", Color) = (0.4, 0.7, 0.2, 0.8)
        _PotionIntensity ("Pool Intensity", Range(0.3, 2.0)) = 0.8
        _LiquidDensity ("Liquid Density", Range(0.1, 2.0)) = 0.6
        _Transparency ("Transparency", Range(0.2, 1.0)) = 0.7
        _MainTex ("Base Texture", 2D) = "white" {}
        
        [Header(Micro Particles)]
        _WhiteParticleColor ("Light Particles", Color) = (0.9, 1.0, 0.8, 1.0)
        _WhiteParticleSize ("Light Particle Size", Range(0.005, 0.1)) = 0.03
        _WhiteParticleDensity ("Light Particle Density", Range(5.0, 25.0)) = 15.0
        _WhiteParticleSpeed ("Light Particle Speed", Range(0.3, 3.0)) = 1.2
        _WhiteParticleIntensity ("Light Particle Intensity", Range(0.0, 6.0)) = 4.0
        
        [Header(Algae Particles)]
        _BlueParticleColor ("Algae Particles", Color) = (0.2, 0.6, 0.3, 1.0)
        _BlueParticleSize ("Algae Particle Size", Range(0.005, 0.08)) = 0.025
        _BlueParticleDensity ("Algae Particle Density", Range(8.0, 30.0)) = 20.0
        _BlueParticleSpeed ("Algae Particle Speed", Range(0.2, 2.0)) = 0.8
        _BlueParticleIntensity ("Algae Particle Intensity", Range(0.0, 6.0)) = 5.0
        
        [Header(Organic Flow)]
        _FlowSpeed ("Flow Speed", Range(0.5, 4.0)) = 1.5
        _SwirlingMotion ("Organic Swirling", Range(0.2, 2.0)) = 0.8
        _VerticalFlow ("Vertical Flow", Range(0.1, 2.0)) = 0.6
        _TurbulenceStrength ("Organic Turbulence", Range(0.2, 2.0)) = 0.8
        _MovementAmplitude ("Movement Amplitude", Range(0.1, 1.0)) = 0.4
        
        [Header(Sphere Optimization)]
        _SphereCompensation ("Sphere UV Compensation", Range(0.0, 2.0)) = 1.0
        _TileScale ("Tile Scale", Range(0.1, 5.0)) = 1.0
        _TileOffset ("Tile Offset", Vector) = (0, 0, 0, 0)
        
        [Header(Glow Effects)]
        _RimColor ("Rim Glow Color", Color) = (0.6, 0.9, 0.7, 1.0)
        _RimPower ("Rim Power", Range(0.5, 5.0)) = 2.0
        _RimIntensity ("Rim Intensity", Range(0.0, 2.0)) = 0.8
        _EmissionIntensity ("Emission Intensity", Range(0.0, 2.0)) = 1.0
        
        [Header(Surface Properties)]
        _Metallic ("Metallic", Range(0,1)) = 0.1
        _Smoothness ("Smoothness", Range(0,1)) = 0.6
        _NormalStrength ("Normal Strength", Range(0.0, 1.0)) = 0.5
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "RenderPipeline"="UniversalPipeline" 
            "Queue"="Transparent"
        }
        LOD 300
        
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
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
                float4 shadowCoord : TEXCOORD4;
                float fogCoord : TEXCOORD5;
                float3 positionOS : TEXCOORD6;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _PotionColor;
                float4 _AlgaeColor;
                float4 _WhiteParticleColor;
                float4 _BlueParticleColor;
                float4 _RimColor;
                float4 _MainTex_ST;
                float4 _TileOffset;
                
                float _PotionIntensity;
                float _LiquidDensity;
                float _Transparency;
                float _WhiteParticleSize;
                float _WhiteParticleDensity;
                float _WhiteParticleSpeed;
                float _WhiteParticleIntensity;
                float _BlueParticleSize;
                float _BlueParticleDensity;
                float _BlueParticleSpeed;
                float _BlueParticleIntensity;
                float _FlowSpeed;
                float _SwirlingMotion;
                float _VerticalFlow;
                float _TurbulenceStrength;
                float _MovementAmplitude;
                float _SphereCompensation;
                float _TileScale;
                float _RimPower;
                float _RimIntensity;
                float _EmissionIntensity;
                float _Metallic;
                float _Smoothness;
                float _NormalStrength;
            CBUFFER_END
            
            // High-performance noise functions
            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }
            
            float noise(float3 x)
            {
                float3 p = floor(x);
                float3 f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                
                return lerp(
                    lerp(lerp(hash(p + float3(0,0,0)), hash(p + float3(1,0,0)), f.x),
                         lerp(hash(p + float3(0,1,0)), hash(p + float3(1,1,0)), f.x), f.y),
                    lerp(lerp(hash(p + float3(0,0,1)), hash(p + float3(1,0,1)), f.x),
                         lerp(hash(p + float3(0,1,1)), hash(p + float3(1,1,1)), f.x), f.y), f.z);
            }
            
            float fbm(float3 x)
            {
                float v = 0.0;
                float a = 0.5;
                float3 shift = float3(100, 100, 100);
                
                for (int i = 0; i < 6; ++i)
                {
                    v += a * noise(x);
                    x = x * 2.0 + shift;
                    a *= 0.5;
                }
                return v;
            }
            
            // Sphere-optimized UV coordinates
            float2 getSphereOptimizedUV(float2 uv, float3 positionOS)
            {
                float2 tiledUV = uv * _TileScale + _TileOffset.xy;
                float sphereFactor = 1.0 - abs(positionOS.y) * _SphereCompensation * 0.5;
                tiledUV.x *= sphereFactor;
                tiledUV = frac(tiledUV);
                return tiledUV;
            }
            
            // Organic liquid motion simulation
            float3 getOrganicMotion(float3 pos, float time, float2 uv)
            {
                float3 motion = float3(0, 0, 0);
                
                // Gentle vertical circulation like in algae pools
                motion.y += sin(time * _FlowSpeed + uv.x * 3.14159) * _VerticalFlow * _MovementAmplitude * 0.3;
                
                // Organic horizontal swirling
                float swirl = time * _SwirlingMotion + uv.y * 3.14159;
                motion.x += sin(swirl) * _SwirlingMotion * _MovementAmplitude * 0.2;
                motion.z += cos(swirl) * _SwirlingMotion * _MovementAmplitude * 0.2;
                
                // Soft turbulence for organic feel
                float3 turbulence = float3(
                    sin(time * 1.5 + pos.y * 3.0) * _TurbulenceStrength,
                    cos(time * 1.2 + pos.x * 2.5) * _TurbulenceStrength * 0.6,
                    sin(time * 1.0 + pos.z * 2.0) * _TurbulenceStrength
                ) * _MovementAmplitude * 0.15;
                
                return motion + turbulence;
            }
            
            // Micro light particles (like suspended matter)
            float generateMicroParticles(float3 pos, float time, float2 uv)
            {
                float3 animatedPos = pos + getOrganicMotion(pos, time, uv);
                
                // Slow floating motion
                animatedPos.y += time * _WhiteParticleSpeed * 0.2;
                animatedPos.x += sin(time * _WhiteParticleSpeed * 0.3 + uv.y * 6.28) * _MovementAmplitude * 0.1;
                animatedPos.z += cos(time * _WhiteParticleSpeed * 0.25 + uv.x * 6.28) * _MovementAmplitude * 0.08;
                
                // Gentle drift
                float driftAngle = time * _SwirlingMotion * 0.3 + pos.y * 2.0;
                animatedPos.x += sin(driftAngle) * _MovementAmplitude * 0.05;
                animatedPos.z += cos(driftAngle) * _MovementAmplitude * 0.05;
                
                float3 particlePos = animatedPos * _WhiteParticleDensity;
                
                // Very fine particles with multiple scales
                float particles = 0.0;
                particles += smoothstep(0.6, 1.0, fbm(particlePos)) * 1.0;
                particles += smoothstep(0.7, 1.0, fbm(particlePos * 1.8)) * 0.8;
                particles += smoothstep(0.75, 1.0, fbm(particlePos * 2.5)) * 0.6;
                particles += smoothstep(0.8, 1.0, fbm(particlePos * 4.0)) * 0.4;
                particles += smoothstep(0.85, 1.0, fbm(particlePos * 6.0)) * 0.3;
                
                // Very small particle size
                particles *= smoothstep(_WhiteParticleSize * 8.0, _WhiteParticleSize * 0.1, 
                                      length(frac(particlePos) - 0.5));
                
                return particles * _WhiteParticleIntensity;
            }
            
            // Algae particles (green organic matter)
            float generateAlgaeParticles(float3 pos, float time, float2 uv)
            {
                float3 animatedPos = pos + getOrganicMotion(pos, time * 0.6, uv);
                
                // Slower, more organic movement
                animatedPos.y += time * _BlueParticleSpeed * 0.15;
                animatedPos.x += cos(time * _BlueParticleSpeed * 0.4 + uv.x * 6.28) * _MovementAmplitude * 0.12;
                animatedPos.z += sin(time * _BlueParticleSpeed * 0.3 + uv.y * 6.28) * _MovementAmplitude * 0.1;
                
                // Organic clustering motion
                float clusterAngle = -time * _SwirlingMotion * 0.2 + pos.x * 1.5;
                animatedPos.x += sin(clusterAngle) * _MovementAmplitude * 0.06;
                animatedPos.z += cos(clusterAngle) * _MovementAmplitude * 0.06;
                
                float3 particlePos = animatedPos * _BlueParticleDensity;
                
                // Dense algae particles
                float particles = 0.0;
                particles += smoothstep(0.65, 1.0, fbm(particlePos)) * 1.0;
                particles += smoothstep(0.7, 1.0, fbm(particlePos * 1.5)) * 0.9;
                particles += smoothstep(0.75, 1.0, fbm(particlePos * 2.2)) * 0.7;
                particles += smoothstep(0.8, 1.0, fbm(particlePos * 3.5)) * 0.5;
                particles += smoothstep(0.85, 1.0, fbm(particlePos * 5.0)) * 0.4;
                particles += smoothstep(0.9, 1.0, fbm(particlePos * 7.0)) * 0.3;
                
                // Small algae size
                particles *= smoothstep(_BlueParticleSize * 10.0, _BlueParticleSize * 0.1, 
                                      length(frac(particlePos) - 0.5));
                
                return particles * _BlueParticleIntensity;
            }
            
            // Soft rim lighting for organic feel
            float3 calculateSoftRim(float3 viewDir, float3 normal, float time)
            {
                float rim = 1.0 - saturate(dot(viewDir, normal));
                rim = pow(rim, _RimPower);
                
                // Gentle pulsing
                float pulseRim = sin(time * 2.0) * 0.2 + 0.8;
                rim *= pulseRim;
                
                return _RimColor.rgb * rim * _RimIntensity;
            }
            
            // Subtle normal perturbation for organic surface
            float3 perturbNormalSoft(float3 normal, float3 pos, float time, float2 uv)
            {
                float3 motion = getOrganicMotion(pos, time, uv);
                
                // Gentle surface disturbance
                float3 perturbation = float3(
                    sin(time * 1.5 + pos.x * 4.0) * 0.1,
                    cos(time * 1.2 + pos.z * 3.5) * 0.08,
                    sin(time * 1.8 + pos.y * 4.5) * 0.1
                ) * _NormalStrength * 0.1;
                
                return normalize(normal + perturbation);
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionHCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
                output.shadowCoord = GetShadowCoord(vertexInput);
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                output.positionOS = input.positionOS.xyz;
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float time = _Time.y;
                
                // Get sphere-optimized UV coordinates
                float2 sphereUV = getSphereOptimizedUV(input.uv, input.positionOS);
                
                // Sample base texture
                half4 baseTexture = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sphereUV);
                
                // Base algae pool color with variation
                float depthVar = sin(time * 0.5 + input.positionOS.y * 2.0) * 0.1 + 0.9;
                float3 baseColor = lerp(_PotionColor.rgb, _AlgaeColor.rgb, depthVar * 0.3);
                float3 poolColor = baseColor * _PotionIntensity * _LiquidDensity;
                
                // Generate dense particles
                float microParticles = generateMicroParticles(input.positionWS, time, sphereUV);
                float algaeParticles = generateAlgaeParticles(input.positionWS, time, sphereUV);
                
                // Particle color contributions
                float3 microContribution = _WhiteParticleColor.rgb * microParticles;
                float3 algaeContribution = _BlueParticleColor.rgb * algaeParticles;
                
                // Combine pool base with particles
                float3 finalColor = poolColor + microContribution + algaeContribution;
                
                // Calculate soft surface normal
                float3 perturbedNormal = perturbNormalSoft(input.normalWS, input.positionWS, time, sphereUV);
                
                // Calculate soft rim lighting
                float3 viewDir = normalize(input.viewDirWS);
                float3 rimLighting = calculateSoftRim(viewDir, perturbedNormal, time);
                
                // Soft emission effects
                float3 emission = rimLighting;
                emission += microContribution * _EmissionIntensity * 0.3;
                emission += algaeContribution * _EmissionIntensity * 0.2;
                
                finalColor += emission;
                
                // Apply base texture
                finalColor *= baseTexture.rgb;
                
                // Calculate lighting
                Light mainLight = GetMainLight(input.shadowCoord);
                float3 lighting = LightingLambert(mainLight.color, mainLight.direction, perturbedNormal);
                finalColor *= lighting;
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogCoord);
                
                // Calculate final alpha with particle density
                float particleDensity = (microParticles + algaeParticles) * 0.1;
                float finalAlpha = _Transparency + particleDensity;
                finalAlpha = saturate(finalAlpha);
                
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }
    
    FallBack "Universal Render Pipeline/Transparent"
}
