using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Adapted from https://github.com/Unity-Technologies/ScriptableRenderPipeline/blob/master/com.unity.render-pipelines.universal/Runtime/Passes/RenderObjectsPass.cs
public class ClipSphereRenderPass : ScriptableRenderPass {
    private FilteringSettings filterSettings;
    private List<ShaderTagId> shaderTagIds = new List<ShaderTagId>() {
        new ShaderTagId("SRPDefaultUnlit"),
        new ShaderTagId("UniversalForward"),
        new ShaderTagId("LightweightForward")
    };
    private RenderStateBlock renderStateBlock;

    public static EventHandler<Vector3> OnProxyPass;

    public ClipSphereRenderPass(int layerMask, RenderQueueType queue) {
        if(queue == RenderQueueType.Opaque) {
            filterSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
        } else {
            filterSettings = new FilteringSettings(RenderQueueRange.transparent, layerMask);
        }
        renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        var camera = renderingData.cameraData.camera;
        SortingCriteria sortFlags;
        if(filterSettings.renderQueueRange == RenderQueueRange.opaque) {
            sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
        } else {
            sortFlags = SortingCriteria.CommonTransparent;
        }
        var drawSettings = CreateDrawingSettings(shaderTagIds, ref renderingData, sortFlags);

        CommandBuffer cmd = CommandBufferPool.Get("ClipSphere");
        cmd.EnableShaderKeyword("CLIP_SPHERE_ON");
        cmd.SetGlobalFloat("_ClipObjEdgeThickness", 0.01f);
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        var baseCameraMatrix = camera.worldToCameraMatrix;

        foreach(var proxy in ProxyNode.Instances) {
            if(!proxy.isActiveAndEnabled) {
                continue;
            }
            if(proxy.State == ProxyNode.ProxyState.Minimized) {
                continue;
            }

            foreach(var mark in proxy.Marks) {
                if(mark == null || !mark.isActiveAndEnabled) {
                    continue;
                }

                cmd.SetGlobalColor("_ClipObjEdgeColor", proxy.Color);
                cmd.SetGlobalVector("_ClipObjPosition", mark.transform.position);
                cmd.SetGlobalVector("_ClipObjScale", 0.5f * proxy.ProxyScaleFactor * mark.transform.localScale);
                var clipTransform = GetWarpTransform(proxy.transform, mark.transform);
                cmd.SetGlobalMatrix("_ClipTransform", clipTransform);
                cmd.SetGlobalMatrix("_ClipTransformInv", clipTransform.inverse);
                context.ExecuteCommandBuffer(cmd);

                OnProxyPass?.Invoke(this, GetProxyCameraPosition(proxy.transform, mark.transform));

                // Stupid hack, because the other versions crashed or didn't work
                var scp = new ScriptableCullingParameters();
                camera.TryGetCullingParameters(true, out scp);
                scp.cullingPlaneCount = 6;
                scp.SetCullingPlane(0, new Plane(new Vector3(1, 0, 0), mark.transform.position - new Vector3(1, 0, 0) * mark.transform.localScale.x));
                scp.SetCullingPlane(1, new Plane(new Vector3(-1, 0, 0), mark.transform.position + new Vector3(1, 0, 0) * mark.transform.localScale.x));
                scp.SetCullingPlane(2, new Plane(new Vector3(0, 1, 0), mark.transform.position - new Vector3(0, 1, 0) * mark.transform.localScale.y));
                scp.SetCullingPlane(3, new Plane(new Vector3(0, -1, 0), mark.transform.position + new Vector3(0, 1, 0) * mark.transform.localScale.y));
                scp.SetCullingPlane(4, new Plane(new Vector3(0, 0, 1), mark.transform.position - new Vector3(0, 0, 1) * mark.transform.localScale.z));
                scp.SetCullingPlane(5, new Plane(new Vector3(0, 0, -1), mark.transform.position + new Vector3(0, 0, 1) * mark.transform.localScale.z));
                var cullResults = context.Cull(ref scp);

                context.DrawRenderers(cullResults, ref drawSettings, ref filterSettings, ref renderStateBlock);
            }
        }

        cmd.SetGlobalMatrix("_ClipTransform", Matrix4x4.identity);
        cmd.DisableShaderKeyword("CLIP_SPHERE_ON");
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    private Matrix4x4 GetWarpTransform(Transform proxy, Transform mark) {
        var scale = new Vector3(
            proxy.localScale.x / mark.localScale.x,
            proxy.localScale.y / mark.localScale.y,
            proxy.localScale.z / mark.localScale.z);

        return Matrix4x4.Translate(proxy.position) *
                Matrix4x4.Rotate(proxy.rotation) *
                Matrix4x4.Scale(scale) *
                Matrix4x4.Translate(-mark.position);
    }

    private Vector3 GetProxyCameraPosition(Transform proxy, Transform mark) {
        var cam = Camera.main.transform;

        var scale = new Vector3(
            mark.localScale.x / proxy.localScale.x,
            mark.localScale.y / proxy.localScale.y,
            mark.localScale.z / proxy.localScale.z);

        var proxy2mark = Matrix4x4.Translate(mark.position) *
                Matrix4x4.Rotate(Quaternion.FromToRotation(proxy.forward, mark.forward)) *
                Matrix4x4.Scale(scale) *
                Matrix4x4.Translate(-proxy.position);

        return proxy2mark.MultiplyPoint(cam.position);
    }
}

// Adapted from https://github.com/Unity-Technologies/ScriptableRenderPipeline/blob/master/com.unity.render-pipelines.universal/Runtime/RendererFeatures/RenderObjects.cs
public class ClipSphereRenderFeature : ScriptableRendererFeature {
    private ClipSphereRenderPass opaquePass;
    private ClipSphereRenderPass transparentPass;

    [System.Serializable]
    public class ClipSphereSettings {
        public LayerMask LayerMask;
    }

    public ClipSphereSettings settings = new ClipSphereSettings();

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if(opaquePass != null) {
            renderer.EnqueuePass(opaquePass);
        }
        if(transparentPass != null) {
            renderer.EnqueuePass(transparentPass);
        }
    }

    public override void Create() {
        opaquePass = new ClipSphereRenderPass(settings.LayerMask, RenderQueueType.Opaque);
        opaquePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        transparentPass = new ClipSphereRenderPass(settings.LayerMask, RenderQueueType.Transparent);
        transparentPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }
}
