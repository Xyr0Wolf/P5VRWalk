using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class ActionToVisibilityISX : MonoBehaviour
    {

        [FormerlySerializedAs("m_ActionReference")]
        [SerializeField]
        InputActionProperty actionReference;

        [FormerlySerializedAs("m_TargetGameobject")]
        [SerializeField]
        GameObject targetGameobject = null;

        void Start() => actionReference.action?.Enable();

        void Update()
        {
            if (targetGameobject == null)
                return;

            if (actionReference.action != null
                && actionReference.action.controls.Count > 0
                && actionReference.action.enabled)
            {
                targetGameobject.SetActive(true);
                return;
            }

            // No Matching devices:
            targetGameobject.SetActive(false);
        }
    }
}
