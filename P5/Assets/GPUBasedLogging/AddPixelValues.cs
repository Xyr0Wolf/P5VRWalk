using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Animations;

namespace DefaultNamespace.GPUBasedLogging
{
    public struct AddPixelValues : IJobFor
    {
        [NativeSetThreadIndex] int m_ThreadIndex;
        public void Execute(int index)
        {
            Debug.Log(new FixedString64Bytes($"{m_ThreadIndex}"));
        }
    }
}
