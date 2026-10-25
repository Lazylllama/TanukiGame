Shader "Custom/ThrowArc"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        _EdgeSoftness ("Edge Softness", Range(0,1)) = 0.3
        _DotFill ("Dot Fill", Range(0,1)) = 0.5
        _DotSpacing ("Dot Spacing", Range(0,4)) = 1
        [HideInInspector] _ArcLength ("Arc Length", Float) = 10
        [HideInInspector] _LineWidth ("Line Width", Float) = 0.3
        _FadeStart ("Fade Start", Range(0,0.5)) = 0.1
        _FadeEnd ("Fade End", Range(0,0.5)) = 0.3

        _MarchSpeed ("March Speed", Range(0,10)) = 5
        [HideInInspector] _UnscaledTime ("Unscaled Time", float) = 0

        [HideInInspector] _Charge ("Charge", Range(0,1)) = 0.5
        _ChargeGlow ("Charge Glow", Float) = 2
    }

    
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

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
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _EdgeSoftness;
                float _DotFill;
                float _DotSpacing;
                float _ArcLength;
                float _LineWidth;
                float _FadeStart;
                float _FadeEnd;

                float _MarchSpeed;
                float _UnscaledTime;

                float _Charge;
                float _ChargeGlow;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color;
                return OUT;
            }


            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = _Color * IN.color;

                float along = IN.uv.x * _ArcLength - _UnscaledTime * _MarchSpeed;
                float across = (IN.uv.y - 0.5) * _LineWidth;

                float x = (frac(along / _DotSpacing) - 0.5) * _DotSpacing;

                float radius = _LineWidth * 0.5;
                float halfLength = max(_DotFill * _DotSpacing * 0.5 - radius, 0.0);

                float distance = length(float2(max(abs(x) - halfLength, 0.0), across)) - radius;

                float soft = _EdgeSoftness * radius;
                float shape = 1.0 - smoothstep(-soft, 0.0, distance);

                float fadeStart = smoothstep(0.0, _FadeStart, IN.uv.x);
                float fadeEnd = 1.0 - smoothstep(1.0 - _FadeEnd, 1.0, IN.uv.x);


                color.a *= shape * fadeStart * fadeEnd;
                color.rgb *= 1.0 + _Charge * _ChargeGlow;


                return color;
                //return half4(IN.uv.x, IN.uv.y, 0, 1);
            }
            ENDHLSL
        }
    }
}