Shader "Custom/HitFlash"
{
    Properties
    {
        [MainColor] _MainTex ("Sprite Texture", 2D) = "white" {} // SpriteRenderer wants this, not used
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _FlashColor ("Flash Color", Color) = (1, 0, 0, 1)
        _FlashAmount ("Flash Amount", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Tags
            {
                "LightMode" = "Universal2D"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Core2D.hlsl"

            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/ShapeLightShared.hlsl"

            #pragma multi_compile_instancing
            #pragma multi_compile _ DEBUG_DISPLAY

            struct Attributes
            {
                COMMON_2D_INPUTS
                half4 color : COLOR;
            };

            struct Varyings
            {
                COMMON_2D_LIT_OUTPUTS
                float4 color : COLOR;
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/Lit2DCommon.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _FlashColor;
                float _FlashAmount;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                SetUpSpriteInstanceProperties();

                IN.positionOS.xy *= unity_SpriteProps.xy;

                Varyings OUT = CommonLitVertex(IN);
                OUT.uv = IN.uv;
                OUT.color = IN.color * unity_SpriteColor;
                return OUT;
            }


            half4 frag(Varyings IN) : SV_Target
            {
                half4 lit = CommonLitFragment(IN, IN.color);
                lit.rgb = lerp(lit.rgb, _FlashColor.rgb, _FlashAmount);
                return lit;
            }
            ENDHLSL
        }
    }
}