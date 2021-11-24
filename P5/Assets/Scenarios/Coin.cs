using System;
using Unity.Mathematics;
using UnityEngine;

public class Coin : MonoBehaviour
{
    void Update()
    {
        var position = transform.position;
        transform.position = new Vector3(position.x, 1+math.sin(Time.time*1.5f), position.z);
        transform.rotation = Quaternion.Euler(0,Time.time*45,0);
    }

    void OnTriggerEnter(Collider other) => Destroy(gameObject);
}
