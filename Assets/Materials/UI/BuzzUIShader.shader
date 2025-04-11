
Shader "PDXUnderground/BuzzUIShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (0.827, 0.722, 0.416, 1)
        _EmissionColor ("Emission Color", Color) = (0.827, 0.722, 0.416, 1)
        _EmissionIntensity ("Emission Intensity", Range(0, 2)) = 0.5
        _GrainTex ("Grain Texture", 2D) = "white" {}
        _GrainIntensity ("Grain Intensity", Range(0, 1)) = 0.2
        _EdgeWear ("Edge Wear", Range(0, 1)) = 0.3
        _Glossiness ("Smoothness", Range(0, 1)) = 0.2
        _PulseRate ("Pulse Rate", Range(0, 5)) = 1.0
        _PulseIntensity ("Pulse Intensity", Range(0, 0.5)) = 0.1
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 grainUV : TEXCOORD1;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };
            
            sampler2D _MainTex;
            sampler2D _GrainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _EmissionColor;
            float _EmissionIntensity;
            float _GrainIntensity;
            float _EdgeWear;
            float _Glossiness;
            float _PulseRate;
            float _PulseIntensity;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.grainUV = v.uv * 3.0; // Scale grain texture
                o.color = v.color * _Color;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the main texture
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // Add period-appropriate paper grain
                fixed4 grain = tex2D(_GrainTex, i.grainUV + _Time.y * 0.05);
                col.rgb = lerp(col.rgb, col.rgb * grain.rgb, _GrainIntensity);
                
                // Add edge wear effect (more worn at edges)
                float2 edgeDistance = abs(i.uv - 0.5) * 2.0;
                float edge = saturate(max(edgeDistance.x, edgeDistance.y));
                col.rgb *= 1.0 - (edge * edge * _EdgeWear);
                
                // Add subtle pulsing emission effect (period-appropriate glow like from lantern light)
                float pulse = _PulseIntensity * (sin(_Time.y * _PulseRate) * 0.5 + 0.5);
                col.rgb += _EmissionColor.rgb * _EmissionIntensity * pulse * col.a;
                
                // Apply period-appropriate subtle sheen/glossiness
                float3 viewDir = normalize(float3(0.5, 0.5, 1.0));
                float3 normalDir = float3(0, 0, 1);
                float spec = pow(max(0, dot(reflect(-viewDir, normalDir), viewDir)), 10) * _Glossiness;
                col.rgb += spec * _EmissionColor.rgb * col.a;
                
                return col;
            }
            ENDCG
        }
    }
    
    FallBack "UI/Default"
}

