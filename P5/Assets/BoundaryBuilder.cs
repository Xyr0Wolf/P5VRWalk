using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.XR;

public class BoundaryBuilder : MonoBehaviour
{
    public NativeArray<float2> m_Points;
    CommandBuffer m_DecalCommandBuffer;
    [SerializeField] Texture2D picture;
    [SerializeField] private InputActionReference load;

    Camera m_MainCam;
    void Start()
    {
        m_MainCam = Camera.main;
            
        var list = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances(list);
        var inputSubsystem = list.FirstOrDefault(s => s.running);
        if (inputSubsystem != null)
        {
            // inputSubsystem.boundaryChanged += FillArrayWithBoundaryPoints; -- This should only update at start of unity
            FillArrayWithBoundaryPoints(inputSubsystem);
            load.action.performed += context => FillArrayWithBoundaryPoints(inputSubsystem);
        }

        // Tested OVR boundary
        // if (OVRManager.isHmdPresent && OVRManager.boundary != null) {
        //     var points = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);
        //     if (m_Points.IsCreated) m_Points.Dispose();
        //     m_Points = new NativeArray<float2>(points.Select(point => new float2(point.x, point.z)).ToArray(), Allocator.Persistent);
        // }

        if (m_Points.Length == 0)
        {
            m_Points = new NativeArray<float2>(100, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < m_Points.Length; i++)
            {
                var circle = new float2();
                var t = (i + 1) / (float) m_Points.Length;
                math.sincos(2*math.PI*t,out circle.x, out circle.y);
                m_Points[i] = circle*(5+noise.snoise(new float2(0,t*10)));
            }
        }

        // Set line renderer
        var lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = m_Points.Length;
        lineRenderer.SetPositions(m_Points.Select(f => new Vector3(f.x,1,f.y)).ToArray());

        // Create line cam
        var lineCam = new GameObject("Line Cam").AddComponent<Camera>();
        lineCam.transform.position = Vector3.up*10;
        lineCam.transform.rotation = Quaternion.Euler(90,0,0);
        lineCam.orthographic = true;
        lineCam.orthographicSize = 10;
        lineCam.aspect = 1;
        lineCam.cullingMask = 1 << LayerMask.NameToLayer("Lines");
        lineCam.targetTexture = new RenderTexture(1024,1024,0, GraphicsFormat.R32G32_SFloat);
        lineCam.stereoTargetEye = StereoTargetEyeMask.None;
        lineCam.clearFlags = CameraClearFlags.SolidColor;
        lineCam.backgroundColor = Color.clear;
        lineCam.useOcclusionCulling = false;

        // Draw decal
        m_DecalCommandBuffer = new CommandBuffer {name = "Draw Wall Decal"};
        var decalMaterial = new Material(Shader.Find("Hidden/DrawWallsDeferredDecal"));
        decalMaterial.SetTexture("pic", picture);
        m_DecalCommandBuffer.Blit(lineCam.targetTexture,BuiltinRenderTextureType.CurrentActive, decalMaterial);
        ToggleWallRender();
        ToggleWallRender();
    }

    private bool isWallRenderEnabled = false;
        
    public void ToggleWallRender() {
        isWallRenderEnabled = !isWallRenderEnabled;
        if (isWallRenderEnabled)
            m_MainCam.AddCommandBuffer(CameraEvent.AfterImageEffectsOpaque, m_DecalCommandBuffer);
        else
            m_MainCam.RemoveCommandBuffer(CameraEvent.AfterImageEffectsOpaque, m_DecalCommandBuffer);
    }
        
    void FillArrayWithBoundaryPoints(XRInputSubsystem inputSubsystem)
    {
        // Create Points Array
        var currentBoundaries = new List<Vector3>();
        if (inputSubsystem.TryGetBoundaryPoints(currentBoundaries))
            if(m_Points.IsCreated)
                m_Points.Dispose();
        m_Points = new NativeArray<float2>(currentBoundaries.Select(v => new float2(v.x, v.z)).ToArray(), Allocator.Persistent);

        var lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = m_Points.Length;
        lineRenderer.SetPositions(m_Points.Select(f => new Vector3(f.x,1,f.y)).ToArray());
    }

    void OnDrawGizmosSelected()
    {
        for (var i = 0; i < m_Points.Length; i++)
        {
            var pointV = new Vector3(m_Points[i].x, 0, m_Points[i].y);
            Gizmos.color = Color.HSVToRGB(i / (float) m_Points.Length, 1, 1);
            Gizmos.DrawLine(pointV,pointV+Vector3.up*5f);
                
            var pointNext = m_Points[(i+1)%m_Points.Length];
            var pointVNext = new Vector3(pointNext.x, 0, pointNext.y);
            Gizmos.DrawLine(pointV+Vector3.up,pointVNext+Vector3.up);
        }
    }

    void OnDestroy()
    {
        m_DecalCommandBuffer?.Dispose();
        m_Points.Dispose();
    }
}