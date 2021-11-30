using UnityEngine;

namespace DefaultNamespace {
    public class PrintSrcRTFromCam : MonoBehaviour {
        [SerializeField] RenderTexture src;
        private void OnRenderImage(RenderTexture src, RenderTexture dest) {
            this.src = src;
            Graphics.Blit(src, dest);
        }
    }
}