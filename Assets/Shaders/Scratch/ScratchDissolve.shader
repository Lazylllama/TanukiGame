Shader "Custom/ScratchDissolve"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            blend One One
            Cull Off
            ZTest Always
            ZWrite off
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Assets/Shaders/Lib/Noise.hlsl"

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
                
                // Sample the scratch values from sorrounding values.
                float brightness = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                float b1 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(IN.uv.x + 0.004 + noise2D(IN.uv * 10) * 0.04, IN.uv.y));
                float b2 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(IN.uv.x, IN.uv.y + 0.004 + noise2D(IN.uv * 10) * 0.04));
                float b3 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(IN.uv.x - 0.004 - noise2D(IN.uv * 10) * 0.04, IN.uv.y));
                float b4 = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, float2(IN.uv.x, IN.uv.y - 0.004 - noise2D(IN.uv * 10) * 0.04));
                
                // Set this pixel to the highhest of the samples.
                float b = max(max(max(max(brightness, b1), b2), b3), b4);
                
                return half4(b.xxxx);
            }
            ENDHLSL
        }
    }
}