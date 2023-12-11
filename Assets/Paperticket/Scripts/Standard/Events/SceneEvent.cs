using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Paperticket;

public class SceneEvent : MonoBehaviour {
    [System.Serializable] enum SceneBehaviour { OnSceneLoad, OnSceneUnload, OnSceneAlmostReady, OnSceneMadeActive }
    [System.Serializable] enum EventBehaviour { OneTimeUse, ResendOnEnable, Looping }

    [SerializeField] bool debugging = false;

    [Header("CONTROLS")]
    [Space(5)]
    [SerializeField] float timeBeforeEvent = 0;
    [SerializeField] SceneBehaviour sceneBehaviour = 0;
    [SerializeField] EventBehaviour eventBehaviour = 0;

    [Header("EVENTS")]
    [Space(5)]
    [SerializeField] UnityEvent2[] sceneEvents = null;
    int sceneEventsInt = 0;    

    bool disabled = false;

    // Start is called before the first frame update
    void OnEnable() {
        disabled = false;

        switch (sceneBehaviour) {
            case SceneBehaviour.OnSceneLoad:
                SceneUtilities.OnSceneLoad += SendEvent;
                break;
            case SceneBehaviour.OnSceneUnload:
                SceneUtilities.OnSceneUnload += SendEvent;
                break;
            case SceneBehaviour.OnSceneAlmostReady:
                SceneUtilities.OnSceneAlmostReady += SendEvent;
                break;
            case SceneBehaviour.OnSceneMadeActive:
                SceneUtilities.OnSceneMadeActive += SendEvent;
                break;
            default:
                break;
        }

    }

    private void OnDisable() {
        switch (sceneBehaviour) {
            case SceneBehaviour.OnSceneLoad:
                SceneUtilities.OnSceneLoad -= SendEvent;
                break;
            case SceneBehaviour.OnSceneUnload:
                SceneUtilities.OnSceneUnload -= SendEvent;
                break;
            case SceneBehaviour.OnSceneAlmostReady:
                SceneUtilities.OnSceneAlmostReady -= SendEvent;
                break;
            case SceneBehaviour.OnSceneMadeActive:
                SceneUtilities.OnSceneMadeActive -= SendEvent;
                break;
            default:
                break;
        }
    }

    void SendEvent() {
        if (disabled) return;

        if (timeBeforeEvent > 0) {

            if (waitForEventCo != null) {
                Debug.LogWarning("[SceneEvent] Already waiting to send event, cancelling...");
                return;
            }

            waitForEventCo = StartCoroutine(WaitForEvent());

            if (debugging) Debug.Log("[SceneEvent] Sending event.");
        } 
    }

    Coroutine waitForEventCo = null;
    IEnumerator WaitForEvent() {
        yield return new WaitForSeconds(timeBeforeEvent);
        
        // Invoke the current event if it has listeners
        if (sceneEvents.Length > 0 && sceneEvents[sceneEventsInt] != null) {
            sceneEvents[sceneEventsInt].Invoke();
        }

        if (debugging) Debug.Log("[SceneEvent] Sending TriggerEvent #" + sceneEventsInt+"!");

        CheckEventBehaviour();

        waitForEventCo = null;
    }



    void CheckEventBehaviour() {

        // Figure out what to do next
        switch (eventBehaviour) {

            case EventBehaviour.OneTimeUse:
                // Destroy script as there are more components and/or children beneath this object
                if (GetComponents<Component>().Length > 2 || transform.childCount > 0) {
                    if (debugging) Debug.Log("[SceneEvent] One Time Use. There are still more components/children, destroying only this script.");
                    Destroy(this);
                }
                // Destroy game object as this was the last script remaining 
                else {
                    if (debugging) Debug.Log("[SceneEvent] One Time Use. No more components/children here, destroying this object.");
                    Destroy(gameObject);
                }
                break;

            case EventBehaviour.ResendOnEnable:
                // Increment the event, then disable the loop and wait for next enable
                sceneEventsInt = (sceneEventsInt + 1) % sceneEvents.Length;
                if (debugging) Debug.Log("[SceneEvent] Resend On Enable. Incrementing event, then waiting until the next time this script turns on.");
                disabled = true;
                break;
                

            case EventBehaviour.Looping:
                // Increment the event and keep going
                sceneEventsInt = (sceneEventsInt + 1) % sceneEvents.Length;
                if (debugging) Debug.Log("[SceneEvent] Looping. Incrementing event and starting again.");
                break;

            default:
                Debug.LogError("[SceneEvent] ERROR -> Bad event behaviour defined!");
                disabled = true;
                break;
        }


    }

}
