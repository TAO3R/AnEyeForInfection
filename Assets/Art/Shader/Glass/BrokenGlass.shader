Shader "Custom/BrokenScreen"
{
    Properties
    {
        [HideInInspector] _BlitTexture("Base (RGB)", 2D) = "white" {}

        _DirectionMap("Direction (RG)", 2D) = "bump" {}
        _DiffuseTex("Crack Texture", 2D) = "white" {}

        _Refraction("Refraction", Range(-1.0, 1.0)) = 0.2
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            Name "BrokenScreenPass"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            TEXTURE2D(_DirectionMap);
            SAMPLER(sampler_DirectionMap);

            TEXTURE2D(_DiffuseTex);
            SAMPLER(sampler_DiffuseTex);

            float _Refraction;

            Varyings Vert(Attributes input)
            {
                Varyings o;

                float2 pos;
                pos.x = (input.vertexID == 2) ? 3.0 : -1.0;
                pos.y = (input.vertexID == 1) ? 3.0 : -1.0;

                o.positionHCS = float4(pos, 0, 1);
                return o;
            }

            float4 Frag(Varyings i) : SV_Target
            {
                float2 uv = GetNormalizedScreenSpaceUV(i.positionHCS);

                //  Horizontal flip 
                uv.x = 1.0 - uv.x;

                float3 packed = SAMPLE_TEXTURE2D(_DirectionMap, sampler_DirectionMap, uv).xyz;

                if (packed.r < 0.001 && packed.g < 0.001 && packed.b < 0.001)
                    return float4(0, 0, 0, 1);

                float3 unpacked = UnpackNormal(float4(packed, 1.0));

                float2 directionColor = unpacked.xy * -1.0;

                float2 distortedUV = uv + directionColor * _Refraction;

                float3 screen = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, distortedUV).rgb;

                float3 cracks = SAMPLE_TEXTURE2D(_DiffuseTex, sampler_DiffuseTex, uv).rgb;

                float3 result = screen * cracks;

                return float4(result, 1.0);
            }

            ENDHLSL
        }
    }

    FallBack Off
}
