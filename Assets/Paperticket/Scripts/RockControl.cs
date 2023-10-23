using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using TMPro;
using Paperticket;
using System.Linq;

public class RockControl : MonoBehaviour {

    public VideoController videoController;

    [SerializeField] VideoClip RockVideo = null;
    [SerializeField] float tempDelay = 0;

    [Space(10)]
    [SerializeField] bool debugging = false;

    bool videoReady = false;

    
    [SerializeField] GameObject sparks;


    void Start() {
        StartCoroutine(StartVideo());
    }

    IEnumerator StartVideo() {
        yield return new WaitForSeconds(0.5f);

        videoController.OnVideoStarted += VideoReady;
        videoController.SetNextVideo(RockVideo);

        yield return new WaitUntil(() => videoReady);
        videoController.OnVideoStarted -= VideoReady;
        yield return new WaitForSeconds(0.1f);
        videoController.PauseVideo();
        yield return new WaitForSeconds(tempDelay);
        videoController.PlayVideo();

    }
    void VideoReady() {
        videoReady = true;
    }



    void Update() {
        if (Input.GetKeyDown(KeyCode.S)) ToggleSparks();
    }   

    void ToggleSparks () {
        sparks.SetActive(!sparks.activeSelf);
    }
  

}
///OLD
///
//[SerializeField] GameObject audience;
//[SerializeField] GameObject lightbeams;
//[SerializeField] TextMeshPro ambientText;

//[SerializeField] GameObject importMarimba;
//[SerializeField] GameObject calculateMarimba;


//[SerializeField] Transform daeVideo;
//List<GameObject> daePlanes = new List<GameObject>();
//int planesIndex = 0;


//[SerializeField] GameObject atlasPlanes;
//if (Input.GetKeyDown(KeyCode.A)) ToggleAudience();
//else if (Input.GetKeyDown(KeyCode.L)) CycleAmbientLight();
//else if (Input.GetKeyDown(KeyCode.V)) CycleDaeVideo();
//else if (Input.GetKeyDown(KeyCode.M)) ToggleMarimbaNormals();
//else if (Input.GetKeyDown(KeyCode.P)) ToggleVideoPlanes();
//void ToggleAudience () {
//    if (!audience.activeSelf) {
//        audience.SetActive(true);
//        SceneCameraSettings.instance.ApplySettings("SetSkybox");
//    } else {
//        audience.SetActive(false);
//        SceneCameraSettings.instance.ApplySettings("SetBlack");
//    }
//}
//void ToggleLightBeams() {
//    lightbeams.SetActive(!lightbeams.activeSelf);
//}

//void ToggleMarimbaNormals() {
//    importMarimba.SetActive(!importMarimba.activeSelf);
//    calculateMarimba.SetActive(!calculateMarimba.activeSelf);
//}

//void CycleAmbientLight() {
//    SceneLightSettings.instance.ApplyNextSettings();
//    ambientText.text = SceneLightSettings.instance.currentName;
//}

//void ToggleVideoPlanes() {
//    atlasPlanes.SetActive(!atlasPlanes.activeSelf);
//}

//void CycleDaeVideo() {
//    daePlanes[planesIndex].gameObject.SetActive(false);

//    planesIndex = (planesIndex + 1) % daePlanes.Count;
//    daePlanes[planesIndex].gameObject.SetActive(true);
//}

//void CheckForActive() {
//    StartCoroutine(CheckingForActive());
//}
//IEnumerator CheckingForActive() {

//    if (SceneUtilities.instance.CheckSceneActive(gameObject.scene.name)) {
//        ambientText.text = SceneLightSettings.instance.currentName;
//    } else {
//        yield return new WaitForSeconds(0.1f);
//        SceneUtilities.OnSceneMadeActive += CheckForActive;
//    }
//}
//void OnDisable() {
//    SceneUtilities.OnSceneMadeActive -= CheckForActive;
//}