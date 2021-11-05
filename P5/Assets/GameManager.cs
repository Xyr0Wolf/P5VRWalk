using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    enum Scenarios
    {
        EmptyFar,
        InterestingObjectsFar,
        MirageFar,
        CoinsFar,
        
        EmptyNear,
        InterestingObjectsNear,
        MirageNear,
        CoinsNear
    }

    [Header("Standard Settings")]
    [SerializeField] float timeBetweenSwitchInSeconds = 8;

    [Header("Fog Settings")]
    [SerializeField] float nearFog = 0.85f;
    [SerializeField] float farFog = 0.15f;
    
    [Header("Text Screen Settings")]
    [SerializeField] Text textScreen;
    [SerializeField] float textSpeed;
    [SerializeField] float textDistanceFar;
    [SerializeField] float textDistanceNear;

    [Header("Scenario Prefabs")]
    [SerializeField] GameObject interestingObjectsPrefab;
    [SerializeField] GameObject miragePrefab;
    [SerializeField] GameObject coinsPrefab;

    Camera m_Cam;
    HeatMapper m_HeatMapper;
    
    float m_TimeBetweenSwitchInSecondsLeft;
    Scenarios m_NextScenario = Scenarios.InterestingObjectsFar;
    bool m_IsNearFog;
    float m_TextLerpDistance;
    List<GameObject> m_ScenarioObjects = new List<GameObject>();

    void Start()
    {
        m_Cam = Camera.main;
        textScreen.text = "";
        m_HeatMapper = FindObjectOfType<HeatMapper>();
    }

    void Update()
    {
        // Retrieve data
        var camTransform = m_Cam.transform;
        var textScreenTransform = textScreen.transform;
        
        // Count timer downwards
        if (m_TimeBetweenSwitchInSecondsLeft > 0)
            m_TimeBetweenSwitchInSecondsLeft -= Time.deltaTime;
        else
        {
            m_TimeBetweenSwitchInSecondsLeft = timeBetweenSwitchInSeconds;
            textScreen.text = "";
            SwitchScenario();
        }
        
        // Set fog
        RenderSettings.fogDensity = math.lerp(RenderSettings.fogDensity, m_IsNearFog ? nearFog:farFog, Time.deltaTime*5);

        // Show count down
        if (m_TimeBetweenSwitchInSecondsLeft < 6)
        {
            // Set counter
            var timeLeft = math.floor(m_TimeBetweenSwitchInSecondsLeft);
            textScreen.text = timeLeft<0 ? "" : timeLeft.ToString();
            
            // Move text based on fraction time
            var timeFraction = m_TimeBetweenSwitchInSecondsLeft-timeLeft;
            //if (timeFraction < m_TextLerpDistance) 
            //    textScreenTransform.position = GetTextAwayFromCameraPosition(camTransform, timeFraction);
            m_TextLerpDistance = math.smoothstep(0.1f, 1, timeFraction);
        }
        
        // Set camera text placement
        textScreenTransform.position = Vector3.Lerp(textScreenTransform.position, GetTextAwayFromCameraPosition(camTransform, m_TextLerpDistance), Time.deltaTime * textSpeed);
        textScreen.transform.rotation = Quaternion.Slerp(textScreenTransform.rotation,camTransform.rotation,Time.deltaTime * textSpeed);

    }

    Vector3 GetTextAwayFromCameraPosition(Transform camTransform, float textLerpDistance) =>
        camTransform.position + camTransform.forward * math.lerp(textDistanceNear,textDistanceFar, textLerpDistance);

    void DestroyCurrentScenario()
    {
        foreach (var scenarioObject in m_ScenarioObjects)
            Destroy(scenarioObject);
        m_ScenarioObjects.Clear();
    }
    
    void SwitchScenario()
    {
        DestroyCurrentScenario();
        m_HeatMapper.UpdateHeatMaps(nameof(m_NextScenario));
        switch (m_NextScenario)
        {
            case Scenarios.InterestingObjectsFar:
                DoInterestingObjects();
                m_NextScenario = Scenarios.MirageFar;
                break;
            case Scenarios.MirageFar:
                DoMirage();
                m_NextScenario = Scenarios.CoinsFar;
                break;
            case Scenarios.CoinsFar:
                DoCoins();
                m_NextScenario = Scenarios.EmptyNear;
                break;
            
            case Scenarios.EmptyNear:
                m_IsNearFog = true;
                m_NextScenario = Scenarios.InterestingObjectsNear;
                break;
            case Scenarios.InterestingObjectsNear:
                DoInterestingObjects();
                m_NextScenario = Scenarios.MirageNear;
                break;
            case Scenarios.MirageNear:
                DoMirage();
                m_NextScenario = Scenarios.CoinsNear;
                break;
            case Scenarios.CoinsNear:
                DoCoins();
                break;
        }
    }

    void DoInterestingObjects() => m_ScenarioObjects.Add(Instantiate(interestingObjectsPrefab));

    void DoMirage() => m_ScenarioObjects.Add(Instantiate(miragePrefab));

    void DoCoins() => m_ScenarioObjects.Add(Instantiate(coinsPrefab));
}
