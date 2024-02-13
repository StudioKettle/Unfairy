using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket {
    public class PostAudioEvent : MonoBehaviour {

        [SerializeField] bool debugging = false;

        [Header("AUDIO EVENTS")]
        [Space(5)]
        [SerializeField] AudioEvent[] audioEvents = null;
        [SerializeField] AudioSwitch[] audioSwitches = null;

        #region Events

        public void SendEvent(int eventIndex) {

            if (eventIndex >= audioEvents.Length) return;


            if (audioEvents[eventIndex].timeBeforeEvent > 0) {
                StartCoroutine(WaitForEvent(eventIndex));
            } else {
                PostEvent(eventIndex);
            }
        }


        IEnumerator WaitForEvent(int eventIndex) {
            yield return new WaitForSeconds(audioEvents[eventIndex].timeBeforeEvent);
            PostEvent(eventIndex);
        }

        void PostEvent(int eventIndex) {
            var audioEvent = audioEvents[eventIndex];
            
            // Play at the optional source GO, otherwise play on this GO
            var optionalSource = (audioEvent.optionalSource == null) ? PTUtilities.instance.headProxy.gameObject : audioEvent.optionalSource;

            audioEvent.audioEvent.Post(optionalSource, audioEvent.callbackFlags, null);

            if (debugging) Debug.Log("[PostAudioEvent] Posting audio event '"+audioEvent.audioEvent.Name+"' at GameObject '"+optionalSource+"'");
        }


        #endregion


        #region Switches


        public void ChangeSwitch(int switchIndex) {

            if (switchIndex >= audioSwitches.Length) return;


            if (audioSwitches[switchIndex].timeBeforeSwitch > 0) {
                StartCoroutine(WaitForSwitch(switchIndex));
            } else {
                SetSwitch(switchIndex);
            }
        }


        IEnumerator WaitForSwitch(int switchIndex) {
            yield return new WaitForSeconds(audioSwitches[switchIndex].timeBeforeSwitch);
            SetSwitch(switchIndex);
        }

        void SetSwitch(int switchIndex) {
            var audioSwitch = audioSwitches[switchIndex];

            // Play at the optional source GO, otherwise play on this GO
            var optionalSource = (audioSwitch.optionalSource == null) ? PTUtilities.instance.headProxy.gameObject : audioSwitch.optionalSource;

            audioSwitch.audioSwitch.SetValue(optionalSource);

            if (debugging) Debug.Log("[PostAudioEvent] Setting switch '" + audioSwitch.audioSwitch.Name + "' at GameObject '" + optionalSource + "'");
        }

        #endregion
    }

    [System.Serializable]
    public class AudioEvent {

        [Min(0)] public float timeBeforeEvent = 0;
        public AK.Wwise.Event audioEvent = null;
        public AK.Wwise.CallbackFlags callbackFlags = null;
        public GameObject optionalSource = null;

        public AudioEvent (float timeBeforeEvent, AK.Wwise.Event audioEvent, AK.Wwise.CallbackFlags callbackFlags = null, GameObject optionalSource = null) {
            this.timeBeforeEvent = timeBeforeEvent;
            this.audioEvent = audioEvent;
            this.callbackFlags = callbackFlags;
            this.optionalSource = optionalSource;
        }        
    }

    [System.Serializable]
    public class AudioSwitch {
        [Min(0)] public float timeBeforeSwitch = 0;
        public AK.Wwise.Switch audioSwitch = null;
        public GameObject optionalSource = null;

        public AudioSwitch(float timeBeforeSwitch, AK.Wwise.Switch audioSwitch, GameObject optionalSource = null) {
            this.timeBeforeSwitch = timeBeforeSwitch;
            this.audioSwitch = audioSwitch;
            this.optionalSource = optionalSource;
        }
    }
}