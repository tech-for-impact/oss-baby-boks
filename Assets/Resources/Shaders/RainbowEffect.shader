Shader "Custom/RainbowEffect" {
    
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}  
        _RainbowIntensity ("Rainbow Intensity", Range(0,1)) = 0.5  
        _Alpha ("Alpha", Range(0,1)) = 0.7  
        _Speed ("Speed", Float) = 1.0  
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _RainbowIntensity;
            float _Alpha;
            float _Speed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                
                float hue = fmod(_Time.y * _Speed, 1.0);
                float3 rainbow = hsv2rgb(float3(hue, 1.0, 1.0));
\
                fixed3 blendedColor = lerp(texColor.rgb, texColor.rgb * rainbow, _RainbowIntensity);

                return fixed4(blendedColor, _Alpha);
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}