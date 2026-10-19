Shader "Custom/StormTransition"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {} //? SpriteRenderer wants this, not used
        _Color ("Color", Color) = (1, 1, 1, 1) //? transition color
        _Progress ("Progress", Range(0,1)) = 0 //? less value = less fill
        _EdgeAmount ("Edge wobble", Range(0,0.5)) = 0.15 //? how drastic the wobble is
        _EdgeSoftness ("Edge softness", Range(0,0.1)) = 0.01 //? basically how much to fade out the edges
        _EdgeLean ("Edge lean", Range(0,0.5)) = 0.1

        _RainColor ("Rain Color", Color) = (0.6, 0.65, 0.8, 0.5)
        _RainColumns ("Rain Columns", Range(10,200)) = 80
        _RainSlant ("Rain Slant", Range(-2,2)) = 0.5

        _RimColor ("Rim Color", Color) = (0.2, 0.2, 0.2, 1)
        _RimWidth ("Rim Width", Range(0,0.5)) = 0.1

        [ToggleUI] _Reversed ("Reversed", Float) = 0
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
                float _Reversed;
                float4 _RainColor;
                float _RainColumns;
                float _RainSlant;
                float _EdgeLean;
                float4 _RimColor;
                float _RimWidth;
            CBUFFER_END

            float hash1D(float n) { return frac(sin(n * 127.1) * 43758.5453); }
            float hash2D(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            
            float noise1D(float x)
            {
                float i = floor(x);
                float f = frac(x);
                f = f * f * (3.0 - 2.0 * f);
                return lerp(hash1D(i), hash1D(i + 1.0), f);
            }

            //? value noise not perlin noise
            float noise2D(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                float a = hash2D(i);
                float b = hash2D(i + float2(1.0, 0.0));
                float c = hash2D(i + float2(0.0, 1.0));
                float d = hash2D(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            float wobble(float y, float t)
            {
                float sum = 0.0;
                sum += noise1D(y * 3.0 + t * 0.4) * 0.5;
                sum += noise1D(y * 7.0 - t * 0.9) * 0.25;
                sum += noise1D(y * 15.0 + t * 1.7) * 0.125;
                return sum / 0.875;
            }

            float clouds(float2 uv, float t)
            {
                float sum = 0.0;
                sum += noise2D(uv * 4.0 + float2(-t * 0.2, 0)) * 0.5;
                sum += noise2D(uv * 8.0 + float2(-t * 0.35, 0)) * 0.25;
                sum += noise2D(uv * 16.0 + float2(-t * 0.6, 0)) * 0.125;
                return sum / 0.875;
            }

            float rain(float2 uv, float t)
            {
                float x = uv.x + uv.y * _RainSlant;

                float column = floor(x * _RainColumns);
                float inCol = frac(x * _RainColumns);
                float rand = hash1D(column);

                float y = frac(uv.y * 1.5 + t * (1.5 + rand) + rand * 10);

                float streak = smoothstep(0.0, 0.2, y) * (1.0 - smoothstep(0.02, 0.05 * (rand * 4), y));
                //? fading rain drop
                float thin = 1.0 - smoothstep(0.05, 0.15, abs(inCol - 0.5));
                float hasDrop = step(0.75, rand);

                return streak * thin * hasDrop;
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
                float2 noFlipUV = IN.uv; //? no flip :)
                float2 uv = IN.uv;

                uv.x = _Reversed ? 1 - uv.x : uv.x;

                //? first 0.5 is half speed, second is to retract it so you cant see it at 0% progress
                float offset = (wobble(uv.y, _Time.y * 0.5) - 0.5) * _EdgeAmount;

                float margin = _EdgeAmount * 0.5 + _EdgeSoftness + _EdgeLean;
                float edge = lerp(-margin, 1 + margin, _Progress) + offset + uv.y * _EdgeLean;

                float covered = 1 - smoothstep(edge - _EdgeSoftness, edge, uv.x);
                float distance = edge - uv.x;
                float rim = 1 - smoothstep(0, _RimWidth, abs(distance));

                float3 storm = 0;

                storm += _Color.rgb * lerp(0.4, 1.4, clouds(noFlipUV, _Time.y)); //? clouds
                storm += _RainColor.rgb * rain(noFlipUV, _Time.y) * _RainColor.a; //? rain
                storm = lerp(storm, _RimColor.rgb, rim) ; //? rim + fade rain

                return half4(storm, covered);
            }
            ENDHLSL
        }
    }
}