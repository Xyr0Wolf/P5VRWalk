using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace DefaultNamespace
{
    [ExecuteInEditMode]
    public class DrawWorldPosBuffer : MonoBehaviour
    {
        ComputeBuffer m_ScreenDataVertexBuffer;
        GraphicsBuffer m_QuadIndexesGraphicsBuffer;
        CommandBuffer m_WorldPosCommandBuffer;
        
        static readonly int k_WorldPosRTId = Shader.PropertyToID("world_space_rt");
        Camera m_CamMain;
        void OnEnable()
        {
            m_CamMain = Camera.main;
            if (!m_CamMain) return;
            m_CamMain.depthTextureMode = DepthTextureMode.Depth;
            m_ScreenDataVertexBuffer = new ComputeBuffer(4, UnsafeUtility.SizeOf<ScreenDataVertex>());
            UpdateScreenDataVertexBuffer(m_CamMain);
            Shader.SetGlobalBuffer("screen_data_vertex_buffer", m_ScreenDataVertexBuffer);

            // Fill Quad GraphicsBuffer
            m_QuadIndexesGraphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Index, 6, sizeof(int));
            m_QuadIndexesGraphicsBuffer.SetData(new []{0,1,2,0,2,3});
            // 0---1
            // | \ |
            // 3---2
            
            // Fill CommandBuffer
            m_WorldPosCommandBuffer = new CommandBuffer {name = "Generate World Position RenderTexture"};
            m_WorldPosCommandBuffer.GetTemporaryRT(k_WorldPosRTId, m_CamMain.pixelWidth, m_CamMain.pixelHeight,0, FilterMode.Bilinear,GraphicsFormat.R32G32B32A32_SFloat);
            m_WorldPosCommandBuffer.SetRenderTarget(k_WorldPosRTId);
            m_WorldPosCommandBuffer.DrawProcedural(m_QuadIndexesGraphicsBuffer, Matrix4x4.identity, new Material(Shader.Find("Hidden/WorldPosRT")), 0, MeshTopology.Triangles, 6);
            m_CamMain.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_WorldPosCommandBuffer);
        }
        
        void OnDestroy()
        {
            //m_CamMain.RemoveCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, m_WorldPosCommandBuffer);
            m_WorldPosCommandBuffer.Dispose();
            m_QuadIndexesGraphicsBuffer.Dispose();
            m_ScreenDataVertexBuffer.Dispose();
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
}
