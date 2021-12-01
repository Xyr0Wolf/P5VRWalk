using System;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace DefaultNamespace.GPUBasedLogging
{
    public class VelToCsv : MonoBehaviour
    {
        NativeList<float> m_Velocities;
        Camera m_Cam;
        void Start()
        {
            m_Cam = Camera.main;
            m_Velocities = new NativeList<float>(100000,Allocator.Persistent);
        }

        float3 m_CamPosLast;
        void Update()
        {
            float3 camPos = m_Cam.transform.position;
            m_Velocities.Add(math.distance(camPos,m_CamPosLast)/Time.deltaTime);
            m_CamPosLast = camPos;
        }

        private void OnDestroy() {
            m_Velocities.Dispose();
        }

        public void Reset() => m_Velocities.Clear();

        public void CreateOrAppendAndReset(string columnName)
        {
            if (!enabled) return;
            using var file = new StreamWriter(Application.persistentDataPath + $"/Velocities.tsv", true, Encoding.ASCII);
            var writtenColumn = $"{TickOnStart.s_DateTimeNowTicks}\t{columnName}\t" + string.Join("\t", m_Velocities.AsArray().Select(vel => vel.ToString("0.0000000000")));
            file.WriteLine(writtenColumn);
            
            m_Velocities.Clear();
        }
    }
}
