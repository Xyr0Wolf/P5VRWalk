using System;
using DefaultNamespace.GPUBasedLogging;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class LiveHeatMapper : MonoBehaviour
{
    [SerializeField] ComputeShader liveHeatMapCompute;
    [SerializeField] Material drawRTMaterial;

    // Aggregated map, which is an accumulation over time
    RenderTexture m_AggregateMap;
    
    RenderTexture m_OutputHeatMapRenderTexture;
    RenderTexture m_OutputTimeMapRenderTexture;
    RenderTexture m_OutputTimeMapWithTopRenderTexture;
    
    // Camera render to put under time map
    [SerializeField] Camera topCam;

    // Settings
    [SerializeField] float initialGain = 2;
    [SerializeField] float endGain = 1;

    // Update on new capture
    float m_TimeSinceCaptureBegan;

    Camera m_Cam;
    static readonly int k_MainTEX = Shader.PropertyToID("_MainTex");
    string m_DateTimeNowTicks;
    
    [Header("Testing Variables")]
    [SerializeField] float4 testVar = 1;

    void Start()
    {
        m_DateTimeNowTicks = DateTime.Now.Ticks.ToString();
        m_TimeSinceCaptureBegan = Time.time;
        m_Cam = Camera.main;

        // Create aggregate map
        m_AggregateMap = new RenderTexture(2048, 2048,0, GraphicsFormat.R32G32_SFloat) {enableRandomWrite = true};
        m_AggregateMap.Create();
        liveHeatMapCompute.SetTexture(0,"aggregate_map", m_AggregateMap);
        liveHeatMapCompute.SetTexture(1,"aggregate_map", m_AggregateMap);
        
        // Create top map texture
        var topCamRT = new RenderTexture(2048, 2048,32);
        topCam.targetTexture = topCamRT;
        liveHeatMapCompute.SetTexture(1, "top_map", topCamRT);
        
        // Create texture for output drawing
        m_OutputHeatMapRenderTexture = new RenderTexture(2048, 2048,0) {enableRandomWrite = true};
        m_OutputHeatMapRenderTexture.Create();
        liveHeatMapCompute.SetTexture(1,"output_heat_map", m_OutputHeatMapRenderTexture);
        
        m_OutputTimeMapRenderTexture = new RenderTexture(2048, 2048,0) {enableRandomWrite = true};
        m_OutputTimeMapRenderTexture.Create();
        liveHeatMapCompute.SetTexture(1,"output_time_map", m_OutputTimeMapRenderTexture);
        
        m_OutputTimeMapWithTopRenderTexture = new RenderTexture(2048, 2048,0) {enableRandomWrite = true};
        m_OutputTimeMapWithTopRenderTexture.Create();
        liveHeatMapCompute.SetTexture(1,"output_time_map_with_top", m_OutputTimeMapWithTopRenderTexture);
        
        // Prepare ComputeBuffer
        liveHeatMapCompute.SetFloat("initial_gain", initialGain);
        liveHeatMapCompute.SetFloat("end_gain", endGain);
        
        drawRTMaterial.SetTexture(k_MainTEX, m_OutputTimeMapRenderTexture);

        // Add Command Buffer to draw HeatMapOnScreen
        var commandBuffer = new CommandBuffer {name = "Draw HeatMap"};
        var drawDebugHeatmapOnScreenMaterial = new Material(Shader.Find("HiddenShader/DrawDebugHeatmapOnScreen"));
        drawDebugHeatmapOnScreenMaterial.SetTexture("heat_map", m_OutputHeatMapRenderTexture);
        drawDebugHeatmapOnScreenMaterial.SetTexture("time_map", m_OutputTimeMapRenderTexture);
        drawDebugHeatmapOnScreenMaterial.SetTexture("time_map_with_top", m_OutputTimeMapWithTopRenderTexture);
        commandBuffer.Blit(null,BuiltinRenderTextureType.CameraTarget, drawDebugHeatmapOnScreenMaterial);
        m_Cam.AddCommandBuffer(CameraEvent.AfterEverything, commandBuffer);
    }

    void Update()
    {
        var cameraPosition = m_Cam.transform.position;
        liveHeatMapCompute.SetFloats("place_been", cameraPosition.x, cameraPosition.z);
        liveHeatMapCompute.SetFloat("time_since_capture_began", Time.time-m_TimeSinceCaptureBegan);
        liveHeatMapCompute.Dispatch(0, 64, 64, 1);

        // Update live
        //liveHeatMapCompute.SetFloat("initial_gain", initialGain);
        //liveHeatMapCompute.SetFloat("end_gain", endGain);
        //liveHeatMapCompute.SetFloats("test_var", testVar.x,testVar.y,testVar.z,testVar.w);
    }
    
    void FixedUpdate()
    {
        // Draw textures based on aggregate_map
        topCam.Render();
        liveHeatMapCompute.Dispatch(1, 64, 64, 1);
    }

    public void UpdateHeatMaps(string imageName)
    {
        // Convert RenderTexture to Texture2D
        var oldRT = RenderTexture.active;

        var outputHeatMapTexture2D = new Texture2D(2048, 2048);
        RenderTexture.active = m_OutputHeatMapRenderTexture;
        outputHeatMapTexture2D.ReadPixels(new Rect(0, 0, 2048, 2048), 0, 0);

        var outputTimeMapTexture2D = new Texture2D(2048, 2048);
        RenderTexture.active = m_OutputTimeMapRenderTexture;
        outputTimeMapTexture2D.ReadPixels(new Rect(0, 0, 2048, 2048), 0, 0);
        
        var outputTimeMapWithTopTexture2D = new Texture2D(2048, 2048);
        RenderTexture.active = m_OutputTimeMapWithTopRenderTexture;
        outputTimeMapWithTopTexture2D.ReadPixels(new Rect(0, 0, 2048, 2048), 0, 0);

        // Calculate Coverage
        var saturatedMapRT = new RenderTexture(m_AggregateMap.width, m_AggregateMap.height,0,GraphicsFormat.R32_SFloat){useMipMap = true};
        var saturateRedMaterial = new Material(Shader.Find("Hidden/SaturateRedRT"));
        saturateRedMaterial.SetTexture("_MainTex", m_AggregateMap);
        Graphics.Blit(m_AggregateMap, saturatedMapRT, saturateRedMaterial);
        var heatCoverage = 100f * IntrusionCalculator.GetCoverPercentage(saturatedMapRT);
        
        // Save Texture2D
        System.IO.File.WriteAllBytes(Application.persistentDataPath + $"/{m_DateTimeNowTicks}_HeatMap_{imageName}_{heatCoverage:0.00}p.png", outputHeatMapTexture2D.EncodeToPNG());
        System.IO.File.WriteAllBytes(Application.persistentDataPath + $"/{m_DateTimeNowTicks}_TimeMap_{imageName}.png", outputTimeMapTexture2D.EncodeToPNG());
        System.IO.File.WriteAllBytes(Application.persistentDataPath + $"/{m_DateTimeNowTicks}_TimeMapWithTop_{imageName}.png", outputTimeMapWithTopTexture2D.EncodeToPNG());
        
        Debug.Log(Application.persistentDataPath);

        RenderTexture.active = m_AggregateMap;
        GL.Clear(false, true, Color.clear);
        m_TimeSinceCaptureBegan = Time.time;

        RenderTexture.active = oldRT;
    }
}
