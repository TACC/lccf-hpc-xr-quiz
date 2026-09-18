//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2022 zSpace, Inc.  All Rights Reserved.
//
//////////////////////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace zSpace.zView
{
    public static class CameraStateSavingUtility
    {
        //////////////////////////////////////////////////////////////////
        // Public Types
        //////////////////////////////////////////////////////////////////

        public struct CameraState
        {
            public Vector3 transformPosition;
            public Quaternion transformRotation;
            public Matrix4x4 projectionMatrix;
            public float nearClipPlane;
            public float farClipPlane;
            public CameraClearFlags clearFlags;
            public Color backgroundColor;
            public int cullingMask;
            public RenderTexture targetTexture;
        }

        //////////////////////////////////////////////////////////////////
        // Public Methods
        //////////////////////////////////////////////////////////////////

        public static void SaveCameraState(
            Camera camera, ref CameraState cameraState)
        {
            cameraState.transformPosition = camera.transform.position;
            cameraState.transformRotation = camera.transform.rotation;
            cameraState.projectionMatrix = camera.projectionMatrix;
            cameraState.nearClipPlane = camera.nearClipPlane;
            cameraState.farClipPlane = camera.farClipPlane;
            cameraState.clearFlags = camera.clearFlags;
            cameraState.backgroundColor = camera.backgroundColor;
            cameraState.cullingMask = camera.cullingMask;
            cameraState.targetTexture = camera.targetTexture;
        }

        public static void RestoreCameraState(
            ref CameraState cameraState, Camera camera)
        {
            camera.transform.position = cameraState.transformPosition;
            camera.transform.rotation = cameraState.transformRotation;
            camera.projectionMatrix = cameraState.projectionMatrix;
            camera.nearClipPlane = cameraState.nearClipPlane;
            camera.farClipPlane = cameraState.farClipPlane;
            camera.clearFlags = cameraState.clearFlags;
            camera.backgroundColor = cameraState.backgroundColor;
            camera.cullingMask = cameraState.cullingMask;
            camera.targetTexture = cameraState.targetTexture;
        }
    }
}
