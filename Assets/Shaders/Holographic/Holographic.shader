Shader "Custom/HOLO"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        
        [Header(Holo)]
        _HoloStrength("Holo Strength", Range(0, 1)) = 1.0
        _MaskLow("Mask Low", Range(0, 1)) = 0.0
        _MaskHigh("Mask High", Range(0, 1)) = 0.375
        _Tilt("Tilt", Vector) = (0, 0, 0, 0)
        _Aspect("Aspect Ratio", float) = 1.0
        
        [Header(Sparkle)]
        _SparkleStrength("Sparkle Strength", float) = 4.0
        _FlakeCount("Flake Count", int) = 30
        _GlowWidth("Glow Width", Float) = 0.4
        
        [Header(Burn)]
        _BurnColor("Burn Color", Color) = (0.9, 0.4, 0.2, 1.0)
        _Burn("Burn", Range(0, 1)) = 0
        _BurnBrightness("Burn Brightness", Float) = 4.0
        _GlowThickness("Glow Thickness", Range(0, 1)) = 0.1
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
                vector _Tilt;
                float4 _MainTex_ST;
                float4 _BurnColor;
                float  _HoloStrength;
                float  _MaskLow;
                float  _MaskHigh;
                float  _Point;
                float  _Aspect;
                float  _SparkleStrength;
                float  _GlowWidth;
                float  _Burn;
                float  _BurnBrightness;
                float  _GlowThickness;
                int    _FlakeCount;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            float3 screen(float3 a, float3 b)
            {
                return 1.0 - (1.0 - a) * (1.0 - b);
            }

            float2 hash2(float2 input)
            {
                float a = input.x * 127.1 + input.y * 311.7;
                float b = input.x * 265.7 + input.y * 945.3;

                a = frac(sin(a) * 43758.5453);
                b = frac(cos(b) * 83712.8374);

                return float2(a, b);
            }

            float3 hash3(float2 input)
            {
                float a = input.x * 127.1 + input.y * 311.7;
                float b = input.x * 269.5 + input.y * 183.3;
                float c = input.x * 419.2 + input.y * 371.9;

                return frac(sin(float3(a, b, c)) * 43758.5453);
            }
            
            float noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash2(i).x;
                float b = hash2(i + float2(1.0, 0.0)).x;
                float c = hash2(i + float2(0.0, 1.0)).x;
                float d = hash2(i + float2(1.0, 1.0)).x;

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }
            
            float3 voronoi(float2 uv, int cells)
            {
                int2 correctedCells = float2(cells, cells / _Aspect);
                float2 cellCoord = frac(uv * (float2)correctedCells);
                float2 cell = floor(uv * (float2)correctedCells);

                float smallestDist = 1e+9;
                float2 id;

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        float2 currentCell = cell + float2(x, y);
                        float2 currentSeed = hash2(currentCell);
                        float dist = distance(currentSeed + currentCell, cellCoord + cell);

                        if (dist < smallestDist)
                        {
                            smallestDist = dist;
                            id = currentCell;
                        }
                    }
                }

                return float3(id, smallestDist);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;
                
                float burnNoise = noise2D(float2(IN.uv.x * 10.0 * _Aspect, IN.uv.y * 10.0)) * 0.625;
                burnNoise += noise2D(float2(IN.uv.x * 17.0 * _Aspect, IN.uv.y * 17.0)) * 0.25;
                burnNoise += noise2D(float2(IN.uv.x * 29.0 * _Aspect, IN.uv.y * 29.0)) * 0.125;
                
                color.a *= step(burnNoise, 1 - _Burn);
                
                float brightness = dot(color.rgb, float3(0.299, 0.587, 0.114));
                float mask = smoothstep(_MaskLow, _MaskHigh, brightness);

                float diagonal = IN.uv.x + IN.uv.y - (_Tilt.x + _Tilt.y) * 0.5;
                float dist = abs(((IN.uv.x + IN.uv.y) - ((_Tilt.x + _Tilt.y + 2.0) / 4.0) * 2.2 + 0.1));

                float3 voro = voronoi(IN.uv, _FlakeCount);

                float favoriteTilt = distance(hash2(voro.xy) * 2.0 - 1.0, _Tilt.xy);
                float colorShift = hash2(voro.xy * 3 + 15).x;

                float glow = 1 - smoothstep(0, _GlowWidth, favoriteTilt);
                float3 rainBow = 0.5 + 0.5 * cos(6.28318 * (diagonal + float3(0, 0.33, 0.67)));

                color.rgb = lerp(color.rgb, screen(color.rgb, rainBow), _HoloStrength * mask);

                float3 shiftedRainBow = 0.5 + 0.5 * cos(6.28318 * (diagonal + colorShift + float3(0, 0.33, 0.67)));

                color.rgb += (glow * _SparkleStrength * _HoloStrength) * shiftedRainBow * mask;
                
                float burnEdge = smoothstep(1 - _Burn - _GlowThickness, 1 - _Burn, burnNoise);
                
                float burnBrightness = noise2D(IN.uv * 10 + 232 + float2(0, fmod(_Time.y, 100))) * 0.625;
                burnBrightness += noise2D(IN.uv * 17 + 123 + float2(0, fmod(_Time.y * 2, 100))) * 0.25;
                burnBrightness += noise2D(IN.uv * 29 + 534 + float2(0, fmod(_Time.y * 5, 100))) * 0.125;
                
                color.rgb = lerp(color.rgb, _BurnColor.rgb * burnBrightness * _BurnBrightness, burnEdge);
                
                //return half4(_Tilt.xy, 0.0, 1.0);
                //return half4(mask.xxx, color.a);

                return color;
            }
            ENDHLSL
        }
    }
}