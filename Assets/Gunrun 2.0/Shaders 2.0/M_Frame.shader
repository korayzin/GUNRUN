Shader "Custom/ShatafatFrame"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _GlowColor ("Glow Color", Color) = (0, 1, 1, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 2.0
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 3.0
        _HueSpeed ("Color Cycle Speed", Range(0, 5)) = 1.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _GlowColor;
            float _GlowIntensity;
            float _PulseSpeed;
            float _HueSpeed;

            // Simple Hue Rotation Function
            float3 shift_hue(float3 col, float Shift)
            {
                float3 P = float3(0.55735, 0.55735, 0.55735) * dot(float3(0.55735, 0.55735, 0.55735), col);
                float3 U = col - P;
                float3 V = cross(float3(0.55735, 0.55735, 0.55735), U);    
                col = U * cos(Shift * 6.2832) + V * sin(Shift * 6.2832) + P;
                return col;
            }

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, IN.texcoord);
                
                // 1. Nabız Etkisi (Pulse)
                float pulse = (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5);
                
                // 2. Dinamik Renk Geçişi (Hue Shift)
                float3 shiftedGlow = shift_hue(_GlowColor.rgb, _Time.y * _HueSpeed * 0.1);
                
                // 3. Parlama Hesaplama
                // Texture'ın alpha kanalını kullanarak sadece dolu kısımları parlatıyoruz
                float3 glowEffect = shiftedGlow * _GlowIntensity * pulse * tex.a;
                
                // 4. Sonuç birleştirme
                fixed4 finalColor = tex;
                finalColor.rgb += glowEffect * (1.0 - tex.rgb * 0.5); // Orijinal dokuyu çok ezmeden üzerine ekle
                finalColor.rgb *= IN.color.rgb;
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}