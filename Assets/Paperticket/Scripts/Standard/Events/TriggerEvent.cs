using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Paperticket;

public class TriggerEvent : MonoBehaviour {
    [System.Serializable] enum EventBehaviour { OneTimeUse, ResendOnEnable, Looping }
    [System.Serializable] enum TriggerBehaviour { OnTriggerEnter, OnTriggerStay, OnTriggerExit }

    [SerializeField] bool debugging = false;

    [Header("CONTROLS")]

    [SerializeField] LayerMask targetLayers = 0;
    [SerializeField] string targetTag = "";
    [SerializeField] string targetName = "";
    [Space(5)]
    [SerializeField] TriggerBehaviour triggerBehaviour = 0;
    [SerializeField] float timeBeforeEvent = 0;
    [SerializeField] EventBehaviour eventBehaviour = 0;

    [Header("EVENTS")]
    [Space(5)]
    [SerializeField] UnityEvent2[] triggerEvents = null;
    int triggerEventsInt = 0;    

    Rigidbody rb = null;
    bool disabled = false;

    // Start is called before the first frame update
    void OnEnable() {
        disabled = false;
    }

    void OnTriggerEnter(Collider other) {
        if (disabled || triggerBehaviour != TriggerBehaviour.OnTriggerEnter) return;

        // Skip the object if it is the wrong layer / tag / name
        if (!other.gameObject.CheckLayerAndTag(targetLayers, targetTag, debugging)) return;
        if (targetName.Length > 0 && other.gameObject.name != targetName) {
            if (debugging) Debug.Log("[TriggerEvent] GameObject '"+other.gameObject.name+"' does not match target name '"+targetName+"', returning");
            return;
        }
        
        // Trigger the delayed event
        if (waitForEventCo == null) {
            if (debugging) Debug.Log("[TriggerEvent] Trigger entered! Beginning wait for event...");
            waitForEventCo = StartCoroutine(WaitForEvent());
        }
    }

    void OnTriggerStay(Collider other) {
        if (disabled || triggerBehaviour != TriggerBehaviour.OnTriggerStay) return;

        // Skip the object if it is the wrong layer / tag / name
        if (!other.gameObject.CheckLayerAndTag(targetLayers, targetTag, debugging)) return;
        if (targetName.Length > 0 && other.gameObject.name != targetName) {
            if (debugging) Debug.Log("[TriggerEvent] GameObject '"+other.gameObject.name+"' does not match target name '"+targetName+"', returning");
            return;
        }

        // Trigger the delayed event
        if (waitForEventCo == null) {
            if (debugging) Debug.Log("[TriggerEvent] Trigger stayed! Beginning wait for event...");
            waitForEventCo = StartCoroutine(WaitForEvent());
        }
    }

    void OnTriggerExit(Collider other) {
        if (disabled || triggerBehaviour != TriggerBehaviour.OnTriggerExit) return;

        // Skip the object if it is the wrong layer / tag / name
        if (!other.gameObject.CheckLayerAndTag(targetLayers, targetTag, debugging)) return;
        if (targetName.Length > 0 && other.gameObject.name != targetName) {
            if (debugging) Debug.Log("[TriggerEvent] GameObject '"+other.gameObject.name+"' does not match target name '"+targetName+"', returning");
            return;
        }

        // Trigger the delayed event
        if (waitForEventCo == null) {
            if (debugging) Debug.Log("[TriggerEvent] Trigger exited! Beginning wait for event...");
            waitForEventCo = StartCoroutine(WaitForEvent());
        }
    }


    Coroutine waitForEventCo = null;
    IEnumerator WaitForEvent() {
        yield return new WaitForSeconds(timeBeforeEvent);
        
        // Invoke the current event if it has listeners
        if (triggerEvents.Length > 0 && triggerEvents[triggerEventsInt] != null) {
            triggerEvents[triggerEventsInt].Invoke();
        }

        if (debugging) Debug.Log("[TriggerEvent] Sending TriggerEvent #"+triggerEventsInt+"!");

        CheckEventBehaviour();

        waitForEventCo = null;
    }



    void CheckEventBehaviour() {

        // Figure out what to do next
        switch (eventBehaviour) {

            case EventBehaviour.OneTimeUse:
                // Destroy script as there are more components and/or children beneath this object
                if (GetComponents<Component>().Length > 2 || transform.childCount > 0) {
                    if (debugging) Debug.Log("[TriggerEvent] One Time Use. There are still more components/children, destroying only this script.");
                    Destroy(this);
                }
                // Destroy game object as this was the last script remaining 
                else {
                    if (debugging) Debug.Log("[TriggerEvent] One Time Use. No more components/children here, destroying this object.");
                    Destroy(gameObject);
                }
                break;

            case EventBehaviour.ResendOnEnable:
                // Increment the event, then disable the loop and wait for next enable
                triggerEventsInt = (triggerEventsInt + 1) % triggerEvents.Length;
                if (debugging) Debug.Log("[TriggerEvent] Resend On Enable. Incrementing event, then waiting until the next time this script turns on.");
                disabled = true;
                break;
                

            case EventBehaviour.Looping:
                // Increment the event and keep going
                triggerEventsInt = (triggerEventsInt + 1) % triggerEvents.Length;
                if (debugging) Debug.Log("[TriggerEvent] Looping. Incrementing event and starting again.");
                break;

            default:
                Debug.LogError("[TriggerEvent] ERROR -> Bad event bevehaviour defined!");
                disabled = true;
                break;
        }


    }



    //bool CheckLayerAndTag(GameObject collidedObject) {
    //    if (!targetLayers.Test(collidedObject.layer)) {
    //        if (debugging) Debug.Log("[TriggerEvent] GameObject '" + collidedObject.name + "' layer '" + LayerMask.LayerToName(collidedObject.layer) + "' is invalid, ignoring.");
    //        return false;
    //    }
    //    if (targetTag.Length > 0 && collidedObject.tag != targetTag) {
    //        if (debugging) Debug.Log("[TriggerEvent] GameObject '" + collidedObject.name + "' tag '" + collidedObject.tag + "' is invalid, ignoring.");
    //        return false;
    //    }
    //    return true;
    //}
}
