Shader "Custom/Clouds"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        _SDFTexture("Signed Distance Field Texture", 2D) =  "white" {}
        _MoonPos("Moon Positon", Vector) = (0, 0, 0, 0)
        _Col1("Color 1", Color) = (1, 1, 1, 1)
        _Col2("Color 2", Color) = (1, 1, 1, 1)
        _Col3("Color 3", Color) = (1, 1, 1, 1)
        _Col4("Color 4", Color) = (1, 1, 1, 1)
        _DistFromMoon("Distance from moon", float) = 100
        _BackDensity("Backlight Density", float) = 1
        _Radius("Falloff Radius", float) = 1
        _StartDepth("Start Depth", float) = 1
        _T1("Band 1 start", float) = 0.33
        _T2("Band 2 start", float) = 0.66
        _T3("Band 3 start", float) = 0.99
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
                float3 positionWS : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            TEXTURE2D(_SDFTexture);
            SAMPLER(sampler_SDFTexture);
            

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _SDFTexture_TexelSize;
                float4 _MoonPos;
                float4 _Col1;
                float4 _Col2;
                float4 _Col3;
                float4 _Col4;
                float  _DistFromMoon;
                float  _BackDensity;
                float  _Radius;
                float _StartDepth;
                float _T1;
                float _T2;
                float _T3;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = (floor(IN.uv * _SDFTexture_TexelSize.zw) + 0.5) * _SDFTexture_TexelSize.xy;
                
                float d = SAMPLE_TEXTURE2D(_SDFTexture, sampler_SDFTexture, uv).r;
                float thickness = sqrt(1 - pow(1 - d, 2)) * (0.8 + noise2D(uv.xy * float2(1.78, 1) * 15) * 0.4);
                
                float2 toMoon = normalize(_MoonPos.xy - IN.positionWS.xy);
                //float3 moonDir = normalize(float3(toMoon, -_DistFromMoon));
                float3 moonDir = normalize(float3(toMoon, -0.3));
                float stepSize = 0.003;
                int StepAmount = 50;
                
                float3 startPos = float3(uv, thickness * 0.04 * _StartDepth);
                float sum = 0;
                
                for (int i = 1; i <= StepAmount; i++)
                {
                    float3 pos = startPos + moonDir * i * stepSize;
                    float shadowD = SAMPLE_TEXTURE2D_LOD(_SDFTexture, sampler_SDFTexture, pos.xy, 0).r;
                    float shadowThickness = sqrt(1 - pow(1 - shadowD, 2)) * 0.04 * (0.8 + noise2D(pos.xy * float2(1.78, 1) * 15) * 0.4);
                    if (abs(pos.z) < shadowThickness) sum += stepSize;
                }
                
                float light = exp(-sum * 15);
                
                float dist = distance(_MoonPos.xy, IN.positionWS.xy);
                float falloff = saturate(1 - dist / _Radius);
                float backlight = exp(-_BackDensity * d) * falloff;
                
                //light = max(light, backlight);
                light = pow(light, 1.0 / 2.2);
                
                int band = step(_T1, light) + step(_T2, light) + step(_T3, light);
                
                float4 selectedColor;
                
                if (band == 0) selectedColor = _Col1;
                if (band == 1) selectedColor = _Col2;
                if (band == 2) selectedColor = _Col3;
                if (band == 3) selectedColor = _Col4;
                
                return half4(selectedColor.rgb, d > 0 ? 1 : 0);
            }
            ENDHLSL
        }
    }
}