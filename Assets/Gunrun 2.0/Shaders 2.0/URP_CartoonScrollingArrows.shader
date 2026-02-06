Shader "Custom/URP_CartoonCurveArrows"
{
    Properties
    {
        [Header(Color Settings)]
        _MainColor ("Arrow Color", Color) = (0, 1, 0.5, 1)
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _BackColor ("Background Color", Color) = (0, 0, 0, 0)
        
        [Header(Movement)]
        _ScrollSpeed ("Scroll Speed", Float) = 1.5
        _ArrowDensity ("Arrow Density", Float) = 4.0
        
        [Header(Curve Settings)]
        _CurveStrength ("Curve Strength", Range(0, 0.5)) = 0.1
        _CurveFrequency ("Curve Frequency", Range(1, 10)) = 3.0

        [Header(Cartoon Shape)]
        _ArrowThickness ("Inner Thickness", Range(0.01, 0.2)) = 0.05
        _OutlineThickness ("Outline Width", Range(0.01, 0.2)) = 0.03
        _Sharpness ("Edge Sharpness", Range(0, 0.05)) = 0.005
        
        [Header(Effects)]
        _FadeRange ("Edge Fade Range", Range(0.01, 0.5)) = 0.15
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _MainColor;
                float4 _OutlineColor;
                float4 _BackColor;
                float _ScrollSpeed;
                float _ArrowDensity;
                float _CurveStrength;
                float _CurveFrequency;
                float _ArrowThickness;
                float _OutlineThickness;
                float _Sharpness;
                float _FadeRange;
            CBUFFER_END

            Varyings vert (Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.uv;
                float rawX = uv.x; // Orijinal X pozisyonunu sakla

                // --- CURVE EFEKTİ ---
                // rawX'e bağlı bir sinüs dalgası oluştur ve bunu Y eksenine ekle.
                // Bu, okların yolunu büker.
                float curveOffset = sin(rawX * _CurveFrequency) * _CurveStrength;
                uv.y += curveOffset;

                // --- AKIŞ ---
                float scroll = _Time.y * _ScrollSpeed;
                uv.x = frac((uv.x - scroll) * _ArrowDensity);

                // --- ŞEKİL ---
                float x_pos = uv.x - 0.5; 
                // Y mesafesini artık bükülmüş yeni uv.y üzerinden hesaplıyoruz
                float y_dist = abs(uv.y - 0.5);
                
                float arrow_sdf = abs(x_pos + y_dist);
                
                // 1. Dış hat (Outline)
                float total_thickness = _ArrowThickness + _OutlineThickness;
                float outline_mask = smoothstep(total_thickness + _Sharpness, total_thickness, arrow_sdf);
                
                // 2. İç dolgu
                float inner_mask = smoothstep(_ArrowThickness + _Sharpness, _ArrowThickness, arrow_sdf);
                
                // 3. Kesilme koruması (Curve arttıkça bu limiti biraz açmak gerekebilir)
                // Yolu takip etmeleri için limiti biraz yumuşattık.
                float y_limit = smoothstep(0.6, 0.4, y_dist); 
                outline_mask *= y_limit;
                inner_mask *= y_limit;

                // 4. Kenar Fade
                float edge_fade = smoothstep(0, _FadeRange, rawX) * smoothstep(1, 1 - _FadeRange, rawX);

                // Renk Birleştirme
                float4 finalColor = lerp(_BackColor, _OutlineColor, outline_mask);
                finalColor = lerp(finalColor, _MainColor, inner_mask);
                
                finalColor.a *= (outline_mask * edge_fade);

                return finalColor;
            }
            ENDHLSL
        }
    }
}