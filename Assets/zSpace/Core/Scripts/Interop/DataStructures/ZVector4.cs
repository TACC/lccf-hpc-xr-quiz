////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2021 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

using System.Runtime.InteropServices;

using UnityEngine;

namespace zSpace.Core.Interop
{
    [StructLayout(LayoutKind.Explicit, Pack = 8)]
    public struct ZVector4
    {
        [FieldOffset(0)]
        public float x;

        [FieldOffset(4)]
        public float y;

        [FieldOffset(8)]
        public float z;

        [FieldOffset(12)]
        public float w;

        public ZVector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public ZVector4(Vector4 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
            this.w = v.w;
        }

        /// <summary>
        /// Converts the ZVector4 struct to Unity's corresponding
        /// Vector4 struct.
        /// </summary>
        /// 
        /// <param name="flipHandedness">
        /// Flag to specify whether to flip the resultant Vector4's
        /// handedness (e.g. from left to right or vice-versa).
        /// </param>
        /// 
        /// <returns>
        /// Vector4 initialized based on the current state of the
        /// ZVector4.
        /// </returns>
        public Vector4 ToVector4(bool flipHandedness = true)
        {
            return new Vector4(
                this.x, this.y, flipHandedness ? -this.z : this.z, this.w);
        }
    }
}
