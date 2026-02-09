Shader "Custom/StylizedFireball"
{
    Properties
    {
        [MainTexture] _MainTex("Main Texture", 2D) = "white" {}
        _NoiseTex("Noise Texture", 2D) = "white" {}
        [HDR] _BaseColor("Base Color", Color) = (1, .5, 0, 1)
        [HDR] _EmissionColor("Emission Color", Color) = (2, 1, 0, 1)
        _Speed("Scroll Speed", Vector) = (0.5, 0.2, 0, 0)
        _DistortionStrength("Distortion Strength", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZWrite Off
        Blend One One // Additive blending for fire effect

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float4 _MainTex_ST;
            float4 _BaseColor;
            float4 _EmissionColor;
            float4 _Speed;
            float _DistortionStrength;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                // Zaman bazlı kaydırma (panning)
                float2 uvScroll = IN.uv + _Speed.xy * _Time.y;
                
                // Noise ile distorsiyon (bozulma) oluşturma
                float noise = tex2D(_NoiseTex, uvScroll).r;
                float2 distortedUV = IN.uv + (noise * _DistortionStrength);
                
                // Ana doku okuma
                half4 mainTex = tex2D(_MainTex, distortedUV);
                
                // Renk ve Parlama birleştirme
                half3 finalColor = mainTex.rgb * _BaseColor.rgb;
                finalColor += mainTex.rgb * _EmissionColor.rgb * noise;

                return half4(finalColor, mainTex.a);
            }
            ENDHLSL
        }
    }
}