// Production-ready Blur shader for TextMeshPro UGUI
// Based on TMP_SDF-Mobile but with multi-tap sampling for soft blur
Shader "TextMeshPro/Mobile/Distance Field Blur" {

Properties {
    _FaceColor          ("Face Color", Color) = (1,1,1,1)
    _FaceDilate         ("Face Dilate", Range(-1,1)) = 0
    _FaceSoftness       ("Face Softness", Range(0,1)) = 0

    _OutlineColor       ("Outline Color", Color) = (0,0,0,1)
    _OutlineWidth       ("Outline Thickness", Range(0,1)) = 0
    _OutlineSoftness    ("Outline Softness", Range(0,1)) = 0

    _UnderlayColor      ("Border Color", Color) = (0,0,0,.5)
    _UnderlayOffsetX    ("Border OffsetX", Range(-1,1)) = 0
    _UnderlayOffsetY    ("Border OffsetY", Range(-1,1)) = 0
    _UnderlayDilate     ("Border Dilate", Range(-1,1)) = 0
    _UnderlaySoftness   ("Border Softness", Range(0,1)) = 0

    _WeightNormal       ("Weight Normal", float) = 0
    _WeightBold         ("Weight Bold", float) = .5

    _ShaderFlags        ("Flags", float) = 0
    _ScaleRatioA        ("Scale RatioA", float) = 1
    _ScaleRatioB        ("Scale RatioB", float) = 1
    _ScaleRatioC        ("Scale RatioC", float) = 1

    _MainTex            ("Font Atlas", 2D) = "white" {}
    _TextureWidth       ("Texture Width", float) = 512
    _TextureHeight      ("Texture Height", float) = 512
    _GradientScale      ("Gradient Scale", float) = 5
    _ScaleX             ("Scale X", float) = 1
    _ScaleY             ("Scale Y", float) = 1
    _PerspectiveFilter  ("Perspective Correction", Range(0, 1)) = 0.875
    _Sharpness          ("Sharpness", Range(-1,1)) = 0

    _VertexOffsetX      ("Vertex OffsetX", float) = 0
    _VertexOffsetY      ("Vertex OffsetY", float) = 0

    _ClipRect           ("Clip Rect", vector) = (-32767, -32767, 32767, 32767)
    _MaskSoftnessX      ("Mask SoftnessX", float) = 0
    _MaskSoftnessY      ("Mask SoftnessY", float) = 0

    // Blur Specific Properties
    [Header(Blur Settings)]
    _BlurStrength       ("Blur Strength", Range(0, 20)) = 0
    _BlurSoftness       ("Blur Softness", Range(0, 1)) = 0
    _BlurIntensity      ("Blur Intensity", Range(0, 2)) = 1
    [Toggle(BLUR_GLOW_ON)] _BlurGlowMode ("Glow Mode", Float) = 0

    _StencilComp        ("Stencil Comparison", Float) = 8
    _Stencil            ("Stencil ID", Float) = 0
    _StencilOp          ("Stencil Operation", Float) = 0
    _StencilWriteMask   ("Stencil Write Mask", Float) = 255
    _StencilReadMask    ("Stencil Read Mask", Float) = 255

    _CullMode           ("Cull Mode", Float) = 0
    _ColorMask          ("Color Mask", Float) = 15
}

SubShader {
    Tags
    {
        "Queue"="Transparent"
        "IgnoreProjector"="True"
        "RenderType"="Transparent"
    }

    Stencil
    {
        Ref [_Stencil]
        Comp [_StencilComp]
        Pass [_StencilOp]
        ReadMask [_StencilReadMask]
        WriteMask [_StencilWriteMask]
    }

    Cull [_CullMode]
    ZWrite Off
    Lighting Off
    Fog { Mode Off }
    ZTest [unity_GUIZTestMode]
    Blend One OneMinusSrcAlpha
    ColorMask [_ColorMask]

    Pass {
        CGPROGRAM
        #pragma vertex VertShader
        #pragma fragment PixShader
        #pragma shader_feature __ OUTLINE_ON
        #pragma shader_feature __ UNDERLAY_ON UNDERLAY_INNER
        #pragma shader_feature __ BLUR_GLOW_ON

        #pragma multi_compile __ UNITY_UI_CLIP_RECT
        #pragma multi_compile __ UNITY_UI_ALPHACLIP

        #include "UnityCG.cginc"
        #include "UnityUI.cginc"
        #include "../../TextMesh Pro/Shaders/TMPro_Properties.cginc"

        // Explicitly declare variables that might be missing in some environments
        float _UIMaskSoftnessX;
        float _UIMaskSoftnessY;
        int _UIVertexColorAlwaysGammaSpace;
        
        float _FaceSoftness;
        float _BlurStrength;
        float _BlurSoftness;
        float _BlurIntensity;

        struct vertex_t {
            UNITY_VERTEX_INPUT_INSTANCE_ID
            float4  vertex          : POSITION;
            float3  normal          : NORMAL;
            fixed4  color           : COLOR;
            float4  texcoord0       : TEXCOORD0;
            float2  texcoord1       : TEXCOORD1;
        };

        struct pixel_t {
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
            float4  vertex          : SV_POSITION;
            fixed4  faceColor       : COLOR;
            fixed4  outlineColor    : COLOR1;
            float4  texcoord0       : TEXCOORD0;            // Texture UV, Mask UV
            half4   param           : TEXCOORD1;            // Scale(x), BiasIn(y), BiasOut(z), Bias(w)
            half4   mask            : TEXCOORD2;            // Position in clip space(xy), Softness(zw)
            #if (UNDERLAY_ON | UNDERLAY_INNER)
            float4  texcoord1       : TEXCOORD3;            // Texture UV, alpha, reserved
            half2   underlayParam   : TEXCOORD4;            // Scale(x), Bias(y)
            #endif
        };

        pixel_t VertShader(vertex_t input)
        {
            pixel_t output;

            UNITY_INITIALIZE_OUTPUT(pixel_t, output);
            UNITY_SETUP_INSTANCE_ID(input);
            UNITY_TRANSFER_INSTANCE_ID(input, output);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            float bold = step(input.texcoord0.w, 0);

            float4 vert = input.vertex;
            vert.x += _VertexOffsetX;
            vert.y += _VertexOffsetY;
            float4 vPosition = UnityObjectToClipPos(vert);

            float2 pixelSize = vPosition.w;
            pixelSize /= float2(_ScaleX, _ScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

            float scale = rsqrt(dot(pixelSize, pixelSize));
            scale *= abs(input.texcoord0.w) * _GradientScale * (_Sharpness + 1);
            if(UNITY_MATRIX_P[3][3] == 0) scale = lerp(abs(scale) * (1 - _PerspectiveFilter), scale, abs(dot(UnityObjectToWorldNormal(input.normal.xyz), normalize(WorldSpaceViewDir(vert)))));

            float weight = lerp(_WeightNormal, _WeightBold, bold) / 4.0;
            weight = (weight + _FaceDilate) * _ScaleRatioA * 0.5;

            float layerScale = scale;

            scale /= 1 + ((_OutlineSoftness + _FaceSoftness) * _ScaleRatioA * scale);
            float bias = (0.5 - weight) * scale - 0.5;
            float outline = _OutlineWidth * _ScaleRatioA * 0.5 * scale;

            if (_UIVertexColorAlwaysGammaSpace && !IsGammaSpace())
            {
                input.color.rgb = UIGammaToLinear(input.color.rgb);
            }
            float opacity = input.color.a;
            #if (UNDERLAY_ON | UNDERLAY_INNER)
            opacity = 1.0;
            #endif

            fixed4 faceColor = fixed4(input.color.rgb, opacity) * _FaceColor;
            faceColor.rgb *= faceColor.a;

            fixed4 outlineColor = _OutlineColor;
            outlineColor.a *= opacity;
            outlineColor.rgb *= outlineColor.a;
            outlineColor = lerp(faceColor, outlineColor, sqrt(min(1.0, (outline * 2))));

            #if (UNDERLAY_ON | UNDERLAY_INNER)
            layerScale /= 1 + ((_UnderlaySoftness * _ScaleRatioC) * layerScale);
            float layerBias = (.5 - weight) * layerScale - .5 - ((_UnderlayDilate * _ScaleRatioC) * .5 * layerScale);

            float x = -(_UnderlayOffsetX * _ScaleRatioC) * _GradientScale / _TextureWidth;
            float y = -(_UnderlayOffsetY * _ScaleRatioC) * _GradientScale / _TextureHeight;
            float2 layerOffset = float2(x, y);
            #endif

            // Generate UV for the Masking Texture
            float4 clampedRect = clamp(_ClipRect, -2e10, 2e10);
            float2 maskUV = (vert.xy - clampedRect.xy) / (clampedRect.zw - clampedRect.xy);

            // Populate structure for pixel shader
            output.vertex = vPosition;
            output.faceColor = faceColor;
            output.outlineColor = outlineColor;
            output.texcoord0 = float4(input.texcoord0.x, input.texcoord0.y, maskUV.x, maskUV.y);
            output.param = half4(scale, bias - outline, bias + outline, bias);

            const half2 maskSoftness = half2(max(_UIMaskSoftnessX, _MaskSoftnessX), max(_UIMaskSoftnessY, _MaskSoftnessY));
            output.mask = half4(vert.xy * 2 - clampedRect.xy - clampedRect.zw, 0.25 / (0.25 * maskSoftness + pixelSize.xy));
            #if (UNDERLAY_ON || UNDERLAY_INNER)
            output.texcoord1 = float4(input.texcoord0.xy + layerOffset, input.color.a, 0);
            output.underlayParam = half2(layerScale, layerBias);
            #endif

            return output;
        }


        // PIXEL SHADER
        fixed4 PixShader(pixel_t input) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(input);

            // 5-tap Multi-sample Blur Logic
            float2 texelSize = float2(1.0 / _TextureWidth, 1.0 / _TextureHeight);
            float2 blurOffset = _BlurStrength * texelSize;

            half d = tex2D(_MainTex, input.texcoord0.xy).a;
            
            // Branchless-ish multi-tap sampling
            // Using a threshold to skip if blur is nearly zero
            if (_BlurStrength > 0.05) {
                d += tex2D(_MainTex, input.texcoord0.xy + float2(blurOffset.x, blurOffset.y)).a;
                d += tex2D(_MainTex, input.texcoord0.xy + float2(-blurOffset.x, -blurOffset.y)).a;
                d += tex2D(_MainTex, input.texcoord0.xy + float2(blurOffset.x, -blurOffset.y)).a;
                d += tex2D(_MainTex, input.texcoord0.xy + float2(-blurOffset.x, blurOffset.y)).a;
                d *= 0.2;
            }

            // Apply additional softness factor for the "blur" feel
            float currentScale = input.param.x;
            if (_BlurSoftness > 0) {
                currentScale /= (1.0 + _BlurSoftness * 5.0);
            }

            half dist = d * currentScale;
            
            // Standard TMP coloring logic
            half4 c = input.faceColor * saturate(dist - input.param.w);

            #ifdef OUTLINE_ON
            c = lerp(input.outlineColor, input.faceColor, saturate(dist - input.param.z));
            c *= saturate(dist - input.param.y);
            #endif

            #if UNDERLAY_ON
            half ud = tex2D(_MainTex, input.texcoord1.xy).a * input.underlayParam.x;
            c += float4(_UnderlayColor.rgb * _UnderlayColor.a, _UnderlayColor.a) * saturate(ud - input.underlayParam.y) * (1 - c.a);
            #endif

            #if UNDERLAY_INNER
            half sud = saturate(dist - input.param.z);
            half ud2 = tex2D(_MainTex, input.texcoord1.xy).a * input.underlayParam.x;
            c += float4(_UnderlayColor.rgb * _UnderlayColor.a, _UnderlayColor.a) * (1 - saturate(ud2 - input.underlayParam.y)) * sud * (1 - c.a);
            #endif

            // Glow Mode: Intensify color
            #ifdef BLUR_GLOW_ON
            c.rgb *= _BlurIntensity;
            #endif

            // Clipping support
            #if UNITY_UI_CLIP_RECT
            half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(input.mask.xy)) * input.mask.zw);
            c *= m.x * m.y;
            #endif

            #if (UNDERLAY_ON | UNDERLAY_INNER)
            c *= input.texcoord1.z;
            #endif

            #if UNITY_UI_ALPHACLIP
            clip(c.a - 0.001);
            #endif

            return c;
        }
        ENDCG
    }
}

CustomEditor "TMPro.EditorUtilities.TMP_SDFShaderGUI"
}
