using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Paperticket;

public class MultiEvent : MonoBehaviour {
    [System.Serializable] public enum EventBehaviour { ResendOnEnable, ResendOnEnableOnce, Looping, LoopingOnce }
    [System.Serializable] enum TimeBehaviour { Scaled, Unscaled }

    [SerializeField] bool debugging = false;

    [Header("CONTROLS")]

    [SerializeField] float timeBeforeEvent = 0;
    [SerializeField] EventBehaviour eventBehaviour = 0;
    [SerializeField] TimeBehaviour timeBehaviour = 0;
    public EventBehaviour _eventBehaviour {
        set { eventBehaviour = value; }
    }

    [Header("EVENTS")]
    [Space(5)]
    [SerializeField] UnityEvent2[] multiEvents = null;
    int multiEventsInt = 0;

    float timeToChange = 0;

    bool disabled = false;

    // Start is called before the first frame update
    void OnEnable() {
        disabled = false;
        if (multiEventsInt >= multiEvents.Length) multiEventsInt = 0;

        if (timeBehaviour == TimeBehaviour.Scaled) timeToChange = Time.time + timeBeforeEvent;
        else timeToChange = Time.unscaledTime + timeBeforeEvent;
    }

    // Update is called once per frame
    void Update() {
        if (disabled) return;

        // Wait for the required time to pass
        if (timeBehaviour == TimeBehaviour.Scaled && Time.time > timeToChange ||
            timeBehaviour == TimeBehaviour.Unscaled && Time.unscaledTime > timeToChange) {

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
                    if (debugging) Debug.Log("[MultiEvent] Looping. Resetting timer and starting again.");

                    // Restart at the first event if we've run out of events
                    if (multiEventsInt >= multiEvents.Length) multiEventsInt = 0;

                    // Reset the timer and keep going
                    if (timeBehaviour == TimeBehaviour.Scaled) timeToChange = Time.time + timeBeforeEvent;
                    else timeToChange = Time.unscaledTime + timeBeforeEvent;

                    break;

                case EventBehaviour.LoopingOnce:
                    // Check if we just sent out the last event
                    if (multiEventsInt >= multiEvents.Length) {

                        // Disable script as there are more components and/or children beneath this object
                        if (GetComponents<Component>().Length > 2 || transform.childCount > 0) {
                            if (debugging) Debug.Log("[MultiEvent] Looping Once. Finished last event, but still more components/children here, disabling only this script.");
                            disabled = true;
                            enabled = false;
                        }
                        // Disable game object as this was the last script remaining 
                        else {
                            if (debugging) Debug.Log("[MultiEvent] Looping Once. Finished last event, no more components/children here, disabling this object.");
                            gameObject.SetActive(false);
                        }                        
                        break;
                    }
                    // Otherwise, reset the timer and keep going
                    else if (debugging) Debug.Log("[MultiEvent] Looping Once. Still more objects to go.");     
                    if (timeBehaviour == TimeBehaviour.Scaled) timeToChange = Time.time + timeBeforeEvent;
                    else timeToChange = Time.unscaledTime + timeBeforeEvent;

                    break;

                default:
                    Debug.LogError("[MultiEvent] ERROR -> Bad event bevehaviour defined!");
                    disabled = true;
                    break;
            }

        }
    }

}
