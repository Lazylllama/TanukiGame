Shader "Custom/ScratchBrush"
{
    Properties
    {
        [MainTexture] _MainTex("Sprite Texture", 2D) = "white" {}
        _PrevBrushUV("Previous Brush UV", Vector) = (-1, -1, 0, 0)
        _BrushUV("Brush UV", Vector) = (-1, -1, 0, 0)
        _BrushRadius("Brush Radius", float) = 0.05
        _Aspect("Aspect ratio", float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            BlendOp Max
            blend One One
            Cull Off
            ZTest Always
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
                float4 _MainTex_ST;
                float4 _BrushUV;
                float4 _PrevBrushUV;
                float  _BrushRadius;
                float  _Aspect;
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
                float2 uvWithAspect = float2(IN.uv.x * _Aspect, IN.uv.y);
                float2 start = float2(_BrushUV.x * _Aspect, _BrushUV.y);
                float2 end = float2(_PrevBrushUV.x * _Aspect, _PrevBrushUV.y);
                float distAlongLine = dot(uvWithAspect - start, end - start) / dot(end - start, end - start);
                distAlongLine = clamp(distAlongLine, 0, 1);
                float2 pointOnLine = start + distAlongLine * (end - start);
                
                //float lit = step(distance(pointOnLine, uvWithAspect), _BrushRadius);
                float lit = 1 - smoothstep(_BrushRadius, _BrushRadius + 0.02, distance(pointOnLine, uvWithAspect));
                
                return half4(lit.xxxx);
            }
            ENDHLSL
        }
    }
}