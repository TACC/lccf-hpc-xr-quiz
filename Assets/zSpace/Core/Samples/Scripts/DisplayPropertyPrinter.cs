////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2021 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

using System;
using UnityEngine;
using UnityEngine.UI;

using zSpace.Core.Sdk;

namespace zSpace.Core.Samples
{
    public class DisplayPropertyPrinter : MonoBehaviour
    {
        ////////////////////////////////////////////////////////////////////////
        // Monobehaviour Callbacks
        ////////////////////////////////////////////////////////////////////////

        void Start()
        {
            this._text = this.gameObject.GetComponent<Text>();
            this.UpdateValues();
        }

        ////////////////////////////////////////////////////////////////////////
        // Public Methods
        ////////////////////////////////////////////////////////////////////////

        public void UpdateValues()
        {
            this._text.text = "Display Properties\n" +
                "\nZProvider" +
                "\nDisplayReferenceSize: {0}" +
                "\nDisplayReferenceResolution: {1}" +
                "\nDisplaySize: {2}" +
                "\nDisplayResolution: {3}" +
                "\nDisplayMetersPerPixelForReferenceResolution: {4}" +
                "\nDisplayMetersPerPixelForNativeResolution: {5}" +
                "\nDisplayScale: {6}" +
                "\nDisplayScaleFactor: {7}" +
                "\nWindowSize: {8}" +
                "\nWindowSizePixelsForReferenceResolution: {9}" +
                "\nWindowSizePixelsForNativeResolution: {10}" +
                "\n\nCurrentDisplay Attributes\n {11}";

            this._text.text = string.Format(this._text.text,
                ZProvider.DisplayReferenceSize.ToString("N5"),
                ZProvider.DisplayReferenceResolution.ToString(),
                ZProvider.DisplaySize.ToString("N5"),
                ZProvider.DisplayResolution.ToString(),
                ZProvider.DisplayMetersPerPixelForReferenceResolution
                    .ToString("N8"),
                ZProvider.DisplayMetersPerPixelForNativeResolution
                    .ToString("N8"),
                ZProvider.DisplayScale.ToString("N5"),
                ZProvider.DisplayScaleFactor,
                ZProvider.WindowSize.ToString("N8"),
                ZProvider.WindowSizePixelsForReferenceResolution.ToString(),
                ZProvider.WindowSizePixelsForNativeResolution.ToString(),
                this.DisplayAttributesAsString(ZProvider.CurrentDisplay));
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Methods
        ////////////////////////////////////////////////////////////////////////

        private string DisplayAttributesAsString(ZDisplay display)
        {
            string attributeString = "";
            foreach (ZDisplayAttribute attribute in
                Enum.GetValues(typeof(ZDisplayAttribute)))
            {
                attributeString += attribute.ToString() + ": " +
                    display.GetAttribute(attribute) + "\n";
            }
            return attributeString;
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Members
        ////////////////////////////////////////////////////////////////////////

        private ZDisplay _display;
        private Text _text;
    }
}
