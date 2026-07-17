////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2021 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

using System;

using UnityEngine;

using zSpace.Core.Interop;
using zSpace.Core.Sdk;

namespace zSpace.Core
{
    public sealed partial class ZProvider
    {
        private sealed class State : IDisposable
        {
            private State()
            {
                this.IsInitialized = false;
                this.Context = null;
                this.Viewport = null;

#if UNITY_WEBGL
                // In WebGL builds, do not try to initialize the native plugin
                // because it is not available and trying to call any extern
                // methods that map to functions in the native plugin leads to
                // hard abort errors that cannot be detected or recovered from.

                Debug.Log(
                    "zSpace Provider running in WebGL mode. Reverting to " +
                    "mock tracker-less, monoscopic 3D.");
#else
                try
                {
                    // Initialize logging for the plugin.
                    ZPlugin.InitializeLogging();

                    // Initialize the zSpace context.
                    this.Context = new ZContext();

#if UNITY_EDITOR
                    // SR device editor mode
                    // This needs to be false to stop lenticular effect when
                    // in-editor and "edit mode", especially when out of focus.
                    if (this.IsSRDeviceEditorModeEnabled)
                    {
                        this.Context.IsStereoDisplayEnabled = false;
                    }
#endif

                    // Attempt to retrieve the zSpace display.
                    ZDisplay display = this.Context.DisplayManager.GetDisplay(
                        ZDisplayType.zSpace);

                    // Fetch and initialize the primary viewport.
                    this.Viewport = this.Context.GetPrimaryViewport();
                    this.Viewport.Position =
                        (display != null) ? display.Position : Vector2Int.zero;

                    this.IsInitialized = true;
                }
                catch
                {
                    if (Application.isPlaying)
                    {
                        Debug.LogWarning(
                            "Failed to properly initialize the zSpace " +
                            "Provider. Reverting to mock tracker-less, " +
                            "monoscopic 3D.");
                    }
                    
                    this.Dispose();
                }
#endif
            }

            ~State()
            {
                this.Dispose();
            }

            ////////////////////////////////////////////////////////////////////
            // Public Static Methods
            ////////////////////////////////////////////////////////////////////

            /// <summary>
            /// A reference to the zSpace Provider's persistent state.
            /// </summary>
            public static State Instance
            {
                get
                {
                    if (s_instance == null)
                    {
                        s_instance = new State();
                    }

                    return s_instance;
                }
            }

            /// <summary>
            /// Shut down and clean up the zSpace Provider's persistent state. 
            /// This includes shutting down the state's SDK context.
            /// </summary>
            public static void ShutDown()
            {
                if (s_instance != null)
                {
                    s_instance.Dispose();
                    s_instance = null;
                }
            }

            ////////////////////////////////////////////////////////////////////
            // Public Properties
            ////////////////////////////////////////////////////////////////////

            /// <summary>
            /// Gets whether the zSpace Provider's persistent state (e.g. SDK 
            /// context) has been properly initialized.
            /// </summary>
            public bool IsInitialized { get; private set; }

            /// <summary>
            /// The zSpace SDK context.
            /// </summary>
            public ZContext Context { get; private set; }

            /// <summary>
            /// The primary viewport for managing the application window's
            /// position and size as well as its corresponding stereo frustum.
            /// </summary>
            public ZViewport Viewport { get; private set; }

#if UNITY_EDITOR
            /// <summary>
            /// SR device editor mode
            /// A flag signaling zCore to handle an assortment of
            /// properties and states differently to effectively preview
            /// apps in-editor on SR devices.
            /// </summary>
            public bool IsSRDeviceEditorModeEnabled
            {
                get
                {
                    return
                        Application.isEditor &&
                        this.Context != null &&
                        this.Context.IsRunningOnSRDevice;
                }
            }
#endif

            ////////////////////////////////////////////////////////////////////
            // Public Methods
            ////////////////////////////////////////////////////////////////////

            public void Dispose()
            {
                if (this.Viewport != null)
                {
                    this.Viewport.Dispose();
                }

                if (this.Context != null)
                {
                    this.Context.Dispose();
                }

                this.Viewport = null;
                this.Context = null;

                this.IsInitialized = false;

                ZPlugin.ShutDownLogging();
            }

            ////////////////////////////////////////////////////////////////////
            // Private Members
            ////////////////////////////////////////////////////////////////////

            private static State s_instance = null;
        }
    }
}
