//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2022 zSpace, Inc.  All Rights Reserved.
//
//////////////////////////////////////////////////////////////////////////

#if ZSPACE_UNITY_RENDER_PIPELINE_UNIVERSAL

using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace zSpace.zView
{
    public static class URPUtility
    {
        //////////////////////////////////////////////////////////////////
        // Public Methods
        //////////////////////////////////////////////////////////////////

        public static bool QueryIsUsingURP()
        {
            UnityEngine.Rendering.RenderPipelineAsset
                activeRenderPipelineAsset =
                    UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset;

            bool isUsingURP =
                activeRenderPipelineAsset.GetType() ==
                typeof(UniversalRenderPipelineAsset);

            return isUsingURP;
        }

        public static bool TryFindZViewURPScriptableRendererAndFeatures(
            int zViewURPRendererIndex,
            out int foundZViewURPScriptableRendererIndex,
            out ScriptableRenderer foundZViewURPScriptableRenderer,
            out VirtualCameraARCompositingURPScriptableRendererFeature
                foundZViewURPARCompositingScriptableRendererFeature)
        {
            // If a URP renderer index is specified, then only look for the
            // zView URP renderer features in the renderer at the specified
            // index.  If no URP renderer index is specified, then attempt to
            // search through all URP renderers to find one that has the zView
            // URP renderer features.

            UniversalRenderPipelineAsset universalRenderPipelineAsset =
                UniversalRenderPipeline.asset;

            if (zViewURPRendererIndex >= 0)
            {
                foundZViewURPScriptableRendererIndex = zViewURPRendererIndex;

                foundZViewURPScriptableRenderer =
                    universalRenderPipelineAsset.GetRenderer(
                        zViewURPRendererIndex);

                bool didFindARCompositingFeature =
                    TryFindZViewURPARCompositingScriptableRendererFeatureInScriptableRenderer(
                        foundZViewURPScriptableRenderer,
                        out foundZViewURPARCompositingScriptableRendererFeature);

                return didFindARCompositingFeature;
            }
            else
            {
                ScriptableRenderer defaultRenderer =
                    universalRenderPipelineAsset.scriptableRenderer;

                int numRenderers = -1;

                if (s_universalRenderPipelineAssetClassRedererDataListFieldInfo !=
                    null)
                {
                    ScriptableRendererData[] rendererDataList =
                        s_universalRenderPipelineAssetClassRedererDataListFieldInfo
                            .GetValue(universalRenderPipelineAsset)
                            as ScriptableRendererData[];

                    if (rendererDataList != null)
                    {
                        numRenderers = rendererDataList.Length;
                    }
                }

                bool seenDefaultRendererOnce = false;

                const int
                    MAX_ALLOWED_NULL_RENDERERS_BETWEEN_NON_NULL_RENDERERS = 5;

                int numNullRenderersSeenSinceLastNonNullRenderer = 0;

                int curRendererIndex = 0;

                while (true)
                {
                    // If the number of renderers is known, then use that
                    // information to stop iterating through the sequence of
                    // renderers once the maximum valid renderer index is
                    // exceeded.
                    if (numRenderers >= 0 && curRendererIndex >= numRenderers)
                    {
                        break;
                    }

                    ScriptableRenderer curRenderer =
                        universalRenderPipelineAsset.GetRenderer(
                            curRendererIndex);

                    // HACK:  If the number of renderers is not known, attempt
                    // to detect the end of the sequence of renderers by
                    // tracking how many times the default renderer has been
                    // seen.  When the current renderer index is greater than
                    // the maximum valid renderer index, the GetRenderer() call
                    // above is expected to return the default renderer.  The
                    // default renderer is also expected to be seen once before
                    // exceeding the maximum valid renderer index.  As a
                    // result, when the default renderer has been seen more
                    // than once, then the end of the sequence of renderers has
                    // been reached.
                    //
                    // Note:  This will not work if the default renderer has
                    // been explicitly added to the sequence of renderers more
                    // than once.
                    //
                    // Note:  This hack is used because querying the number of
                    // renderers is currently only possible using reflection
                    // (the UniversalRenderPipelineAsset class does not expose
                    // the number of renderers via its public interface), which
                    // may fail if the implementation of the
                    // UniversalRenderPipelineAsset class is changed.
                    if (numRenderers < 0)
                    {
                        if (curRenderer == defaultRenderer)
                        {
                            if (seenDefaultRendererOnce)
                            {
                                break;
                            }
                            else
                            {
                                seenDefaultRendererOnce = true;
                            }
                        }

                        // As a safeguard in case the GetRenderer()
                        // implementation is changed to return null for
                        // out-of-bounds renderer indexes, also stop iteration
                        // if more than a maximum allowed number of null
                        // renderers are seen since the last non-null renderer.
                        if (curRenderer == null)
                        {
                            ++numNullRenderersSeenSinceLastNonNullRenderer;

                            if (numNullRenderersSeenSinceLastNonNullRenderer >
                                MAX_ALLOWED_NULL_RENDERERS_BETWEEN_NON_NULL_RENDERERS)
                            {
                                break;
                            }
                        }
                        else
                        {
                            numNullRenderersSeenSinceLastNonNullRenderer = 0;
                        }
                    }

                    // Null renderers do not necessarilly indicate that the end
                    // of the sequence of renderers has been reached.  Because
                    // of this, ignore null renderers rather than stopping
                    // iteration if a null renderer is encountered (unless too
                    // many null renderers are seen in a row; see above for
                    // details).
                    if (curRenderer != null)
                    {
                        bool didFindARCompositingFeature =
                            TryFindZViewURPARCompositingScriptableRendererFeatureInScriptableRenderer(
                                curRenderer,
                                out foundZViewURPARCompositingScriptableRendererFeature);

                        if (didFindARCompositingFeature)
                        {
                            foundZViewURPScriptableRendererIndex =
                                curRendererIndex;
                            foundZViewURPScriptableRenderer = curRenderer;
                            return true;
                        }
                    }

                    ++curRendererIndex;
                }

                foundZViewURPScriptableRendererIndex = -1;
                foundZViewURPScriptableRenderer = null;
                foundZViewURPARCompositingScriptableRendererFeature = null;
                return false;
            }
        }

        public static bool
            TryFindZViewURPARCompositingScriptableRendererFeatureInScriptableRenderer(
                ScriptableRenderer scriptableRenderer,
                out VirtualCameraARCompositingURPScriptableRendererFeature
                    foundZViewURPARCompositingScriptableRendererFeature)
        {
            if (s_scriptableRendererClassRendererFeaturesPropertyInfo == null)
            {
                foundZViewURPARCompositingScriptableRendererFeature = null;
                return false;
            }

            List<ScriptableRendererFeature> scriptableRendererFeatures =
                s_scriptableRendererClassRendererFeaturesPropertyInfo.GetValue(
                    scriptableRenderer) as List<ScriptableRendererFeature>;

            if (scriptableRendererFeatures == null)
            {
                foundZViewURPARCompositingScriptableRendererFeature = null;
                return false;
            }

            for (int i = 0; i < scriptableRendererFeatures.Count; ++i)
            {
                ScriptableRendererFeature curScriptableRendererFeature =
                    scriptableRendererFeatures[i];

                if (!(curScriptableRendererFeature is
                    VirtualCameraARCompositingURPScriptableRendererFeature))
                {
                    continue;
                }

                foundZViewURPARCompositingScriptableRendererFeature =
                    curScriptableRendererFeature as
                        VirtualCameraARCompositingURPScriptableRendererFeature;
                return true;
            }

            foundZViewURPARCompositingScriptableRendererFeature = null;
            return false;
        }

        //////////////////////////////////////////////////////////////////
        // Static Constructor
        //////////////////////////////////////////////////////////////////

        static URPUtility()
        {
            s_universalRenderPipelineAssetClassRedererDataListFieldInfo =
                typeof(UniversalRenderPipelineAsset).GetField(
                    "m_RendererDataList",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            if (s_universalRenderPipelineAssetClassRedererDataListFieldInfo ==
                null)
            {
                Debug.LogWarning(
                    "zView URP logic failed to find " +
                    "UniversalRenderPipelineAsset.m_RendererDataList field " +
                    "via reflection. zView may not work with URP.");
            }

            s_scriptableRendererClassRendererFeaturesPropertyInfo =
                typeof(ScriptableRenderer).GetProperty(
                    "rendererFeatures",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            if (s_scriptableRendererClassRendererFeaturesPropertyInfo == null)
            {
                Debug.LogError(
                    "zView URP logic failed to find " +
                    "ScriptableRenderer.rendererFeatures property via " +
                    "reflection. zView will not work with URP.");
            }
        }

        //////////////////////////////////////////////////////////////////
        // Private Members
        //////////////////////////////////////////////////////////////////

        private static readonly FieldInfo
            s_universalRenderPipelineAssetClassRedererDataListFieldInfo;

        private static readonly PropertyInfo
            s_scriptableRendererClassRendererFeaturesPropertyInfo;
    }
}

#endif
