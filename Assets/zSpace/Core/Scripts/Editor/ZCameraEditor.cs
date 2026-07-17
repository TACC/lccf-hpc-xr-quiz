////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2020 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

using System.Linq;

using UnityEditor;
using UnityEngine;

namespace zSpace.Core
{
    [CustomEditor(typeof(ZCamera))]
    public class ZCameraEditor : Editor
    {
        ////////////////////////////////////////////////////////////////////////
        // Editor Callbacks
        ////////////////////////////////////////////////////////////////////////

        public override void OnInspectorGUI()
        {
            this.InitializeGUIStyles();

            this.CheckIsMainCamera();

            DrawDefaultInspector();

            this.serializedObject.ApplyModifiedProperties();
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Methods
        ////////////////////////////////////////////////////////////////////////

        private void InitializeGUIStyles()
        {
            if (this._helpBoxStyle == null)
            {
                this._helpBoxStyle = GUI.skin.GetStyle("HelpBox");
                this._helpBoxStyle.richText = true;
            }
        }

        private void CheckIsMainCamera()
        {
            ZCamera camera = this.target as ZCamera;

            // Check whether this is the main camera.
            if (!camera.CompareTag("MainCamera") &&
                ZProvider.IsInitialized &&
                ZProvider.Context != null &&
                ZProvider.Context.IsXROverlaySupported)
            {
                EditorGUILayout.HelpBox(
                    "<b>EDITOR:</b> This camera will not render to the " +
                    "XR Overlay. To enable XR Overlay rendering, please " +
                    "set this camera's associated tag to <color=#add8e6ff>" +
                    "MainCamera</color>.",
                    MessageType.Info);

                EditorGUILayout.Space();
            }
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Members
        ////////////////////////////////////////////////////////////////////////

        private GUIStyle _helpBoxStyle = null;
    }

    [CustomPropertyDrawer(
        typeof(ZCameraEyeColorRenderTextureDXGIFormatPropertyAttribute))]
    public class ZCameraEyeColorRenderTextureDXGIFormatPropertyDrawer :
        PropertyDrawer
    {
        ////////////////////////////////////////////////////////////////////////
        // Editor Callbacks
        ////////////////////////////////////////////////////////////////////////

        public override void OnGUI(
            Rect position, SerializedProperty property, GUIContent label)
        {
            // Note: The Unity editor does not officially support using the
            // EditorGUILayout methods in custom PropertyDrawer classes. To
            // work around this, this method uses the EditorGUI methods and
            // does manual layout calculations (some layout calculations are
            // also done in the GetPropertyHeight() method).
            //
            // Side note: In some Unity versions, it seems to be possible to
            // use the EditorGUILayout methods in a custom PropertyDrawer class
            // if that custom PropertyDrawer class is only used for properties
            // of MonoBehaviour classes that also use a custom Editor class
            // (there are still some issues with layout in this case, but they
            // can be worked around by making the GetPropertyHeight() method
            // return a height of 0). Even though this is possible, this method
            // still does not use the EditorGUILayout methods because they are
            // not officially supported, so the behavior that allows them to
            // work when a custom Editor class is also being used could break
            // at any time.

            if (this._dxgiFormatErrorMessageString != null)
            {
                Rect errorMessagePosition = new Rect(
                    x: position.x,
                    y: position.y,
                    width: position.width,
                    height: this._dxgiFormatErrorMessageHeight);

                EditorGUI.HelpBox(
                    errorMessagePosition,
                    this._dxgiFormatErrorMessageString,
                    MessageType.Error);
            }

            Rect propertyFieldPosition = new Rect(
                x: position.x,
                y: position.y + this._propertyFieldYOffset,
                width: position.width,
                height: EditorGUIUtility.singleLineHeight);

            EditorGUI.PropertyField(
                propertyFieldPosition,
                property,
                new GUIContent(
                    "Eye Color Render Texture DXGI Format",
                    tooltip:
                        "The integer value of the DXGI format to use for " +
                        "the eye color render textures. Set to 0 to attempt " +
                        "to determine the DXGI format based on the selected " +
                        "eye color render texture format. If not set to 0, " +
                        "then must correspond to a valid color DXGI format " +
                        "with a number, order, size, and type of channels " +
                        "that is compatible with the selected eye color " +
                        "render texture format."));
        }

        public override float GetPropertyHeight(
            SerializedProperty property, GUIContent label)
        {
            this.InitializeGUIStyles();

            this._dxgiFormatErrorMessageString =
                this.GetDXGIFormatErrorMessageString(property);

            if (this._dxgiFormatErrorMessageString != null)
            {
                GUIContent dxgiFormatErrorMessageGUIContent = new GUIContent(
                    this._dxgiFormatErrorMessageString);

                this._dxgiFormatErrorMessageHeight =
                    this._helpBoxStyle.CalcHeight(
                        dxgiFormatErrorMessageGUIContent,
                        EditorGUIUtility.currentViewWidth);

                this._propertyFieldYOffset =
                    this._dxgiFormatErrorMessageHeight +
                    EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                this._propertyFieldYOffset = 0.0f;
            }

            float totalHeight =
                this._propertyFieldYOffset +
                EditorGUIUtility.singleLineHeight +
                EditorGUIUtility.standardVerticalSpacing;

            return totalHeight;
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Methods
        ////////////////////////////////////////////////////////////////////////

        private void InitializeGUIStyles()
        {
            if (this._helpBoxStyle == null)
            {
                this._helpBoxStyle = GUI.skin.GetStyle("HelpBox");
            }
        }

        private string GetDXGIFormatErrorMessageString(
            SerializedProperty property)
        {
            ZCamera zCamera = (ZCamera)(property.serializedObject.targetObject);

            uint eyeColorRenderTextureDxgiFormat =
                ZCamera.GetDXGIFormatForRenderTextureFormat(
                    zCamera.EyeColorRenderTextureFormat,
                    // Assume that the render texture format is not an sRGB
                    // format for the purposes of determining whether a DXGI
                    // format for the render texture format is known. This is
                    // safe because all supported DXGI sRGB formats also have a
                    // corresponding non-sRGB format.
                    isSRGBFormat: false);

            string errorMessageString = null;

            if (eyeColorRenderTextureDxgiFormat == 0 &&
                zCamera.EyeColorRenderTextureDXGIFormat == 0)
            {
                errorMessageString = string.Format(
                    "A DXGI format for the eye color render texture " +
                    "must be explicitly specified because the DXGI " +
                    "format corresponding to the currently selected eye " +
                    "color render texture format ({0}) is not known.",
                    zCamera.EyeColorRenderTextureFormat);
            }

            return errorMessageString;
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Members
        ////////////////////////////////////////////////////////////////////////

        private GUIStyle _helpBoxStyle = null;

        private string _dxgiFormatErrorMessageString = null;
        private float _dxgiFormatErrorMessageHeight = 0.0f;

        private float _propertyFieldYOffset = 0.0f;
    }
}
