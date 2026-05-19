Shader "Custom/VFX/EnergyOrbGlow"
{
    Properties
    {
        [MainTexture] _MainTex ("Texture", 2D) = "white" {}
        [MainColor] _BaseColor ("Color", Color) = (0, 0.5, 1, 1)
        _Intensity ("Glow Intensity", Range(0, 10)) = 2.0
        _PulseSpeed ("Pulse Speed", Range(0, 10)) = 1.0
        _PulseAmount ("Pulse Amount", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardLit"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            CBUFFER_START(UnityPerMaterial)
            float4 _BaseColor;
            float _Intensity;
            float _PulseSpeed;
            float _PulseAmount;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseAmount;
                float3 positionOS = input.positionOS.xyz * pulse;
                
                output.positionCS = TransformObjectToHClip(positionOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half4 tex = tex2D(_MainTex, input.uv);
                half4 finalColor = tex * _BaseColor * _Intensity * input.color;
                return finalColor;
            }
            ENDHLSL
        }
    }
}