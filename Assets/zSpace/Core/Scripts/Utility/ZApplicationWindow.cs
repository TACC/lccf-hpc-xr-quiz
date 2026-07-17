////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2020 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

using zSpace.Core.Extensions;
using zSpace.Core.Interop;

namespace zSpace.Core.Utility
{
    public static class ZApplicationWindow
    {
        ////////////////////////////////////////////////////////////////////////
        // Public Static Properties
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The virtual desktop position and size in pixels of the 
        /// application window.
        /// </summary>
        /// 
        /// <remarks>
        /// When running from the Unity Editor, the position and size 
        /// correspond to the Game View window.
        /// </remarks>
        public static RectInt Rect
        {
            get
            {
#if UNITY_EDITOR
                // Grab the position and size of the GameView window
                // when running from the editor.
                EditorWindow gameViewWindow = 
                    EditorWindowExtensions.GetGameViewWindow();

                RectInt gameViewWindowRect = gameViewWindow.GetRect();

                // SR device editor mode
                if (ZProvider.IsSRDeviceEditorModeEnabled)
                {
                    System.IntPtr gameViewWindowWindowHandle =
                        GetGameViewWindowWindowHandleFromCache(gameViewWindow);

                    RectInt gameViewWindowPhysicalRect = gameViewWindowRect;

                    // Query the GameView window's physical rect, which
                    // contains the position and size of the window at the
                    // native resolution of the display that the window is
                    // associated with (this is generally the display which
                    // contains the largest portion of the window's rect),
                    // regardless of the display scale of the display.
                    {
                        int gameViewWindowPhysicalLeft = 0;
                        int gameViewWindowPhysicalTop = 0;
                        int gameViewWindowPhysicalRight = 0;
                        int gameViewWindowPhysicalBottom = 0;

                        Sdk.ZPluginError result = ZPlugin.GetWindowPhysicalRect(
                            gameViewWindowWindowHandle,
                            ref gameViewWindowPhysicalLeft,
                            ref gameViewWindowPhysicalTop,
                            ref gameViewWindowPhysicalRight,
                            ref gameViewWindowPhysicalBottom);

                        // If querying the GameView window's physical rect
                        // fails, requery the GameView window's window handle,
                        // bypassing the cache, and then attempt to query the
                        // physical rect again.  In some cases, Unity will
                        // recreate the GameView window in a way that the
                        // GameView window window handle caching logic cannot
                        // detect, resulting in the cached window handle being
                        // incorrect.  One case where this can happen is when
                        // the GameView window is undocked and the user undocks
                        // the GameView window from itself by dragging the
                        // GameView tab in the already undocked GameView
                        // window.
                        if (result != Sdk.ZPluginError.Ok)
                        {
                            gameViewWindowWindowHandle =
                                GetGameViewWindowWindowHandleFromCache(
                                    gameViewWindow, forceCacheRefresh: true);

                            result = ZPlugin.GetWindowPhysicalRect(
                                gameViewWindowWindowHandle,
                                ref gameViewWindowPhysicalLeft,
                                ref gameViewWindowPhysicalTop,
                                ref gameViewWindowPhysicalRight,
                                ref gameViewWindowPhysicalBottom);
                        }

                        if (result == Sdk.ZPluginError.Ok)
                        {
                            gameViewWindowPhysicalRect = new RectInt(
                                gameViewWindowPhysicalLeft,
                                gameViewWindowPhysicalTop,
                                gameViewWindowPhysicalRight -
                                    gameViewWindowPhysicalLeft,
                                gameViewWindowPhysicalBottom -
                                    gameViewWindowPhysicalTop);
                        }
                    }

                    int gameViewWindowScaleFactorPercentageInt = -1;

                    {
                        Sdk.ZPluginError result =
                            ZPlugin.GetWindowScaleFactorPercentage(
                                gameViewWindowWindowHandle,
                                ref gameViewWindowScaleFactorPercentageInt);

                        // If querying the GameView window's scale factor
                        // percentage fails, requery the GameView window's
                        // window handle, bypassing the cache, and then attempt
                        // to query the scale factor percentage again.  In some
                        // cases, Unity will recreate the GameView window in a
                        // way that the GameView window window handle caching
                        // logic cannot detect, resulting in the cached window
                        // handle being incorrect.  One case where this can
                        // happen is when the GameView window is undocked and
                        // the user undocks the GameView window from itself by
                        // dragging the GameView tab in the already undocked
                        // GameView window.
                        if (result != Sdk.ZPluginError.Ok)
                        {
                            gameViewWindowWindowHandle =
                                GetGameViewWindowWindowHandleFromCache(
                                    gameViewWindow, forceCacheRefresh: true);

                            result = ZPlugin.GetWindowScaleFactorPercentage(
                                gameViewWindowWindowHandle,
                                ref gameViewWindowScaleFactorPercentageInt);
                        }

                        if (result != Sdk.ZPluginError.Ok)
                        {
                            gameViewWindowScaleFactorPercentageInt = 100;
                        }
                    }

                    float gameViewWindowScaleFactor =
                        gameViewWindowScaleFactorPercentageInt / 100.0f;

                    // Compute the rect of the game view within the GameView
                    // window by taking into account various window components
                    // such as borders and tabs.  The sizes of the window
                    // components are queried from the EditorWindowExtensions
                    // class and are the same sizes used internally by the
                    // EditorWindowExtensions GetRect() extension method.
                    // However, these sizes use logical (i.e. scaled) pixels as
                    // units and are therefore scaled by the display scale
                    // factor to convert the units to native pixels, which are
                    // units used for the GameView window's physical rect.
                    gameViewWindowRect = new RectInt(
                        gameViewWindowPhysicalRect.x +
                            Mathf.RoundToInt(
                                gameViewWindow.GetBorderWidth() *
                                    gameViewWindowScaleFactor),
                        gameViewWindowPhysicalRect.y +
                            Mathf.RoundToInt(
                                gameViewWindow.GetTabHeight() *
                                    gameViewWindowScaleFactor),
                        gameViewWindowPhysicalRect.width,
                        gameViewWindowPhysicalRect.height -
                            Mathf.RoundToInt(
                                gameViewWindow.GetExcessHeight() *
                                    gameViewWindowScaleFactor));
                }

                return gameViewWindowRect;
#elif UNITY_STANDALONE_WIN
                // Grab the position and size of the standalone player's
                // window when running a standalone build.
                int x = 0;
                int y = 0;
                ZPlugin.GetWindowPosition(out x, out y);

                return new RectInt(x, y, Screen.width, Screen.height);
#else
                return new RectInt(0, 0, Screen.width, Screen.height);
#endif
            }
        }

        /// <summary>
        /// The size in pixels of the application window.
        /// </summary>
        /// 
        /// <remarks>
        /// When running from the Unity Editor, the size corresponds 
        /// to the Game View window.
        /// </remarks>
        public static Vector2Int Size
        {
            get
            {
#if UNITY_EDITOR
                return Rect.size;
#else
                return new Vector2Int(Screen.width, Screen.height);
#endif
            }
        }

        ////////////////////////////////////////////////////////////////
        // Private Static Methods
        ////////////////////////////////////////////////////////////////

#if UNITY_EDITOR
        private static System.IntPtr GetGameViewWindowWindowHandleFromCache(
            EditorWindow gameViewWindow, bool forceCacheRefresh = false)
        {
            bool isGameViewWindowDocked = gameViewWindow.IsDocked();

            bool hasGameViewWindowWindowHandleChanged =
                s_gameViewWindowWindowHandle == System.IntPtr.Zero ||
                s_previousGameViewWindowIsDockedState !=
                    isGameViewWindowDocked ||
                s_previousGameViewWindowInstanceId !=
                    gameViewWindow.GetInstanceID();

            if (hasGameViewWindowWindowHandleChanged || forceCacheRefresh)
            {
                s_gameViewWindowWindowHandle = gameViewWindow.GetWindowHandle();

                s_previousGameViewWindowIsDockedState = isGameViewWindowDocked;
                s_previousGameViewWindowInstanceId =
                    gameViewWindow.GetInstanceID();
            }

            return s_gameViewWindowWindowHandle;
        }
#endif

        ////////////////////////////////////////////////////////////////
        // Private Static Members
        ////////////////////////////////////////////////////////////////

#if UNITY_EDITOR
        private static System.IntPtr s_gameViewWindowWindowHandle =
            System.IntPtr.Zero;
        private static bool s_previousGameViewWindowIsDockedState = false;
        private static int s_previousGameViewWindowInstanceId = 0;
#endif
    }
}
