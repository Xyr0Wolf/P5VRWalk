using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.XR.Oculus;
using Unity.XR.Oculus.Input;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using Random = Unity.Mathematics.Random;

namespace DefaultNamespace
{
    [ExecuteInEditMode]
    public class BoundaryBuilder : MonoBehaviour
    {
        NativeArray<float2> m_Points;
        ComputeBuffer m_PointBuffer;

        void OnEnable() => Start();

        void Start()
        {
            var list = new List<XRInputSubsystem>();
            SubsystemManager.GetInstances(list);
            var inputSubsystem = list.FirstOrDefault(s => s.running);
            if (inputSubsystem != null)
            {
                inputSubsystem.boundaryChanged += FillArrayWithBoundaryPoints;
                FillArrayWithBoundaryPoints(inputSubsystem);
            }
            
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
            
            m_PointBuffer?.Dispose();
            m_PointBuffer = new ComputeBuffer(m_Points.Length,UnsafeUtility.SizeOf<float2>(), ComputeBufferType.Structured);
            m_PointBuffer.SetData(m_Points);
            Shader.SetGlobalBuffer("points", m_PointBuffer);
        }

        void FillArrayWithBoundaryPoints(XRInputSubsystem inputSubsystem)
        {
            // Create Points Array
            var currentBoundaries = new List<Vector3>();
            if (inputSubsystem.TryGetBoundaryPoints(currentBoundaries))
                if(m_Points.IsCreated)
                    m_Points.Dispose();
            m_Points = new NativeArray<float2>(currentBoundaries.Select(v => new float2(v.x, v.z)).ToArray(), Allocator.Persistent);
            
            // Create Points Compute Buffer
            m_PointBuffer?.Dispose();
            m_PointBuffer = new ComputeBuffer(m_Points.Length,UnsafeUtility.SizeOf<float2>());
            m_PointBuffer.SetData(m_Points);
            Shader.SetGlobalBuffer("points", m_PointBuffer);
        }

        void OnDrawGizmosSelected()
        {
            var points = new float2[m_PointBuffer.count];
            m_PointBuffer.GetData(points);
            for (var i = 0; i < points.Length; i++)
            {
                var pointV = new Vector3(points[i].x, 0, points[i].y);
                Gizmos.color = Color.HSVToRGB(i / (float) points.Length, 1, 1);
                Gizmos.DrawLine(pointV,pointV+Vector3.up*5f);
                
                var pointNext = points[(i+1)%points.Length];
                var pointVNext = new Vector3(pointNext.x, 0, pointNext.y);
                Gizmos.DrawLine(pointV+Vector3.up,pointVNext+Vector3.up);
            }
        }

        void OnDisable() => m_PointBuffer?.Dispose();
    }
}
