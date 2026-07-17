////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2021 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

using zSpace.Core.Extensions;

namespace zSpace.Core.Input
{
    public class ZMouse : ZPointer
    {
        ////////////////////////////////////////////////////////////////////////
        // MonoBehaviour Callbacks
        ////////////////////////////////////////////////////////////////////////

        protected override void OnEnable()
        {
            base.OnEnable();

            // Do not change the visibility of the hardware mouse cursor in
            // WebGL builds. Instead, leave the hardware mouse cursor visible
            // at all times. This is necessary because the in-scene mouse
            // cursor (rendered by the ZMouseCursor component) is never shown
            // in WebGL builds due to high latency between mouse movement and
            // updates to the visible position of the in-scene mouse cursor in
            // WebGL builds.
            //
            // This also applies to other hardware mouse cursor visibility
            // changes below that also are disabled in WebGL builds.
#if !UNITY_WEBGL
            Cursor.visible = false;
#endif
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            // See comment in OnEnable() for details on why this is disabled in
            // WebGL builds.
#if !UNITY_WEBGL
            Cursor.visible = true;
#endif
        }

        protected override void Start()
        {
            base.Start();

            // See comment in OnEnable() for details on why this is disabled in
            // WebGL builds.
#if !UNITY_WEBGL
            Cursor.visible = false;
#endif
        }

        protected override void Update()
        {
            base.Update();

#if UNITY_EDITOR
            // Request that the hardware mouse cursor be hidden when it is
            // within the bounds of the game view and not hidden when it is
            // outside the bounds of the game view.  This helps prevent the
            // hardware mouse cursor from being shown at the same time as the
            // in-scene mouse cursor when in the Unity editor while ensuring
            // that the hardware mouse cursor is visible when interacting with
            // the Unity editor UI.

            Vector3 mousePosition = UnityEngine.Input.mousePosition;

            bool isMouseCursorWithinGameView =
                mousePosition.x >= 0.0f &&
                mousePosition.y >= 0.0f &&
                mousePosition.x <= Screen.width &&
                mousePosition.y <= Screen.height;

            Cursor.visible = !isMouseCursorWithinGameView;
#endif
        }

        private void OnApplicationPause(bool isPaused)
        {
            // See comment in OnEnable() for details on why this is disabled in
            // WebGL builds.
#if !UNITY_WEBGL
            Cursor.visible = isPaused;
#endif
        }

        ////////////////////////////////////////////////////////////////////////
        // Public Properties
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// The unique id of the mouse pointer.
        /// </summary>
        public override int Id
        {
            get { return 1000; }
        }

        /// <summary>
        /// The current visibility state of the mouse.
        /// </summary>
        /// 
        /// <remarks>
        /// Since the mouse is not a 6-DOF trackable target and is present
        /// on all platforms we currently support (e.g. Windows), IsVisible
        /// is hard-coded to true.
        /// </remarks>
        public override bool IsVisible
        {
            get { return true; }
        }

        /// <summary>
        /// The number of buttons supported by the mouse.
        /// </summary>
        public override int ButtonCount
        {
            get { return 3; }
        }

        /// <summary>
        /// The current scroll delta for the mouse.
        /// </summary>
        /// 
        /// <remarks>
        /// The scroll delta for the mouse is only stored in Vector2.y
        /// (Vector2.x is ignored).
        /// </remarks>
        public override Vector2 ScrollDelta
        {
            get 
            {
                return UnityEngine.Input.mouseScrollDelta;
            }
        }

        /// <summary>
        /// The pose of the pointer's current end point in world space.
        /// </summary>
        /// 
        /// <remarks>
        /// In this particular case, this will be the the mouse cursor's 
        /// world pose.
        /// </remarks>
        public override Pose EndPointWorldPose
        {
            get
            {
                if (this.EventCamera != null)
                {
                    return new Pose( this.HitInfo.worldPosition,
                        this.EventCamera.ZeroParallaxPose.rotation);
                }
                else
                {
                    return new Pose( this.HitInfo.worldPosition,
                        this.transform.rotation);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // Public Methods
        ////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Gets whether the specified button is pressed.
        /// </summary>
        /// 
        /// <param name="id">
        /// The integer id of the specified button.
        /// </param>
        /// 
        /// <returns>
        /// True if the specified button is pressed. False otherwise.
        /// </returns>
        public override bool GetButton(int id)
        {
            return UnityEngine.Input.GetMouseButton(id);
        }

        ////////////////////////////////////////////////////////////////////////
        // Protected Methods
        ////////////////////////////////////////////////////////////////////////

        protected override Pose ComputeWorldPose()
        {
            Ray mouseRay = this.EventCamera.Camera.ScreenPointToRay(
                UnityEngine.Input.mousePosition);

            return mouseRay.ToPose(this.EventCamera.transform.up);
        }
    }
}
