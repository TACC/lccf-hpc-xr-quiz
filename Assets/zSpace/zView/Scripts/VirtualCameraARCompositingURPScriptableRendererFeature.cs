//////////////////////////////////////////////////////////////////////////
//
//  Copyright (C) 2022 zSpace, Inc.  All Rights Reserved.
//
//////////////////////////////////////////////////////////////////////////

#if ZSPACE_UNITY_RENDER_PIPELINE_UNIVERSAL

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace zSpace.zView
{
    public class VirtualCameraARCompositingURPScriptableRendererFeature :
        ScriptableRendererFeature
    {
        //////////////////////////////////////////////////////////////////
        // Public Properties
        //////////////////////////////////////////////////////////////////

        public Color MaskColor { get; set; }

        public int MaskLayer { get; set; }
        public int EnvironmentLayers { get; set; }

        public bool IsTransparencyEnabled { get; set; }

        public RenderTexture FullSceneRenderTexture { get; set; }
        public RenderTexture NonEnvironmentRenderTexture { get; set; }

        public ushort ImageWidth { get; set; }
        public ushort ImageHeight { get; set; }

        //////////////////////////////////////////////////////////////////
        // ScriptableRendererFeature Overrides
        //////////////////////////////////////////////////////////////////

        public override void Create()
        {
            this._maskDepthRenderTargetHandle.Init("_MaskDepthTex");
            this._nonEnvironmentDepthRenderTargetHandle.Init("_NonEnvironmentDepthTex");


            _maskDepthRenderPass = new DepthRenderPass("Mask");
            _nonEnvironmentDepthRenderPass =
                new DepthRenderPass("Non-Environment");

            _compositingRenderPass = new CompositingRenderPass(this);
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            CommandBuffer cmd =
                CommandBufferPool.Get(RenderTargetCreationCommandBufferName);

            RenderTextureDescriptor descriptor =
                renderingData.cameraData.cameraTargetDescriptor;

            // Mask depth render pass.

            {
                _maskDepthRenderPass.SetDepthRenderTargetHandle(
                    _maskDepthRenderTargetHandle);
                _maskDepthRenderPass.SetDepthRenderTextureDescriptor(
                    descriptor);

                _maskDepthRenderPass.SetLayerMask(1 << this.MaskLayer);

                renderer.EnqueuePass(_maskDepthRenderPass);
            }

            // Non-environment depth render pass.
            //
            // This render pass is only performed if transparency is not
            // enabled because it is not used if transparency is enabled.

            if (!this.IsTransparencyEnabled)
            {
                _nonEnvironmentDepthRenderPass.SetDepthRenderTargetHandle(
                    _nonEnvironmentDepthRenderTargetHandle);
                _nonEnvironmentDepthRenderPass.SetDepthRenderTextureDescriptor(
                    descriptor);

                _nonEnvironmentDepthRenderPass.SetLayerMask(
                    renderingData.cameraData.camera.cullingMask &
                    ~(1 << this.MaskLayer) & ~(this.EnvironmentLayers));

                renderer.EnqueuePass(_nonEnvironmentDepthRenderPass);
            }

            renderer.EnqueuePass(_compositingRenderPass);
        }

        //////////////////////////////////////////////////////////////////
        // Private Types
        //////////////////////////////////////////////////////////////////

        public class DepthRenderPass : ScriptableRenderPass
        {
            //////////////////////////////////////////////////////////////////
            // Public Constants
            //////////////////////////////////////////////////////////////////

            const string ProfilerTagRoot =
                "zView Virtual Camera AR Depth Pass";

            //////////////////////////////////////////////////////////////////
            // Public Methods
            //////////////////////////////////////////////////////////////////

            public DepthRenderPass(string passInstanceDescription)
            {
                _profilerTag = string.Format(
                    "{0} ({1})", ProfilerTagRoot, passInstanceDescription);

                this.renderPassEvent = RenderPassEvent.AfterRendering;

                // Note:  This ShaderTagId must match the value of the
                // LightMode tag in the depth render shader.
                _depthRenderShaderTagId = new ShaderTagId("DepthOnly");

                _depthRenderShader =
                    Shader.Find("zSpace/zView/URP/DepthRender");
                _depthRenderMaterial = new Material(_depthRenderShader);

                _renderFilteringSettings = new FilteringSettings(
                    renderQueueRange: RenderQueueRange.opaque,
                    layerMask: -1);
            }

            public void SetDepthRenderTargetNameId(
                int depthRenderTargetNameId)
            {
                _depthRenderTargetId =
                    new RenderTargetIdentifier(depthRenderTargetNameId);
            }

            public void SetDepthRenderTargetHandle(
                RenderTargetHandle depthRenderTargetHandle)
            {
                _depthRenderTargetHandle = depthRenderTargetHandle;
            }

            public void SetDepthRenderTextureDescriptor(
                RenderTextureDescriptor depthRenderTextureDescriptor)
            {
                _depthRenderTextureDescriptor = depthRenderTextureDescriptor;
            }

            public void SetLayerMask(int layerMask)
            {
                _renderFilteringSettings.layerMask = layerMask;
            }

            //////////////////////////////////////////////////////////////////
            // ScriptableRenderPass Overrides
            //////////////////////////////////////////////////////////////////

            public override void Configure(
                CommandBuffer cmd,
                RenderTextureDescriptor cameraTextureDescriptor)
            {
                cmd.GetTemporaryRT(
                    _depthRenderTargetHandle.id,
                    _depthRenderTextureDescriptor,
                    FilterMode.Point);

                this.ConfigureTarget(_depthRenderTargetHandle.Identifier());
                // Clear the depth render target to white, which corresponds to
                // the depth value of geometry that is the maximum possible
                // distance away from the camera.
                this.ConfigureClear(
                    ClearFlag.All, new Color(1.0f, 1.0f, 1.0f, 1.0f));
            }

            public override void Execute(
                ScriptableRenderContext context,
                ref RenderingData renderingData)
            {
                var cmd = CommandBufferPool.Get(_profilerTag);

                using (new ProfilingScope(cmd, new ProfilingSampler(_profilerTag)))
                {
                    context.ExecuteCommandBuffer(cmd);

                    cmd.Clear();

                    // Update globals for the depth render shader.
                    cmd.SetGlobalFloat(
                        "_Log2FarPlusOne",
                        (float)Math.Log(
                            renderingData.cameraData.camera.farClipPlane + 1,
                            2));

                    context.ExecuteCommandBuffer(cmd);

                    var cameraDefaultOpaqueSortFlags =
                        renderingData.cameraData.defaultOpaqueSortFlags;

                    var depthRenderDrawingSettings = this.CreateDrawingSettings(
                        _depthRenderShaderTagId,
                        ref renderingData,
                        cameraDefaultOpaqueSortFlags);

                    if (depthRenderDrawingSettings.overrideMaterial != null)
                    {
                        Debug.Log(
                            "Depth render DrawingSettings already has " +
                            "override material after CreateDrawingSettings() " +
                            "call.");
                    }

                    depthRenderDrawingSettings.overrideMaterial =
                        _depthRenderMaterial;

                    context.DrawRenderers(
                        renderingData.cullResults,
                        ref depthRenderDrawingSettings,
                        ref _renderFilteringSettings);
                }

                context.ExecuteCommandBuffer(cmd);

                CommandBufferPool.Release(cmd);
            }

            public override void FrameCleanup(CommandBuffer cmd)
            {
                if (_depthRenderTargetHandle != RenderTargetHandle.CameraTarget)
                {
                    cmd.ReleaseTemporaryRT(_depthRenderTargetHandle.id);
                    _depthRenderTargetHandle = RenderTargetHandle.CameraTarget;
                }
            }

            //////////////////////////////////////////////////////////////////
            // Private Members
            //////////////////////////////////////////////////////////////////

            private string _profilerTag;

            private RenderTargetIdentifier _depthRenderTargetId;
            private RenderTargetHandle _depthRenderTargetHandle =
                RenderTargetHandle.CameraTarget;
            private RenderTextureDescriptor _depthRenderTextureDescriptor;

            private ShaderTagId _depthRenderShaderTagId;

            private Shader _depthRenderShader;
            private Material _depthRenderMaterial;

            private FilteringSettings _renderFilteringSettings;
        }

        public class CompositingRenderPass : ScriptableRenderPass
        {
            //////////////////////////////////////////////////////////////////
            // Public Constants
            //////////////////////////////////////////////////////////////////

            const string ProfilerTag =
                "zView Virtual Camera AR Compositing Pass";

            //////////////////////////////////////////////////////////////////
            // Public Methods
            //////////////////////////////////////////////////////////////////

            public CompositingRenderPass(
                VirtualCameraARCompositingURPScriptableRendererFeature
                    rendererFeature)
            {
                _rendererFeature = rendererFeature;

                this.renderPassEvent = RenderPassEvent.AfterRendering;

                _compositorShaderRGB =
                    Shader.Find("zSpace/zView/URP/CompositorRGB");

                _compositorMaterialRGB = new Material(_compositorShaderRGB);

                _compositorShaderRGBA =
                    Shader.Find("zSpace/zView/URP/CompositorRGBA");

                _compositorMaterialRGBA = new Material(_compositorShaderRGBA);
            }

            //////////////////////////////////////////////////////////////////
            // ScriptableRenderPass Overrides
            //////////////////////////////////////////////////////////////////

            public override void OnCameraSetup(
                CommandBuffer cmd, ref RenderingData renderingData)
            {
                this.ConfigureInput(ScriptableRenderPassInput.None);

                this.ConfigureClear(ClearFlag.None, Color.green);

                this.ConfigureTarget(
                    renderingData.cameraData.renderer.cameraColorTarget);
            }

            public override void Execute(
                ScriptableRenderContext context,
                ref RenderingData renderingData)
            {
                CommandBuffer cmd = CommandBufferPool.Get(ProfilerTag);

                using (new ProfilingScope(
                    cmd, new ProfilingSampler(ProfilerTag)))
                {
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    if (_rendererFeature.IsTransparencyEnabled)
                    {
                        cmd.SetGlobalTexture(
                            "_NonEnvironmentColorTex",
                            new RenderTargetIdentifier(
                                _rendererFeature.NonEnvironmentRenderTexture));

                        cmd.SetGlobalTexture(
                            "_MaskDepthTex",
                            _rendererFeature._maskDepthRenderTargetHandle.id);

                        context.ExecuteCommandBuffer(cmd);

                        Blit(
                            cmd,
                            _rendererFeature.FullSceneRenderTexture,
                            renderingData.cameraData.renderer.cameraColorTarget,
                            _compositorMaterialRGBA);
                    }
                    else
                    {
                        cmd.SetGlobalTexture(
                            "_NonEnvironmentDepthTex",
                            _rendererFeature
                                ._nonEnvironmentDepthRenderTargetHandle.id);

                        cmd.SetGlobalTexture(
                            "_MaskDepthTex",
                            _rendererFeature._maskDepthRenderTargetHandle.id);

                        cmd.SetGlobalColor(
                            "_MaskColor", _rendererFeature.MaskColor);

                        context.ExecuteCommandBuffer(cmd);

                        Blit(
                            cmd,
                            _rendererFeature.FullSceneRenderTexture,
                            renderingData.cameraData.renderer.cameraColorTarget,
                            _compositorMaterialRGB);
                    }
                }

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);
            }

            //////////////////////////////////////////////////////////////////
            // Private Members
            //////////////////////////////////////////////////////////////////

            private VirtualCameraARCompositingURPScriptableRendererFeature
                _rendererFeature;

            private Shader _compositorShaderRGB;
            private Material _compositorMaterialRGB;
            private Shader _compositorShaderRGBA;
            private Material _compositorMaterialRGBA;
        }

        //////////////////////////////////////////////////////////////////
        // Private Members
        //////////////////////////////////////////////////////////////////

        private const string RenderTargetCreationCommandBufferName =
            "zSpace_zViewVirtualCameraARCompositingURPRendererFeature_" +
            "RenderTargetCreationCommandBuffer";

        private static readonly int s_maskDepthRenderTargetNameId =
            Shader.PropertyToID(
                "zSpace_zViewVirtualCameraARCompositingURPRendererFeature_" +
                "MaskDepthRenderTarget");
        private static readonly int s_nonEnvironmentDepthRenderTargetNameId =
            Shader.PropertyToID(
                "zSpace_zViewVirtualCameraARCompositingURPRendererFeature_" +
                "NonEnvironmentDepthRenderTarget");

        private RenderTargetHandle _maskDepthRenderTargetHandle;
        private RenderTargetHandle _nonEnvironmentDepthRenderTargetHandle;

        private DepthRenderPass _maskDepthRenderPass;
        private DepthRenderPass _nonEnvironmentDepthRenderPass;

        private CompositingRenderPass _compositingRenderPass;
    }
}

#endif