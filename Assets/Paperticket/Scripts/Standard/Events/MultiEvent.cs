using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Paperticket;

public class MultiEvent : MonoBehaviour {
    [System.Serializable] enum EventBehaviour { ResendOnEnable, ResendOnEnableOnce, Looping }
    [System.Serializable] enum TimeBehaviour { Scaled, Unscaled }

    [SerializeField] bool debugging = false;

    [Header("CONTROLS")]

    [SerializeField] float timeBeforeEvent = 0;
    [SerializeField] EventBehaviour eventBehaviour = 0;
    [SerializeField] TimeBehaviour timeBehaviour = 0;

    [Header("EVENTS")]
    [Space(5)]
    [SerializeField] UnityEvent2[] multiEvents = null;
    int multiEventsInt = 0;    

    bool disabled = false;

    // Start is called before the first frame update
    void OnEnable() {
        disabled = false;
    }

    // Update is called once per frame
    void Update() {
        if (disabled) return;

        // Wait for the required time to pass
        if (timeBehaviour == TimeBehaviour.Scaled && Time.time > timeBeforeEvent ||
            timeBehaviour == TimeBehaviour.Unscaled && Time.unscaledTime > timeBeforeEvent) {

            // Invoke the current event if it has listeners
            if (multiEvents.Length > 0 && multiEvents[multiEventsInt] != null) {
                if (debugging) Debug.Log("[MultiEvent] MultiEvents[" + multiEventsInt + "] called!");
                multiEvents[multiEventsInt].Invoke();
            }
            multiEventsInt += 1;

            // Figure out what to do next
            switch (eventBehaviour) {

                case EventBehaviour.ResendOnEnable:
                    // Disable the update loop and wait for next enable
                    if (debugging) Debug.Log("[MultiEvent] Resend On Enable. Waiting for next time this script turns on.");
                    disabled = true;
                    break;

                case EventBehaviour.ResendOnEnableOnce:
                    // Disable if we still have events remaining
                    if (multiEventsInt < multiEvents.Length) {
                        if (debugging) Debug.Log("[MultiEvent] Resend On Enable Once. Still more events, waiting for next time this script turns on.");
                        disabled = true;
                    }
                    // Destroy if there are no events remaining
                    else {

                        // Destroy script as there are more components and/or children beneath this object
                        if (GetComponents<Component>().Length > 2 || transform.childCount > 0) {
                            if (debugging) Debug.Log("[MultiEvent] Resend On Enable Once. Finished last event, but still more components/children here, destroying only this script.");
                            Destroy(this);
                        }
                        // Destroy game object as this was the last script remaining 
                        else {
                            if (debugging) Debug.Log("[MultiEvent] Resend On Enable Once. Finished last event, no more components/children here, destroying this object.");
                            Destroy(gameObject);
                        }
                    } 
                    break;

                case EventBehaviour.Looping:
                    // Immediately reset the timer and keep going
                    if (debugging) Debug.Log("[MultiEvent] Looping. Resetting timer and starting again.");
                    if (timeBehaviour == TimeBehaviour.Scaled) timeBeforeEvent = Time.time + timeBeforeEvent;
                    else timeBeforeEvent = Time.unscaledTime + timeBeforeEvent;
                    break;

                default:
                    Debug.LogError("[MultiEvent] ERROR -> Bad event bevehaviour defined!");
                    disabled = true;
                    break;
            }

        }
    }

}
