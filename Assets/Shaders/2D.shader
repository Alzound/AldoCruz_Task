Shader "URP 2D/Simple Sprite Unlit (Offset + AntiBleed)"
{
    Properties
    {
        [PerRendererData][MainTexture] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [PerRendererData] _RendererColor ("Renderer Color", Color) = (1,1,1,1)
        [Toggle(PIXELSNAP_ON)] _PixelSnap ("Pixel Snap", Float) = 0

        // Margen para evitar samplear exactamente en el borde (clamp reducido)
        _EdgeEpsilon ("Edge Epsilon", Range(0, 0.01)) = 0.001
    }

    SubShader
    {
        Tags{
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
            "PreviewType"="Plane"
            "UniversalMaterialType"="Unlit"
        }

        Cull Off
        ZWrite Off
        // Blend pensado para color premultiplicado por alpha
        Blend One OneMinusSrcAlpha

        Pass
        {
            Name "SpriteUnlit"
            Tags{ "LightMode"="Universal2D" }

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float4 _MainTex_ST;   // (tiling.xy, offset.xy)
                float _EdgeEpsilon;   // margen anti-bleed
            CBUFFER_END

            // PerRendererData (por renderizador; no entra al CBUFFER)
            float4 _RendererColor;

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float4 PixelSnapHClip(float4 hpos)
            {
            #if defined(PIXELSNAP_ON)
                float2 pixelSize = 1.0 / _ScreenParams.xy;
                hpos.xy = floor(hpos.xy / hpos.w / pixelSize + 0.5) * pixelSize * hpos.w;
            #endif
                return hpos;
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                float4 hpos = TransformObjectToHClip(float4(IN.positionOS, 1));
                OUT.positionHCS = PixelSnapHClip(hpos);

                OUT.uv = IN.uv * _MainTex_ST.xy + _MainTex_ST.zw; // tiling + offset
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // ---- UV wrap + clamp reducido (anti-bleed) ----
                float2 uv = frac(IN.uv);                   // wrap 0..1
                float e = _EdgeEpsilon;
                uv = uv * (1.0 - 2.0 * e) + e;            // clamp(uv, e, 1-e)

                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // ---- Premultiplica por alpha (reduce halos) ----
                tex.rgb *= tex.a;

                // Tint final
                half4 tint = _Color * _RendererColor * IN.color;
                half4 outCol = tex * tint;

                // Asegura rangos seguros (equivalente a clamp/saturate en color)
                outCol = saturate(outCol);
                return outCol;
            }
            ENDHLSL
        }
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}
