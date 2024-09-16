using UnityEngine;

namespace UnityGameFramework.Runtime
{
    public sealed partial class DebuggerComponent : GameFrameworkComponent
    {
        private sealed class GraphicsInformationWindow : ScrollableDebuggerWindowBase
        {
            protected override void OnDrawScrollableWindow()
            {
                GUILayout.Label("<b>Graphics Information</b>");
                GUILayout.BeginVertical("box");
                {
                    DrawItem("Device ID", SystemInfo.graphicsDeviceID.ToString());
                    DrawItem("Device Name", SystemInfo.graphicsDeviceName);
                    DrawItem("Device Vendor ID", SystemInfo.graphicsDeviceVendorID.ToString());
                    DrawItem("Device Vendor", SystemInfo.graphicsDeviceVendor);
                    DrawItem("Device Type", SystemInfo.graphicsDeviceType.ToString());
                    DrawItem("Device Version", SystemInfo.graphicsDeviceVersion);
                    DrawItem("Memory Size", Utility.Text.Format("{0} MB", SystemInfo.graphicsMemorySize));
                    DrawItem("Multi Threaded", SystemInfo.graphicsMultiThreaded.ToString());
                    DrawItem("Rendering Threading Mode", SystemInfo.renderingThreadingMode.ToString());
                    DrawItem("HRD Display Support Flags", SystemInfo.hdrDisplaySupportFlags.ToString());
                    DrawItem("Shader Level", GetShaderLevelString(SystemInfo.graphicsShaderLevel));
                    DrawItem("Global Maximum LOD", Shader.globalMaximumLOD.ToString());
                    DrawItem("Global Render Pipeline", Shader.globalRenderPipeline);
                    DrawItem("Min OpenGLES Version", Graphics.minOpenGLESVersion.ToString());
                    DrawItem("Active Tier", Graphics.activeTier.ToString());
                    DrawItem("Active Color Gamut", Graphics.activeColorGamut.ToString());
                    DrawItem("Preserve Frame Buffer Alpha", Graphics.preserveFramebufferAlpha.ToString());
                    DrawItem("NPOT Support", SystemInfo.npotSupport.ToString());
                    DrawItem("Max Texture Size", SystemInfo.maxTextureSize.ToString());
                    DrawItem("Supported Render Target Count", SystemInfo.supportedRenderTargetCount.ToString());
                    DrawItem("Supported Random Write Target Count",
                        SystemInfo.supportedRandomWriteTargetCount.ToString());
                    DrawItem("Copy Texture Support", SystemInfo.copyTextureSupport.ToString());
                    DrawItem("Uses Reversed ZBuffer", SystemInfo.usesReversedZBuffer.ToString());
                    DrawItem("Max Cubemap Size", SystemInfo.maxCubemapSize.ToString());
                    DrawItem("Graphics UV Starts At Top", SystemInfo.graphicsUVStartsAtTop.ToString());
                    DrawItem("Constant Buffer Offset Alignment", SystemInfo.constantBufferOffsetAlignment.ToString());


                    DrawItem("Has Hidden Surface Removal On GPU", SystemInfo.hasHiddenSurfaceRemovalOnGPU.ToString());
                    DrawItem("Has Dynamic Uniform Array Indexing In Fragment Shaders",
                        SystemInfo.hasDynamicUniformArrayIndexingInFragmentShaders.ToString());
                    DrawItem("Has Mip Max Level", SystemInfo.hasMipMaxLevel.ToString());
                    DrawItem("Uses Load Store Actions", SystemInfo.usesLoadStoreActions.ToString());
                    DrawItem("Max Compute Buffer Inputs Compute", SystemInfo.maxComputeBufferInputsCompute.ToString());
                    DrawItem("Max Compute Buffer Inputs Domain", SystemInfo.maxComputeBufferInputsDomain.ToString());
                    DrawItem("Max Compute Buffer Inputs Fragment",
                        SystemInfo.maxComputeBufferInputsFragment.ToString());
                    DrawItem("Max Compute Buffer Inputs Geometry",
                        SystemInfo.maxComputeBufferInputsGeometry.ToString());
                    DrawItem("Max Compute Buffer Inputs Hull", SystemInfo.maxComputeBufferInputsHull.ToString());
                    DrawItem("Max Compute Buffer Inputs Vertex", SystemInfo.maxComputeBufferInputsVertex.ToString());
                    DrawItem("Max Compute Work Group Size", SystemInfo.maxComputeWorkGroupSize.ToString());
                    DrawItem("Max Compute Work Group Size X", SystemInfo.maxComputeWorkGroupSizeX.ToString());
                    DrawItem("Max Compute Work Group Size Y", SystemInfo.maxComputeWorkGroupSizeY.ToString());
                    DrawItem("Max Compute Work Group Size Z", SystemInfo.maxComputeWorkGroupSizeZ.ToString());
                    DrawItem("Supports Sparse Textures", SystemInfo.supportsSparseTextures.ToString());
                    DrawItem("Supports 3D Textures", SystemInfo.supports3DTextures.ToString());
                    DrawItem("Supports Shadows", SystemInfo.supportsShadows.ToString());
                    DrawItem("Supports Raw Shadow Depth Sampling",
                        SystemInfo.supportsRawShadowDepthSampling.ToString());
                    DrawItem("Supports Compute Shader", SystemInfo.supportsComputeShaders.ToString());
                    DrawItem("Supports Instancing", SystemInfo.supportsInstancing.ToString());

                    DrawItem("Supports 2D Array Textures", SystemInfo.supports2DArrayTextures.ToString());
                    DrawItem("Supports Motion Vectors", SystemInfo.supportsMotionVectors.ToString());
                    DrawItem("Supports Cubemap Array Textures", SystemInfo.supportsCubemapArrayTextures.ToString());
                    DrawItem("Supports 3D Render Textures", SystemInfo.supports3DRenderTextures.ToString());

                    DrawItem("Supports Texture Wrap Mirror Once", SystemInfo.supportsTextureWrapMirrorOnce.ToString());

                    DrawItem("Supports Graphics Fence", SystemInfo.supportsGraphicsFence.ToString());
                    DrawItem("Supports Async Compute", SystemInfo.supportsAsyncCompute.ToString());
                    DrawItem("Supports Multi-sampled Textures", SystemInfo.supportsMultisampledTextures.ToString());

                    DrawItem("Supports Async GPU Readback", SystemInfo.supportsAsyncGPUReadback.ToString());
                    DrawItem("Supports 32bits Index Buffer", SystemInfo.supports32bitsIndexBuffer.ToString());
                    DrawItem("Supports Hardware Quad Topology", SystemInfo.supportsHardwareQuadTopology.ToString());
                    DrawItem("Supports Mip Streaming", SystemInfo.supportsMipStreaming.ToString());
                    DrawItem("Supports Multi-sample Auto Resolve",
                        SystemInfo.supportsMultisampleAutoResolve.ToString());
                    DrawItem("Supports Separated Render Targets Blend",
                        SystemInfo.supportsSeparatedRenderTargetsBlend.ToString());
                    DrawItem("Supports Set Constant Buffer", SystemInfo.supportsSetConstantBuffer.ToString());
                    DrawItem("Supports Geometry Shaders", SystemInfo.supportsGeometryShaders.ToString());
                    DrawItem("Supports Ray Tracing", SystemInfo.supportsRayTracing.ToString());
                    DrawItem("Supports Tessellation Shaders", SystemInfo.supportsTessellationShaders.ToString());
                    DrawItem("Supports Compressed 3D Textures", SystemInfo.supportsCompressed3DTextures.ToString());
                    DrawItem("Supports Conservative Raster", SystemInfo.supportsConservativeRaster.ToString());
                    DrawItem("Supports GPU Recorder", SystemInfo.supportsGpuRecorder.ToString());
                    DrawItem("Supports Multi-sampled 2D Array Textures",
                        SystemInfo.supportsMultisampled2DArrayTextures.ToString());
                    DrawItem("Supports Multiview", SystemInfo.supportsMultiview.ToString());
                    DrawItem("Supports Render Target Array Index From Vertex Shader",
                        SystemInfo.supportsRenderTargetArrayIndexFromVertexShader.ToString());
                }
                GUILayout.EndVertical();
            }

            private string GetShaderLevelString(int shaderLevel)
            {
                return Utility.Text.Format("Shader Model {0}.{1}", shaderLevel / 10, shaderLevel % 10);
            }
        }
    }
}