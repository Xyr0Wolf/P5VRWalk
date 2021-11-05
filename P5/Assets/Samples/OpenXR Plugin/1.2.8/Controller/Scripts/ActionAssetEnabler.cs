using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class ActionAssetEnabler : MonoBehaviour
    {
        [FormerlySerializedAs("m_ActionAsset")]
        [SerializeField] InputActionAsset actionAsset;

        private void OnEnable()
        {
            if(actionAsset != null) 
                actionAsset.Enable();
        }
    }
}
