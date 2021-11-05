using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class HeatMapper : MonoBehaviour
{
    [SerializeField] InputActionReference saveBtn;
    [SerializeField] ComputeShader heatMapCompute;
    [SerializeField] float bellGain = 50;
    [SerializeField] float endGain = 0.01f;
    [SerializeField] float timeScale = 0.000001f;
    
    NativeList<float2> m_PlacesBeen;
    Camera m_Cam;
    void Start()
    {
        m_Cam = Camera.main;
        m_PlacesBeen = new NativeList<float2>(10000, Allocator.Persistent);

        var outputHeatMapRenderTexture = new RenderTexture(2048, 2048,0) {enableRandomWrite = true};
        outputHeatMapRenderTexture.Create();
        heatMapCompute.SetTexture(0,"output_heat_map", outputHeatMapRenderTexture);
        
        var outputTimeMapRenderTexture = new RenderTexture(2048, 2048,0) {enableRandomWrite = true};
        outputTimeMapRenderTexture.Create();
        heatMapCompute.SetTexture(0,"output_time_map", outputTimeMapRenderTexture);

        saveBtn.action.performed += context =>
        {
            // Prepare ComputeBuffer
            var placesBeenArr = m_PlacesBeen.ToArray();
            using var placesBeenBuffer = new ComputeBuffer(placesBeenArr.Length,2*4);
            placesBeenBuffer.SetData(placesBeenArr);
            heatMapCompute.SetBuffer(0,"places_been", placesBeenBuffer);
            heatMapCompute.SetInt("places_been_count", placesBeenArr.Length);
            heatMapCompute.SetFloat("bell_gain", bellGain);
            heatMapCompute.SetFloat("end_gain", endGain);
            heatMapCompute.SetFloat("time_scale", timeScale);

            // Write to RenderTexture
            heatMapCompute.Dispatch(0,64,64,1);
            placesBeenBuffer.Dispose();
            
            // Convert RenderTexture to Texture2D
            var oldRT = RenderTexture.active;
            
            var outputHeatMapTexture2D = new Texture2D(2048,2048);
            RenderTexture.active = outputHeatMapRenderTexture;
            outputHeatMapTexture2D.ReadPixels(new Rect(0,0,2048,2048),0,0);
            outputHeatMapTexture2D.Apply();
            
            var outputTimeMapTexture2D = new Texture2D(2048,2048);
            RenderTexture.active = outputTimeMapRenderTexture;
            outputTimeMapTexture2D.ReadPixels(new Rect(0,0,2048,2048),0,0);
            outputTimeMapTexture2D.Apply();

            RenderTexture.active = oldRT;
            
            // Save Texture2D
            System.IO.File.WriteAllBytes(Application.persistentDataPath + "/HeatMap.png", outputHeatMapTexture2D.EncodeToPNG());
            System.IO.File.WriteAllBytes(Application.persistentDataPath + "/TimeMap.png", outputTimeMapTexture2D.EncodeToPNG());
            Debug.Log(Application.persistentDataPath);
        };
    }

    void FixedUpdate()
    {
        var cameraPosition = m_Cam.transform.position;
        m_PlacesBeen.Add(new float2(cameraPosition.x,cameraPosition.z));
    }
}
