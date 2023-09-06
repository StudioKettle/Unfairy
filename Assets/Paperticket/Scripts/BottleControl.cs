using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using TMPro;
using Paperticket;

public class BottleControl : MonoBehaviour {

    [SerializeField] TextMeshPro tableText;
    [SerializeField] VideoController videoController;

    [SerializeField] VideoClip[] clips;
    int clipsInt = 0;

    [SerializeField] GameObject[] rooms;
    int roomsInt = 0;

    [Space(5)]
    [SerializeField] bool cycleVideoEvent = true;
    [SerializeField] UnityEvent2 onCycleVideo;

    void Start() {
        StartCoroutine(StartFirstVideo());
    }

    IEnumerator StartFirstVideo() {
        yield return new WaitForSeconds(0.5f);

        videoController.videoPlayer.clip = clips[0];
        videoController.videoPlayer.Play();

        //foreach(GameObject room in rooms) {
        //    room.SetActive(room == rooms[0]);
        //}

        UpdateText();

    }



    private void Update() {
        //if (Input.GetKeyDown(KeyCode.R)) CycleRoom();
        
        if (Input.GetKeyDown(KeyCode.V)) CycleVideo();
    }


    public void CycleVideo() {
        Debug.Log("[BottleControl] Cycling video...");

        clipsInt = (clipsInt + 1) % clips.Length;

        videoController.videoPlayer.clip = clips[clipsInt];
        videoController.videoPlayer.Play();

        UpdateText();

        if (cycleVideoEvent && onCycleVideo != null) onCycleVideo.Invoke();
    }

    //public void CycleRoom() {
    //    Debug.Log("[BottleControl] Cycling room...");

    //    rooms[roomsInt].SetActive(false);
    //    roomsInt = (roomsInt + 1) % rooms.Length;
    //    rooms[roomsInt].SetActive(true);

    //    UpdateText();

    //}



    void UpdateText() {
        tableText.text = "<b>Current Video</b>" + System.Environment.NewLine + clips[clipsInt].name + " (" + clipsInt + ")";// + System.Environment.NewLine + 
                         //System.Environment.NewLine +
                         //"<b>Room Size</b>" + System.Environment.NewLine + rooms[roomsInt].name + " (" + roomsInt + ")";
    }

}
