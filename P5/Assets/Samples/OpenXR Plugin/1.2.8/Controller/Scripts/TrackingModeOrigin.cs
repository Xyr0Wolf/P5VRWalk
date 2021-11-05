using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UnityEngine.XR.OpenXR.Samples.ControllerSample
{
    public class TrackingModeOrigin : MonoBehaviour
    {
        [FormerlySerializedAs("m_RecenteredImage")]
        [SerializeField]
        Image recenteredImage;

        [FormerlySerializedAs("m_RecenteredOffColor")]
        [SerializeField]
        Color recenteredOffColor = Color.red;

        [FormerlySerializedAs("m_RecenteredColor")]
        [SerializeField]
        Color recenteredColor = Color.green;

        [FormerlySerializedAs("m_RecenteredColorResetTime")]
        [SerializeField]
        float recenteredColorResetTime = 1.0f;

        float m_LastRecenteredTime;

        [SerializeField]
        TrackingOriginModeFlags m_CurrentTrackingOriginMode;

        [SerializeField]
        Text m_CurrentTrackingOriginModeDisplay;

        [FormerlySerializedAs("m_DesiredTrackingOriginMode")]
        [SerializeField] TrackingOriginModeFlags desiredTrackingOriginMode;

        static List<XRInputSubsystem> s_InputSubsystems = new List<XRInputSubsystem>();

        void OnEnable()
        {
            SubsystemManager.GetInstances(s_InputSubsystems);
            foreach (var t in s_InputSubsystems) 
                t.trackingOriginUpdated += TrackingOriginUpdated;
        }

        void OnDisable()
        {
            SubsystemManager.GetInstances(s_InputSubsystems);
            foreach (var t in s_InputSubsystems) 
                t.trackingOriginUpdated -= TrackingOriginUpdated;
        }

        public void OnDesiredSelectionChanged(int newValue) => desiredTrackingOriginMode = (TrackingOriginModeFlags) (newValue == 0 ? 0 : 1 << (newValue - 1));

        void TrackingOriginUpdated(XRInputSubsystem obj) => m_LastRecenteredTime = Time.time;

        void Update()
        {
            XRInputSubsystem subsystem = null;

            SubsystemManager.GetInstances(s_InputSubsystems);
            if (s_InputSubsystems.Count > 0) 
                subsystem = s_InputSubsystems[0];

            if (m_CurrentTrackingOriginMode != desiredTrackingOriginMode & desiredTrackingOriginMode != TrackingOriginModeFlags.Unknown) 
                subsystem?.TrySetTrackingOriginMode(desiredTrackingOriginMode);

            m_CurrentTrackingOriginMode = subsystem?.GetTrackingOriginMode() ?? TrackingOriginModeFlags.Unknown;

            if (m_CurrentTrackingOriginModeDisplay != null)
                m_CurrentTrackingOriginModeDisplay.text = m_CurrentTrackingOriginMode.ToString();

            if (recenteredImage != null)
            {
                var lerp = (Time.time - m_LastRecenteredTime) / recenteredColorResetTime;
                lerp = Mathf.Clamp(lerp, 0.0f, 1.0f);
                recenteredImage.color = Color.Lerp(recenteredColor, recenteredOffColor, lerp);
            }
        }
    }
}
