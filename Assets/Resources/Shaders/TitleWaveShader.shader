Shader "UI/TitleWave"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaveStrength1 ("Wave Strength 1", Range(0, 0.2)) = 0.05
        _WaveSpeed1 ("Wave Speed 1", Range(0, 10)) = 3.0
        _Distortion1 ("Distortion 1", Range(0, 20)) = 5.0  // 더 큰 범위로 조정

        _WaveStrength2 ("Wave Strength 2", Range(0, 0.2)) = 0.03
        _WaveSpeed2 ("Wave Speed 2", Range(0, 15)) = 5.0     // 다른 속도
        _Distortion2 ("Distortion 2", Range(0, 30)) = 8.0    // 다른 왜곡
        _WaveDirection2("Wave Direction 2", Vector) = (1, -1, 0) // 두 번째 웨이브 방향

        _WaveStrength3 ("Wave Strength 3", Range(0, 0.1)) = 0.02 // 더 작은 세 번째 웨이브
        _WaveSpeed3 ("Wave Speed 3", Range(0, 20)) = 7.0
        _Distortion3 ("Distortion 3", Range(0, 40)) = 12.0
        _WaveDirection3("Wave Direction 3", Vector) = (-1, 1, 0)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR; // Sprite Renderer 컬러 사용
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float2 worldPos : TEXCOORD1;
                fixed4 color : COLOR; //컬러 fragment로 전달
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float _WaveStrength1;
            float _WaveSpeed1;
            float _Distortion1;

            float _WaveStrength2;
            float _WaveSpeed2;
            float _Distortion2;
            float2 _WaveDirection2;

            float _WaveStrength3;
            float _WaveSpeed3;
            float _Distortion3;
            float2 _WaveDirection3;


            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // UV 좌표를 중심으로 하는 상대 위치 계산 (더 일반적인 방법)
                float2 offset = v.uv - float2(0.5, 0.5);

                // 첫 번째 웨이브
                float wave1 = sin(_Time.y * _WaveSpeed1 + length(offset) * _Distortion1) * _WaveStrength1;

                // 두 번째 웨이브 (방향과 속도, 왜곡을 다르게)
                float2 dir2 = normalize(_WaveDirection2.xy); // 정규화
                float wave2 = sin(_Time.y * _WaveSpeed2 + dot(offset, dir2) * _Distortion2) * _WaveStrength2;

                // 세 번째 웨이브 (더 작고 빠른 웨이브)
                float2 dir3 = normalize(_WaveDirection3.xy);
                float wave3 = sin(_Time.y * _WaveSpeed3 + dot(offset, dir3) * _Distortion3) * _WaveStrength3;



                // 모든 웨이브를 합산하여 UV 좌표를 왜곡
                o.uv = v.uv + (wave1 + wave2 + wave3) * offset;  //offset방향으로 uv왜곡
                //o.uv = v.uv + wave1 * normalize(offset) + wave2 * dir2 + wave3 * dir3;  //다른 방법
                o.worldPos = v.vertex.xy;
                o.color = v.color;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // 텍스처와 Sprite Renderer의 컬러를 곱함
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                return col;
            }
            ENDCG
        }
    }
}

/*
Shader "UI/VerticalWaveEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WaveStrength ("Wave Strength", Range(0, 0.2)) = 0.05
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 3.0
        _Distortion ("Distortion", Range(0, 10)) = 5.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float _WaveStrength;
            float _WaveSpeed;
            float _Distortion;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                // 중심으로부터의 상대 위치 계산 (x는 그대로, y만 사용)
                float centerY = 0.5;
                float offsetY = v.uv.y - centerY;

                // 세로 방향으로만 물결 효과 적용
                float wave = sin(_Time.y * _WaveSpeed + offsetY * _Distortion) * _WaveStrength;

                // y축 변형만 적용 (세로로만 울렁이게)
                o.uv.y += wave;

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
*/