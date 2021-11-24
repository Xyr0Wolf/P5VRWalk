using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class MoveRandomly : MonoBehaviour
{
    float m_TimeLeft;
    [SerializeField] float time = 5f;
    [SerializeField] float speed = 5f;
    [SerializeField] float angularSpeed = 5f;

    float3 m_GOToLocation;
    float3 direction;
    // Update is called once per frame
    void Update()
    {
        if (m_TimeLeft < 0)
        {
            m_TimeLeft = time;
            m_GOToLocation.xz = Random.insideUnitCircle * 6;
        } else m_TimeLeft -= Time.deltaTime;

        if (Vector3.Distance(transform.position, m_GOToLocation) > 0.1)
        {
            direction = math.lerp(direction, math.normalize(m_GOToLocation-(float3)transform.position), Time.deltaTime * angularSpeed);
            transform.position += (Vector3)direction * Time.deltaTime * speed;
        }
    }
}
