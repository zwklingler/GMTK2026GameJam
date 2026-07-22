Shader "Custom/URPToonWithOutline"
{
    Properties
    {
        [Header(Base Settings)]
        _MainTex ("Base Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1, 1, 1, 1)

        [Header(Toon Shading)]
        _ShadowColor ("Shadow Color", Color) = (0.2, 0.2, 0.3, 1)
        _StepSmoothness ("Step Smoothness", Range(0.0, 0.1)) = 0.01
        _Shades ("Shades", Range(2, 10)) = 3
        _MainLightStrength ("Main (Directional) Light Strength", Range(0.0, 1.0)) = 0.0
        _AmbientStrength ("Ambient Fill", Range(0.0, 1.0)) = 0.15
        

        [Header(Cross Hatching)]
        [Toggle(_CROSSHATCH_ON)] _CrossHatchEnabled ("Enable Cross Hatching", Float) = 0
        _HatchColor ("Hatch Ink (multiplied)", Color) = (0, 0, 0, 1)
        _HatchScale ("Hatch Spacing (pixels)", Range(2, 200)) = 40
        _HatchThickness ("Hatch Line Thickness", Range(0.0, 0.5)) = 0.07
        _HatchRotation ("Hatch Base Rotation", Range(0.0, 6.2831)) = 0.785
        _HatchStrength ("Hatch Strength", Range(0.0, 1.0)) = 0.85
        _HatchShadeStart ("Hatch Starts At Shade", Range(0.0, 1.0)) = 0.6
        _HatchShadeDense ("Dense Cross At Shade", Range(0.0, 1.0)) = 0.3


        [Header(Outline Settings)]
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineWidth ("Outline Width (world units / meters)", Range(0.0, 0.2)) = 0.02
        _OutlineMinPixels ("Outline Min Pixels (fades out below this)", Range(0.0, 5.0)) = 0.75
        _OutlineMaxPixels ("Outline Max Pixels (clamp when very close)", Range(1.0, 32.0)) = 6.0
        _OutlineFadeStartDist ("Outline Fade Start Distance (0 = off)", Range(0.0, 200.0)) = 0.0
        _OutlineFadeEndDist ("Outline Fade End Distance", Range(0.1, 400.0)) = 80.0
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Opaque" 
            "RenderPipeline"="UniversalPipeline"
        }

        // First pass for inverted hull outline
        Pass
        {            
            Name "ToonOutline"

            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma vertex outlineVert
            #pragma fragment outlineFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float  fade         : TEXCOORD0;
                float  debugPixels  : TEXCOORD1;
            };

            float4 _OutlineColor;
            float _OutlineWidth;
            float _OutlineMinPixels;
            float _OutlineMaxPixels;
            float _OutlineFadeStartDist;
            float _OutlineFadeEndDist;

            Varyings outlineVert(Attributes input)
            { 
                Varyings output; 

                float3 posWS    = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = normalize(TransformObjectToWorldNormal(input.normalOS));

                float4 clipPos       = TransformWorldToHClip(posWS);
                float2 screenSize    = max(_ScreenParams.xy, float2(16.0, 16.0));
                float  projScaleY    = abs(UNITY_MATRIX_P._m11);
                float  viewDepth     = max(clipPos.w, 1e-4);
                float  pixelsPerMeter = (screenSize.y * 0.5) * projScaleY / viewDepth;

                float widthPixels = _OutlineWidth * pixelsPerMeter;

                // Clamp so it can't balloon when the camera is very close
                widthPixels = min(widthPixels, max(_OutlineMaxPixels, 1.0));

                // Distance fade
                if (_OutlineFadeStartDist > 0.0)
                {
                    float dist = distance(_WorldSpaceCameraPos, posWS);
                    float k = saturate((dist - _OutlineFadeStartDist) /
                                       max(_OutlineFadeEndDist - _OutlineFadeStartDist, 1e-3));
                    widthPixels *= (1.0 - k);
                }

                float worldWidth = widthPixels / max(pixelsPerMeter, 1e-4);
                posWS += normalWS * worldWidth;

                output.positionCS = TransformWorldToHClip(posWS);

                // Fade out (alpha) rather than collapsing the outline
                output.fade = saturate(widthPixels - _OutlineMinPixels + 1.0);
                output.debugPixels = widthPixels;

                return output; 
            }

            half4 outlineFrag(Varyings input) : SV_Target
            {
                return half4(_OutlineColor.rgb, _OutlineColor.a * input.fade);
            }
            ENDHLSL
        }

        // Second pass for toon shading
        Pass
        {
            Name "ToonShading"
            Tags { "LightMode"="UniversalForward" }
            
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex shadingVert
            #pragma fragment shadingFrag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE

            // Enable point and spot light contributions
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile _ _FORWARD_PLUS
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP

            // Required for the togle
            #pragma shader_feature_local _CROSSHATCH_ON
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS   : SV_POSITION;
                float3 normalWS     : TEXCOORD0;
                float2 uv           : TEXCOORD1;
                float3 positionWS   : TEXCOORD2;
            };

            Texture2D _MainTex;
            SamplerState sampler_MainTex;

            float4 _BaseColor;
            float4 _ShadowColor;
            float _StepThreshold;
            float _StepSmoothness;
            float _Shades;
            float _MainLightStrength;
            float _AmbientStrength;

            // Cross hatching
            float4 _HatchColor;
            float _HatchScale;
            float _HatchThickness;
            float _HatchRotation;
            float _HatchStrength;
            float _HatchShadeStart;
            float _HatchShadeDense;

            float HatchLayer(float2 screenPos, float rotation, float spacing, float thickness)
            {
                float s = sin(rotation);
                float c = cos(rotation);
                float2 rotated = float2(c * screenPos.x - s * screenPos.y, s * screenPos.x + c * screenPos.y);

                float coord = rotated.x / max(spacing, 1.0);
                float distToLine = min(frac(coord), 1.0 - frac(coord)); // 0 at a line, 0.5 midway
                return 1.0 - smoothstep(0.0, thickness, distToLine);
            }

            float ComputeHatch(float2 screenPos, float shade)
            {
                float spacing = _HatchScale;
                float thick   = _HatchThickness;
                float rot     = _HatchRotation;

                float ink = 0.0;

                // First cross at 0 deg and 90 deg, comes in together
                if (shade < _HatchShadeStart)
                {
                    ink = max(ink, HatchLayer(screenPos, rot,            spacing, thick));
                    ink = max(ink, HatchLayer(screenPos, rot + 1.5708,   spacing, thick));
                }

                // Second cross for deeper shadow at 45 deg and 135 deg
                if (shade < _HatchShadeDense)
                {
                    ink = max(ink, HatchLayer(screenPos, rot + 0.7854,   spacing, thick));
                    ink = max(ink, HatchLayer(screenPos, rot + 2.3562,   spacing, thick));
                }

                return ink;
            }

            Varyings shadingVert(Attributes input)
            {
                Varyings output;
                float3 posWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionWS = posWS;
                output.positionCS = TransformWorldToHClip(posWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                return output;
            }

            half4 shadingFrag(Varyings input) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);

                Light mainLight = GetMainLight();
                float3 lightDirWS = normalize(mainLight.direction);

                float NdotL = dot(normalWS, lightDirWS);
                float lighting = (NdotL * 0.5 + 0.5) * _MainLightStrength;

                #if defined(_ADDITIONAL_LIGHTS)
                uint pixelLightCount = GetAdditionalLightsCount();

                InputData inputData = (InputData)0;
                inputData.positionWS = input.positionWS;
                inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

                LIGHT_LOOP_BEGIN(pixelLightCount)
                    Light addLight = GetAdditionalLight(lightIndex, input.positionWS);
                    float addAtten  = addLight.distanceAttenuation * addLight.shadowAttenuation;
                    float addNdotL  = saturate(dot(normalWS, addLight.direction));
                    lighting += addNdotL * addAtten;
                LIGHT_LOOP_END
                #endif

                lighting += _AmbientStrength;
                lighting = saturate(lighting);

                float shades = max(1.0, _Shades);
                float value = lighting * shades;

                float band = floor(value);
                float t = frac(value);

                float edge = smoothstep(1.0 - _StepSmoothness, 1.0 + _StepSmoothness, t);

                float toonIntensity = (band + edge) / shades;

                float4 texColor = _MainTex.Sample(sampler_MainTex, input.uv) * _BaseColor;

                float3 finalToonColor = lerp(_ShadowColor.rgb, texColor.rgb, toonIntensity);
            
                
                #ifdef _CROSSHATCH_ON
                {
                    float ink = ComputeHatch(input.positionCS.xy, toonIntensity) * _HatchStrength;
                    finalToonColor = lerp(finalToonColor, finalToonColor * _HatchColor.rgb, ink);
                }
                #endif
                
                return half4(finalToonColor, texColor.a);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}
