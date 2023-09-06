using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket {

    public class HandVelocityEvent : MonoBehaviour {


        [Header("HAND CONTROLS")]
        [Space(10)]
        [SerializeField] float velocitySensitivity = 0.8f;
        [SerializeField] AnimationCurve rotationSensitivity = AnimationCurve.EaseInOut(0,0,1,1);

        [Header("PROGRESS CONTROLS")]
        [Space(10)]
        [SerializeField] float progressSpeed = 55;
        [SerializeField] float minDeltaPerFrame = 0.05f;


        [Header("READ ONLY")]
        [Space(10)]
        [SerializeField] [Range(0, 1)] float progress;
        float rotateVelocity;
        float rotationTotal;
        bool finished;

        [Space(10)]
        [SerializeField] List<ProgressEvent> progressEvents = new List<ProgressEvent>();



        // Update is called once per frame
        void Update() {
            if (finished) return;

            CalculateHandRotation();
            CheckProgressEvents();

            if (progress >= 1) {
                finished = true;

                if (GetComponents<Component>().Length > 2) {
                    Destroy(this);
                } else {
                    Destroy(gameObject);
                }

            }
        }


        void CalculateHandRotation() {

            // Save the current controller velocity and apply senitivity curve 
            rotateVelocity = Mathf.Clamp01(PTUtilities.instance.ControllerVelocity.magnitude / velocitySensitivity);
            rotationTotal = rotationSensitivity.Evaluate(rotateVelocity);

            if (rotationTotal > minDeltaPerFrame) progress = Mathf.Clamp01(progress + (rotationTotal * progressSpeed * 0.0001f));

        }


        int progressEventIndex = 0;
        void CheckProgressEvents() {

            for (int i = progressEventIndex; i < progressEvents.Count; i++) {
                if (progress >= progressEvents[i].threshold) {
                    if (progressEvents[i].progressEvent != null) progressEvents[i].progressEvent.Invoke();
                    progressEventIndex += 1;
                }

            }

        }

    }

}