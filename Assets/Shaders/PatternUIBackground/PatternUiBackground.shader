Shader "Custom/PatternUiBackground"
{
    Properties
    {
        _TopColor("Top Color", Color) = (1, 1, 1, 1)
        _BottomColor("Bottom Color", Color) = (0, 0, 0, 1)

        _PatternTexture("Pattern Texture", 2D) = "white" {}
        _PatternScale("Pattern Scale", Float) = 8
        _PatternColor("Pattern Color", Color) = (1, 1, 1, 1)
        _PatternScrollSpeed("Pattern Scroll Speed", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" "Queue" = "Transparent"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            ZTest Always

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

            TEXTURE2D(_PatternTexture);
            SAMPLER(sampler_PatternTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _TopColor;
                float4 _BottomColor;

                float _PatternScale;
                float4 _PatternColor;
                float4 _PatternScrollSpeed;
                float4 _PatternTexture_TexelSize;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.color = IN.color;
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = lerp(_BottomColor, _TopColor, IN.uv.y);

                float screenAspect = _ScreenParams.x / _ScreenParams.y;
                float tileAspect = _PatternTexture_TexelSize.z / _PatternTexture_TexelSize.w;
                float2 patternUV = IN.uv * float2(screenAspect / tileAspect, 1) * _PatternScale;

                patternUV += _PatternScrollSpeed.xy * _Time.y;

                half4 mask = SAMPLE_TEXTURE2D(_PatternTexture, sampler_PatternTexture, patternUV).a * _PatternColor.a;

                color.rgb = lerp(color.rgb, _PatternColor.rgb, mask);

                return color * IN.color;
            }
            ENDHLSL
        }
    }
}