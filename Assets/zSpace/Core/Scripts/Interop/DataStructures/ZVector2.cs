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
    public struct ZVector2
    {
        [FieldOffset(0)]
        public float x;

        [FieldOffset(4)]
        public float y;

        public ZVector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public ZVector2(Vector2 v)
        {
            this.x = v.x;
            this.y = v.y;
        }

        /// <summary>
        /// Converts the ZVector2 struct to Unity's corresponding
        /// Vector2 struct.
        /// </summary>
        /// 
        /// <returns>
        /// Vector2 initialized based on the current state of the
        /// ZVector2.
        /// </returns>
        public Vector2 ToVector2()
        {
            return new Vector2(this.x, this.y);
        }
    }
}
