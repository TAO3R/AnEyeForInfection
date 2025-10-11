Shader "Unlit/CRT"
{
    Properties
    {
        _MainTex ("CRT Input (RenderTexture)", 2D) = "white" {}

        // === Effect amounts (defaults follow your macros) ===
        _AberrationAmount ("aberration_amount", Range(0,2)) = 0.7
        _NoiseAmount      ("noise_amount",      Range(0,1)) = 0.7
        _VignetteAmount   ("vignette_amount",   Range(0,2)) = 0.7
        _RoundedAmount    ("rounded_amount",    Range(0,2)) = 0.7

        _PixelateAmount   ("pixelate_amount",   Range(0,1)) = 0.7
        _MaskAmount       ("mask_amount",       Range(0,1)) = 0.7

        _BloomAmount      ("bloom_amount",      Range(0,1)) = 0.7
        _DistortAmount    ("distortion_amount", Range(0,1)) = 0.7

        // Moving scanline overlay
        _ScanlineIntensity ("Scanline Intensity", Range(0,0.2)) = 0.02
        _ScanlineSpeed     ("Scanline Speed",     Range(0,80))  = 29.0
        _ScanlineFrequency ("Scanline Frequency", Range(0,4))   = 1.0
    }

    SubShader
    {
        Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
        // Premultiplied alpha: color is already multiplied by alpha
        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "CRT_Custom_URP"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize; // (1/w,1/h,w,h)

            // Amounts
            float _AberrationAmount, _NoiseAmount, _VignetteAmount, _RoundedAmount;
            float _PixelateAmount, _MaskAmount;
            float _BloomAmount, _DistortAmount;

            // Scanline params
            float _ScanlineIntensity;
            float _ScanlineSpeed;
            float _ScanlineFrequency;

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_Position;
                float2 uv          : TEXCOORD0;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                VertexPositionInputs p = GetVertexPositionInputs(v.positionOS.xyz);
                o.positionHCS = p.positionCS;
                o.uv = v.uv;
                return o;
            }

            // ===== Helper functions (ports of your GLSL) =====
            float hash3(float3 p)
            {
                p = frac(p * 0.1031);
                p += dot(p, p.yzx + 19.19);
                return frac((p.x + p.y) * p.z);
            }

            float noise3(float3 x)
            {
                float3 p = floor(x);
                float3 f = frac(x);
                float3 m = f * f * (3.0 - 2.0 * f);
                float3 i = p + float3(1.0, 0.0, 0.0);

                float4 h;
                h.x = hash3(p);
                h.y = hash3(i);
                h.z = hash3(p + float3(0.0,1.0,0.0));
                h.w = hash3(i + float3(0.0,1.0,0.0));

                float a = lerp(h.x, h.y, m.x);
                float b = lerp(h.z, h.w, m.x);
                return lerp(a, b, m.y);
            }

            float grain3(float3 x)
            {
                return 0.5 + (4.0 * noise3(x) - noise3(x + 1.0) + noise3(x - 1.0)) / 4.0;
            }

            // 7x7 separable weights used in your bloom pass
            static const float kW[7] = { 0.25, 0.5, 1.0, 2.0, 1.0, 0.5, 0.25 };

            // 7x4 RGB sub-pixel mask (from your M array)
            float3 maskM(int idx)
            {
                // 28 entries = 4 rows x 7 cols
                // Row 0: X X X X X X X
                if (idx < 7) return float3(0,0,0);
                // Rows 1..3: [X, R, R, G, G, B, B]
                int col = idx % 7;
                if (col == 0) return float3(0,0,0);
                if (col == 1 || col == 2) return float3(1,0,0);
                if (col == 3 || col == 4) return float3(0,1,0);
                return float3(0,0,1); // col 5 or 6
            }

            // Barrel distortion
            float2 applyBarrel(float2 uv01, float k)
            {
                float2 uv = uv01 * 2.0 - 1.0;
                float r = length(uv);
                float eps = 1e-5;

                float denom = max(0.2 * k * r * r, eps);
                float2 uvDiv = uv / denom; // kept for parity, not directly used

                float inner = saturate(1.0 - 0.4 * k * r * r);
                float2 uvWarp = uv * (1.0 - sqrt(inner));
                float2 out01 = (uvWarp + 1.0) * 0.5;

                // Gentle blend for small k
                return lerp(uv01, out01, k);
            }

            float vignetteMul(float2 uv01, float amount, float2 texSize)
            {
                float2 centered = (uv01 - 0.5) * float2(1.0, texSize.y / texSize.x * 2.0);
                float v = smoothstep(0.25, 1.0, length(centered));
                return lerp(1.0, 1.0 - saturate(v), amount);
            }

            float roundedCornerMask(float2 fragPx, float2 resPx, float amount)
            {
                float radius = amount * ((resPx.x + resPx.y) * 0.5) * 0.06;
                float2 halfRes = resPx * 0.5;
                float2 d = abs(fragPx - halfRes) - (halfRes - radius);
                d = max(d, 0.0);
                float m = step(length(d) - radius, 0.0);
                return m;
            }

            // Moving scanline overlay (your scanline() port)
            float3 ApplyMovingScanline(float2 uv01, float2 texSize, float3 rgb)
            {
                // Use distorted UV → convert to pixel Y, scale by frequency
                float screenY = uv01.y * texSize.y * _ScanlineFrequency;
                float s = sin(screenY + _Time.y * _ScanlineSpeed);
                rgb -= s * _ScanlineIntensity;
                return rgb;
            }

            float4 frag(Varyings i) : SV_Target
            {
                float2 texSize = _MainTex_TexelSize.zw; // (w,h)
                float2 fragPx  = i.uv * texSize;        // pixel coords
                float2 uv0     = i.uv;

                // 1) Barrel distortion
                float2 uv = applyBarrel(uv0, saturate(_DistortAmount));
                if (uv.x <= 0.0 || uv.x >= 1.0 || uv.y <= 0.0 || uv.y >= 1.0)
                    return float4(0,0,0,0);

                // Base sample at distorted UV
                float4 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // 2) 7x4 pixelation (on pixel grid similar to your snippet)
                float2 uvBucket = floor((fragPx / texSize) * (texSize / float2(7.0, 4.0)));
                float hex_offset = fmod(uvBucket.x, 2.0) * 2.0;
                float yHalfSel   = floor(fmod(fragPx.y, 4.0) / 2.0);
                float yHex       = yHalfSel * hex_offset * 0.5;

                float3 pixSum = 0;
                [unroll] for (int y = 0; y < 4; ++y)
                {
                    [unroll] for (int x = 0; x < 7; ++x)
                    {
                        float2 sp = (uvBucket * float2(7.0, 4.0) + float2(x, y)) / texSize;
                        pixSum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, sp).rgb;
                    }
                }
                float3 pixAvg = pixSum / 28.0;
                float3 pixMixed = lerp(baseCol.rgb, pixAvg, saturate(_PixelateAmount));

                // 3) RGB sub-pixel mask
                int ix7 = (int)fmod(fragPx.x, 7.0);
                int iy4 = (int)fmod(fragPx.y + hex_offset, 4.0);
                int idx = iy4 * 7 + ix7;
                float3 triad = maskM(idx);
                float3 masked = lerp(pixMixed, pixMixed * triad, saturate(_MaskAmount));

                // 4) Aberration + RGB grain (distorted UV)
                float2 center = float2(0.5, 0.5);
                float2 aber_dis = (uv - center) * _AberrationAmount * length(uv - center) * 0.05;

                float r = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).r;
                float g = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - aber_dis).g;
                float b = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - 2.0 * aber_dis).b;
                float3 aber = float3(r, g, b);

                float frame = floor(_Time.y * 60.0);
                float3 rgb_grain = float3(
                    grain3(float3(fragPx, frame)),
                    grain3(float3(fragPx, frame + 9.0)),
                    grain3(float3(fragPx, frame - 9.0))
                );
                float3 grainMix = lerp(aber, lerp(aber * rgb_grain, aber + (rgb_grain - 1.0), 0.5), saturate(_NoiseAmount));

                // Blend masked (pixel/mask) with aberration/grain
                float3 combined = lerp(masked, grainMix, 0.5);

                // 5) Vignette
                combined *= vignetteMul(uv, _VignetteAmount, texSize);

                // 6) Bloom (7x7 weighted blur) at distorted uv
                float3 bloomSum = 0;
                [unroll] for (int dx = -3; dx <= 3; ++dx)
                {
                    [unroll] for (int dy = -3; dy <= 3; ++dy)
                    {
                        float2 ofs = float2(dx, dy) * _MainTex_TexelSize.xy;
                        float w = kW[dx + 3] * kW[dy + 3];
                        bloomSum += w * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + ofs).rgb;
                    }
                }
                float3 bloomCol = bloomSum / 7.0;
                float3 postBloom = lerp(combined, bloomCol, saturate(_BloomAmount * _MaskAmount));

                // 7) Moving scanline overlay (on top of image content, before bezel masks)
                postBloom = ApplyMovingScanline(uv, texSize, postBloom);
                postBloom = saturate(postBloom);

                // === Premultiplied alpha output for transparent rounded edges ===
                // 8) Rounded corners + AA-aware barrel edge mask -> alpha
                float rc = roundedCornerMask(fragPx, texSize, _RoundedAmount);
                float v  = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                float2 gv = float2(ddx(v), ddy(v));
                float AA  = 2.0 * length(gv);
                float edge = smoothstep(-AA, AA, v);

                float alpha = rc * edge;          // 0 outside, smooth border inside
                float3 rgb  = postBloom * alpha;  // premultiply color

                return float4(rgb, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
