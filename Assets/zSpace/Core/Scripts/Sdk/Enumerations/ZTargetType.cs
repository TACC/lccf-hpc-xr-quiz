////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2021 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

namespace zSpace.Core.Sdk
{
    /// <summary>
    /// Defines the types of 6-DOF trackable targets supported.
    /// </summary>
    public enum ZTargetType
    {
        /// <summary>
        /// The target corresponding to the user's head.
        /// </summary>
        Head = 0,

        /// <summary>
        /// The target corresponding to the user's primary hand.
        /// </summary>
        Primary = 1,

        /// <summary>
        /// The target corresponding to the user's left eye.
        /// </summary>
        LeftEye = 1000,

        /// <summary>
        /// The target corresponding to the user's right eye.
        /// </summary>
        RightEye = 1001,

        /// <summary>
        /// The target corresponding to the user's center eye.
        /// </summary>
        CenterEye = 1002,
    }
}
