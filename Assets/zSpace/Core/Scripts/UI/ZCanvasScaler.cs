////////////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2007-2021 zSpace, Inc.  All Rights Reserved.
//
////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

namespace zSpace.Core.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Canvas))]
    public class ZCanvasScaler : MonoBehaviour
    {
        ////////////////////////////////////////////////////////////////////////
        // MonoBehaviour Callbacks
        ////////////////////////////////////////////////////////////////////////

        private void Awake()
        {
            this._rectTransform = this.GetComponent<RectTransform>();
        }

        private void Update()
        {
            this.UpdateSize();
            this.UpdateScale();
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Methods
        ////////////////////////////////////////////////////////////////////////

        private void UpdateSize()
        {
            this._rectTransform.sizeDelta =
                ZProvider.WindowSizePixelsForReferenceResolution;
        }

        private void UpdateScale()
        {
            Vector2 metersPerPixel =
                ZProvider.DisplayMetersPerPixelForReferenceResolution;

            this._rectTransform.localScale = new Vector3(
                metersPerPixel.x,
                metersPerPixel.y,
                Mathf.Min(metersPerPixel.x, metersPerPixel.y));
        }

        ////////////////////////////////////////////////////////////////////////
        // Private Members
        ////////////////////////////////////////////////////////////////////////

        private RectTransform _rectTransform = null;
    }
}
