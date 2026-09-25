Shader "Custom/SpriteUnlitHLSL"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Ref1Tint ("Reflection Tint (A = strength)", Color) = (1,0.4,0.4,0.6)
        _OffsetWorld ("Offset (percent of distance from center)", Float) = 0.5
        _BlurWorld   ("Blur (world units)", Float) = 0.02
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;     // SpriteRenderer color comes in here
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4  color      : COLOR;
                float2 uv         : TEXCOORD0;
                float4 screenPos  : TEXCOORD1;
                float2 uvPerUnit : TEXCOORD2;
                float offsetUV : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            TEXTURE2D(_CameraSortingLayerTexture);
            SAMPLER(sampler_CameraSortingLayerTexture);
            
            float _PlayerX;

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Ref1Tint;
                half4  _Color;
                float _OffsetWorld;
                float _BlurWorld;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _Color;
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                
                float3 centerWS = TransformObjectToWorld(float3(0, 0, 0));
                float4 a = TransformWorldToHClip(centerWS);
                float4 b = TransformWorldToHClip(centerWS + float3(1, 1, 0));
                OUT.uvPerUnit = abs(b.xy / b.w - a.xy / a.w) * 0.5;
                
                float dist = _PlayerX - centerWS.x; 
                OUT.offsetUV = dist * _OffsetWorld * OUT.uvPerUnit.x;  
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                half4 win = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;

                
                
                //float offsetUV = _OffsetWorld * IN.uvPerUnit.x; 
                float2 step    = _BlurWorld * IN.uvPerUnit;  
                
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                
                float2 sampleUV = screenUV + float2(IN.offsetUV, 0);

                half3 rgb = 0;
                half a = 0;
                half wSum = 0;
                
                [unroll] for (int x = -2; x <= 2; x++)
                    [unroll] for (int y = -2; y <= 2; y++)
                    {
                        half w = exp(-(float)(x * x + y * y) / 4.5);
                        half4 s = SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, sampleUV + float2(x, y) * step);
                        rgb += s.rgb * s.a * w;
                        a += s.a * w;
                        wSum += w;
                    }
                
                half3 ref1 = rgb / max(a, 1e-4);
                half ref1A = a / wSum;
                
                half3 col = lerp(win.rgb, ref1 * _Ref1Tint.rgb, ref1A * _Ref1Tint.a);
                
                return half4(col, win.a);
            }
            ENDHLSL
        }
    }
}