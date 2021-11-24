﻿using System;
using System.Collections.Generic;
using System.Linq;
using Unity.XR.Oculus;
using Unity.XR.Oculus.Input;
using Unity.XR.OpenVR;
using UnityEngine;
using UnityEngine.XR;

namespace DefaultNamespace
{
    public class BoundaryBuilder : MonoBehaviour
    {
        void Update()
        {
            var list = new List<XRInputSubsystem>();
            SubsystemManager.GetInstances(list);
            var inputSubsystem = list.First(s => s.running);
            inputSubsystem.boundaryChanged += subsystem =>
            {
                var currentBoundaries = new List<Vector3>();
                if (inputSubsystem.TryGetBoundaryPoints(currentBoundaries))
                    foreach (var point in currentBoundaries)
                        Debug.DrawLine(point,point+Vector3.up*10f);
            };
            
            //new OculusLoader().
            
            // var currentBoundaries = new List<Vector3>();
            // if (inputSubsystem.TryGetBoundaryPoints(currentBoundaries))
            //     foreach (var point in currentBoundaries)
            //         Debug.DrawLine(point,point+Vector3.up*10f);
        }
    }
}
