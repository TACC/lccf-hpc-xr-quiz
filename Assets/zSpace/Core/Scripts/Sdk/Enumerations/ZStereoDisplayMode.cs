////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2021 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

namespace zSpace.Core.Sdk
{
    /// <summary>
    /// Defines the stereo display modes that may be requested for different
    /// types of zSpace hardware.
    /// </summary>
    public enum ZStereoDisplayMode
    {
        /// <summary>
        /// Unity quad-buffer stereo stereo display mode mode.
        /// </summary>
        UnityQuadBufferStereo = 0,

        /// <summary>
        /// zCore native plugin stereo display mode.
        /// </summary>
        NativePlugin = 1,
    }
}
