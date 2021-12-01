using System;
using System.Collections;
using System.Linq;
using DefaultNamespace.GPUBasedLogging;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManagerArtifacts : MonoBehaviour {
    [SerializeField] InputActionReference pressToContinue;
    [FormerlySerializedAs("ArtifactInfos")] 
    [SerializeField] SceneClipInfoCollection[] sceneClipInfoCollections;
    private int sceneClipInfoActiveIndex = 0;
    private int sceneClipCollectionActiveIndex = 0;
    private GameObject currentSceneClip;
    
    [Header("Text Screen Settings")]
    [SerializeField] Text textScreen;
    [SerializeField] float textSpeed;
    [SerializeField] float textDistance;
    private Transform m_CanvasTransform;
    private Camera _camera;
    
    // Loggers
    private LiveHeatMapper m_LiveHeatMapper;
    private VelToCsv m_VelToCsv;
    private IntrusionCalculator m_IntrusionCalculator;

    public void SwitchSceneClip() { // The one to call
        if (currentSceneClip)
            Destroy(currentSceneClip);
        if (specialCaseObj)
            Destroy(specialCaseObj);
        
        sceneClipInfoActiveIndex++;
        if(sceneClipInfoActiveIndex >= sceneClipInfoCollections[sceneClipCollectionActiveIndex].sceneClipInfos.Length) {
            SwitchSceneClipCollection();
            sceneClipInfoActiveIndex = 0;
        }

        var activeSceneCollection = sceneClipInfoCollections[sceneClipCollectionActiveIndex];
        var activeSceneClipInfo = activeSceneCollection.sceneClipInfos[sceneClipInfoActiveIndex];
        
        Debug.Log($"Now doing: {activeSceneClipInfo.name}");
        
        // Do initial setup
        if (activeSceneClipInfo.prefab)
            currentSceneClip = Instantiate(activeSceneClipInfo.prefab);

        switch (activeSceneClipInfo.loggingStates) {
            case LoggingStates.StartRecording: 
                m_LiveHeatMapper.ResetLogging();
                m_VelToCsv.ResetLogging();
                m_IntrusionCalculator.ResetLogging();
                break;
            case LoggingStates.EndRecording: 
                m_LiveHeatMapper.SaveAndReset(activeSceneCollection.name);
                m_VelToCsv.CreateOrAppendAndReset(activeSceneCollection.name);
                m_IntrusionCalculator.SaveAndReset(activeSceneCollection.name);
                break;
        }
        
        textScreen.text = activeSceneClipInfo.explanation;

        switch (activeSceneClipInfo.type) {
            case SceneClipInfoType.Explain:
                IEnumerator Explain() {
                    yield return new WaitForSeconds(10);
                    SwitchSceneClip();
                }

                StartCoroutine(Explain());
                break;
            case SceneClipInfoType.GoToBoundary:
                currentSceneClip.GetComponent<GoToTrigger>().Setup(this, GoToTypes.Boundary);
                break;
            case SceneClipInfoType.GoToNear:
                currentSceneClip.GetComponent<GoToTrigger>().Setup(this, GoToTypes.Near);
                break;
            case SceneClipInfoType.GoToCenter:
                currentSceneClip.GetComponent<GoToTrigger>().Setup(this, GoToTypes.Center);
                if (activeSceneClipInfo.specialCase)
                    specialCaseObj = Instantiate(specialCaseObjPrefab);
                break;
            case SceneClipInfoType.PrefabControlled:
                currentSceneClip.GetComponent<ISetup>().Setup(this);
                break;
            case SceneClipInfoType.WalkSim:
                IEnumerator WalkSim() {
                    textScreen.text = "";
                    yield return new WaitForSeconds(60);
                    textScreen.text = "5";
                    yield return new WaitForSeconds(1);
                    textScreen.text = "3";
                    yield return new WaitForSeconds(1);
                    textScreen.text = "2";
                    yield return new WaitForSeconds(1);
                    textScreen.text = "1";
                    yield return new WaitForSeconds(1);
                    SwitchSceneClip();
                }
                
                StartCoroutine(WalkSim());
                break;
            case SceneClipInfoType.LogAndWait: 
                IEnumerator LogAndWait() {
                    textScreen.text = "2";
                    yield return new WaitForSeconds(1);
                    textScreen.text = "1";
                    yield return new WaitForSeconds(1);
                    SwitchSceneClip();
                }
                
                StartCoroutine(LogAndWait());
                break;
        }
    }
    
    void SwitchSceneClipCollection() {
        sceneClipCollectionActiveIndex++;
        Debug.Log($"Now active: {sceneClipInfoCollections[sceneClipCollectionActiveIndex].name}");

        if (sceneClipCollectionActiveIndex >= sceneClipInfoCollections.Length) {
            Application.Quit();
            textScreen.text = "Thx for testing, you can take off the headset now";
        }
    }

    private void Update() {
        var camTransform = _camera.transform;
        m_CanvasTransform.position = Vector3.Lerp(m_CanvasTransform.position, camTransform.position+camTransform.forward*textDistance, Time.deltaTime*textSpeed);
        m_CanvasTransform.rotation = Quaternion.Slerp(m_CanvasTransform.rotation, camTransform.rotation, Time.deltaTime*textSpeed);
    }
    private void Start() {
        sceneClipInfoCollections = sceneClipInfoCollections.OrderBy(a => Random.value).ToArray();
        Debug.Log($"Now active: {sceneClipInfoCollections[sceneClipCollectionActiveIndex].name}");

        m_CanvasTransform = textScreen.transform.parent;
        _camera = Camera.main;
        textScreen.text = "Say when you are ready :)";
        pressToContinue.action.performed += context => {
            Debug.Log("You pressed continue");
            SwitchSceneClip();
        };

        m_LiveHeatMapper = GetComponent<LiveHeatMapper>();
        m_VelToCsv = GetComponent<VelToCsv>();
        m_IntrusionCalculator = GetComponent<IntrusionCalculator>();
    }

    [Header("Misc")] 
    [SerializeField] private GameObject specialCaseObjPrefab;
    private GameObject specialCaseObj;
}

[Serializable]
struct SceneClipInfoCollection {
    public string name;
    public SceneClipInfo[] sceneClipInfos;
}

[Serializable]
struct SceneClipInfo {
    public string name;
    public GameObject prefab;
    public LoggingStates loggingStates;
    public SceneClipInfoType type;
    public string explanation;
    public bool specialCase;
}

enum SceneClipInfoType {
    Explain,
    GoToBoundary,
    GoToNear,
    GoToCenter,
    PrefabControlled,
    WalkSim,
    LogAndWait,
    WaitForGoAhead
}

enum LoggingStates {
    StartRecording,
    EndRecording,
    NoRecording,
}