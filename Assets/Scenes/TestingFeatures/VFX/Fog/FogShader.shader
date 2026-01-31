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

        _Saturation ("Saturation", Range(0,1)) = 1

        _PixelSizeX ("Pixel Size X", Float) = 320
        _PixelSizeY ("Pixel Size Y", Float) = 180
        _EnablePixelation ("Enable Pixelation", Float) = 1
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

            float _Saturation;

            float _PixelSizeX;
            float _PixelSizeY;
            float _EnablePixelation;

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

            fixed4 ApplySaturation(fixed4 col, float saturation)
            {
                float gray = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb = lerp(gray, col.rgb, saturation);
                return col;
            }

            float2 PixelateUV(float2 uv)
            {
                float2 pixelCount = float2(_PixelSizeX, _PixelSizeY);
                return floor(uv * pixelCount) / pixelCount;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // ----- Pixelation -----
                if (_EnablePixelation > 0.5)
                {
                    uv = PixelateUV(uv);
                }

                fixed4 col = tex2D(_MainTex, uv);

                // ----- Depth -----
                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);
                float linearDepth = LinearEyeDepth(rawDepth);

                // ----- Distance fog -----
                float distFog = saturate((linearDepth - _FogStart) / (_FogEnd - _FogStart));

                // ----- World position reconstruction -----
                float4 clipPos;
                clipPos.xy = uv * 2.0 - 1.0;
                clipPos.z = rawDepth;
                clipPos.w = 1.0;

                float4 worldPos = mul(_InverseViewProjection, clipPos);
                worldPos.xyz /= worldPos.w;

                // ----- Height fog -----
                float heightFog = saturate((_FogHeight - worldPos.y) * _FogHeightDensity);

                float fogFactor = max(distFog, heightFog);
                col.rgb = lerp(col.rgb, _FogColor.rgb, fogFactor);

                // ----- Color quantization -----
                if (_EnableQuantization > 0.5)
                {
                    col = QuantizeColor(col, _ColorSteps);
                }

                // ----- Saturation -----
                col = ApplySaturation(col, _Saturation);

                return col;
            }
            ENDCG
        }
    }
}
