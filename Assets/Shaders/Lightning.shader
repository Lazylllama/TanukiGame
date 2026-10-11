Shader "Custom/Lightning"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {} // SpriteRenderer wants this, not used
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend One One   // additive: the bolt ADDS light to whatever is behind it
        ZWrite Off
        Cull Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = IN.uv;
                OUT.color       = IN.color;
                return OUT;
            }
            
            float bolt_intensity(float d)
            {
                float core = exp(-d * 80.0) * 3.0;
                float glow = exp(-d * 12.0) * 0.4;
                float edge = 1.0 - smoothstep(0.3, 0.5, d);
                
                float result = (core + glow) * edge;
                
                return result;
            }
            
            float hash(float n) {return frac(sin(n * 127.1) * 43758.5453);}
            
            
            float init_octave(float uv_y, float weigth, int segments, float seed)
            {
                float s = uv_y * float(segments);
                float i = floor(s);
                float f = frac(s);
                
                float value = lerp(hash(i + seed * 37.0), hash(i + 1.0 + seed * 37.0), f);
                float x_bolt = 0.5 + (value - 0.5) * 0.3;
                
                return x_bolt *= weigth; 
            }
            
            float octaves_sum(float uv_y, int octaves, float seed)
            {
                int base_segments = 8;
                float base_weigth = 0.5;
                
                float sum = 0.0;
                float weight_sum = 0.0;
                
                for (int i = 0; i < octaves; i++)
                {
                    float o = init_octave(uv_y, base_weigth * pow(0.5, float(i)), base_segments * pow(2, float(i)), seed);
                    sum += o;
                    weight_sum += base_weigth * pow(0.5, float(i));
                }
                
                sum /= weight_sum;
                
                return sum;
            }
            
            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                
                float seed = floor(_Time.y * 0.5);
                
                float sum = octaves_sum(uv.y, 3, seed);
                
                float brightness = bolt_intensity(abs(uv.x - sum));
                
                brightness *= exp(-frac(_Time.y * 0.5) * 10);

                half3 col = _Color.rgb * brightness * IN.color.rgb;
                return half4(col, brightness);
            }
            ENDHLSL
        }
    }
}