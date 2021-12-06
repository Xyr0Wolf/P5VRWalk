using System;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class GoToTrigger : MonoBehaviour {
    [SerializeField] private Vector3 centerLocation = new Vector3(0.1f,0.51f,0.1f);
    [SerializeField] private float distanceAwayFromCam = 2f;
    
    private GameManagerArtifacts _gameManagerArtifacts;

    public void Setup(GameManagerArtifacts gameManagerArtifacts, GoToTypes goToType) {
        _gameManagerArtifacts = gameManagerArtifacts;

        switch (goToType) {
            case GoToTypes.Near:
                float3 camPos = Camera.main.transform.position;
                float3 rUnitSphere = Random.onUnitSphere;
                rUnitSphere *= distanceAwayFromCam;
                transform.position = new Vector3(camPos.x + rUnitSphere.x, 0.51f,camPos.z+rUnitSphere.z);
                break;
            case GoToTypes.Boundary: 
                var boundaryBuilder = FindObjectOfType<BoundaryBuilder>();
                var randomPoint = boundaryBuilder.m_Points[Random.Range(0, boundaryBuilder.m_Points.Length)];
                var pointTowardCenter = randomPoint+math.normalize(-randomPoint)*distanceAwayFromCam;
                transform.position = new float3(pointTowardCenter.x, 0.51f, pointTowardCenter.y);
                break;
            case GoToTypes.Center:
                transform.position = centerLocation;
                break;
        }
    }

    private bool hitAlready = false;
    private void OnTriggerEnter(Collider other) {
        if(_gameManagerArtifacts && !hitAlready) {
            hitAlready = true;
            _gameManagerArtifacts.SwitchSceneClip();
        }
    }
}

public enum GoToTypes {
    Near,
    Boundary,
    Center
}