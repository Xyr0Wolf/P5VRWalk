using System;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManagerArtifacts : MonoBehaviour {
    [SerializeField] InputActionReference pressToContinue;
    [SerializeField] SceneClipInfoCollection[] ArtifactInfos;
    private int sceneClipInfoActiveIndex = 0;
    private int sceneClipCollectionActiveIndex = 0;
    private GameObject currentSceneClip;
    
    [Header("Text Screen Settings")]
    [SerializeField] Text textScreen;
    [SerializeField] float textSpeed;
    [SerializeField] float textDistance;
    private Transform m_CanvasTransform;
    private Camera _camera;

    public void SwitchSceneClip() { // The one to call
        Destroy(currentSceneClip);
        sceneClipInfoActiveIndex++;
        
    }
    
    void SwitchSceneClipCollection() {
        sceneClipCollectionActiveIndex++;
    }

    private void Update() {
        var camTransform = _camera.transform;
        m_CanvasTransform.position = Vector3.Lerp(m_CanvasTransform.position, camTransform.position+camTransform.forward*textDistance, Time.deltaTime*textSpeed);
        m_CanvasTransform.rotation = Quaternion.Slerp(m_CanvasTransform.rotation, camTransform.rotation, Time.deltaTime*textSpeed);
    }

    private void Start() {
        ArtifactInfos = ArtifactInfos.OrderBy(a => Random.value).ToArray();
        m_CanvasTransform = textScreen.transform.parent;
        _camera = Camera.main;
    }
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
}

enum SceneClipInfoType {
    Explain,
    GoToBoundary,
    GoToNear,
    GoToCenter,
    PrefabControlled,
    WalkSim,
    LogAndWait
}

enum LoggingStates {
    StartRecording,
    EndRecording,
    NoRecording,
}