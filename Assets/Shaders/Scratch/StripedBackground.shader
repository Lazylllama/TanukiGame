Shader "Custom/StripedBackground"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        _Color1("Color 1", Color) = (1, 1, 1, 1)
        _Color2("Color 2", Color) = (0, 0, 0, 1)
        _Stripes("Stripe Amount", Int) = 10
        _Aspect("Aspect", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZTest Always
            ZWrite off
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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color1;
                float4 _Color2;
                float  _Aspect;
                int    _Stripes;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // The UV adjusted for the aspect ratio.
                float2 adjustedUV = float2(IN.uv.x * _Aspect, IN.uv.y);
                
                // The uv adjusted for the amount of stripes.
                adjustedUV *= _Stripes;
                
                float diagonal = adjustedUV.x + adjustedUV.y;
                
                float colorValue = smoothstep(0.48, 0.52, frac(diagonal));
                
                half4 color = lerp(_Color1, _Color2, colorValue) * IN.color;
                
                return half4(color);
            }
            ENDHLSL
        }
    }
}