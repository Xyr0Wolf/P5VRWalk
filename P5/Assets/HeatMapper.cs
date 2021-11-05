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
    [SerializeField] float bellGain = 5;
    [SerializeField] float endGain = 2;
    NativeList<float2> m_PlacesBeen;
    Camera m_Cam;

    [SerializeField] RenderTexture outputRenderTexture;
    void Start()
    {
        m_Cam = Camera.main;
        m_PlacesBeen = new NativeList<float2>(10000, Allocator.Persistent);

        outputRenderTexture = new RenderTexture(2048, 2048,0) {enableRandomWrite = true};
        outputRenderTexture.Create();
        heatMapCompute.SetTexture(0,"result", outputRenderTexture);

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

            // Write to RenderTexture
            heatMapCompute.Dispatch(0,64,64,1);
            placesBeenBuffer.Dispose();
            
            // Convert RenderTexture to Texture2D
            var outputTexture2D = new Texture2D(2048,2048);
            var oldRT = RenderTexture.active;
            RenderTexture.active = outputRenderTexture;
            outputTexture2D.ReadPixels(new Rect(0,0,2048,2048),0,0);
            outputTexture2D.Apply();
            RenderTexture.active = oldRT;
            
            // Save Texture2D
            System.IO.File.WriteAllBytes(Application.persistentDataPath + "/SavedScreen.png", outputTexture2D.EncodeToPNG());
            Debug.Log(Application.persistentDataPath + "/SavedScreen.png");
        };
    }

    void FixedUpdate()
    {
        var cameraPosition = m_Cam.transform.position;
        m_PlacesBeen.Add(new float2(cameraPosition.x,cameraPosition.z));
    }
}
