//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2022 zSpace, Inc.  All Rights Reserved.
//
//////////////////////////////////////////////////////////////////////////

Shader "zSpace/zView/URP/CompositorRGB"
{
    Properties 
    {
        [MainTexture] _MainTex("Main Texture", 2D) = "white"{}
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

                TEXTURE2D(_NonEnvironmentDepthTex);
                SAMPLER(sampler_NonEnvironmentDepthTex);

                TEXTURE2D(_MaskDepthTex);
                SAMPLER(sampler_MaskDepthTex);

                CBUFFER_START(UnityPerMaterial)
                    float4 _MaskColor;
                CBUFFER_END

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

                    float nonEnvironmentDepth = zViewDecodeFloatRGBA(
                        SAMPLE_TEXTURE2D(
                            _NonEnvironmentDepthTex,
                            sampler_NonEnvironmentDepthTex,
                            i.uv));

                    float4 color = SAMPLE_TEXTURE2D(
                        _MainTex, sampler_MainTex, i.uv);

                    if (nonEnvironmentDepth < maskDepth || maskDepth > 0.999)
                    {
                        color.a = 1.0;
                        return color;
                    }
                    else
                    {
                        return _MaskColor;
                    }
                }

            ENDHLSL 
        }
    }

    Fallback off
}
