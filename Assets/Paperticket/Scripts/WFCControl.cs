using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using TMPro;
using Paperticket;

public class WFCControl : MonoBehaviour {


    public VideoController videoController;

    [SerializeField] VideoClip WFCVideo = null;

    //[Space(10)]
    //[SerializeField] UnityEvent2 cycleSphereEvent = null;
    [Space(10)]
    [SerializeField] bool debugging = false;

    bool videoReady = false;

    [Space(10)]
    public GameObject particles;
    public MeshRenderer floorRend;
    Material floorMat;
    public Color floorCol;
    float floorInt;


    void Start() {      
        StartCoroutine(StartVideo());
    }

    IEnumerator StartVideo() {
        yield return new WaitForSeconds(0.5f);

        videoController.OnVideoStarted += VideoReady;
        videoController.SetNextVideo(WFCVideo);

        yield return new WaitUntil(() => videoReady);
        videoController.OnVideoStarted -= VideoReady;

    }
    void VideoReady() {
        videoReady = true;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) ToggleGrid();
        else if (Input.GetKeyDown(KeyCode.F)) ToggleParticles();
    }

   



    void ToggleGrid() {
        floorInt = Mathf.Abs(floorInt - 0.5f);
        floorMat.color = new Color(floorInt, floorInt, floorInt, 0.5f);
    }


    void ToggleParticles() {
        particles.SetActive(!particles.activeSelf);
    }

}

/// OLD
/// 
//Debug.Log("[WFC Control] Hallo! This is a test message to make sure Development Mode is working :3 ");

//floorMat = floorRend.material;

//sphereText.text = "<b>Current Sphere</b> = '" + videoSpheres[0].name + "' (0)";

//for (int i = 0; i < videoSphereParent.childCount; i++) {
//    videoSphereParent.GetChild(i).gameObject.SetActive(false);
//}

//videoSpheres[0].gameObject.SetActive(true);
//videoSpheres[0].GetComponent<ThrownObjectController>().EnableSphere();

//void ToggleVideoPlaying() {

//    var vidPlayer = videoController.videoPlayer;

//    if (vidPlayer.gameObject.activeInHierarchy) {

//        if (vidPlayer.isPlaying) vidPlayer.Pause();
//        else vidPlayer.Play();

//    }

//    if (debugging) Debug.Log("[WFC Control] Toggled play/pause on " + vidPlayer.gameObject.name);

//}



//public void CycleVideo() {
//    Debug.Log("[BottleControl] Cycling video...");

//    videoClipsInt = (videoClipsInt + 1) % videoClips.Length;

//    videoController.videoPlayer.clip = videoClips[videoClipsInt];
//    videoController.videoPlayer.Play();

//    UpdateText();

//    //if (cycleVideoEvent && onCycleVideo != null) onCycleVideo.Invoke();
//}

//void UpdateText() {
//    sphereText.text = "<b>Current Sphere</b> = '" + videoSpheres[videoSphereInt].name + "' (" + videoSphereInt + ")" + System.Environment.NewLine
//                       + "<b>Current Video</b> = '" + videoClips[videoClipsInt].name + "' (" + videoClipsInt + ")";
//}

////Coroutine cyclingSpheresCo;
//void CycleSpheres() {
//    //if (cyclingSpheresCo != null) return;
//    if (debugging) Debug.Log("[WFC Control] Cycling Spheres");
//    //cyclingSpheresCo = StartCoroutine(CyclingSpheres());

//    // Turn off current sphere
//    videoSpheres[videoSphereInt].SetActive(false);

//    // Pick the next sphere in the list
//    videoSphereInt = (videoSphereInt + 1) % videoSpheres.Count;

//    // Enable the new sphere
//    videoSpheres[videoSphereInt].SetActive(true);

//    // Send an event
//    if (cycleSphereEvent != null) cycleSphereEvent.Invoke();

//    // Change the text on the table
//    sphereText.text = "<b>Current Sphere</b> = '" + videoSpheres[videoSphereInt].name + "' (" + videoSphereInt + ")";

//}

//IEnumerator CyclingSpheres() {

//    // Turn off current sphere
//    videoSpheres[videoSphereInt].SetActive(false);

//    // Pick the next sphere in the list
//    videoSphereInt = (videoSphereInt + 1) % videoSpheres.Count;

//    //// Check whether the right video player is enabled
//    //if ((!videoPlayer2D.gameObject.activeSelf && videoSpheres[videoSphereInt].GetComponent<Video3DHeadAdjust>() == null) ||
//    //    (!videoPlayer3D.gameObject.activeSelf && videoSpheres[videoSphereInt].GetComponent<Video3DHeadAdjust>() != null)) {

//    //    if (debugging) Debug.Log("[WFC Control] Wrong video player active, attempting to switch video players...");
//    //    yield return SwitchingVideoPlayers();
//    //}
//    // if (debugging) Debug.Log("Switch check passed");

//    // Enable the new sphere
//    videoSpheres[videoSphereInt].SetActive(true);

//    // Send an event
//    if (cycleSphereEvent != null) cycleSphereEvent.Invoke();

//    // Change the text on the table
//    sphereText.text = "<b>Current Sphere</b> = '" + videoSpheres[videoSphereInt].name + "' (" + videoSphereInt + ")";

//    cyclingSpheresCo = null;
//}


////public void SwitchVideoPlayers() {
////    Debug.Log("[WFC Control] Switching video players...");
////    StartCoroutine(SwitchingVideoPlayers());
////}

//IEnumerator SwitchingVideoPlayers() {

//    var was2D = videoPlayer2D.gameObject.activeSelf;
//    var previousTime = was2D ? videoPlayer2D.time : videoPlayer3D.time;

//    wasPlaying = was2D ? videoPlayer2D.isPlaying : videoPlayer3D.isPlaying;


//    //var player2Dtime = videoPlayer2D.time;
//    //var player3Dtime = videoPlayer3D.time;

//    if (debugging) Debug.Log("[WFC Control] VideoPlayer2D time 1 = " + videoPlayer2D.time);
//    if (debugging) Debug.Log("[WFC Control] VideoPlayer3D time 1 = " + videoPlayer3D.time);


//    videoPlayer2D.gameObject.SetActive(!videoPlayer2D.gameObject.activeSelf);
//    videoPlayer3D.gameObject.SetActive(!videoPlayer3D.gameObject.activeSelf);

//    //var is2D = videoPlayer2D.gameObject.activeSelf;
//    var videoPlayer = was2D ? videoPlayer3D : videoPlayer2D;

//    yield return new WaitUntil(() => videoPlayer.canSetTime);

//    videoPlayer.time = previousTime;

//    //if (was2D) {
//    //    videoPlayer.time = player2Dtime % videoPlayer.clip.length;
//    //} else {
//    //    videoPlayer.time = (player3Dtime + 1) % videoPlayer.clip.length;
//    //}

//    yield return null;

//    if (debugging) Debug.Log("[WFC Control] VideoPlayer isPrepared 1 = " + videoPlayer.isPrepared);
//    yield return new WaitUntil(() => videoPlayer.isPrepared);
//    if (debugging) Debug.Log("[WFC Control] VideoPlayer isPrepared 2 = " + videoPlayer.isPrepared);

//    if (debugging) Debug.Log("[WFC Control] VideoPlayer2D time 2 = " + videoPlayer2D.time);
//    if (debugging) Debug.Log("[WFC Control] VideoPlayer3D time 2 = " + videoPlayer3D.time);

//    if (!wasPlaying) videoPlayer.Pause();

//}

