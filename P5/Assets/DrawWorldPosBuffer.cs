using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.XR;

public class DrawWorldPosBuffer : MonoBehaviour
{
    ComputeBuffer m_ScreenDataVertexBuffer;
    GraphicsBuffer m_QuadIndexesGraphicsBuffer;
    CommandBuffer m_WorldPosCommandBuffer;
    RenderTexture m_worldPosRT;
        
    static readonly int k_WorldPosRTId = Shader.PropertyToID("world_space_rt");
    Camera m_CamMain;

    void Start()
    {
        m_CamMain = Camera.main;
        if (!m_CamMain) return;
        m_CamMain.depthTextureMode = DepthTextureMode.DepthNormals;
        m_ScreenDataVertexBuffer = new ComputeBuffer(4, UnsafeUtility.SizeOf<ScreenDataVertex>());
        UpdateScreenDataVertexBuffer(m_CamMain);
        Shader.SetGlobalBuffer("screen_data_vertex_buffer", m_ScreenDataVertexBuffer);

        // Fill Quad GraphicsBuffer
        m_QuadIndexesGraphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, 6, sizeof(int));
        m_QuadIndexesGraphicsBuffer.SetData(new []{0,1,2,0,2,3});
        // 0---1
        // | \ |
        // 3---2

        // Create RT
        m_worldPosRT = new RenderTexture(
            XRSettings.eyeTextureWidth > 0 ? XRSettings.eyeTextureWidth : m_CamMain.pixelWidth, 
            XRSettings.eyeTextureHeight > 0 ? XRSettings.eyeTextureHeight : m_CamMain.pixelHeight,
            0, GraphicsFormat.R32G32B32A32_SFloat)
        {
            graphicsFormat = GraphicsFormat.R32G32B32A32_SFloat,
            volumeDepth = 2,
            vrUsage = VRTextureUsage.TwoEyes,
            dimension = XRSettings.isDeviceActive ? TextureDimension.Tex2DArray : TextureDimension.Tex2D
        };
        m_worldPosRT.Create();
        Shader.SetGlobalTexture(k_WorldPosRTId, m_worldPosRT);
            
        // Fill CommandBuffer
        m_WorldPosCommandBuffer = new CommandBuffer {name = "Generate World Position RenderTexture"};
        m_WorldPosCommandBuffer.SetRenderTarget(m_worldPosRT);
        m_WorldPosCommandBuffer.DrawProcedural(m_QuadIndexesGraphicsBuffer, Matrix4x4.identity, 
            new Material(Shader.Find("Hidden/WorldPosRT")), 0, MeshTopology.Triangles, 6,2);
        m_WorldPosCommandBuffer.SetRenderTarget(m_CamMain.activeTexture);
        m_CamMain.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_WorldPosCommandBuffer);
    }

    void OnDestroy()
    {
        //m_CamMain.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_WorldPosCommandBuffer);
        m_WorldPosCommandBuffer?.Dispose();
        m_QuadIndexesGraphicsBuffer?.Dispose();
        m_ScreenDataVertexBuffer?.Dispose();
    }

    void Update()
    {
        UpdateScreenDataVertexBuffer(m_CamMain);
    }

    void UpdateScreenDataVertexBuffer(Camera cam)
    {
        var camTransform = cam.transform;

        var fovScale = (float) math.tan(math.radians(cam.fieldOfView * 0.5));
        float3 toRight = camTransform.right * fovScale * cam.aspect;
        float3 toTop = camTransform.up * fovScale;

        // Fill Screen ComputeBuffer
        float3 camForward = camTransform.forward;
        var camFarClipPlane = cam.farClipPlane;
        using var screenDataVertexArray = new NativeArray<ScreenDataVertex>(4, Allocator.Temp, NativeArrayOptions.UninitializedMemory)
        {
            [0] = new ScreenDataVertex((camForward - toRight + toTop) * camFarClipPlane, new float2(0, 0)), // Top left
            [1] = new ScreenDataVertex((camForward + toRight + toTop) * camFarClipPlane, new float2(1, 0)), // Top right
            [2] = new ScreenDataVertex((camForward + toRight - toTop) * camFarClipPlane, new float2(1, 1)), // Bottom right
            [3] = new ScreenDataVertex((camForward - toRight - toTop) * camFarClipPlane, new float2(0, 1))  // Bottom left
        }; m_ScreenDataVertexBuffer.SetData(screenDataVertexArray);
    }
        
    void OnDrawGizmosSelected()
    {
        var screenDataVertexArray = new ScreenDataVertex[m_ScreenDataVertexBuffer.count];
        m_ScreenDataVertexBuffer.GetData(screenDataVertexArray);

        var camMainPos = Camera.main.transform.position;
        for (var index = 0; index < screenDataVertexArray.Length; index++)
        {
            var screenDataVertex = screenDataVertexArray[index];
            Gizmos.color = Color.HSVToRGB(index/(float)screenDataVertexArray.Length, 1, 1);
            Gizmos.DrawLine(camMainPos, camMainPos + (Vector3) screenDataVertex.m_RayToFarPlane);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ScreenDataVertex
    {
        float2 m_UV;
        public float3 m_RayToFarPlane;
        public ScreenDataVertex(float3 rayToFarPlane, float2 uv)
        {
            m_RayToFarPlane = rayToFarPlane;
            m_UV = uv;
        }
    }
}