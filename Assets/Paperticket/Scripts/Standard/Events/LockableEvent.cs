using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket {
    public class LockableEvent : MonoBehaviour {

        [SerializeField] bool debugging = false;

        [Header("CONTROLS")]
        [Space(10)]
        [SerializeField] [Min(0)] float timeBeforeEvent;

        [Header("LIVE VARIABLES")]
        [Space(10)]
        [SerializeField] bool locked;

        [Header("EVENT")]
        [Space(5)]
        [SerializeField] UnityEvent2 onEvent = null;

        Coroutine waitCo;

        public void ToggleLock(bool toggle) {
            locked = toggle;

            if (debugging) Debug.Log("[LockableEvent] Setting event status: " + (locked ? "locked" : "unlocked"));
        }

        public void SendEvent(bool lockEvent ) {
            if (locked || onEvent == null) return;

            if (timeBeforeEvent > 0) {

                if (waitCo != null) {
                    Debug.LogWarning("[LockableEvent] Already waiting to send event, cancelling...");
                    return;
                }

                waitCo = StartCoroutine(WaitForEvent(lockEvent));

            } else {

                onEvent.Invoke();
                if (lockEvent) locked = true;

                if (debugging) Debug.Log("[LockableEvent] Sending event! Event status: " + (locked ? "locked" : "unlocked"));
            }
        }


        IEnumerator WaitForEvent(bool lockEvent) {

            yield return new WaitForSeconds(timeBeforeEvent);

            onEvent.Invoke();
            if (lockEvent) locked = true;
            waitCo = null;

            if (debugging) Debug.Log("[LockableEvent] Sending event! Event status: " + (locked ? "locked" : "unlocked"));           
        }
    }

}