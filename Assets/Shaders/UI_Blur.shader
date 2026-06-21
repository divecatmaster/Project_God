Shader "Custom/UI/Blur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _Color ("Tint", Color) = (1, 1, 1, 1)
        _BlurSize ("Blur Size", Range(0, 10)) = 2

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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
            Name "UIBlur"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 _Color;
            float4 _ClipRect;

            float _BlurSize;

            v2f vert(appdata_t v)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.uv = v.texcoord;
                OUT.color = v.color * _Color;

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.uv;

                // BlurSize가 0이면 원본 그대로 반환
                if (_BlurSize <= 0.001)
                {
                    fixed4 original = tex2D(_MainTex, uv) * IN.color;

                    #ifdef UNITY_UI_CLIP_RECT
                    original.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                    #endif

                    return original;
                }

                float2 offset = _MainTex_TexelSize.xy * _BlurSize;

                fixed4 col = fixed4(0, 0, 0, 0);
                float weightSum = 0;

                #define ADD_SAMPLE(x, y, w) \
                    col += tex2D(_MainTex, uv + offset * float2(x, y)) * w; \
                    weightSum += w;

                ADD_SAMPLE(-2, -2, 0.04)
                ADD_SAMPLE(-1, -2, 0.06)
                ADD_SAMPLE( 0, -2, 0.08)
                ADD_SAMPLE( 1, -2, 0.06)
                ADD_SAMPLE( 2, -2, 0.04)

                ADD_SAMPLE(-2, -1, 0.06)
                ADD_SAMPLE(-1, -1, 0.08)
                ADD_SAMPLE( 0, -1, 0.10)
                ADD_SAMPLE( 1, -1, 0.08)
                ADD_SAMPLE( 2, -1, 0.06)

                ADD_SAMPLE(-2,  0, 0.08)
                ADD_SAMPLE(-1,  0, 0.10)
                ADD_SAMPLE( 0,  0, 0.12)
                ADD_SAMPLE( 1,  0, 0.10)
                ADD_SAMPLE( 2,  0, 0.08)

                ADD_SAMPLE(-2,  1, 0.06)
                ADD_SAMPLE(-1,  1, 0.08)
                ADD_SAMPLE( 0,  1, 0.10)
                ADD_SAMPLE( 1,  1, 0.08)
                ADD_SAMPLE( 2,  1, 0.06)

                ADD_SAMPLE(-2,  2, 0.04)
                ADD_SAMPLE(-1,  2, 0.06)
                ADD_SAMPLE( 0,  2, 0.08)
                ADD_SAMPLE( 1,  2, 0.06)
                ADD_SAMPLE( 2,  2, 0.04)

                #undef ADD_SAMPLE

                // 밝기 보정
                col /= weightSum;

                // UI Image Color와 Material Color 반영
                col *= IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                return col;
            }

            ENDCG
        }
    }
}