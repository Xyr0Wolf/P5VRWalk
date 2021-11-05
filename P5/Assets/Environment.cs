using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class Environment : MonoBehaviour
{
    [SerializeField] InputActionReference toggleFogBtn;
    [SerializeField] bool isNearEnabled = true;
    float m_FogDensityDesired;
    void Start()
    {
        m_FogDensityDesired = 0.15f;
        toggleFogBtn.action.performed += context =>
        {
            isNearEnabled = !isNearEnabled;
            m_FogDensityDesired = isNearEnabled ? 0.85f : 0.15f;
        };
    }

    void Update() => RenderSettings.fogDensity = math.lerp(RenderSettings.fogDensity, m_FogDensityDesired, Time.deltaTime*10);
}
