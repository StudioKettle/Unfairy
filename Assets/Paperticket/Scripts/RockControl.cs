using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using TMPro;
using Paperticket;
using System.Linq;

public class RockControl : MonoBehaviour {

    //public enum KeyboardBank { Bank1, Bank2, Bank3 }

    public VideoController videoController;

    [SerializeField] VideoClip RockVideo = null;

    //[Space(10)]
    //[SerializeField] KeyboardBank keyboardBank = KeyboardBank.Bank1;

    //[SerializeField] List<AK.Wwise.Event> bank1;
    //[SerializeField] List<AK.Wwise.Event> bank2;
    //[SerializeField] List<AK.Wwise.Event> bank3;

    [Space(10)]
    [SerializeField] float boredomSpeed = 0.1f;
    [SerializeField] float hitAddition = 0.05f;
    [SerializeField] float hitMinThreshold = 0.5f;
    [Space(5)]
    [SerializeField] AK.Wwise.Event winAKEvent;
    [SerializeField] float winMarker = 0.75f;
    [Space(5)]
    [SerializeField] AK.Wwise.Event fail1AKEvent;
    [SerializeField] float fail1Marker = 0.25f;
    [Space(5)]
    [SerializeField] AK.Wwise.Event fail2AKEvent;
    [SerializeField] float fail2Marker = 0f;

    [Space(10)]
    [SerializeField] TextMeshPro debugText;
    [SerializeField] bool debugging = false;


    [Header("READ ONLY")]
    [SerializeField] [Range(0, 1)] float performance = 0.5f;

    bool videoReady = false;
    bool performing = false;
    Coroutine calculatingPerformanceCo = null;
    
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
        //yield return new WaitForSeconds(0.1f);
        //videoController.PauseVideo();
        //yield return null;
        //videoController.PlayVideo();

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


    public void StartPerformance() {
        if (calculatingPerformanceCo != null) StopCoroutine(calculatingPerformanceCo);
        calculatingPerformanceCo = StartCoroutine(CalculatingPerformance());
        if (debugging) Debug.Log("[RockControl] Starting Performance, calculating crowd boredom vs keyboard.");
    }

    public void StopPerformance() {
        if (calculatingPerformanceCo != null) StopCoroutine(calculatingPerformanceCo);
        if (debugging) Debug.Log("[RockControl] Stopping Performance, no longer calculating crowd boredom vs keyboard.");

        performing = false;

    }

    public void AddToPerformance() {
        if (!performing) return;

        if (performance < hitMinThreshold) {
            performance = hitMinThreshold;
            if (debugging) Debug.Log("[RockControl] Adding to Performance from less than "+ hitMinThreshold + ", resetting to "+ hitMinThreshold);
        }

        performance = Mathf.Clamp01(performance + hitAddition);
        if (debugging) Debug.Log("[RockControl] Adding " + hitAddition + " to Performance, now at " + performance);
    }

    IEnumerator CalculatingPerformance() {

        bool winActivated = false;
        bool fail1Activated = false;
        bool fail2Activated = false;

        performing = true;

        while (performing) {

            performance = Mathf.Clamp01(performance - (boredomSpeed * Time.deltaTime));

            if (!winActivated && performance >= winMarker) {
                winAKEvent.Post(gameObject);
                winActivated = true;
                if (debugging) Debug.Log("[RockControl] Hit win marker");
            }
            else if (winActivated && performance < winMarker) {
                winActivated = false;
            } 
            
            if (!fail1Activated && performance <= fail1Marker) {
                fail1AKEvent.Post(gameObject);
                fail1Activated = true;
                if (debugging) Debug.Log("[RockControl] Hit fail1 marker");
            }
            else if (fail1Activated && performance > fail1Marker) {
                fail1Activated = false;
            } 

            if (!fail2Activated && performance <= fail2Marker) {
                fail2AKEvent.Post(gameObject);
                fail2Activated = true;
                if (debugging) Debug.Log("[RockControl] Hit fail2 marker");
            }
            else if (fail2Activated && performance > fail2Marker) {
                fail2Activated = false;
            }

            debugText.text =    "Performance = " + performance + System.Environment.NewLine +
                                System.Environment.NewLine +
                                "Cheer No.		> 0.75" + System.Environment.NewLine +
                                "SmallBoo No. 	< 0.25" + System.Environment.NewLine +
                                "LargeBoo No. 	= 0";

            yield return null;

        }

    }


    //public void SwitchBank(KeyboardBank nextBank) {
    //    keyboardBank = nextBank;
    //}

    //public void PlayNote(int noteID, GameObject source = null) {

    //    switch (keyboardBank) {
    //        case KeyboardBank.Bank1:
    //            PTUtilities.instance.PostAudioEvent(bank1[noteID], source);
    //            break;
    //        case KeyboardBank.Bank2:
    //            break;
    //        case KeyboardBank.Bank3:
    //            break;
    //        default:
    //            break;
    //    }

    //}


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