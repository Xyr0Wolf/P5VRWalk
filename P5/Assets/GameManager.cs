using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
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
        CoinsNear,
        
        End
    }

    [Header("Standard Settings")]
    [SerializeField] float timeBetweenSwitchInSeconds = 20;

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
    LiveHeatMapper m_LiveHeatMapper;
    
    float m_TimeBetweenSwitchInSecondsLeft;
    Scenarios m_NextScenario = Scenarios.InterestingObjectsFar;
    bool m_IsNearFog;
    float m_TextLerpDistance;
    List<GameObject> m_ScenarioObjects = new List<GameObject>();
    bool m_IsDone;
    

    void Start()
    {
        m_TimeBetweenSwitchInSecondsLeft = timeBetweenSwitchInSeconds;
        m_Cam = Camera.main;
        textScreen.text = "";
        m_HeatMapper = GetComponent<HeatMapper>();
        m_LiveHeatMapper = GetComponent<LiveHeatMapper>();
    }

    void Update()
    {
        // Retrieve data
        var camTransform = m_Cam.transform;
        var textScreenTransform = textScreen.transform;
        
        // Set camera text placement
        textScreenTransform.position = Vector3.Lerp(textScreenTransform.position, GetTextAwayFromCameraPosition(camTransform, m_TextLerpDistance), Time.deltaTime * textSpeed);
        textScreen.transform.rotation = Quaternion.Slerp(textScreenTransform.rotation,camTransform.rotation,Time.deltaTime * textSpeed);
        if(m_IsDone) return;
        
        // Count timer downwards
        if (m_TimeBetweenSwitchInSecondsLeft > 0)
            m_TimeBetweenSwitchInSecondsLeft -= Time.deltaTime;
        else
        {
            m_TimeBetweenSwitchInSecondsLeft = timeBetweenSwitchInSeconds;
            textScreen.text = "";
            StartCoroutine(SwitchScenario());
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
            m_TextLerpDistance = math.smoothstep(0.1f, 1, timeFraction);
        }

    }

    Vector3 GetTextAwayFromCameraPosition(Transform camTransform, float textLerpDistance) =>
        camTransform.position + camTransform.forward * math.lerp(textDistanceNear,textDistanceFar, textLerpDistance);

    void DestroyCurrentScenario()
    {
        foreach (var scenarioObject in m_ScenarioObjects)
            Destroy(scenarioObject);
        m_ScenarioObjects.Clear();
    }
    
    IEnumerator SwitchScenario()
    {
        m_LiveHeatMapper.UpdateHeatMaps($"{(int)m_NextScenario-1}{m_NextScenario-1}");
        yield return new WaitForSeconds(1f);
        
        DestroyCurrentScenario();
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
                m_NextScenario = Scenarios.End;
                break;
            case Scenarios.End:
                m_TextLerpDistance = 0;
                textScreen.text = "Thanks for testing <3";
                m_IsDone = true;
                Application.Quit();
                break;
        }
    }

    void DoInterestingObjects() => m_ScenarioObjects.Add(Instantiate(interestingObjectsPrefab));

    void DoMirage() => m_ScenarioObjects.Add(Instantiate(miragePrefab));

    void DoCoins() => m_ScenarioObjects.Add(Instantiate(coinsPrefab));
}
