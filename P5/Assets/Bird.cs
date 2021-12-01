using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DefaultNamespace {
    [RequireComponent(typeof(Animator))]
    public class Bird : MonoBehaviour {
        [Header("Input")]
        [SerializeField] private InputActionReference pressToSteerToCenter;
        [SerializeField] Material triggerMaterial;
        [SerializeField] private Color triggerColor;
        
        [Header("Bird physics")]
        [SerializeField] private float3 offset;
        [SerializeField] private float accelerationSpeed = 1f;
        [SerializeField] private float threshold = 0.01f;
        [SerializeField] private float maxVelMagnitudeSQ = 1f;
        [SerializeField] [Range(0,1)] private float friction = 0.5f;

        [Header("Bird To Center")] 
        [SerializeField] private float timeToStayAtCenter;
        private float timeToStayAtCenterLeft;

        [Header("Wing Flaps")] 
        [SerializeField] private float secondsBetweenFlaps;
        [SerializeField] private float speedSqBeforeFlaps;
        
        private float3 vel;
        private Camera m_Cam;
        private Animator anim;

        private void Start() {
            m_Cam = Camera.main;
            triggerMaterial.SetColor ("_EmissionColor", triggerColor);
            anim = GetComponent<Animator>();
            StartCoroutine(FlapWings());

            pressToSteerToCenter.action.performed += context => timeToStayAtCenterLeft = timeToStayAtCenter;
        }

        IEnumerator FlapWings() {
            while (enabled) {
                yield return new WaitForSeconds(secondsBetweenFlaps);
                if(math.length(vel) > speedSqBeforeFlaps)
                    anim.SetTrigger("Fly");
            }
        }


        private void Update() {
            float3 dstPoint = m_Cam.transform.TransformPoint(offset);
            if (timeToStayAtCenterLeft > 0) {
                timeToStayAtCenterLeft -= Time.deltaTime;
                dstPoint = math.up()*1.5f;
            }

            float3 currentBirdLocation = transform.position;
            if (math.distance(currentBirdLocation, dstPoint) < threshold) return;
            
            var goInDir = math.normalize(dstPoint-currentBirdLocation);
            vel += goInDir*Time.deltaTime*accelerationSpeed;
            vel *= 1-friction;

            var velLengthSq = math.lengthsq(vel);
            vel = velLengthSq > maxVelMagnitudeSQ ? math.normalize(vel)*maxVelMagnitudeSQ : vel;
            transform.position += (Vector3)vel*Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation,quaternion.LookRotationSafe(vel, math.up()), Time.deltaTime*5);
        }

        private void OnDestroy() {
            triggerMaterial.SetColor ("_EmissionColor", Color.black);
        }
    }
}