using System;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class ActionToButtonISX : MonoBehaviour
    {
        [SerializeField] InputActionReference actionReference;
        [SerializeField] Color enabledColor = Color.green;
        [SerializeField] Color disabledColor = Color.red;
        [SerializeField] Image image;

        Graphic m_Graphic;
        Graphic[] m_Graphics;

        private void OnEnable()
        {
            if (image == null)
                Debug.LogWarning("ActionToButton Monobehaviour started without any associated image. This input will not be reported.", this);

            m_Graphic = gameObject.GetComponent<Graphic>();
            m_Graphics = gameObject.GetComponentsInChildren<Graphic>();
        }

        Type m_LastActiveType;

        void Update()
        {
            if (actionReference != null && actionReference.action != null && image != null && actionReference.action.enabled && actionReference.action.controls.Count > 0)
            {
                SetVisible(true);

                var typeToUse = actionReference.action.activeControl != null ? actionReference.action.activeControl.valueType : m_LastActiveType;

                if(typeToUse == typeof(bool))
                {
                    m_LastActiveType = typeof(bool);
                    bool value = actionReference.action.ReadValue<bool>();
                    image.color = value ? enabledColor : disabledColor;
                }
                else if(typeToUse == typeof(float))
                {
                    m_LastActiveType = typeof(float);
                    float value = actionReference.action.ReadValue<float>();
                    image.color = value > 0.5 ? enabledColor : disabledColor;
                } else image.color = disabledColor;
            } else SetVisible(false);
        }

        void SetVisible(bool visible)
        {
            if (m_Graphic != null)
                m_Graphic.enabled = visible;

            foreach (var t in m_Graphics)
                t.enabled = visible;
        }
    }

}
