Shader "Custom/CellShadedWall"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _ToonSteps ("Toon Steps", Range(1,10)) = 4
        _UpNormalThreshold ("Upward Normal Threshold", Range(0, 1)) = 0.9
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX
            #pragma multi_compile _ _ADDITIONAL_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
            };

            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            float4 _Color;
            float _ToonSteps;
            float _UpNormalThreshold;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.worldPos = TransformObjectToWorld(IN.positionOS);
                OUT.shadowCoord = TransformWorldToShadowCoord(OUT.worldPos);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float3 normal = normalize(IN.normalWS);

                // Upward facing = pitch black
                if (normal.y > _UpNormalThreshold)
                {
                    return float4(0, 0, 0, 1);
                }

                // Sample shadow map
                float shadowAtten = MainLightRealtimeShadow(IN.shadowCoord);

                // Get light info
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float NdotL = max(0, dot(normal, -lightDir)); // Light comes FROM direction

                // Apply toon steps
                float stepped = floor(NdotL * _ToonSteps) / (_ToonSteps - 1);

                float3 albedo = tex2D(_BaseMap, IN.uv).rgb * _Color.rgb;

                // Final color with shadow
                float3 litColor = albedo * stepped * mainLight.color * shadowAtten;

                return float4(litColor, 1);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}