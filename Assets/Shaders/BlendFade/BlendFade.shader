Shader "Custom/BlendFade"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        [HDR] _BaseColor("Base Color", Color) = (1,1,1,1)
        _FadeStart("Fade Start", Range(0,1)) = 0.0
        _FadeEnd("Fade End", Range(0,1)) = 0.7
        [ToggleUI] _OneDirectional("One Directional", Float) = 0
        _FromCorner("From Corner", Range(0,4)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
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
                half4 _BaseColor;
                float _FadeStart;
                float _FadeEnd;
                float _OneDirectional;
                float _FromCorner;
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
                half4 color = _BaseColor * IN.color;

                // Calculate the fade based on the distance from the center
                float distanceFromCenter = distance(IN.uv.x, float2(0.5, 0.5));
                float fade = smoothstep(_FadeEnd, _FadeStart, distanceFromCenter);

                if (_OneDirectional > 0.5)
                {
                    fade = smoothstep(_FadeEnd, _FadeStart, IN.uv.x);
                }

                // alawys one directional
                if (_FromCorner > 0)
                    fade = smoothstep(_FadeEnd, _FadeStart, distance(IN.uv, float2(0.0, 1.0)));
                if (_FromCorner >= 1)
                    fade = smoothstep(_FadeEnd, _FadeStart, distance(IN.uv, float2(0.0, 0.0)));
                if (_FromCorner >= 2)
                    fade = smoothstep(_FadeEnd, _FadeStart, distance(IN.uv, float2(1.0, 0.0)));
                if (_FromCorner >= 3)
                    fade = smoothstep(_FadeEnd, _FadeStart, distance(IN.uv, float2(1.0, 1.0)));

                color.a *= fade;

                return color;
            }
            ENDHLSL
        }
    }
}