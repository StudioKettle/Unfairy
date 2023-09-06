using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket {
    public class AngleEvent : MonoBehaviour {
        [System.Serializable] enum EventBehaviour { OneTimeUse, ResendOnEnable, Looping }
        [System.Serializable] enum AngleBehaviour { Headset, LeftHand, RightHand, OptionalTransform }

        [SerializeField] bool debugging = false;
        [SerializeField] Transform transformToCheck = null;

        [Header("CONTROLS")]
        [SerializeField] float thresholdAngle = 10;
        [SerializeField] float checkRate = 1f;
        [Space(5)]
        [SerializeField] AngleBehaviour angleToCheck = 0;
        [SerializeField] Transform optionalTransform = null;
        [Space(5)]
        [SerializeField] float timeBeforeEvent = 0;
        [SerializeField] EventBehaviour eventBehaviour = 0;

        [Header("LIVE VARIABLES")]
        [Space(10)]
        [SerializeField] bool locked;

        [Header("EVENTS")]
        [Space(5)]
        [SerializeField] UnityEvent2 onEvent = null;
        [Space(5)]
        [SerializeField] UnityEvent2 offEvent = null;

        bool disabled = false;

        bool seen = false;

        void OnEnable() {
            if (transformToCheck == null) {
                Debug.Log("[AngleEvent] No TransformToCheck assigned! Assigning this transform ('" + name + "')");
                transformToCheck = transform;
            }
            
            disabled = false;
            seen = false;

            StartCoroutine(TestingAngle());
        }

        IEnumerator TestingAngle() {
            Vector3 forward = Vector3.zero;
            Vector3 pos = Vector3.zero;

            while (!disabled) {

                yield return new WaitForSeconds(checkRate);

                switch (angleToCheck) {
                    case AngleBehaviour.Headset:
                        forward = PTUtilities.instance.HeadsetForward();
                        pos = PTUtilities.instance.HeadsetPosition();
                        if (debugging) Debug.Log("[AngleEvent] Checking angle to headset...");
                        break;
                    case AngleBehaviour.LeftHand:
                        forward = PTUtilities.instance.leftController.transform.forward;
                        pos = PTUtilities.instance.leftController.transform.position;
                        if (debugging) Debug.Log("[AngleEvent] Checking angle to left hand...");
                        break;
                    case AngleBehaviour.RightHand:
                        forward = PTUtilities.instance.rightController.transform.forward;
                        pos = PTUtilities.instance.rightController.transform.position;

                        if (debugging) Debug.Log("[AngleEvent] Checking angle to right hand...");
                        break;
                    case AngleBehaviour.OptionalTransform:
                        if (optionalTransform != null) {
                            forward = optionalTransform.forward;
                            pos = optionalTransform.position;
                            if (debugging) Debug.Log("[AngleEvent] Checking angle to transform '"+optionalTransform.name+"'...");
                        } else {
                            Debug.LogWarning("[AngleEvent] Optional Transform not set! Ignoring");
                            yield return null;
                            continue;
                        }
                        break;
                    default:
                        Debug.LogError("[AngleEvent] Bad AngleBehaviour defined! Cancelling");
                        yield break;
                }

                if (debugging) Debug.Log("[AngleEvent] Angle to exit region = " + Vector3.Angle(forward, transform.position - pos) + " / " + thresholdAngle);

                if (Vector3.Angle(forward, transformToCheck.position - pos) < thresholdAngle) {
                    
                    if (waitForEventCo == null) {
                        if (debugging) Debug.Log("[AngleEvent] Angle reached! Beginning wait for event!");
                        waitForEventCo = StartCoroutine(WaitForEvent());
                    }

                } else {

                    if (waitForEventCo != null) {
                        if (debugging) Debug.Log("[AngleEvent] Out of angle, cancelling previous wait for event");
                        StopCoroutine(waitForEventCo);
                        waitForEventCo = null;
                    }

                    if (seen) {
                        if (debugging) Debug.Log("[AngleEvent] Sending offEvent");
                        if (offEvent != null) offEvent.Invoke();
                    } 
                }
                                                         

            }

        }

        

        Coroutine waitForEventCo = null;
        IEnumerator WaitForEvent( ) {
            yield return new WaitForSeconds(timeBeforeEvent);
            onEvent.Invoke();

            if (debugging) Debug.Log("[AngleEvent] Sending onEvent!");

            CheckEventBehaviour();

            waitForEventCo = null;
        }



        void CheckEventBehaviour() {

            // Figure out what to do next
            switch (eventBehaviour) {

                case EventBehaviour.OneTimeUse:
                    // Destroy script as there are more components and/or children beneath this object
                    if (GetComponents<Component>().Length > 2 || transform.childCount > 0) {
                        if (debugging) Debug.Log("[AngleEvent] One Time Use. There are still more components/children, destroying only this script.");
                        Destroy(this);
                    }
                    // Destroy game object as this was the last script remaining 
                    else {
                        if (debugging) Debug.Log("[AngleEvent] One Time Use. No more components/children here, destroying this object.");
                        Destroy(gameObject);
                    }
                    break;

                case EventBehaviour.ResendOnEnable:
                    // Disable the loop and wait for next enable
                    if (debugging) Debug.Log("[AngleEvent] Resend On Enable. Waiting for next time this script turns on.");
                    disabled = true;
                    break;

                case EventBehaviour.Looping:
                    // Immediately reset the timer and keep going
                    seen = true;
                    if (debugging) Debug.Log("[AngleEvent] Looping. Resetting timer and starting again.");
                    break;

                default:
                    Debug.LogError("[AngleEvent] ERROR -> Bad event bevehaviour defined!");
                    disabled = true;
                    break;
            }


        }


    }
}
