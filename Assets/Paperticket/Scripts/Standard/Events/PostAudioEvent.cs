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

            audioEvent.audioEvent.Post(gameObject, audioEvent.callbackFlags, null);

            if (debugging) Debug.Log("[PostAudioEvent] Sending audio event '"+audioEvent.audioEvent.Name+"'");
        }
    }

    [System.Serializable]
    public class AudioEvent {

        [Min(0)] public float timeBeforeEvent = 0;
        public AK.Wwise.Event audioEvent = null;
        public AK.Wwise.CallbackFlags callbackFlags = null;

        public AudioEvent (float timeBeforeEvent, AK.Wwise.Event audioEvent, AK.Wwise.CallbackFlags callbackFlags = null) {
            this.timeBeforeEvent = timeBeforeEvent;
            this.audioEvent = audioEvent;
            this.callbackFlags = callbackFlags;
        }

    }

}