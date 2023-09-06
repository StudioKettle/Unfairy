using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;

namespace Paperticket {
    public class CounterEvent : MonoBehaviour {

        [SerializeField] bool debugging = false;
        [Header("CONTROLS")]
        [Space(10)]
        [SerializeField] int eventThreshold = 1;
        [Space(10)]
        [SerializeField] float timeBeforeEvent = 0;
        [SerializeField] bool oneUse = true;
        [SerializeField] bool resetCounter = false;
        [Space(10)]
        [SerializeField] UnityEvent2 counterEvent = null;


        int currentCount = 0;
        bool finished = false;


        [Header("LIVE VARIABLES")]
        [Space(5)]
        [SerializeField] bool eventLocked = false;

        public bool locked {
            get { return eventLocked; }
            set { eventLocked = value; }
        }

        void Check() {
            if (finished || eventLocked) return;
            if (currentCount >= eventThreshold) {

                finished = true;

                if (counterEvent == null) return;

                if (timeBeforeEvent > 0) StartCoroutine(WaitForEvent());
                else SendEventAndFinish();
            }
        }

        void SendEventAndFinish() {
            counterEvent.Invoke();

            if (oneUse) {
                if (debugging) Debug.Log("[CounterEvent] One Use enabled, destroying this script");
                Destroy(this);
            } else if (resetCounter) {
                currentCount = 0;
            }
            finished = false;
        }

        IEnumerator WaitForEvent() {
            yield return new WaitForSeconds(timeBeforeEvent);
            SendEventAndFinish();
        }







        #region PUBLIC CALLS

        public void Increment() {
            if (eventLocked) return;
            currentCount += 1;
            Check();
        }

        public void Decrement() {
            if (eventLocked) return;
            currentCount -= 1;
            Check();
        }

        public void Add( int amount ) {
            if (eventLocked) return;
            currentCount += amount;
            Check();
        }
        public void Remove( int amount ) {
            if (eventLocked) return;
            currentCount -= amount;
            Check();
        }


        public void ResetCounter() {
            currentCount = 0;
        }


        #endregion

    }
}