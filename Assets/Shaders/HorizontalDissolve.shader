Shader "Custom/UI/HorizontalDissolve"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        _FadeProgress ("Fade Progress", Range(0, 1)) = 0.0
        _Feather ("Feather Amount", Range(0, 1)) = 0.1
        [Toggle(REVERSE_DIRECTION)] _ReverseDirection ("Reverse (Bottom to Top)", Float) = 0
        
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        
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

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma multi_compile_local _ REVERSE_DIRECTION

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float4 positionUI   : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _FadeProgress;
            float _Feather;
            float4 _ClipRect;

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.positionUI = input.positionOS;
                output.color = input.color * _Color;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 color = tex2D(_MainTex, input.uv) * input.color;

                // Vertical Fade Logic
                // uv.y: 0 (bottom) to 1 (top)
                // Top-to-Bottom: Progress 0 (all visible) to 1 (all gone)
                // At progress 0.5, top half is gone.
                // This means the visible range is from 0 to (1.0 - progress)
                
                float verticalPos = input.uv.x;
                
                #if REVERSE_DIRECTION
                    // Bottom to Top: Progress 0 (visible) to 1 (gone)
                    // Visible range from (progress) to 1.0
                    float cutoff = _FadeProgress;
                    float alpha_mult = saturate((verticalPos - cutoff) / max(_Feather, 0.0001));
                #else
                    // Top to Bottom:
                    float cutoff = 1.0 - _FadeProgress;
                    // If verticalPos < cutoff, it's visible. 
                    // To get a soft edge at the cutoff:
                    float alpha_mult = saturate((cutoff - verticalPos) / max(_Feather, 0.0001));
                #endif

                color.a *= alpha_mult;

                #ifdef UNITY_UI_ALPHACLIP
                    clip(color.a - 0.001);
                #endif

                return color;
            }
        ENDHLSL
        }
    }
}