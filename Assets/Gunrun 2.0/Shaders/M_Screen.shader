Shader "Custom/M_Screen"
{
    Properties
    {
        _BaseMap ("Base Map (Texture)", 2D) = "white" {}
        _Tint ("Tint Color", Color) = (1,1,1,1)
        _Speed ("Animation Speed", Range(0.1, 5.0)) = 1.2
        _BandIntensity ("Band Intensity", Range(0.0, 2.0)) = 1.0
        _BandCount ("Band Count", Range(2, 16)) = 6
        _NoiseIntensity ("Noise Intensity", Range(0.0, 1.0)) = 0.5
        _ScanlineIntensity ("Scanline Intensity", Range(0.0, 1.0)) = 0.5
        _PixelGridIntensity ("Pixel Grid Intensity", Range(0.0, 1.0)) = 0.3
        _PixelSize ("Pixel Size", Range(0.5, 5.0)) = 1.5
        _RGBSplit ("RGB Split", Range(0.0, 0.01)) = 0.003
        _Brightness ("Brightness", Range(0.0, 2.0)) = 1.0
        _Contrast ("Contrast", Range(0.5, 2.0)) = 1.1
        _Emission ("Emission", Range(0.0, 5.0)) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 200
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma target 4.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _STEREO_INSTANCING_ON
            #pragma multi_compile _ _STEREO_MULTIVIEW_ON
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            CBUFFER_START(UnityPerMaterial)
                float4 _Tint;
                float _Speed;
                float _BandIntensity;
                float _BandCount;
                float _NoiseIntensity;
                float _ScanlineIntensity;
                float _PixelGridIntensity;
                float _PixelSize;
                float _RGBSplit;
                float _Brightness;
                float _Contrast;
                float _Emission;
            CBUFFER_END
            float hash12(float2 p) {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 19.19);
                return frac((p3.x + p3.y) * p3.z);
            }
            float3 getBands(float2 uv, float t) {
                float y = uv.y;
                float band = 0.0;
                float3 color = 0;
                for (int i = 0; i < 8; i++) {
                    if (i >= int(_BandCount)) break;
                    float phase = t * _Speed * (0.7 + 0.2 * i) + i * 1.7;
                    float pos = sin(y * (2.5 + i) * 3.1415 + phase) * 0.5 + 0.5;
                    float width = 0.25 + 0.12 * sin(phase + i);
                    float mask = smoothstep(width, width - 0.08, abs(pos - 0.5));
                    float3 bandColor = 0.5 + 0.5 * float3(sin(phase + i), sin(phase + i * 1.3 + 2.0), sin(phase + i * 1.7 + 4.0));
                    color += bandColor * mask;
                }
                return color * _BandIntensity;
            }
            float getStatic(float2 uv, float t) {
                float n = hash12(uv * 320.0 + t * 60.0);
                n = lerp(n, hash12(uv * 640.0 + t * 120.0), 0.5);
                return n;
            }
            float getScanline(float2 uv, float t) {
                float scan = sin(uv.y * 800.0 + t * 8.0) * 0.5 + 0.5;
                return lerp(1.0, scan, _ScanlineIntensity);
            }
            float getPixelGrid(float2 uv) {
                float2 pixelUV = uv * _PixelSize * 100.0;
                float2 grid = abs(frac(pixelUV) - 0.5) / fwidth(pixelUV);
                float gridLine = 1.0 - min(grid.x, grid.y);
                gridLine = smoothstep(0.0, 1.0, gridLine);
                return lerp(1.0, 1.0 - gridLine * 0.5, _PixelGridIntensity);
            }
            float3 getRGBSplit(float2 uv, float t, float3 baseColor) {
                float offset = sin(t * 2.0 + uv.y * 10.0) * _RGBSplit;
                float3 c;
                c.r = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv + float2(offset, 0)).r;
                c.g = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).g;
                c.b = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv - float2(offset, 0)).b;
                // Tint and bands overlay
                float3 bands = getBands(uv, t);
                c = lerp(c, c * bands, 0.5);
                return c;
            }
            float3 adjust(float3 c) {
                c = (c - 0.5) * _Contrast + 0.5;
                c *= _Brightness;
                return c;
            }
            Varyings vert(Attributes input) {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                o.uv = input.uv;
                return o;
            }
            half4 frag(Varyings i) : SV_Target {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                
                float t = _Time.y;
                float2 uv = i.uv;
                // Sample BaseMap and apply tint
                float3 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv).rgb * _Tint.rgb;
                // RGB split and bands on BaseMap
                float3 color = getRGBSplit(uv, t, baseColor);
                // Add static
                float staticVal = getStatic(uv, t);
                color = lerp(color, float3(staticVal, staticVal, staticVal), _NoiseIntensity);
                // Scanlines
                color *= getScanline(uv, t);
                // Pixel grid
                color *= getPixelGrid(uv);
                // Final adjust
                color = adjust(color);
                // Emission
                color *= _Emission;
                return half4(color, 1.0);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Unlit"
}
