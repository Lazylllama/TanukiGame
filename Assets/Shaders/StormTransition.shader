Shader "Custom/StormTransition"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {} //? SpriteRenderer wants this, not used
        _Color ("Color", Color) = (1, 1, 1, 1) //? transition color
        _Progress ("Progress", Range(0,1)) = 0 //? less value = less fill
        _EdgeAmount ("Edge wobble", Range(0,0.5)) = 0.15 //? how drastic the wobble is
        _EdgeSoftness ("Edge softness", Range(0,0.1)) = 0.01 //? basically how much to fade out the edges
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
        ZTest Always
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
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _Progress;
                float _EdgeAmount;
                float _EdgeSoftness;
            CBUFFER_END

            float hash(float n) { return frac(sin(n * 127.1) * 43758.5453); }

            float noise1D(float x)
            {
                float i = floor(x);
                float f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(hash(i), hash(i + 1.0), f);
            }

            float wobble(float y, float t)
            {
                float sum = 0.0;
                sum += noise1D(y * 3.0 + t * 0.4) * 0.5;
                sum += noise1D(y * 7.0 - t * 0.9) * 0.25;
                sum += noise1D(y * 15.0 + t * 1.7) * 0.125;
                return sum / 0.875;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                //? first 0.5 is half speed, second is to retract it so you cant see it at 0% progress
                float offset = (wobble(uv.y, _Time.y * 0.5) - 0.5) * _EdgeAmount;

                float margin = _EdgeAmount * 0.5 + _EdgeSoftness;
                float edge = lerp(-margin, 1 + margin, _Progress) + offset;

                float covered = 1 - smoothstep(edge - _EdgeSoftness, edge, uv.x);
                return half4(_Color.rgb, covered);
            }
            ENDHLSL
        }
    }
}