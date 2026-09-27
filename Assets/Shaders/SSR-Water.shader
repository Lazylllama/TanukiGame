Shader "Custom/WaterReflection"
{
    Properties
    {
        [MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _RippleStrength ("Ripple Strength", Float) = 0.01
        _RippleFreq ("Ripple Frequency", Float) = 8
        _RippleSpeed ("Ripple Speed", Float) = 2
        _WaterTint ("Water color (A = strength)", Color) = (0.2, 0.5, 0.8, 0.35)
        _DeepWaterTint ("Deep water color (A = strength", Color) = (0.2, 0.5, 0.8, 0.35)
        _FoamColor ("Foam color", Color) = (1,1,1,1)
        _FoamThickness ("Foam edge thickness", Float) = 1
        _FoamBubbleScale ("Foam bubble scale", Float) = 1
        _FoamBubbleSpeed ("Foam bubble speed", Float) = 1
        _FadeDistance ("Fade distance", Float) = 0.5
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
                float4 color      : COLOR;     // SpriteRenderer color
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                half4  color      : COLOR;
                float2 uv         : TEXCOORD0;
                float4 screenPos  : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            TEXTURE2D(_CameraSortingLayerTexture);
            SAMPLER(sampler_CameraSortingLayerTexture);
            

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                half4  _Color;
                half4  _WaterTint;
                half4  _DeepWaterTint;
                float  _RippleStrength;
                float  _RippleFreq;
                float  _RippleSpeed;
                half4  _FoamColor;
                float  _FoamThickness;
                float  _FoamBubbleScale;
                float  _FoamBubbleSpeed;
                float  _FadeDistance;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _Color;
                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                return OUT;
            }
            
            float2 UVToScreen(float2 targetUV, float2 uv, float2 screenUV)
            {
                float2 duvdx = ddx(uv), duvdy = ddy(uv);
                float2 dsdx = ddx(screenUV), dsdy = ddy(screenUV);
                
                float2x2 A = float2x2(duvdx.x, duvdy.x, duvdx.y, duvdy.y);
                float2x2 B = float2x2(dsdx.x, dsdy.x, dsdx.y, dsdy.y);
                
                float det = A._11 * A._22 - A._12 * A._21;
                float2x2 Ainv = float2x2(A._22, -A._12, -A._21, A._11) / det;
                
                return screenUV + mul(B, mul(Ainv, targetUV - uv));
            }
            
            float2 Hash22(float2 p)
            {
                float3 p3 = frac(p.xyx * float3(0.1031, 0.1030, 0.0973));
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.xx + p3.yz) * p3.zy);
            }
            
            float Worley(float2 p, float t)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                float minDist = 8.0;

                [unroll] for (int y = -1; y <= 1; y++)
                [unroll] for (int x = -1; x <= 1; x++)
                {
                    float2 o = float2(x, y);
                    float2 h = Hash22(cell + o);
                    h = 0.5 + 0.5 * sin(t + 6.2831 * h);   // points wobble around over time
                    float2 d = o + h - f;
                    minDist = min(minDist, dot(d, d));
                }
                return sqrt(minDist);
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                
                float2 p = IN.positionWS.xy * _RippleFreq;
                float t = _Time.y * _RippleSpeed;
              
                float2 ripple;
                ripple.x = sin(p.y + t) + 0.5 * sin(p.y * 2.3 + p.x * 1.7 - t * 1.3);
                ripple.y = 0.5 * sin(p.x * 0.7 + t * 0.8);
                
                float2 reflectedUV = float2(IN.uv.x, 2 - IN.uv.y);
                reflectedUV += ripple * _RippleStrength * (1 - IN.uv.y);
                
                float2 reflectedUVOnScreen = UVToScreen(reflectedUV, IN.uv, screenUV);
                
                float2 foamHighEdgeYScreen = UVToScreen(float2(IN.uv.x, 1), IN.uv, screenUV);
                float2 foamLowEdgeYScreen = foamHighEdgeYScreen;
                foamLowEdgeYScreen.y -= _FoamThickness;

                half4 col = SAMPLE_TEXTURE2D(_CameraSortingLayerTexture, sampler_CameraSortingLayerTexture, reflectedUVOnScreen);
                    
                float screenEdgeFadeFactor = 0;
                if (reflectedUVOnScreen.y > 0.8) screenEdgeFadeFactor = smoothstep(0.8, 1.0, reflectedUVOnScreen.y);
                screenEdgeFadeFactor = 1 - screenEdgeFadeFactor;
                
                //return half4(screenEdgeFadeFactor.xxx, 1.0);
                
                float waterFadeFactor = 0;
                waterFadeFactor = smoothstep(1 - _FadeDistance, 1, IN.uv.y);
                
                //return half4(waterFadeFactor.xxx, 1);
                
                //half3 tinted = lerp(col.rgb, _WaterTint.rgb, 1 - _WaterTint.a * waterFadeFactor * screenEdgeFadeFactor);
                //half3 tinted = lerp(col.rgb, _WaterTint.rgb, _WaterTint.a);
                half3 tinted = lerp(half3(1,1,1), _WaterTint.rgb, _WaterTint.a);
                tinted = col.rgb * tinted;
                tinted = lerp(tinted, _DeepWaterTint, 1 - waterFadeFactor * screenEdgeFadeFactor);
                
                if (screenUV.y < foamHighEdgeYScreen.y && screenUV.y > foamLowEdgeYScreen.y) {
                    float gradient = (foamHighEdgeYScreen.y - screenUV.y) / (foamHighEdgeYScreen.y - foamLowEdgeYScreen.y);

                    float noise = saturate(1 - Worley(IN.positionWS.xy * _FoamBubbleScale, _Time.y * _FoamBubbleSpeed));
                    
                    float foamAmount = (1 - gradient) * lerp(1, noise, gradient);
                    
                    tinted = lerp(tinted, _FoamColor.rgb, foamAmount);
                }
                
                
                
                return half4(tinted, 1) * IN.color;
            }
            ENDHLSL
        }
    }
}