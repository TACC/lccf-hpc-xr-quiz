////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2021 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

using System;

using UnityEngine;

using zSpace.Core.Interop;

namespace zSpace.Core.Sdk
{
    public class ZContext : ZNativeResource
    {
        /// <summary>
        /// The ZContext constructor.
        /// </summary>
        /// 
        /// <remarks>
        /// Will throw an exception if the zSpace SDK failed to initialize.
        /// </remarks>
        public ZContext()
        {
            this.DisplayManager = null;
            this.TargetManager = null;
            this.MouseEmulator = null;

            ZPlugin.ThrowOnError(ZPlugin.Initialize(out this._nativePtr));

            // Set the created native plugin context as the context to be used
            // by the native plugin on the Unity render thread.
            ZPlugin.ThrowOnError(
                ZPlugin.SetRenderThreadContext(this._nativePtr));

            this.DisplayManager = new ZDisplayManager(this);
            this.TargetManager = new ZTargetManager(this);
            this.MouseEmulator = new ZMouseEmulator(this);
            this.MouseEmulator.Target = this.TargetManager.StylusTarget;
        }

        ~ZContext()
        {
            this.Dispose(false);
        }

        ////////////////////////////////////////////////////////////////////////
        // Public Properties
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The manager responsible for managing display information
        /// corresponding to all active displays.
        /// </summary>
        public ZDisplayManager DisplayManager { get; private set; }

        /// <summary>
        /// The manager responsible for managing tracking information
        /// corresponding to all active, trackable targets.
        /// </summary>
        /// 
        /// <remarks>
        /// Currently the zSpace glasses and stylus are the only supported
        /// trackable targets.
        /// </remarks>
        public ZTargetManager TargetManager { get; private set; }

        /// <summary>
        /// The mouse emulator which provides support for allowing any
        /// 6-DOF trackable target to emulate the system-level mouse.
        /// </summary>
        public ZMouseEmulator MouseEmulator { get; private set; }

        /// <summary>
        /// The version of zSpace SDK runtime that is currently installed
        /// on the user's machine.
        /// </summary>
        public Version RuntimeVersion
        {
            get
            {
                int major = 0;
                int minor = 0;
                int patch = 0;
                ZPlugin.LogOnError(ZPlugin.GetRuntimeVersion(
                    this._nativePtr, out major, out minor, out patch),
                    "GetRuntimeVersion");

                return new Version(major, minor, patch);
            }
        }

        /// <summary>
        /// Checks whether the application is currently running on an SR
        /// device.
        /// </summary>
        public bool IsRunningOnSRDevice
        {
            get
            {
                bool isRunningOnSRDevice = false;
                ZPlugin.LogOnError(ZPlugin.IsRunningOnSRDevice(
                    this._nativePtr, out isRunningOnSRDevice),
                    "IsRunningOnSRDevice");

                return isRunningOnSRDevice;
            }
        }

        /// <summary>
        /// Specifies whether tracking is enabled.
        /// </summary>
        /// 
        /// <remarks>
        /// This property acts as a global flag to enable or disable 
        /// updates for all tracking related information.
        /// </remarks>
        public bool IsTrackingEnabled
        {
            get
            {
                bool isEnabled = false;
                ZPlugin.LogOnError(
                    ZPlugin.IsTrackingEnabled(this._nativePtr, out isEnabled),
                    "IsTrackingEnabled");

                return isEnabled;
            }
            set
            {
                ZPlugin.LogOnError(
                    ZPlugin.SetTrackingEnabled(this._nativePtr, value),
                    "SetTrackingEnabled");
            }
        }

        /// <summary>
        /// The render mode requested for the current zSpace hardware.
        /// </summary>
        public ZStereoDisplayMode StereoDisplayMode
        {
            get
            {
                ZStereoDisplayMode stereoDisplayMode =
                    ZStereoDisplayMode.UnityQuadBufferStereo;
                ZPlugin.LogOnError(
                    ZPlugin.GetStereoDisplayMode(
                        this._nativePtr, out stereoDisplayMode),
                    "GetStereoDisplayMode");

                return stereoDisplayMode;
            }
        }

        /// <summary>
        /// Whether or not displaying stereo is enabled.
        /// </summary>
        public bool IsStereoDisplayEnabled
        {
            get
            {
                bool isEnabled = true;
                ZPlugin.LogOnError(
                    ZPlugin.IsStereoDisplayEnabled(
                        this._nativePtr, out isEnabled),
                    "IsStereoDisplayEnabled");

                return isEnabled;
            }

            set
            {
                ZPlugin.LogOnError(
                    ZPlugin.SetStereoDisplayEnabled(this._nativePtr, value),
                    "SetStereoDisplayEnabled");
            }
        }

        /// <summary>
        /// Whether or not the application needs to tie the stereo display
        /// enabled state to its focus state.
        /// </summary>
        public bool MustTieStereoDisplayEnabledToApplicationFocus
        {
            get
            {
                bool mustTieStereoDisplayEnabledToApplicationFocus = true;
                ZPlugin.LogOnError(
                    ZPlugin.GetMustTieStereoDisplayEnabledToApplicationFocus(
                        this._nativePtr,
                        out mustTieStereoDisplayEnabledToApplicationFocus),
                    "GetMustTieStereoDisplayEnabledToApplicationFocus");

                return mustTieStereoDisplayEnabledToApplicationFocus;
            }
        }

        /// <summary>
        /// The resolution of the eye images used for displaying stereo
        /// content.
        /// </summary>
        public Vector2Int PerEyeImageResolution
        {
            get
            {
                int width = 0;
                int height = 0;
                ZPlugin.LogOnError(
                    ZPlugin.GetPerEyeImageResolution(
                        this._nativePtr, out width, out height),
                    "GetPerEyeImageResolution");

                return new Vector2Int(width, height);
            }
        }

        /// <summary>
        /// Whether or not the Unity editor XR overlay is supported.
        /// </summary>
        public bool IsXROverlaySupported
        {
            get
            {
                bool isSupported = false;
                ZPlugin.LogOnError(
                    ZPlugin.IsXROverlaySupported(
                        this._nativePtr, out isSupported),
                    "IsXROverlaySupported");

                return isSupported;
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // Public Methods
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Updates the internal state of the context.
        /// </summary>
        /// 
        /// <remarks>
        /// In general, this method should only be called once per frame.
        /// 
        /// The update is responsible for capturing the latest tracking 
        /// information, forwarding the latest head pose information to all 
        /// active frustums, etc.
        /// </remarks>
        public void Update()
        {
            ZPlugin.LogOnError(ZPlugin.Update(this._nativePtr), "Update");
        }

        /// <summary>
        /// Get the context's primary ZViewport instance.
        /// </summary>
        /// 
        /// <returns>
        /// An instance of the ZViewport class.
        /// </returns>
        public ZViewport GetPrimaryViewport()
        {
            if (this._primaryViewport == null)
            {
                // Create the viewport.
                IntPtr viewportNativePtr;
                ZPlugin.LogOnError(
                    ZPlugin.GetPrimaryViewport(
                        this._nativePtr, out viewportNativePtr),
                    "GetPrimaryViewport");

                ZViewport viewport = new ZViewport(
                    viewportNativePtr,
                    isNativePtrOwner: false);

                // Update the context to ensure the appropriate display
                // angle has been passed to the viewport's frustum.
                this.Update();

                // Initialize the frustum.
                ZFrustum frustum = viewport.Frustum;
                var defaultEyePose = frustum.DefaultEyePose;
                frustum.SetTrackerSpaceEyePoses(defaultEyePose, defaultEyePose);

                this._primaryViewport = viewport;
            }

            return this._primaryViewport;
        }

        /// <summary>
        /// Creates an instance of the ZViewport class at the specified
        /// virtual desktop position.
        /// </summary>
        /// 
        /// <param name="position">
        /// The (x, y) virtual desktop position in pixels corresponding
        /// to the viewport's top-left corner.
        /// </param>
        /// 
        /// <returns>
        /// An instance of the ZViewport class.
        /// </returns>
        public ZViewport CreateViewport(Vector2Int position)
        {
            // Create the viewport.
            IntPtr viewportNativePtr;
            ZPlugin.LogOnError(
                ZPlugin.CreateViewport(this._nativePtr, out viewportNativePtr),
                "CreateViewport");

            ZViewport viewport = new ZViewport(viewportNativePtr);
            viewport.Position = position;

            // Update the context to ensure the appropriate display
            // angle has been passed to the viewport's frustum.
            this.Update();

            // Initialize the frustum.
            ZFrustum frustum = viewport.Frustum;
            var defaultEyePose = frustum.DefaultEyePose;
            frustum.SetTrackerSpaceEyePoses(defaultEyePose, defaultEyePose);

            return viewport;
        }

        /// <summary>
        /// Notifies the context that the application is beginning work on a
        /// new frame.
        /// </summary>
        public void BeginFrame()
        {
            ZPlugin.LogOnError(
                ZPlugin.BeginFrame(this._nativePtr), "BeginFrame");
        }

        /// <summary>
        /// Notifies the context that the application is ending work on the
        /// current frame.
        /// </summary>
        public void EndFrame()
        {
            ZPlugin.LogOnError(
                ZPlugin.EndFrame(this._nativePtr), "EndFrame");
        }

        ////////////////////////////////////////////////////////////////////////
        // Protected Methods
        ////////////////////////////////////////////////////////////////////////

        protected override void Dispose(bool disposing)
        {
            if (this._isDisposed)
            {
                return;
            }

            this._isDisposed = true;

            // Free managed objects.
            if (disposing)
            {
                this.DisplayManager.ClearCache();
                this.TargetManager.ClearCache();
            }

            // Free unmanaged objects.
            ZPlugin.LogOnError(ZPlugin.ShutDown(this._nativePtr), "ShutDown");

            // Call to base class implementation.
            base.Dispose(disposing);
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Methods
        ////////////////////////////////////////////////////////////////////////

        private static bool TryGetSRWeavingTextureFormat(
            int numChannels,
            int bitsPerChannel,
            out TextureFormat textureFormat)
        {
            if (numChannels == 1 && bitsPerChannel == 8)
            {
                textureFormat = TextureFormat.R8;
                return true;
            }

            if (numChannels == 1 && bitsPerChannel == 16)
            {
                textureFormat = TextureFormat.R16;
                return true;
            }

            textureFormat = TextureFormat.R8;
            return false;
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Members
        ////////////////////////////////////////////////////////////////////////

        private bool _isDisposed = false;
        private ZViewport _primaryViewport = null;
    }
}
