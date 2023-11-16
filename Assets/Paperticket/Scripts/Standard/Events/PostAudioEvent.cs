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
}