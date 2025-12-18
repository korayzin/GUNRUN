Shader "Custom/SlimePool"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0, 1, 0.4, 1)
        _EmissionIntensity ("Emission Intensity", Float) = 2
        _NoiseScale ("Noise Scale", Float) = 5
        _DistortStrength ("Distort Strength", Float) = 0.05
        _SecondaryNoiseScale ("Secondary Noise Scale", Float) = 20
        _RippleStrength ("Ripple Strength", Float) = 0.03
        _FresnelPower ("Fresnel Power", Float) = 5
        _NormalStrength ("Normal Strength", Float) = 1
        _SpecularColor ("Specular Color", Color) = (1,1,1,1)
        _SpecularStrength ("Specular Strength", Float) = 1
        _ReflectionColor ("Reflection Color", Color) = (0.2, 0.8, 0.2, 1)
        _MainTex ("Main Texture", 2D) = "white" {}
        
        // Kenar efektleri için yeni parametreler
        _EdgeWidth ("Edge Width", Range(0, 1)) = 0.3
        _EdgeFalloff ("Edge Falloff", Range(0.1, 2)) = 0.8
        _EdgeDistortion ("Edge Distortion", Range(0, 0.5)) = 0.15
        _CenterFalloff ("Center Falloff", Range(0, 1)) = 0.7
        _EdgeThickness ("Edge Thickness Multiplier", Range(0.1, 3)) = 1.5
        _OrganicScale ("Organic Edge Scale", Range(1, 50)) = 15
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _BaseColor;
            float _EmissionIntensity;
            float _NoiseScale;
            float _DistortStrength;
            float _SecondaryNoiseScale;
            float _RippleStrength;
            float _FresnelPower;
            float _NormalStrength;
            float4 _SpecularColor;
            float _SpecularStrength;
            float4 _ReflectionColor;
            
            float _EdgeWidth;
            float _EdgeFalloff;
            float _EdgeDistortion;
            float _CenterFalloff;
            float _EdgeThickness;
            float _OrganicScale;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float edgeDistance : TEXCOORD2;
            };

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float a = hash(i);
                float b = hash(i + float2(1,0));
                float c = hash(i + float2(0,1));
                float d = hash(i + float2(1,1));
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(a, b, u.x) + (c - a)* u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
            }

            // Merkezden kenar mesafesini hesapla
            float calculateEdgeDistance(float2 uv)
            {
                float2 center = float2(0.5, 0.5);
                float2 toEdge = abs(uv - center);
                float maxDist = max(toEdge.x, toEdge.y);
                return maxDist * 2.0; // 0-1 aralığında normalize et
            }

            v2f vert (appdata v)
            {
                v2f o;
                float t = _Time.y;
                
                // Kenar mesafesini hesapla
                float edgeDist = calculateEdgeDistance(v.uv);
                o.edgeDistance = edgeDist;
                
                // Temel noise
                float2 nUV = v.uv * _NoiseScale;
                float n = noise(nUV + t * 0.2);
                
                // Organik kenar noise'ı
                float2 orgUV = v.uv * _OrganicScale;
                float organicNoise = noise(orgUV + t * 0.1);
                
                // Kenar deformasyonu - kenarlarda daha fazla
                float edgeIntensity = smoothstep(1.0 - _EdgeWidth, 1.0, edgeDist);
                float edgeDeformation = edgeIntensity * _EdgeDistortion * organicNoise;
                
                float yOffset = n * _DistortStrength;
                float3 pos = v.vertex.xyz;
                
                // Kenar deformasyonu ekle
                pos.y += yOffset + edgeDeformation;
                
                // Kenarları içe doğru deforme et (balçık yayılımı için)
                float2 centerOffset = (v.uv - 0.5) * edgeIntensity * _EdgeThickness * organicNoise * 0.1;
                pos.xz += centerOffset;
                
                // Secondary ripple displacement for bubbling effect
                float2 nUV2 = v.uv * _SecondaryNoiseScale;
                float n2 = noise(nUV2 + t * 2);
                float ripple = sin((v.uv.x + v.uv.y + t) * 10) * _RippleStrength * n2;
                pos.y += ripple * (1.0 - edgeIntensity * 0.5); // Kenarlarda ripple azalt
                
                o.vertex = UnityObjectToClipPos(float4(pos,1));
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, float4(pos,1)).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;
                float edgeDist = i.edgeDistance;
                
                // Texture sampling
                fixed4 texCol = tex2D(_MainTex, uv);
                float t = _Time.y;
                float n = noise(uv * _NoiseScale + t * 0.5);
                
                // Organik kenar noise'ı
                float organicNoise = noise(uv * _OrganicScale + t * 0.1);
                
                // Kenar yoğunluğu hesapla
                float edgeIntensity = smoothstep(1.0 - _EdgeWidth, 1.0, edgeDist);
                
                // Alpha hesaplama - kenarlardan merkeze doğru yumuşak geçiş
                float baseAlpha = saturate(n * 0.7 + 0.3);
                
                // Kenar alpha modifikasyonu
                float edgeAlpha = 1.0 - pow(edgeIntensity, _EdgeFalloff);
                edgeAlpha *= (1.0 + organicNoise * 0.5); // Organik kenar variasyonu
                edgeAlpha = saturate(edgeAlpha);
                
                // Merkez falloff
                float centerDist = length(uv - 0.5) * 2.0;
                float centerAlpha = 1.0 - smoothstep(_CenterFalloff, 1.0, centerDist);
                
                float alpha = baseAlpha * edgeAlpha * centerAlpha;
                
                fixed4 col = _BaseColor;
                col.a = alpha;
                
                // Combine main texture color detail
                col.rgb *= texCol.rgb;

                // Emission - kenarlarда daha az
                float emissive = n * _EmissionIntensity * (1.0 - edgeIntensity * 0.3);

                // Bubble highlight – brighter spots where noise is high
                float bubbleHighlight = step(0.6, n) * (1.0 - edgeIntensity * 0.5);
                emissive += bubbleHighlight * 0.4;

                // Fresnel edge highlight for wet shine
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float fresnel = pow(1 - saturate(dot(viewDir, float3(0,1,0))), _FresnelPower);
                emissive += fresnel * 0.2 * (1.0 + edgeIntensity); // Kenarlarda biraz daha parlak

                col.rgb += emissive;

                // ---- Specular & Reflection ----
                // Approximate surface normal from noise gradient
                float nRight = noise((uv + float2(0.01,0)) * _NoiseScale + t * 0.5);
                float nUp = noise((uv + float2(0,0.01)) * _NoiseScale + t * 0.5);
                float2 grad = float2(nRight - n, nUp - n);
                float3 fakeNormal = normalize(float3(-grad.x * _NormalStrength, 1, -grad.y * _NormalStrength));

                float3 halfDir = normalize(viewDir + float3(0,1,0)); // light from above
                float spec = pow(saturate(dot(fakeNormal, halfDir)), 16) * _SpecularStrength;
                spec *= (1.0 - edgeIntensity * 0.4); // Kenarlarda specular azalt
                col.rgb = lerp(col.rgb, col.rgb + _SpecularColor.rgb * spec, spec);

                // Simple sci-fi reflection tint via fresnel
                float fres = pow(1 - saturate(dot(viewDir, fakeNormal)), _FresnelPower);
                col.rgb += _ReflectionColor.rgb * fres * 0.3 * (1.0 - edgeIntensity * 0.2);

                // Kenar renk efekti - biraz daha koyu/farklı ton
                col.rgb = lerp(col.rgb, col.rgb * 0.8, edgeIntensity * 0.3);

                return col;
            }
            ENDCG
        }
    }
}
