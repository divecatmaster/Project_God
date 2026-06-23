Shader "Custom/UIGlitchShader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _GlitchIntensity ("Glitch Intensity", Range(0, 1)) = 0
        _BlockScale ("Block Scale", Float) = 15.0
        _NoiseSpeed ("Noise Speed", Float) = 10.0
        _RGBSplit ("RGB Split Offset", Range(0, 0.1)) = 0.01
        _HorizontalDistortion ("Horizontal Distortion", Range(0, 0.2)) = 0.05
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.1
        _ScanlineCount ("Scanline Count", Float) = 500

        // Individual weights to control glitch types
        _BlockWeight ("Block Weight", Range(0, 1)) = 1.0
        _RGBSplitWeight ("RGB Split Weight", Range(0, 1)) = 1.0
        _ScanlineWeight ("Scanline Weight", Range(0, 1)) = 1.0
        _NoiseWeight ("Noise Weight", Range(0, 1)) = 1.0
        _ColorSpikeWeight ("Color Spike Weight", Range(0, 1)) = 1.0
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

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "Default"
        HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

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

            float _GlitchIntensity;
            float _BlockScale;
            float _NoiseSpeed;
            float _RGBSplit;
            float _HorizontalDistortion;
            float _ScanlineIntensity;
            float _ScanlineCount;

            // Weight variables
            float _BlockWeight;
            float _RGBSplitWeight;
            float _ScanlineWeight;
            float _NoiseWeight;
            float _ColorSpikeWeight;

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

            // Pseudo-random number generator
            float rand(float2 co)
            {
                return frac(sin(dot(co.xy ,float2(12.9898,78.233))) * 43758.5453);
            }

            float4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float t = _Time.y * _NoiseSpeed;

                // 1. Digital Block Glitch
                float blockY = floor(uv.y * _BlockScale) / _BlockScale;
                float blockRand = rand(float2(blockY, floor(t)));
                
                float shiftThreshold = 1.0 - _GlitchIntensity;
                float horizontalShift = 0.0;
                if (blockRand > shiftThreshold)
                {
                    horizontalShift = (rand(float2(blockY, floor(t) + 12.34)) - 0.5) * _HorizontalDistortion * _GlitchIntensity * _BlockWeight;
                }

                // Apply horizontal shift to UV
                float2 uvShifted = uv;
                uvShifted.x += horizontalShift;
                uvShifted.x = frac(uvShifted.x);

                // 2. RGB Split (Chromatic Aberration) using shifted UVs
                float splitOffset = _RGBSplit * _GlitchIntensity * _RGBSplitWeight;
                
                float r = tex2D(_MainTex, uvShifted + float2(splitOffset, 0.0)).r;
                float g = tex2D(_MainTex, uvShifted).g;
                float b = tex2D(_MainTex, uvShifted - float2(splitOffset, 0.0)).b;
                float a = tex2D(_MainTex, uvShifted).a;

                float4 col = float4(r, g, b, a);

                // 3. Scanline effect
                float scanline = sin(uv.y * _ScanlineCount) * _ScanlineIntensity * _GlitchIntensity * _ScanlineWeight;
                col.rgb -= scanline;

                // 4. Random noise overlay
                float noise = (rand(uvShifted + t) - 0.5) * 0.15 * _GlitchIntensity * _NoiseWeight;
                col.rgb += noise;

                // 5. Invert and tint spikes
                float colorSpikeRand = rand(float2(floor(t * 1.5), 98.76));
                if (colorSpikeRand > (1.0 - 0.08 * _GlitchIntensity * _ColorSpikeWeight))
                {
                    // Color invert
                    col.rgb = 1.0 - col.rgb;
                }
                else if (colorSpikeRand < (0.05 * _GlitchIntensity * _ColorSpikeWeight))
                {
                    // Neon cyan tint
                    col.rgb = col.rgb * float3(0.0, 1.0, 1.0) + float3(0.0, 0.2, 0.2);
                }

                // UI Clipping support
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                #ifdef UNITY_UI_ALPHACLIP
                clip (col.a - 0.001);
                #endif

                return col * IN.color;
            }
        ENDHLSL
        }
    }
}
