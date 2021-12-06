using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class SpawnBoundary : MonoBehaviour {
    [SerializeField] private GameObject prefab;
    void Start() {
        var boundaryBuilder = FindObjectOfType<BoundaryBuilder>();
        foreach (var point in boundaryBuilder.m_Points) {
            Instantiate(prefab, new Vector3(point.x, 0.05f, point.y), Quaternion.identity, transform);
        }
    }
}
