using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Coins : MonoBehaviour
{
    [SerializeField] GameObject coinPrefab;

    void Start()
    {
        for (var i = 0; i < 6; i++)
        {
            var randomInSquare = new Vector3(Random.Range(-5, 5), 0.5f, Random.Range(-5, 5));
            Instantiate(coinPrefab, randomInSquare, Quaternion.identity, transform);
        }
    }
}
