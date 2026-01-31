Shader "Unlit/FogShader"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0.6,0.6,0.7,1)

        _FogStart ("Fog Start", Float) = 10
        _FogEnd ("Fog End", Float) = 50

        _FogHeight ("Fog Height", Float) = 0
        _FogHeightDensity ("Height Density", Float) = 0.2

        _ColorSteps ("Color Steps", Float) = 16
        _EnableQuantization ("Enable Quantization", Float) = 0
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _CameraDepthTexture;

            float4 _FogColor;
            float _FogStart;
            float _FogEnd;

            float _FogHeight;
            float _FogHeightDensity;

            float _ColorSteps;
            float _EnableQuantization;

            float4x4 _InverseViewProjection;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 QuantizeColor(fixed4 col, float steps)
            {
                col.rgb = floor(col.rgb * steps) / steps;
                return col;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                // ----- Depth -----
                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, i.uv);
                float linearDepth = LinearEyeDepth(rawDepth);

                // ----- Distance fog -----
                float distFog = saturate((linearDepth - _FogStart) / (_FogEnd - _FogStart));

                // ----- World position reconstruction -----
                float4 clipPos;
                clipPos.xy = i.uv * 2.0 - 1.0;
                clipPos.z = rawDepth;
                clipPos.w = 1.0;

                float4 worldPos = mul(_InverseViewProjection, clipPos);
                worldPos.xyz /= worldPos.w;

                // ----- Height fog -----
                float heightFog = saturate((_FogHeight - worldPos.y) * _FogHeightDensity);

                // Combine fog (strongest wins)
                float fogFactor = max(distFog, heightFog);

                col.rgb = lerp(col.rgb, _FogColor.rgb, fogFactor);

                // ----- Optional color quantization -----
                if (_EnableQuantization > 0.5)
                {
                    col = QuantizeColor(col, _ColorSteps);
                }

                return col;
            }
            ENDCG
        }
    }
}
