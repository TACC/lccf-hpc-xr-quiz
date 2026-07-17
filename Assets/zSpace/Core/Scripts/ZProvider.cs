////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2021 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

using zSpace.Core.Sdk;
using zSpace.Core.Utility;

namespace zSpace.Core
{
    [ExecuteInEditMode]
    [DefaultExecutionOrder(ScriptPriority)]
    [DisallowMultipleComponent]
    public sealed partial class ZProvider : ZSingleton<ZProvider>
    {
        public const int ScriptPriority = -1000;

        ////////////////////////////////////////////////////////////////////////
        // Inspector Fields
        ////////////////////////////////////////////////////////////////////////

        [Header("Screen Metrics")]

        [SerializeField]
        [Tooltip(
            "The profile of the reference display the application is being " +
            "designed for.")]
        private ZDisplay.Profile _displayReferenceProfile = 
            ZDisplay.ReferenceProfile;

        [SerializeField]
        [Tooltip("The display reference size in meters.")]
        private Vector2 _displayReferenceSize =
            ZDisplay.GetSize(ZDisplay.ReferenceProfile);

        [SerializeField]
        [Tooltip("The display reference resolution in pixels.")]
        private Vector2Int _displayReferenceResolution =
            ZDisplay.GetNativeResolution(ZDisplay.ReferenceProfile);

        ////////////////////////////////////////////////////////////////////////
        // MonoBehaviour Callbacks
        ////////////////////////////////////////////////////////////////////////

        protected override void Awake()
        {
            CurrentDisplay = null;

            DisplayReferenceSize = ZDisplay.GetSize(ZDisplay.ReferenceProfile);

            DisplayReferenceResolution = ZDisplay.GetNativeResolution(ZDisplay.ReferenceProfile);

            DisplaySize = ZDisplay.GetSize(ZDisplay.ReferenceProfile);

            DisplayResolution = ZDisplay.GetNativeResolution(ZDisplay.ReferenceProfile);

            DisplayMetersPerPixelForReferenceResolution = 
                ZDisplay.GetMetersPerPixel(ZDisplay.ReferenceProfile);

            DisplayMetersPerPixelForNativeResolution = 
                ZDisplay.GetMetersPerPixel(ZDisplay.ReferenceProfile);

            DisplayScale = Vector2.one;

            DisplayScaleFactor = 1;

            WindowSize = ZDisplay.GetSize(ZDisplay.ReferenceProfile);

            WindowSizePixelsForReferenceResolution =
                ZDisplay.GetNativeResolution(ZDisplay.ReferenceProfile);

            WindowSizePixelsForNativeResolution =
                ZDisplay.GetNativeResolution(ZDisplay.ReferenceProfile);

            base.Awake();

            // Perform an update to initialize state.
            this.Update();
        }

        private void OnApplicationFocus(bool isFocused)
        {
            if (IsInitialized &&
                Context.MustTieStereoDisplayEnabledToApplicationFocus)
            {
                Context.IsStereoDisplayEnabled =
#if UNITY_EDITOR
                    // SR device editor mode
                    ZProvider.IsSRDeviceEditorModeEnabled ?  false :
#endif
                    isFocused;
            }
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();

            State.ShutDown();
        }

        private void Update()
        {
            if (IsInitialized)
            {
                RectInt windowRect = ZApplicationWindow.Rect;

                // Update the viewport's position and size based on the 
                // current position and size of the application window.
                Viewport.Rect = windowRect;

                // Get the current display based on the center position
                // of the application window.
                if (!windowRect.Equals(this._previousWindowRect))
                {
                    CurrentDisplay = Context.DisplayManager.GetDisplay(
                        (int)windowRect.center.x, (int)windowRect.center.y);
                }

                // Update the SDK's context.
                Context.Update();

                this._previousWindowRect = windowRect;
            }

            this.UpdateScreenMetrics();
        }

        ////////////////////////////////////////////////////////////////////////
        // Public Static Properties
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Checks whether the zSpace provider has been properly initialized.
        /// </summary>
        /// 
        /// <remarks>
        /// In the scenario that the application is running on a non-zSpace
        /// device, is running on a system that doesn't have the zSpace System
        /// Software installed, etc., IsInitialized will be set to false.
        /// 
        /// Please make sure to check this before attempting to retrieve
        /// the zSpace Context, HeadTarget, StylusTarget, and/or Viewport.
        /// </remarks>
        public static bool IsInitialized
        {
            get
            {
                return State.Instance.IsInitialized;
            }
        }

        /// <summary>
        /// Gets a reference to the zSpace SDK's primary context.
        /// </summary>
        /// 
        /// <remarks>
        /// The primary context will persist for the lifetime of the
        /// application.
        /// 
        /// If ZProvider.IsInitialized is false, this property will be 
        /// set to null.
        /// </remarks>
        public static ZContext Context
        {
            get
            {
                return State.Instance.Context;
            }
        }

        /// <summary>
        /// Gets a reference to the default head target (glasses).
        /// </summary>
        /// 
        /// <remarks>
        /// If ZProvider.IsInitialized is false, this property will be 
        /// set to null.
        /// </remarks>
        public static ZTarget HeadTarget
        {
            get
            {
                if (Context != null)
                {
                    return Context.TargetManager.HeadTarget;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a reference to the default left eye target.
        /// </summary>
        /// 
        /// <remarks>
        /// If ZProvider.IsInitialized is false, this property will be 
        /// set to null.
        /// </remarks>
        public static ZTarget LeftEyeTarget
        {
            get
            {
                if (Context != null)
                {
                    return Context.TargetManager.LeftEyeTarget;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a reference to the default right eye target.
        /// </summary>
        /// 
        /// <remarks>
        /// If ZProvider.IsInitialized is false, this property will be 
        /// set to null.
        /// </remarks>
        public static ZTarget RightEyeTarget
        {
            get
            {
                if (Context != null)
                {
                    return Context.TargetManager.RightEyeTarget;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a reference to the default center eye target.
        /// </summary>
        /// 
        /// <remarks>
        /// If ZProvider.IsInitialized is false, this property will be 
        /// set to null.
        /// </remarks>
        public static ZTarget CenterEyeTarget
        {
            get
            {
                if (Context != null)
                {
                    return Context.TargetManager.CenterEyeTarget;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a reference to the default stylus target.
        /// </summary>
        /// 
        /// <remarks>
        /// If ZProvider.IsInitialized is false, this property will be 
        /// set to null.
        /// </remarks>
        public static ZTarget StylusTarget
        {
            get
            {
                if (Context != null)
                {
                    return Context.TargetManager.StylusTarget;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a reference to the primary viewport.
        /// </summary>
        /// 
        /// <remarks>
        /// The viewport is responsible for managing information about the 
        /// application window's position and size.
        /// 
        /// Additionally, it manages the application's stereo frustum, which
        /// is responsible for computing the perspectives for the left and
        /// right eyes.
        /// 
        /// If ZProvider.IsInitialized is false, this property will be 
        /// set to null.
        /// </remarks>
        public static ZViewport Viewport
        {
            get
            {
                return State.Instance.Viewport;
            }
        }

        /// <summary>
        /// Gets the display that the application window is currently on.
        /// </summary>
        /// 
        /// <remarks>
        /// The center of the application window's viewport is used to
        /// determine which display it's currently on.
        /// 
        /// If ZProvider.IsInitialized is false, this property will be 
        /// set to null.
        /// </remarks>
        public static ZDisplay CurrentDisplay { get; private set; }

        /// <summary>
        /// The display reference size in meters.
        /// </summary>
        /// 
        /// <remarks>
        /// This is leveraged in use cases such as computing display scale
        /// factor.
        /// </remarks>
        public static Vector2 DisplayReferenceSize { get; private set; }

        /// <summary>
        /// The display reference resolution in pixels.
        /// </summary>
        public static Vector2Int DisplayReferenceResolution { get; private set; }

        /// <summary>
        /// The current display size in meters.
        /// </summary>
        /// 
        /// <remarks>
        /// If ZProvider.IsInitialized is set to false, the DisplaySize will
        /// be set to the DisplayReferenceSize.
        /// </remarks>
        public static Vector2 DisplaySize { get; private set; }

        /// <summary>
        /// The current display resolution in pixels.
        /// </summary>
        /// 
        /// <remarks>
        /// If ZProvider.IsInitialized is set to false, the DisplayResolution
        /// will be set to the DisplayReferenceResolution.
        /// </remarks>
        public static Vector2Int DisplayResolution { get; private set; }

        /// <summary>
        /// The meters per pixel conversion factor computed from the current
        /// DisplaySize and DisplayReferenceResolution.
        /// </summary>
        ///
        /// <remarks>
        /// This property is deprecated. Use either the
        /// DisplayMetersPerPixelForReferenceResolution or
        /// DisplayMetersPerPixelsForNativeResolution property instead. See the
        /// comments for these two properties for guidance on which one to use.
        /// </remarks>
        [System.Obsolete(
            "Use DisplayMetersPerPixelForReferenceResolution or " +
            "DisplayMetersPerPixelsForNativeResolution instead.")]
        public static Vector2 DisplayMetersPerPixel
        {
            get
            {
                return DisplayMetersPerPixelForReferenceResolution;
            }

            set
            {
                DisplayMetersPerPixelForReferenceResolution = value;
            }
        }

        /// <summary>
        /// The meters per pixel conversion factor computed from the current
        /// DisplaySize and DisplayReferenceResolution.
        /// </summary>
        ///
        /// <remarks>
        /// Use this conversion factor when converting pixel sizes defined
        /// relative to the display reference resolution to meters. For
        /// example, for UI elements that have been laid out with respect to
        /// the display reference resolution, the pixel sizes of such UI
        /// elements can be converted to meters using this conversion
        /// factor.
        /// </remarks>
        public static Vector2 DisplayMetersPerPixelForReferenceResolution
            { get; set; }

        /// <summary>
        /// The meters per pixel conversion factor computed from the current
        /// DisplaySize and DisplayResolution.
        /// </summary>
        ///
        /// <remarks>
        /// Use this conversion factor when converting pixel sizes defined
        /// relative to the native resolution of the current display to meters.
        /// </remarks>
        public static Vector2 DisplayMetersPerPixelForNativeResolution
            { get; set; }

        /// <summary>
        /// The scale of the current display based on its size relative to the 
        /// DisplayReferenceSize.
        /// </summary>
        public static Vector2 DisplayScale { get; private set; }

        /// <summary>
        /// The scale factor of the current display based on the DisplayScale.
        /// </summary>
        /// 
        /// <remarks>
        /// The current and only scale mode that is supported is "fit inside".
        /// </remarks>
        public static float DisplayScaleFactor { get; set; }

        /// <summary>
        /// The size of the application window in meters.
        /// </summary>
        /// 
        /// <remarks>
        /// If ZProvider.IsInitialized is set to false, the window size (and
        /// aspect ratio) is locked to the display reference size.
        /// </remarks>
        public static Vector2 WindowSize { get; private set; }

        /// <summary>
        /// The size of the application window in reference display pixels.
        /// </summary>
        /// 
        /// <remarks>
        /// This property is deprecated. Use either the
        /// WindowSizePixelsForReferenceResolution or
        /// WindowSizePixelsForNativeResolution property instead. See the
        /// comments for these two properties for guidance on which one to use.
        ///
        /// If ZProvider.IsInitialized is set to false, the window size in 
        /// pixels is locked to the display reference resolution.
        /// </remarks>
        [System.Obsolete(
            "Use WindowSizePixelsForReferenceResolution or " +
            "WindowSizePixelsForNativeResolution instead.")]
        public static Vector2Int WindowSizePixels
        {
            get
            {
                return WindowSizePixelsForReferenceResolution;
            }
        }

        /// <summary>
        /// The size of the application window in reference display pixels.
        /// </summary>
        /// 
        /// <remarks>
        /// Use this window size when working with pixel sizes defined relative
        /// to the display reference resolution. For example, for a UI canvas
        /// that has been laid out with respect to the display reference
        /// resolution, the pixel size of such a UI canvas should generally be
        /// set to this window size at runtime.
        ///
        /// If ZProvider.IsInitialized is set to false, the window size in 
        /// pixels is locked to the display reference resolution.
        /// </remarks>
        public static Vector2Int WindowSizePixelsForReferenceResolution
            { get; private set; }

        /// <summary>
        /// The size of the application window in native display pixels.
        /// </summary>
        /// 
        /// <remarks>
        /// Use this window size when working with pixel sizes defined relative
        /// to the native resolution of the current display.
        ///
        /// If ZProvider.IsInitialized is set to false, the window size in 
        /// pixels is locked to the display reference resolution.
        /// </remarks>
        public static Vector2Int WindowSizePixelsForNativeResolution
            { get; private set; }

#if UNITY_EDITOR
        /// <summary>
        /// SR device editor mode
        /// A flag signaling zCore to handle an assortment of
        /// properties and states differently to effectively preview
        /// apps in-editor on SR devices.
        /// </summary>
        public static bool IsSRDeviceEditorModeEnabled
        {
            get
            {
                return State.Instance.IsSRDeviceEditorModeEnabled;
            }
        }
#endif

        ////////////////////////////////////////////////////////////////////////
        // Private Methods
        ////////////////////////////////////////////////////////////////////////

        private void UpdateScreenMetrics()
        {
            // If the display reference profile is not custom, lock
            // the display reference size to the appropriate value.
            if (this._displayReferenceProfile != ZDisplay.Profile.Custom)
            {
                this._displayReferenceSize = ZDisplay.GetSize(
                    this._displayReferenceProfile);

                this._displayReferenceResolution = ZDisplay.GetNativeResolution(
                    this._displayReferenceProfile);
            }

            this._displayReferenceSize = Vector2.Max(
                ZDisplay.MinimumSize, this._displayReferenceSize);

            this._displayReferenceResolution = Vector2Int.Max(
                Vector2Int.one, this._displayReferenceResolution);

            // Update current display information.
            DisplayReferenceSize = this._displayReferenceSize;
            DisplayReferenceResolution = this._displayReferenceResolution;

            if (CurrentDisplay != null)
            {
                DisplaySize = CurrentDisplay.Size; 
                DisplayResolution = CurrentDisplay.NativeResolution;
            }
            else
            {
                DisplaySize = this._displayReferenceSize;
                DisplayResolution = this._displayReferenceResolution;
            }

            DisplayMetersPerPixelForNativeResolution = new Vector2(
                DisplaySize.x / DisplayResolution.x,
                DisplaySize.y / DisplayResolution.y);

            DisplayScale = ZDisplay.GetScale(DisplayReferenceSize, DisplaySize);
            DisplayScaleFactor = Mathf.Max(DisplayScale.x, DisplayScale.y);

            Vector2 nativeToReferenceDisplayResolutionScale = new Vector2(
                ((float)DisplayReferenceResolution.x) / DisplayResolution.x,
                ((float)DisplayReferenceResolution.y) / DisplayResolution.y);
            
            float nativeToReferenceDisplayResolutionScaleFactor = Mathf.Min(
                nativeToReferenceDisplayResolutionScale.x,
                nativeToReferenceDisplayResolutionScale.y);
            
            DisplayMetersPerPixelForReferenceResolution =
                DisplayMetersPerPixelForNativeResolution /
                nativeToReferenceDisplayResolutionScaleFactor;

            // Update current window information.
            if (IsInitialized)
            {
                WindowSizePixelsForNativeResolution = ZApplicationWindow.Size;
                WindowSizePixelsForReferenceResolution = new Vector2Int(
                    (int)Mathf.Round(
                        WindowSizePixelsForNativeResolution.x *
                            nativeToReferenceDisplayResolutionScaleFactor),
                    (int)Mathf.Round(
                        WindowSizePixelsForNativeResolution.y *
                            nativeToReferenceDisplayResolutionScaleFactor));
                WindowSize = new Vector2(
                    WindowSizePixelsForNativeResolution.x *
                        DisplayMetersPerPixelForNativeResolution.x,
                    WindowSizePixelsForNativeResolution.y *
                        DisplayMetersPerPixelForNativeResolution.y);
            }
            else
            {
                WindowSizePixelsForReferenceResolution = DisplayResolution;
                WindowSizePixelsForNativeResolution = DisplayResolution;
                WindowSize = DisplaySize;
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Members
        ////////////////////////////////////////////////////////////////////////

        private RectInt _previousWindowRect;
    }
}
