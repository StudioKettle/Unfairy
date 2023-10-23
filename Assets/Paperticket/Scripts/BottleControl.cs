using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using TMPro;
using Paperticket;

public class BottleControl : MonoBehaviour {

    [SerializeField] TextMeshPro tableText = null;
    [SerializeField] VideoController videoController = null;

    [SerializeField] VideoClip bottleVideo = null;
    [SerializeField] AK.Wwise.Event bottleAudio = null;
    [SerializeField] AK.Wwise.CallbackFlags callbackFlags = null;

    //[Space(5)]
    //[SerializeField] bool cycleVideoEvent = true;
    //[SerializeField] UnityEvent2 onCycleVideo = null;

    uint audioPlayingId = 0;

    bool videoReady = false;

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


    //void UpdateText() {
    //    //tableText.text = "<b>Current Video</b>" + System.Environment.NewLine + clips[clipsInt].name + " (" + clipsInt + ")";
    //}

}

/// OLD
/// 
//public void CycleVideo() {
//    Debug.Log("[BottleControl] Cycling video...");

//    clipsInt = (clipsInt + 1) % clips.Length;

//    videoController.videoPlayer.clip = clips[clipsInt];
//    videoController.videoPlayer.Play();

//    UpdateText();

//    if (cycleVideoEvent && onCycleVideo != null) onCycleVideo.Invoke();
//}

//public void CycleRoom() {
//    Debug.Log("[BottleControl] Cycling room...");

//    rooms[roomsInt].SetActive(false);
//    roomsInt = (roomsInt + 1) % rooms.Length;
//    rooms[roomsInt].SetActive(true);

//    UpdateText();

//}