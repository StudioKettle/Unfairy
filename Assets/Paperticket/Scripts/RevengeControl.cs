using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using TMPro;
using Paperticket;

public class RevengeControl : MonoBehaviour {

    [SerializeField] TextMeshPro tableText = null;
    [SerializeField] VideoController videoController = null;

    [SerializeField] VideoClip revengeVideo = null;
    //[SerializeField] AK.Wwise.Event revengeAudio = null;
    //[SerializeField] AK.Wwise.CallbackFlags callbackFlags = null;

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
        videoController.SetNextVideo(revengeVideo);

        yield return new WaitUntil(()=> videoReady);
        videoController.OnVideoStarted -= VideoReady;
    }

    void VideoReady() {
        videoReady = true;
    }

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