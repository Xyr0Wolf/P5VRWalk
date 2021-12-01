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
    [SerializeField] float textDistanceFar;
    [SerializeField] float textDistanceNear;

    public void SwitchSceneClip() { // The one to call
        Destroy(currentSceneClip);
        sceneClipInfoActiveIndex++;
        
    }
    
    void SwitchSceneClipCollection() {
        sceneClipCollectionActiveIndex++;
    }

    private void Update() {
        
    }

    private void Start() {
        ArtifactInfos = ArtifactInfos.OrderBy(a => Random.value).ToArray();
        
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
    public SceneClipInfoEncoding sceneClipInfoEncoding;
}

enum SceneClipInfoEncoding {
    CompleteTimer_StartRecording,
    CompleteTimer_StartEndRecording,
    CompleteTimer_EndRecording,
    CompleteTimer_NoRecording,
    
    CompleteCallManual_StartRecording,
    CompleteCallManual_StartEndRecording,
    CompleteCallManual_EndRecording,
    CompleteCallManual_NoRecording,
    
    CompleteCallInput_StartRecording,
    CompleteCallInput_StartEndRecording,
    CompleteCallInput_EndRecording,
    CompleteCallInput_NoRecording
}