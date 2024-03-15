using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using TMPro;
using Paperticket;

public class BottleControl : MonoBehaviour {

    public enum Panning { Center, Left, Right }
    [SerializeField] bool debugging = false;
    [Space(10)]
    [Header("REFERENCES")]
    [SerializeField] VideoController videoController = null;
    [SerializeField] VideoClip bottleVideo = null;
    [Space(5)]
    [SerializeField] AK.Wwise.RTPC rTPC_EarpiecePan = null;
    [Space(10)]
    [Header("CONTROLS")]
    [SerializeField] float bazAudio4Marker = 4;
    [SerializeField] float bazAudio4Delay = 3;
    [SerializeField] UnityEvent2 bazAudio4Event = null;
    

    //[SerializeField] AK.Wwise.Event bottleAudio = null;//
    //[SerializeField] AK.Wwise.CallbackFlags callbackFlags = null;


    //uint audioPlayingId = 0;
    [Space(10)]
    [Header("READ ONLY")]
    [SerializeField] bool videoReady = false;

    void Start() {
        StartCoroutine(StartVideo());
    }

    IEnumerator StartVideo() {
        yield return new WaitForSeconds(0.5f);

        videoController.OnVideoStarted += VideoReady;
        videoController.SetNextVideo(bottleVideo);

        yield return new WaitUntil(()=> videoReady);
        videoController.OnVideoStarted -= VideoReady;

        // Create a callback flag to enable retrieving current time of audio
        //AkCallbackType enablePlayPosition = AkCallbackType.AK_EnableGetSourcePlayPosition;
        // NOTE -> Doing this in inspector, shouldn't be, but we are

        // Start the audio 
        //audioPlayingId = bottleAudio.Post(gameObject, callbackFlags, null);
                
        //yield return new WaitForSeconds(0.25f);

        //int uPosition = 0;
        //AkSoundEngine.GetSourcePlayPosition(audioPlayingId, out uPosition);
        //float time = uPosition / 1000;
        //Debug.Log("Time = " + time);
        //videoController.SetTime(time);
    }

    void VideoReady() {
        videoReady = true;
    }


    public void StartBazAudio4(Transform earpiece) {
        StartCoroutine(WaitingForBazAudio4(earpiece));
    }

    IEnumerator WaitingForBazAudio4(Transform earpiece) {

        // Offset based on where in the video loop we are
        if (videoController.currentVideoTime < bazAudio4Marker) {
            yield return new WaitForSeconds(bazAudio4Delay);
        }

        // Work out which side to pan earpiece audio
        if (PTUtilities.instance.headProxy.InverseTransformPoint(earpiece.position).x < 0) {
            SetEarpiecePan(Panning.Left);
        } else {
            SetEarpiecePan(Panning.Right);
        }

        if (bazAudio4Event != null) bazAudio4Event.Invoke();
    }


    public void SetEarpiecePan(Panning panning) {

        switch (panning) {
            case Panning.Center:
                rTPC_EarpiecePan.SetGlobalValue(0);
                break;
            case Panning.Left:
                rTPC_EarpiecePan.SetGlobalValue(-1);
                break;
            case Panning.Right:
                rTPC_EarpiecePan.SetGlobalValue(1);
                break;
            default:
                Debug.LogError("[BottleControl] Could not SetEapiecePan! Bad Panning value set, ignoring.");
                return;
        }
        
        if (debugging) Debug.Log("[BottleControl] SetEapiecePan to " + panning.ToString());
    }

}
