using System.IO;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

namespace DefaultNamespace.GPUBasedLogging
{
    public class IntrusionCalculator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] float maxHeadsetVelocity = 10f;
        
        RenderTexture m_IntrusionObjectRT;
        Camera m_Cam;
        void Start()
        {
            m_IntrusionObjectRT = new RenderTexture(512, 512, 0, RenderTextureFormat.RFloat)
            {
                useMipMap = true
            };

            var camMain = Camera.main;
            m_Cam = new GameObject("IntrusionCam").AddComponent<Camera>();
            m_Cam.enabled = false;
            m_Cam.transform.SetParent(camMain.transform);
            m_Cam.CopyFrom(camMain);
            m_Cam.farClipPlane = 10f;
            m_Cam.nearClipPlane = 0.01f;
            m_Cam.backgroundColor = Color.clear;
            
            m_Cam.cullingMask = 1 << LayerMask.NameToLayer("Intrusion");
            m_Cam.targetTexture = m_IntrusionObjectRT;
            m_Cam.stereoTargetEye = StereoTargetEyeMask.None;
            m_Cam.SetReplacementShader(Shader.Find("Hidden/DrawWithBellCurve"),"RenderType");
        }

        [SerializeField] Transform movingIntrusion;
        Vector3 m_MovingIntrusionLastPos;
        Vector3 m_LastPos;
        float m_Value;
        void Update()
        {
            m_Cam.Render();

            var currentPos = m_Cam.transform.position + m_Cam.transform.forward;
            var hmdVel = (m_LastPos - currentPos).magnitude/Time.deltaTime;
            m_LastPos = currentPos;
            
            var objVel = 0f;
            if (movingIntrusion)
            {
                objVel = (m_MovingIntrusionLastPos - movingIntrusion.position).magnitude/Time.deltaTime;
                m_MovingIntrusionLastPos = movingIntrusion.position;
            }

            m_Value += GetCoverPercentage(m_IntrusionObjectRT)*Time.deltaTime*(1+math.max(math.saturate(objVel),math.smoothstep(0,maxHeadsetVelocity,hmdVel)));
        }
        
        public void SaveAndReset(string columnName)
        {
            if (!enabled) return;
            using var file = new StreamWriter(Application.persistentDataPath + $"/Intrusion.tsv", true, Encoding.ASCII);
            file.WriteLine($"{TickOnStart.s_DateTimeNowTicks}\t{columnName}\t{m_Value:0.0000000000}");
            
            m_Value = 0f;
        }

        public static float GetCoverPercentage(RenderTexture renderTexture)
        {
            // Copy smallest mip to new 1x1 RenderTexture
            var resultRT = new RenderTexture(1, 1, 0, RenderTextureFormat.RFloat);
            Graphics.CopyTexture(renderTexture, 0, renderTexture.mipmapCount - 1, resultRT, 0, 0);
            
            // Read the 1x1 result into CPU texture (Texture2D)
            var rt = RenderTexture.active;
            RenderTexture.active = resultRT;
            var resultTex = new Texture2D(1, 1, TextureFormat.RFloat, false);
            resultTex.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
            RenderTexture.active = rt;
            
            return resultTex.GetRawTextureData<float>()[0];
        }
    }
}
