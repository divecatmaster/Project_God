Shader "Custom/UIOldFilmShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Master Controls)]
        _MasterIntensity ("Master Intensity", Range(0, 1)) = 1.0

        [Header(Film Grain)]
        _GrainIntensity ("Grain Intensity", Range(0, 1)) = 0.2
        _GrainSpeed ("Grain Speed", Float) = 24.0

        [Header(Scratches)]
        _ScratchIntensity ("Scratch Intensity", Range(0, 1)) = 0.35
        _ScratchSpeed ("Scratch Speed", Float) = 12.0
        _ScratchWidth ("Scratch Width", Float) = 0.0015

        [Header(Dust and Specks)]
        _DustDensity ("Dust Density", Range(0, 1)) = 0.25

        [Header(Vignette)]
        _VignetteIntensity ("Vignette Intensity", Range(0, 1)) = 0.5
        _VignetteSmoothness ("Vignette Smoothness", Range(0, 1)) = 0.5

        [Header(Projector Flicker)]
        _FlickerIntensity ("Flicker Intensity", Range(0, 1)) = 0.12
        _FlickerSpeed ("Flicker Speed", Float) = 18.0

        [Header(Frame Jitter)]
        _JitterIntensity ("Jitter Intensity", Range(0, 1)) = 0.03

        // UI Masking / Stencil Properties
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local __ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local __ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            float _MasterIntensity;
            float _GrainIntensity;
            float _GrainSpeed;
            float _ScratchIntensity;
            float _ScratchSpeed;
            float _ScratchWidth;
            float _DustDensity;
            float _VignetteIntensity;
            float _VignetteSmoothness;
            float _FlickerIntensity;
            float _FlickerSpeed;
            float _JitterIntensity;

            // Pseudo-random noise functions
            float hash11(float p)
            {
                p = frac(p * 0.1031);
                p *= p + 33.33;
                p *= p + p;
                return frac(p);
            }

            float hash21(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;

                // Frame Jitter (subtle vertical/horizontal shift over quantized time)
                float jitterTime = floor(_Time.y * 20.0);
                float2 jitterOffset = (float2(hash11(jitterTime * 1.1) - 0.5, hash11(jitterTime * 2.3) - 0.5)) * _JitterIntensity * 0.008 * _MasterIntensity;
                float2 jitteredUV = uv + jitterOffset;

                // 1. Vignette (Dark edges)
                float2 centeredUV = (jitteredUV - 0.5) * 2.0;
                float dist = length(centeredUV);
                float vigFactor = smoothstep(0.4, 1.2 - (1.0 - _VignetteSmoothness) * 0.5, dist);
                float vignetteAlpha = vigFactor * _VignetteIntensity * _MasterIntensity;

                // 2. Film Grain (High-frequency noise)
                float grainTime = floor(_Time.y * _GrainSpeed);
                float grainNoise = hash21(jitteredUV * 800.0 + grainTime * 17.3);
                // Centered grain noise around dark/light
                float grainAlpha = (grainNoise - 0.5) * _GrainIntensity * _MasterIntensity;

                // 3. Vertical Scratches
                float scratchTime = floor(_Time.y * _ScratchSpeed);
                float scratchAlpha = 0.0;
                // Generate up to 3 scratch lines
                for (int i = 0; i < 3; i++)
                {
                    float scratchSeed = scratchTime * 3.7 + float(i) * 12.81;
                    float activeScratch = step(0.55, hash11(scratchSeed));
                    if (activeScratch > 0.0)
                    {
                        float scratchX = hash11(scratchSeed * 1.9);
                        float lineDist = abs(jitteredUV.x - scratchX);
                        float lineShape = smoothstep(_ScratchWidth, 0.0, lineDist);
                        
                        // Vary vertical continuity
                        float scratchYNoise = hash11(floor(jitteredUV.y * 30.0) + scratchSeed);
                        float verticalCut = step(0.15, scratchYNoise);

                        scratchAlpha += lineShape * verticalCut * (0.6 + 0.4 * hash11(scratchSeed * 4.3));
                    }
                }
                scratchAlpha = saturate(scratchAlpha) * _ScratchIntensity * _MasterIntensity;

                // 4. Dust and Specks
                float dustTime = floor(_Time.y * 14.0);
                float dustNoise = hash21(floor(jitteredUV * float2(60.0, 40.0)) + dustTime * 11.2);
                float dustMask = step(1.0 - _DustDensity * 0.02 * _MasterIntensity, dustNoise);
                float dustAlpha = dustMask * 0.7 * _MasterIntensity;

                // 5. Projector Flicker (Brightness modulation)
                float flickerTime = floor(_Time.y * _FlickerSpeed);
                float flickerVal = (hash11(flickerTime * 7.1) - 0.5) * _FlickerIntensity * _MasterIntensity;

                // Composite dark overlay and light artifacts
                // Base overlay starts black for Vignette, Scratches, and Flicker
                fixed3 overlayColor = fixed3(0, 0, 0);

                // Combine alpha elements:
                // Vignette darkens edges (+)
                // Scratches dark/light (-)
                // Flicker darkens/brightens whole screen
                float totalDarkAlpha = saturate(vignetteAlpha + dustAlpha + scratchAlpha * 0.7 + max(0.0, -flickerVal));
                
                // Add light grain/scratch overlay if grainNoise > 0.5 or positive flicker
                float lightGrain = max(0.0, grainAlpha) + scratchAlpha * 0.3 + max(0.0, flickerVal);
                
                // Combine into net alpha and color
                fixed4 col;
                if (grainAlpha < 0.0)
                {
                    totalDarkAlpha += abs(grainAlpha);
                }

                col.rgb = overlayColor + fixed3(lightGrain, lightGrain, lightGrain);
                col.a = saturate(totalDarkAlpha + lightGrain) * IN.color.a;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}
