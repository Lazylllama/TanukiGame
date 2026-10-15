Shader "Custom/HOLO"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        _HoloStrength("Holo Strength", Range(0, 1)) = 1.0
        _MaskLow("Mask Low", Range(0, 1)) = 0.0
        _MaskHigh("Mask High", Range(0, 1)) = 0.375
        _Point("Point", float) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent"  "RenderPipeline" = "UniversalPipeline"  }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
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
                float  _HoloStrength;
                float  _MaskLow;
                float  _MaskHigh;
                float  _Point;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }
            
            float3 screen(float3 a, float3 b){
                return 1.0 - (1.0 - a) * (1.0 - b);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;
                
                float brightness = dot(color.rgb, float3(0.299, 0.587, 0.114));
                float mask = smoothstep(_MaskLow, _MaskHigh, brightness);
                
                float diagonal = IN.uv.x + IN.uv.y + (_Time.y * 0.1) % 2.0;
                float dist = abs(diagonal - _Point - (_Time.y * 0.1) % 2.0);
                float glow = 1 - smoothstep(0, 0.1, dist);
                float3 rainBow = 0.5 + 0.5 * cos(6.28318 * (diagonal + float3(0, 0.33, 0.67)));
                
                color.rgb = lerp(color.rgb,  screen(color.rgb, rainBow), _HoloStrength * mask);
                
                color.rgb += glow;
                
                //return half4(mask.xxx, color.a);
                
                return color;
            }
            ENDHLSL
        }
    }
}
