Shader "Custom/ParryShockwave"
{
    Properties
    {
        [MainTexture] _BaseMap ("Base Map", 2D) = "white" {}
        _Progress ("Progress", Range(0,1)) = 0.3
        _RingWidth ("Ring Width", Range(0.01,0.5)) = 0.15
        _Strength ("Strength", Range(0,0.01)) = 0.03
        _ChromaticSplit ("Chromatic Split", Range(0,1)) = 0.5
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
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Progress;
                float _RingWidth;
                float _Strength;
                float _ChromaticSplit;
            CBUFFER_END

            TEXTURE2D(_CameraSortingLayerTexture);
            SAMPLER(sampler_CameraSortingLayerTexture);

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.screenPos = ComputeScreenPos(OUT.positionHCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 centered = IN.uv - 0.5;

                float distance = length(centered) * 2.0;
                float radius = _Progress * (1.0 - _RingWidth);
                float ring = 1.0 - smoothstep(0.0, _RingWidth, abs(distance - radius));;

                float2 direction = centered / max(length(centered), 0.0001);
                float2 offset = direction * _Strength * ring;

                offset.x *= _ScreenParams.x / _ScreenParams.y;

                ring *= 1.0 - _Progress;

                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                half4 scene = SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture,
                                               screenUV - offset);

                float2 split = offset * _ChromaticSplit;

                half r = SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture,
                                          screenUV - offset- split).r;
                half g = SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, screenUV).g;
                half b = SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture,
                    screenUV - offset + split).b;

                return half4(r, g, b, 1);
            }
            ENDHLSL
        }
    }
}