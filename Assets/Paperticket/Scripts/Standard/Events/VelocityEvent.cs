using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket {

    public class VelocityEvent : MonoBehaviour {

        [System.Serializable] enum VelocityType { Transform, Rigidbody }
        [System.Serializable] enum EventBehaviour { OneTimeUse, ResendOnEnable, Looping }


        [SerializeField] Transform target;
        new Rigidbody rigidbody;

        [SerializeField] bool debugging;

        [Header("CONTROLS")]
        [Space(10)]
        [SerializeField] EventBehaviour eventBehaviour = EventBehaviour.OneTimeUse;
        [SerializeField] VelocityType velocityType = VelocityType.Transform;
        [Space(5)]
        [SerializeField] float velocitySensitivity = 0.8f;
        [SerializeField] AnimationCurve sensitivityCurve = AnimationCurve.EaseInOut(0,0,1,1);

        [Header("PROGRESS CONTROLS")]
        [Space(10)]
        [SerializeField] float progressSpeed = 55;
        [SerializeField] float minDeltaPerFrame = 0.05f;


        [Header("READ ONLY")]
        [Space(10)]
        [SerializeField] [Range(0, 1)] float progress;
        float velocity;
        float adjustedVelocity;
        bool disabled;

        Vector3 prevPos = Vector3.zero;

        [Space(10)]
        [SerializeField] List<ProgressEvent> progressEvents = new List<ProgressEvent>();

        [HideInInspector] public float Progress { get {return progress; } }

        void OnEnable() {
            rigidbody = (target == null) ? GetComponent<Rigidbody>() : target.GetComponent<Rigidbody>();
            if (velocityType == VelocityType.Rigidbody && rigidbody == null) {
                Debug.LogError("[VelocityEvent] ERROR -> No rigidbody assigned or found! Disabling.");
                enabled = false;
            }
            target = (target == null) ? transform : target;

            prevPos = transform.position;

            disabled = false;
        }

        // Update is called once per frame
        void FixedUpdate() {
            if (disabled) return;

            CalculateProgress();
            CheckProgressEvents();

            if (progress >= 1) Resolve();
        }

        Vector3 newVel = Vector3.zero;
        void CalculateProgress() {

            if (velocityType == VelocityType.Rigidbody) {
                velocity = Mathf.Clamp01(rigidbody.velocity.magnitude / velocitySensitivity);
            } else {
                newVel = (prevPos - transform.position) / Time.fixedDeltaTime;
                velocity = Mathf.Clamp01(newVel.magnitude / velocitySensitivity);
                prevPos = transform.position;
            }

            // Save the current controller velocity and apply senitivity curve        
            adjustedVelocity = sensitivityCurve.Evaluate(velocity);

            if (adjustedVelocity > minDeltaPerFrame) progress = Mathf.Clamp01(progress + (adjustedVelocity * progressSpeed * 0.0001f));

        }


        int progressEventIndex = 0;
        void CheckProgressEvents() {

            for (int i = progressEventIndex; i < progressEvents.Count; i++) {
                if (progress >= progressEvents[i].threshold) {
                    if (progressEvents[i].progressEvent != null) progressEvents[i].progressEvent.Invoke();
                    progressEventIndex = (progressEventIndex + 1) % progressEvents.Count;
                }

            }

        }

        void Resolve() {
            // Figure out what to do next
            switch (eventBehaviour) {

                case EventBehaviour.OneTimeUse:
                    // Destroy script as there are more components and/or children beneath this object
                    if (GetComponents<Component>().Length > 2 || transform.childCount > 0) {
                        if (debugging) Debug.Log("[VelocityEvent] One Time Use. There are still more components/children, destroying only this script.");
                        Destroy(this);
                    }
                    // Destroy game object as this was the last script remaining 
                    else {
                        if (debugging) Debug.Log("[VelocityEvent] One Time Use. No more components/children here, destroying this object.");
                        Destroy(gameObject);
                    }
                    break;

                case EventBehaviour.ResendOnEnable:
                    // Disable the update loop and wait for next enable
                    if (debugging) Debug.Log("[VelocityEvent] Resend On Enable. Waiting for next time this script turns on.");
                    disabled = true;
                    progress = 0;
                    break;

                case EventBehaviour.Looping:
                    // Immediately reset the timer and keep going
                    if (debugging) Debug.Log("[VelocityEvent] Looping. Resetting progress and starting again.");
                    progress = 0;
                    break;

                default:
                    Debug.LogError("[VelocityEvent] ERROR -> Bad event bevehaviour defined!");
                    disabled = true;
                    break;
            }
        }

    }

}