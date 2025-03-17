Shader "Custom/Blur"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _BlurSize("Blur Size", Range(0.0, 10.0)) = 1.0
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
            }
            LOD 100

            // UI�� ���� ó���� �ʿ��ϹǷ� ����
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
                    float4 color : COLOR;
                    float2 uv : TEXCOORD0;
                };

                struct v2f
                {
                    float2 uv : TEXCOORD0;
                    float4 vertex : SV_POSITION;
                    float4 color : COLOR;
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _BlurSize;

                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    o.color = v.color;
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float2 uv = i.uv;

                    // ���� �ؽ�ó �ȼ� ��
                    fixed4 centerColor = tex2D(_MainTex, uv);

                    // Blur�� ������(�ػ󵵿� ���)
                    float2 offset = float2(_BlurSize / _ScreenParams.x, _BlurSize / _ScreenParams.y);

                    // �ֺ� �ȼ� ���� (Box Blur ���)
                    fixed4 sum = centerColor;
                    sum += tex2D(_MainTex, uv + float2(+offset.x, 0));
                    sum += tex2D(_MainTex, uv + float2(-offset.x, 0));
                    sum += tex2D(_MainTex, uv + float2(0, +offset.y));
                    sum += tex2D(_MainTex, uv + float2(0, -offset.y));
                    sum += tex2D(_MainTex, uv + offset);
                    sum += tex2D(_MainTex, uv - offset);
                    sum += tex2D(_MainTex, uv + float2(+offset.x, -offset.y));
                    sum += tex2D(_MainTex, uv + float2(-offset.x, +offset.y));

                    // ���
                    sum /= 9.0;

                    return sum;
                }
                ENDCG
            }
        }

            // UI�� ������ �� �⺻ Unlit/Transparent ���� ����
                    FallBack "Unlit/Transparent"
}
