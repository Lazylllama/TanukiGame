Shader "Custom/Scratch"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        _MaskTex("Mask Texture ", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
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
            
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MaskTex_ST;
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
                // Gets the color of the foil.
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;
                
                // Gets the scratch value of the mask texture.
                float alpha = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, IN.uv);
                
                // Modifies the scratch value with noise
                float d = alpha - noise2D(IN.uv * 300) * 0.6;
                
                // Samples another value up and to the left and also modifies it by the noise.
                float dur = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, IN.uv + 0.005) - noise2D((IN.uv + 0.005) * 300) * 0.6;
                
                // Check if the current and up to the left pixel is scratched or not.
                bool isfoil = d <= 0.2;
                bool durisfoil = dur <= 0.2;
                
                // If both arent scratched return normal foil.
                if (isfoil && durisfoil) return color;
                
                // If the current isn't scratched but the sampled one is return a brighter higlight color.
                if (isfoil && !durisfoil) return float4(color.rgb * 1.6, color.a);
                
                // If the current is scratched but the sampled one isn't return a darker shadow color.
                if (!isfoil && durisfoil) return float4(0.0, 0.0, 0.0, 0.35);
                
                // If both are scratched make it see through.
                if (!isfoil && !durisfoil) color.a = 0.0;
                return color;
            }
            ENDHLSL
        }
    }
}