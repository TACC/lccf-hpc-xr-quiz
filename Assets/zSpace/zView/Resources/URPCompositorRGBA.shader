//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2022 zSpace, Inc.  All Rights Reserved.
//
//////////////////////////////////////////////////////////////////////////

Shader "zSpace/zView/URP/CompositorRGBA"
{
    Properties
    {
        [MainTexture] _MainTex("Main Color Texture", 2D) = "white"{}
    }

    Subshader
    {
        PackageRequirements
        {
            // Unity Universal Render Pipeline 10.0.0 or above.
            "com.unity.render-pipelines.universal": "10.0.0"
        }

        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Fog { Mode off }

            HLSLPROGRAM

                #pragma enable_d3d11_debug_symbols

                #pragma vertex vert
                #pragma fragment frag

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                #include "zViewURPCommon.hlsl"

                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);

                TEXTURE2D(_NonEnvironmentColorTex);
                SAMPLER(sampler_NonEnvironmentColorTex);

                TEXTURE2D(_MaskDepthTex);
                SAMPLER(sampler_MaskDepthTex);

                zViewCompositorVertexToFragment vert(
                    zViewCompositorVertexInput i)
                {
                    return zViewCompositorShadeVertex(i);
                }

                float4 frag(zViewCompositorVertexToFragment i) : COLOR
                {
                    float maskDepth = zViewDecodeFloatRGBA(
                        SAMPLE_TEXTURE2D(
                            _MaskDepthTex, sampler_MaskDepthTex, i.uv));

                    float4 mainColor = SAMPLE_TEXTURE2D(
                        _MainTex, sampler_MainTex, i.uv);

                    float4 nonEnvironmentColor = SAMPLE_TEXTURE2D(
                        _NonEnvironmentColorTex,
                        sampler_NonEnvironmentColorTex,
                        i.uv);

                    if (maskDepth < 0.999)
                    {
                        return nonEnvironmentColor;
                    }
                    else
                    {
                        return mainColor;
                    }
                }

            ENDHLSL
        }
    }

    Fallback off
}
