Shader "Custom/Lightning"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {} // SpriteRenderer wants this, not used
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        _Flash ("Flash", float) = 1.0
        _Seed ("seed", float) = 1.0
        _Reveal ("Reveal", Range(0, 1)) = 1
        _Width("Width", Float) = 1
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
                float  _Flash;
                float  _Seed;
                float  _Reveal;
                float  _Width;
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
            
            float lightning(float2 uv, float offset, float seed)
            {
                float x = uv.x - offset;
                
                float sum = octaves_sum(uv.y, 3, seed);
                float brightness = bolt_intensity(abs(x - sum));
                
                 for (int i = 1; i <= 10; i++)
                {
                    float start_y = 0.3 + hash(seed * i + 14 * i + 4) * 0.5;
                    float start_x = octaves_sum(start_y,3, seed);
                    float branch = octaves_sum(uv.y, 3, seed * 3 + i * 17 + 1);
                    float branch_start = octaves_sum(start_y, 3, seed * 3 + i * 17 + 1);
                    float below = start_y - uv.y;
                    float branch_x = start_x + below * (hash(seed + 5 * i + 17) * 6.0 - 3.0) - branch_start + branch;
                    float branch_fade = 1 - smoothstep(0, 0.05 + hash(seed + 3 * 14 + 3 * i) * 0.15, below);
                    float branch_brightness = bolt_intensity(abs(x - branch_x) * 2.0) * branch_fade * step(0.0, below);
                    brightness = max(brightness, branch_brightness * 0.6);
                }
                
                float boltReveal = saturate(_Reveal * 1.5 - hash(seed * 13 + 12) * 0.5);
                
                brightness *= max(_Flash, 0.0) * smoothstep(1 - boltReveal -0.04, 1 - boltReveal, uv.y);
                
                return brightness;
            }
            
            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;
                uv.x = (uv.x - 0.5) * _Width + 0.5;
                
                float brightness = 0;
                
                for (int i = 0; i < 3; i++)
                {
                    if (i > 0 && hash(_Seed * 12 + i * 2) > 0.3) continue;
                    brightness = max(lightning(uv, (hash(_Seed + i * 13) - 0.5) * 2.0, _Seed + i * 13), brightness);
                }

                half3 col = _Color.rgb * brightness * IN.color.rgb;
                return half4(col, brightness);
            }
            ENDHLSL
        }
    }
}