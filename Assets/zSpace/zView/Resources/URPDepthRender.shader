//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2022 zSpace, Inc.  All Rights Reserved.
//
//////////////////////////////////////////////////////////////////////////

Shader "zSpace/zView/URP/DepthRender" 
{
    Properties
    {
        // None
    }

    SubShader 
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
            Tags
            {
                // Note:  The value of the LightMode tag must be used as the
                // ShaderTagId in the DrawingSettings instance passed to the
                // ScriptableRenderContext.DrawRenderers() call that uses this
                // shader.
                "LightMode"="DepthOnly"
            }

            Fog { Mode Off }

            HLSLPROGRAM

                #pragma enable_d3d11_debug_symbols

                #pragma vertex vert
                #pragma fragment frag

                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

                #include "zViewURPCommon.hlsl"

                struct VertexInput
                {
                    float4 pos : POSITION;
                };

                struct VertexToFragment
                {
                    float4 pos : SV_POSITION;
                    float2 depth : TEXCOORD0;
                };

                float _Log2FarPlusOne;

                VertexToFragment vert(VertexInput i) 
                {
                    float3 worldPos = TransformObjectToWorld(i.pos.xyz);
                    VertexToFragment o;
                    o.pos = TransformWorldToHClip(worldPos);
                    o.depth.x = -TransformWorldToView(worldPos).z;
                    o.depth.y = 0;
                    return o;
                }

                float4 frag(VertexToFragment i) : SV_Target
                {
                    return zViewEncodeFloatRGBA(
                        clamp(
                            log2(i.depth.x + 1) / _Log2FarPlusOne,
                            0,
                            0.999999));
                }

            ENDHLSL
        }
    }
}
