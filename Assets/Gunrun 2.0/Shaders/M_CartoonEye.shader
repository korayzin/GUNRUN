Shader "Custom/M_CartoonEye"
{
    Properties
    {
        [Header(Eye Textures)]
        _MainTexture ("Main Eye Texture", 2D) = "white" {}
        _EyeNormal ("Eye Normal Map", 2D) = "bump" {}
        
        [Header(Cartoon Eye Shape)]
        _EyeRadius ("Eye Radius", Range(0.2, 0.8)) = 0.45
        _PupilSize ("Pupil Size", Range(0.1, 0.4)) = 0.25
        _PupilOffset ("Pupil Position Offset", Vector) = (0, 0.05, 0, 0)
        _EyeShape ("Eye Shape Factor", Range(0.5, 2.0)) = 1.2
        _EyeHeight ("Eye Height Stretch", Range(0.8, 1.5)) = 1.1
        _CornerSharpness ("Eye Corner Sharpness", Range(0.1, 2.0)) = 0.8
        
        [Header(Cartoon Colors)]
        _EyeColor ("Main Eye Color", Color) = (0.2, 0.6, 1.0, 1)
        _PupilColor ("Pupil Color", Color) = (0.05, 0.05, 0.1, 1)
        _EyeballColor ("Eyeball Base Color", Color) = (1, 1, 1, 1)
        _OutlineColor ("Eye Outline Color", Color) = (0.1, 0.1, 0.2, 1)
        
        [Header(Cartoon Highlights)]
        _MainHighlightSize ("Main Highlight Size", Range(0.05, 0.3)) = 0.15
        _MainHighlightOffset ("Main Highlight Offset", Vector) = (-0.1, 0.1, 0, 0)
        _MainHighlightIntensity ("Main Highlight Intensity", Range(0, 10)) = 8.0
        _SecondHighlightSize ("Second Highlight Size", Range(0.02, 0.15)) = 0.08
        _SecondHighlightOffset ("Second Highlight Offset", Vector) = (0.08, -0.05, 0, 0)
        _SecondHighlightIntensity ("Second Highlight Intensity", Range(0, 5)) = 3.0
        _HighlightSharpness ("Highlight Sharpness", Range(1, 20)) = 8
        
        [Header(Stylized Effects)]
        _ColorSaturation ("Color Saturation", Range(0.5, 2.5)) = 1.8
        _ColorContrast ("Color Contrast", Range(0.5, 2.0)) = 1.4
        _Brightness ("Overall Brightness", Range(0.8, 2.5)) = 1.6
        _OutlineWidth ("Outline Width", Range(0.001, 0.02)) = 0.005
        _ShineBoost ("Overall Shine Boost", Range(1, 4)) = 2.5
        
        [Header(Animation Properties)]
        _BlinkSpeed ("Blink Animation Speed", Range(0, 5)) = 2.0
        _EyeMovement ("Eye Movement Strength", Range(0, 0.1)) = 0.02
        _PulseSpeed ("Pulse Animation Speed", Range(0, 3)) = 1.5
        _BouncyMovement ("Bouncy Movement", Range(0, 1)) = 0.4
        
        [Header(Cartoon Lighting)]
        _ToonRamp ("Toon Ramp Steps", Range(2, 8)) = 4
        _AmbientStrength ("Ambient Light", Range(0.2, 1)) = 0.6
        _DirectionalStrength ("Directional Light", Range(0.5, 2)) = 1.2
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
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
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
                float fogCoord : TEXCOORD4;
            };
            
            TEXTURE2D(_MainTexture);
            SAMPLER(sampler_MainTexture);
            TEXTURE2D(_EyeNormal);
            SAMPLER(sampler_EyeNormal);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTexture_ST;
                half _EyeRadius;
                half _PupilSize;
                half4 _PupilOffset;
                half _EyeShape;
                half _EyeHeight;
                half _CornerSharpness;
                half4 _EyeColor;
                half4 _PupilColor;
                half4 _EyeballColor;
                half4 _OutlineColor;
                half _MainHighlightSize;
                half4 _MainHighlightOffset;
                half _MainHighlightIntensity;
                half _SecondHighlightSize;
                half4 _SecondHighlightOffset;
                half _SecondHighlightIntensity;
                half _HighlightSharpness;
                half _ColorSaturation;
                half _ColorContrast;
                half _Brightness;
                half _OutlineWidth;
                half _ShineBoost;
                half _BlinkSpeed;
                half _EyeMovement;
                half _PulseSpeed;
                half _BouncyMovement;
                half _ToonRamp;
                half _AmbientStrength;
                half _DirectionalStrength;
            CBUFFER_END
            
            // Cartoon eye shape function
            half CartoonEyeMask(float2 uv, float2 center, half radius, half shape, half height)
            {
                float2 toCenter = uv - center;
                toCenter.y /= height; // Stretch vertically
                
                half dist = length(toCenter);
                half eyeMask = smoothstep(radius + 0.02, radius - 0.02, dist);
                
                // Add cartoon eye shape with corners
                half angle = atan2(toCenter.y, toCenter.x);
                half cornerEffect = 1.0 + sin(angle * 2.0) * _CornerSharpness * 0.1;
                eyeMask *= cornerEffect;
                
                return saturate(eyeMask);
            }
            
            // Cartoon pupil with perfect circle
            half CartoonPupilMask(float2 uv, float2 center, half radius)
            {
                half dist = distance(uv, center);
                return smoothstep(radius + 0.005, radius - 0.005, dist);
            }
            
            // Cartoon highlight function
            half CartoonHighlight(float2 uv, float2 center, half size, half sharpness)
            {
                half dist = distance(uv, center);
                half highlight = 1.0 - smoothstep(0.0, size, dist);
                return pow(highlight, sharpness);
            }
            
            // Bouncy animation
            float2 BouncyMovement(float2 uv, float time)
            {
                float bounce = sin(time * _PulseSpeed) * _BouncyMovement * 0.01;
                float2 center = float2(0.5, 0.5);
                float2 toCenter = uv - center;
                return uv + toCenter * bounce;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                
                float time = _Time.y;
                float3 positionOS = input.positionOS.xyz;
                
                // Simple bouncy movement
                float bounceX = sin(time * _BlinkSpeed * 1.3) * _EyeMovement;
                float bounceY = cos(time * _BlinkSpeed * 0.8) * _EyeMovement * 0.7;
                float bounceZ = sin(time * _BlinkSpeed * 2.1) * _EyeMovement * 0.3;
                
                positionOS += float3(bounceX, bounceY, bounceZ);
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(positionOS);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                
                output.positionHCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTexture);
                output.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y;
                
                // Apply bouncy movement to UV
                float2 animatedUV = BouncyMovement(input.uv, time);
                float2 centerUV = float2(0.5, 0.5);
                float2 pupilCenter = centerUV + _PupilOffset.xy;
                
                // Create cartoon eye masks
                half eyeMask = CartoonEyeMask(animatedUV, centerUV, _EyeRadius, _EyeShape, _EyeHeight);
                half pupilMask = CartoonPupilMask(animatedUV, pupilCenter, _PupilSize);
                pupilMask *= eyeMask; // Keep pupil inside eye
                
                // Base colors with enhanced saturation
                half4 eyeColor = lerp(_EyeballColor, _EyeColor, eyeMask);
                eyeColor = lerp(eyeColor, _PupilColor, pupilMask);
                
                // Enhance color saturation and contrast for cartoon look
                eyeColor.rgb = pow(eyeColor.rgb, _ColorContrast);
                half3 gray = dot(eyeColor.rgb, half3(0.299, 0.587, 0.114));
                eyeColor.rgb = lerp(gray, eyeColor.rgb, _ColorSaturation);
                
                // Simple lighting
                half3 lightDir = normalize(float3(1, 1, 1));
                half3 normalWS = normalize(input.normalWS);
                float NdotL = saturate(dot(normalWS, lightDir)) * 0.5 + 0.5;
                
                // Toon shading
                half toonFactor = floor(NdotL * _ToonRamp) / _ToonRamp;
                half3 toonDiffuse = half3(1, 1, 1) * toonFactor * _DirectionalStrength;
                half3 ambient = half3(0.4, 0.4, 0.5) * _AmbientStrength;
                
                // Cartoon highlights - main big highlight
                float2 mainHighlightCenter = pupilCenter + _MainHighlightOffset.xy;
                half mainHighlight = CartoonHighlight(animatedUV, mainHighlightCenter, _MainHighlightSize, _HighlightSharpness);
                mainHighlight *= eyeMask; // Keep inside eye
                half3 mainHighlightColor = half3(1, 1, 1) * mainHighlight * _MainHighlightIntensity;
                
                // Secondary smaller highlight
                float2 secondHighlightCenter = pupilCenter + _SecondHighlightOffset.xy;
                half secondHighlight = CartoonHighlight(animatedUV, secondHighlightCenter, _SecondHighlightSize, _HighlightSharpness * 1.5);
                secondHighlight *= eyeMask;
                half3 secondHighlightColor = half3(1, 1, 1) * secondHighlight * _SecondHighlightIntensity;
                
                // Combine lighting
                half3 finalLighting = toonDiffuse + ambient;
                eyeColor.rgb *= finalLighting;
                
                // Add highlights
                eyeColor.rgb += mainHighlightColor;
                eyeColor.rgb += secondHighlightColor;
                
                // Overall brightness and shine boost
                eyeColor.rgb *= _Brightness * _ShineBoost;
                
                // Eye outline effect
                half outlineMask = smoothstep(_EyeRadius - _OutlineWidth, _EyeRadius + _OutlineWidth, distance(animatedUV, centerUV));
                eyeColor.rgb = lerp(eyeColor.rgb, _OutlineColor.rgb, outlineMask * (1.0 - eyeMask));
                
                // Apply fog
                eyeColor.rgb = MixFog(eyeColor.rgb, input.fogCoord);
                
                return eyeColor;
            }
            ENDHLSL
        }
    }
    
    Fallback "Universal Render Pipeline/Lit"
} 