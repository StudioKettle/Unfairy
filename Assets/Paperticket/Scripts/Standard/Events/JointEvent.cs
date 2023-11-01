using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Paperticket;

[RequireComponent(typeof(Joint))]
public class JointEvent : MonoBehaviour {
    [System.Serializable] enum EventBehaviour { OneTimeUse, ResendOnEnable }

    [SerializeField] bool debugging = false;

    [Header("CONTROLS")]

    Joint joint;
    [Space(5)]
    [SerializeField] float timeBeforeEvent = 0;
    [SerializeField] EventBehaviour eventBehaviour = 0;

    [Header("EVENTS")]
    [Space(5)]
    [SerializeField] UnityEvent2[] brokenEvents = null;
    int brokenEventsInt = 0;    

    bool disabled = false;

    // Start is called before the first frame update
    void OnEnable() {

        joint = GetComponent<Joint>();

        if (joint.connectedBody == null) {
            Debug.LogError("[JointEvent] ERROR -> No connected body on joint! Disabling.");
            enabled = false;
        }

        disabled = false;
    }

    void FixedUpdate() {
        if (disabled) return;
        if (joint.connectedBody == null) {
            JointBroken();
        }
    }

    void OnJointBreak(float breakForce) {
        if (disabled) return;
        JointBroken();
    }

    void JointBroken() {

        // Trigger the delayed event
        if (waitForEventCo == null) {
            if (debugging) Debug.Log("[JointEvent] Joint broken! Beginning wait for event...");
            waitForEventCo = StartCoroutine(WaitForEvent());
        }

    }


    Coroutine waitForEventCo = null;
    IEnumerator WaitForEvent() {
        yield return new WaitForSeconds(timeBeforeEvent);
        
        // Invoke the current event if it has listeners
        if (brokenEvents.Length > 0 && brokenEvents[brokenEventsInt] != null) {
            brokenEvents[brokenEventsInt].Invoke();
        }

        if (debugging) Debug.Log("[JointEvent] Sending TriggerEvent #" + brokenEventsInt+ "!");

        CheckEventBehaviour();

        waitForEventCo = null;
    }



    void CheckEventBehaviour() {

        // Figure out what to do next
        switch (eventBehaviour) {

            case EventBehaviour.OneTimeUse:
                // Destroy script as there are more components and/or children beneath this object
                if (GetComponents<Component>().Length > 2 || transform.childCount > 0) {
                    if (debugging) Debug.Log("[JointEvent] One Time Use. There are still more components/children, destroying only this script.");
                    Destroy(this);
                }
                // Destroy game object as this was the last script remaining 
                else {
                    if (debugging) Debug.Log("[JointEvent] One Time Use. No more components/children here, destroying this object.");
                    Destroy(gameObject);
                }
                break;

            case EventBehaviour.ResendOnEnable:
                // Increment the event, then disable the loop and wait for next enable
                brokenEventsInt = (brokenEventsInt + 1) % brokenEvents.Length;
                if (debugging) Debug.Log("[JointEvent] Resend On Enable. Incrementing event, then waiting until the next time this script turns on.");
                disabled = true;
                break;
                

            //case EventBehaviour.Looping:
            //    // Increment the event and keep going
            //    brokenEventsInt = (brokenEventsInt + 1) % brokenEvents.Length;
            //    if (debugging) Debug.Log("[JointEvent] Looping. Incrementing event and starting again.");
            //    break;

            default:
                Debug.LogError("[JointEvent] ERROR -> Bad event bevehaviour defined!");
                disabled = true;
                break;
        }


    }


}
