Shader "Custom/LaneWave"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _WaveSpeed ("Wave Speed", Float) = 1.0
        _WaveHeight ("Wave Height", Float) = 0.5
        _WaveFrequency ("Wave Frequency", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
            };

            fixed4 _Color;
            float _WaveSpeed;
            float _WaveHeight;
            float _WaveFrequency;

            v2f vert (appdata_t v)
            {
                v2f o;
                float wave = sin(v.vertex.x * _WaveFrequency + _Time.y * _WaveSpeed) * 
                             cos(v.vertex.z * _WaveFrequency + _Time.y * _WaveSpeed);
                v.vertex.y += wave * _WaveHeight;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.normal = v.normal;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}