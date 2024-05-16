using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket {
    public class VideoEvent : MonoBehaviour {
        [System.Serializable] enum EventBehaviour { OneTimeUse, ResendOnEnable }

        [Header("REFERENCES")]
        [Space(5)]
        [SerializeField] VideoController videoController = null;

        [Header("CONTROLS")]
        [Space(10)]
        [SerializeField] float videoTimeBeforeEvent = 0;
        [SerializeField] EventBehaviour eventBehaviour = 0;
        [Space(5)]
        [SerializeField] bool debug = false;


        [Header("EVENT")]
        [Space(5)]
        [SerializeField] UnityEvent2 OnEventTriggered = null;

        bool disabled = false;


        // Start is called before the first frame update
        void OnEnable() {
            disabled = false;

            if (!videoController) {
                Debug.LogError("[VideoEvent] ERROR -> No VideoController defined! Disabling...");
                enabled = false;
            }          

        }

        void Update() {
            if (disabled) return;
            if (!videoController.playingVideo) return;
            if (videoController.currentVideoTime >= videoTimeBeforeEvent) {

                // Trigger the event
                if (OnEventTriggered != null) {
                    if (debug) Debug.Log("[VideoEvent] OnEventTriggered called");
                    OnEventTriggered.Invoke();
                }

                // Figure out what to do next
                Resolve();

            }
        }


        public void ForceEvent() {
            // Trigger the event
            if (OnEventTriggered != null) {
                if (debug) Debug.Log("[VideoEvent] OnEventTriggered called");
                OnEventTriggered.Invoke();
            }

            // Figure out what to do next
            Resolve();

        }

        void Resolve() {
            // Figure out what to do next
            switch (eventBehaviour) {

                case EventBehaviour.OneTimeUse:
                    // Destroy script as there are more components and/or children beneath this object
                    if (GetComponents<Component>().Length > 2 || transform.childCount > 0) {
                        if (debug) Debug.Log("[VideoEvent] One Time Use. There are still more components/children, destroying only this script.");
                        Destroy(this);
                    }
                    // Destroy game object as this was the last script remaining 
                    else {
                        if (debug) Debug.Log("[VideoEvent] One Time Use. No more components/children here, destroying this object.");
                        Destroy(gameObject);
                    }
                    break;

                case EventBehaviour.ResendOnEnable:
                    // Disable the update loop and wait for next enable
                    if (debug) Debug.Log("[VideoEvent] Resend On Enable. Waiting for next time this script turns on.");
                    disabled = true;
                    break;

                default:
                    Debug.LogError("[DelayedEvent] ERROR -> Bad event bevehaviour defined!");
                    disabled = true;
                    break;
            }
        }

    }
}