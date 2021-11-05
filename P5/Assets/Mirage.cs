using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Mirage : MonoBehaviour
{
    [SerializeField] GameObject mirageModel;
    [SerializeField] float interval;
    float m_IntervalLeft;
    GameObject m_MirageGO;

    void Update()
    {
        if (m_IntervalLeft < 0)
        {
            if (m_MirageGO) Destroy(m_MirageGO);
            var randomInSquare = new Vector3(Random.Range(-5, 5), -0.1f, Random.Range(-5, 5));
            m_MirageGO = Instantiate(mirageModel, randomInSquare, Quaternion.identity, transform);
            m_IntervalLeft = interval;
        } else m_IntervalLeft -= Time.deltaTime;
    }
}
