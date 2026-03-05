Shader "Custom/WeaponDissolve"
{
    Properties
    {
        [MainTexture] _MainTex("Main Texture", 2D) = "white" {}
        _Color("Color", Color) = (1,1,1,1)
        _DissolveProgress("Dissolve Progress", Range(0,1)) = 1
        _HeightMin("Height Min (Object Y)", Float) = -1
        _HeightMax("Height Max (Object Y)", Float) = 1
        [Toggle] _BuildMode("Build Mode (bottom to top)", Float) = 1
        [HDR] _EdgeColor("Edge Glow Color", Color) = (1, 0.9, 0.6, 1)
        _EdgeWidth("Edge Glow Width", Range(0, 0.2)) = 0.05
        _EdgePower("Edge Glow Power", Range(1, 8)) = 3
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ _STEREO_INSTANCING_ON
            #pragma multi_compile _ _STEREO_MULTIVIEW_ON
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
                float positionOSY : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _EdgeColor;
                float _DissolveProgress;
                float _HeightMin;
                float _HeightMax;
                float _BuildMode;
                float _EdgeWidth;
                float _EdgePower;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.positionOSY = IN.positionOS.y;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);
                float normY = (_HeightMax - _HeightMin) > 0.001
                    ? saturate((IN.positionOSY - _HeightMin) / (_HeightMax - _HeightMin))
                    : 1;
                float edgePos = _BuildMode > 0.5 ? _DissolveProgress : (1 - _DissolveProgress);
                float distFromEdge = abs(normY - edgePos);
                float visible = 0;
                if (_BuildMode > 0.5)
                    visible = step(normY, _DissolveProgress);
                else
                    visible = step(1 - _DissolveProgress, normY);
                clip(visible - 0.5);
                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;
                float edgeGlow = 1 - saturate(distFromEdge / _EdgeWidth);
                edgeGlow = pow(edgeGlow, _EdgePower);
                col.rgb = lerp(col.rgb, _EdgeColor.rgb, edgeGlow * _EdgeColor.a);
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Lit"
}
