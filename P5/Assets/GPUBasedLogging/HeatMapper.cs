﻿using System;
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

    [SerializeField] RenderTexture topCamRT;
    
    NativeList<float2> m_PlacesBeen;
    Camera m_Cam;

    RenderTexture m_OutputHeatMapRenderTexture;
    RenderTexture m_OutputTimeMapRenderTexture;
    RenderTexture m_OutputTimeMapWithTopRenderTexture;
    void Start()
    {
        m_Cam = Camera.main;
        m_PlacesBeen = new NativeList<float2>(10000, Allocator.Persistent);

        m_OutputHeatMapRenderTexture = new RenderTexture(2048, 2048,0) {enableRandomWrite = true};
        m_OutputHeatMapRenderTexture.Create();
        heatMapCompute.SetTexture(0,"output_heat_map", m_OutputHeatMapRenderTexture);
        
        m_OutputTimeMapRenderTexture = new RenderTexture(2048, 2048,0) {enableRandomWrite = true};
        m_OutputTimeMapRenderTexture.Create();
        heatMapCompute.SetTexture(0,"output_time_map", m_OutputTimeMapRenderTexture);
        
        m_OutputTimeMapWithTopRenderTexture = new RenderTexture(2048, 2048,0) {enableRandomWrite = true};
        m_OutputTimeMapWithTopRenderTexture.Create();
        heatMapCompute.SetTexture(0,"output_time_map_with_top", m_OutputTimeMapWithTopRenderTexture);
        
        heatMapCompute.SetTexture(0, "_top_map", topCamRT);
        
        saveBtn.action.performed += callback => UpdateHeatMaps("forced");
    }

    void Update()
    {
        var cameraPosition = m_Cam.transform.position;
        m_PlacesBeen.Add(new float2(cameraPosition.x,cameraPosition.z));
    }
    
    public void UpdateHeatMaps(string imageName)
    {
        // Prepare ComputeBuffer
        var placesBeenArr = m_PlacesBeen.ToArray();
        using var placesBeenBuffer = new ComputeBuffer(placesBeenArr.Length, 2 * 4);
        placesBeenBuffer.SetData(placesBeenArr);
        heatMapCompute.SetBuffer(0, "places_been", placesBeenBuffer);
        heatMapCompute.SetInt("places_been_count", placesBeenArr.Length);
        heatMapCompute.SetFloat("bell_gain", bellGain);
        heatMapCompute.SetFloat("end_gain", endGain);
        heatMapCompute.SetFloat("time_scale", timeScale);

        // Write to RenderTexture
        heatMapCompute.Dispatch(0, 64, 64, 1);
        placesBeenBuffer.Dispose();

        // Convert RenderTexture to Texture2D
        var oldRT = RenderTexture.active;

        var outputHeatMapTexture2D = new Texture2D(2048, 2048);
        RenderTexture.active = m_OutputHeatMapRenderTexture;
        outputHeatMapTexture2D.ReadPixels(new Rect(0, 0, 2048, 2048), 0, 0);
        outputHeatMapTexture2D.Apply();

        var outputTimeMapTexture2D = new Texture2D(2048, 2048);
        RenderTexture.active = m_OutputTimeMapRenderTexture;
        outputTimeMapTexture2D.ReadPixels(new Rect(0, 0, 2048, 2048), 0, 0);
        outputTimeMapTexture2D.Apply();
        
        var outputTimeMapWithTopTexture2D = new Texture2D(2048, 2048);
        RenderTexture.active = m_OutputTimeMapWithTopRenderTexture;
        outputTimeMapWithTopTexture2D.ReadPixels(new Rect(0, 0, 2048, 2048), 0, 0);
        outputTimeMapWithTopTexture2D.Apply();

        RenderTexture.active = oldRT;

        // Save Texture2D
        System.IO.File.WriteAllBytes(Application.persistentDataPath + $"/HeatMap_{imageName}.png", outputHeatMapTexture2D.EncodeToPNG());
        System.IO.File.WriteAllBytes(Application.persistentDataPath + $"/TimeMap_{imageName}.png", outputTimeMapTexture2D.EncodeToPNG());
        System.IO.File.WriteAllBytes(Application.persistentDataPath + $"/TimeMapWithTop_{imageName}.png", outputTimeMapWithTopTexture2D.EncodeToPNG());
        
        Debug.Log(Application.persistentDataPath);
        
        m_PlacesBeen.Clear();
    }
}