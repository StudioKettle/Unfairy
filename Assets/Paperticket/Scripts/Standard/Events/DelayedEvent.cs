using UnityEngine;
using UnityEngine.Events;

namespace Paperticket {
    public class DelayedEvent : MonoBehaviour {
        
        [System.Serializable] enum EventBehaviour { OneTimeUse, ResendOnEnable, Looping }
        [System.Serializable] enum TimeBehaviour { Scaled, Unscaled }

        [Header("CONTROLS")]
        [SerializeField] float timeBeforeEvent = 0;
        [SerializeField] EventBehaviour eventBehaviour = 0;
        [SerializeField] TimeBehaviour timeBehaviour = 0;
        [Space(5)]
        [SerializeField] bool debug = false;
        
        float timeToChange = 0;
        bool disabled = false;

        [Header("EVENT")]
        [SerializeField] UnityEvent2 OnEventTriggered;

        // Start is called before the first frame update
        void OnEnable() {
            disabled = false;

            if (timeBehaviour == TimeBehaviour.Scaled) timeToChange = Time.time + timeBeforeEvent;
            else timeToChange = Time.unscaledTime + timeBeforeEvent;

        }

        // Update is called once per frame
        void Update() {
            if (disabled) return;

            // Wait for the required time to pass
            if (timeBehaviour == TimeBehaviour.Scaled && Time.time > timeToChange ||
                timeBehaviour == TimeBehaviour.Unscaled && Time.unscaledTime > timeToChange) {

                // Trigger the event
                if (OnEventTriggered != null) {
                    if (debug) Debug.Log("[TimedEvent] OnEventTriggered called");
                    OnEventTriggered.Invoke();
                }
                             
                // Figure out what to do next
                switch (eventBehaviour) {

                    case EventBehaviour.OneTimeUse:
                        // Destroy script as there are more components and/or children beneath this object
                        if (GetComponents<Component>().Length > 2 || transform.childCount > 0) {
                            if (debug) Debug.Log("[TimedEvent] One Time Use. There are still more components/children, destroying only this script.");
                            Destroy(this);
                        } 
                        // Destroy game object as this was the last script remaining 
                        else {
                            if (debug) Debug.Log("[TimedEvent] One Time Use. No more components/children here, destroying this object.");
                            Destroy(gameObject);
                        }
                        break;
                    
                    case EventBehaviour.ResendOnEnable:
                        // Disable the update loop and wait for next enable
                        if (debug) Debug.Log("[TimedEvent] Resend On Enable. Waiting for next time this script turns on.");
                        disabled = true;
                        break;
                    
                    case EventBehaviour.Looping:
                        // Immediately reset the timer and keep going
                        if (debug) Debug.Log("[TimedEvent] Looping. Resetting timer and starting again.");
                        if (timeBehaviour == TimeBehaviour.Scaled) timeToChange = Time.time + timeBeforeEvent;
                        else timeToChange = Time.unscaledTime + timeBeforeEvent;
                        break;
                   
                    default:
                        Debug.LogError("[DelayedEvent] ERROR -> Bad event bevehaviour defined!");
                        disabled = true;
                        break;
                }


                // Destroy this script if this is a one time use, otherwise reset
                //if (OneTimeUse) {                   

                //    if (GetComponents<Component>().Length > 2 || transform.childCount > 0) {
                //        if (debug) Debug.Log("[TimedEvent] One time use is enabled, disabling this script");
                //        Destroy(this);
                //    } else {
                //        if (debug) Debug.Log("[TimedEvent] One time use is enabled, destroying this object");
                //        Destroy(gameObject);
                //    } 

                //} else {
                //    timeToChange = Time.time + timeBeforeEvent;
                //}
            }
        }

    }

}