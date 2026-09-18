#ifndef ZSPACE_ZVIEW_URP_COMMON_HLSL_
#define ZSPACE_ZVIEW_URP_COMMON_HLSL_

//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2022 zSpace, Inc.  All Rights Reserved.
//
//////////////////////////////////////////////////////////////////////////

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

// Encoding/decoding [0..1) floats into 8 bit/channel RGBA. Note that 1.0 will
// not be encoded properly.
inline float4 zViewEncodeFloatRGBA(float v)
{
    float4 kEncodeMul = float4(1.0, 255.0, 65025.0, 16581375.0);
    float kEncodeBit = 1.0/255.0;
    float4 enc = kEncodeMul * v;
    enc = frac (enc);
    enc -= enc.yzww * kEncodeBit;
    return enc;
}
inline float zViewDecodeFloatRGBA(float4 enc)
{
    float4 kDecodeDot = float4(1.0, 1/255.0, 1/65025.0, 1/16581375.0);
    return dot( enc, kDecodeDot );
}

struct zViewCompositorVertexInput
{
    float4 pos : POSITION;
    float2 uv : TEXCOORD0;
};

struct zViewCompositorVertexToFragment
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

inline zViewCompositorVertexToFragment zViewCompositorShadeVertex(
    zViewCompositorVertexInput i)
{
    zViewCompositorVertexToFragment o;
    o.pos = TransformObjectToHClip(i.pos.xyz);
    o.uv = i.uv;
    return o;
}

#endif // ZSPACE_ZVIEW_URP_COMMON_HLSL_
