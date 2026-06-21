Shader "Custom/UI/EyeBlink"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _Color ("Tint", Color) = (0, 0, 0, 1)

        _BlinkProgress ("Blink Progress", Range(0, 1)) = 0
        _Feather ("Feather", Range(0.001, 0.2)) = 0.04
        _OvalPower ("Oval Power", Range(0.1, 5)) = 1.8
        _EyeWidth ("Eye Width", Range(0.6, 1.5)) = 1.05

        _TopOffset ("Top Offset", Range(-1, 1)) = 0
        _BottomOffset ("Bottom Offset", Range(-1, 1)) = 0

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
            Name "Default"

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
            fixed4 _Color;
            float4 _ClipRect;

            float _BlinkProgress;
            float _Feather;
            float _OvalPower;
            float _EyeWidth;
            float _TopOffset;
            float _BottomOffset;

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

                // 중심 기준 좌표
                float2 centered = uv - 0.5;

                // 가로 방향 정규화
                // x = 0 일 때 중심, x = 1 일 때 눈의 양 끝
                float x = abs(centered.x) / (0.5 * _EyeWidth);
                x = saturate(x);

                // 타원형 눈 곡선
                // 중심 = 1, 양 끝 = 0
                float eyeCurve = sqrt(saturate(1.0 - x * x));

                // 곡선 형태 조절
                // 1.0 = 기본 타원
                // 높을수록 더 날카로운 눈매
                eyeCurve = pow(eyeCurve, _OvalPower);

                // progress 0 = 완전히 열림
                // progress 1 = 완전히 닫힘
                float halfOpen = lerp(0.52, 0.0, _BlinkProgress);

                // x 위치마다 다른 개방 높이
                float localHalfOpen = halfOpen * eyeCurve;

                float topLimit = 0.5 + localHalfOpen + _TopOffset;
                float bottomLimit = 0.5 - localHalfOpen + _BottomOffset;

                // 위쪽 검은 영역
                float topAlpha = smoothstep(topLimit - _Feather, topLimit + _Feather, uv.y);

                // 아래쪽 검은 영역
                float bottomAlpha = 1.0 - smoothstep(bottomLimit - _Feather, bottomLimit + _Feather, uv.y);

                float alpha = saturate(topAlpha + bottomAlpha);

                fixed4 col = IN.color;
                col.a *= alpha;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                return col;
            }
            ENDCG
        }
    }
}