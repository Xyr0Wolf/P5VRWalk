using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace DefaultNamespace.GPUBasedLogging
{
    public class IntrusionCalculator : MonoBehaviour
    {
        [SerializeField] float value;
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
            m_Cam.backgroundColor = Color.clear;
            
            m_Cam.cullingMask = 1 << LayerMask.NameToLayer("Intrusion");
            m_Cam.targetTexture = m_IntrusionObjectRT;
            m_Cam.SetReplacementShader(Shader.Find("Hidden/DrawWithBellCurve"),"RenderType");
        }
        void Update()
        {
            m_Cam.Render();
            value += GetCoverPercentage(m_IntrusionObjectRT)*Time.deltaTime;
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
