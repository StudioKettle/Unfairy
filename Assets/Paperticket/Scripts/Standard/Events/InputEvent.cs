using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket {

    public class InputEvent : MonoBehaviour {

        public enum ControllerInput { Primary_Button, Trigger, Both }

        [SerializeField] bool debugging = false;

        [Header("CONTROLS")]

        [Space(10)]
        [SerializeField] ControllerInput requiredInput;
        [Space(10)]
        [SerializeField] float timeBeforeEvent = 0;
        [SerializeField] bool OneTimeUse = true;


        [Header("EVENT")]
        [Space(5)]
        [SerializeField] UnityEvent2 inputEvent = null;

        bool finished = false;



        void Update() {
            if (finished) return;
            if (inputEvent == null) return;

            switch (requiredInput) {
                case ControllerInput.Primary_Button:
                    if (!PTUtilities.instance.ControllerPrimaryButton) return;
                    break;
                case ControllerInput.Trigger:
                    if (!PTUtilities.instance.ControllerTriggerButton) return;
                    break;
                case ControllerInput.Both:
                    if (!PTUtilities.instance.ControllerPrimaryButton && !PTUtilities.instance.ControllerTriggerButton) return;
                    break;
                default:
                    Debug.LogError("[InputEvent] ERROR -> Bad controller input enum result! Something has gone terribly wrong...");
                    break;
            }

            finished = true;

            if (timeBeforeEvent > 0) StartCoroutine(WaitForEvent());
            else SendEventAndFinish();

        }


        void SendEventAndFinish() {
            inputEvent.Invoke();

            if (OneTimeUse) {
                if (debugging) Debug.Log("[CounterEvent] One Use enabled, destroying this script");
                Destroy(this);
            } 

            finished = false;
        }

        IEnumerator WaitForEvent() {
            yield return new WaitForSeconds(timeBeforeEvent);
            SendEventAndFinish();
        }

    }

}