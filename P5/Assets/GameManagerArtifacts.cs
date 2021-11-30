using System;
using Unity.Collections;
using UnityEngine;

public class GameManagerArtifacts : MonoBehaviour {
    [SerializeField] SceneClipInfo[] ArtifactInfos;
    private int sceneClipActiveIndex = 0;
    private GameObject currentSceneClip;
    
    void SwitchSceneClip() {
        Destroy(currentSceneClip);
        sceneClipActiveIndex++;
    }
}

[Serializable]
struct SceneClipInfo {
    public GameObject Prefab;
    public string name;
    public SceneClipInfoEncoding SceneClipInfoEncoding;
}

enum SceneClipInfoEncoding {
    CompleteTimer,
    CompleteCall,
    
    
}