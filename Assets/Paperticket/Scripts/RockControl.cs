using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using TMPro;
using System.Linq;
using Paperticket;


namespace Paperticket {
    [System.Serializable]
    public enum BarLength { Double, Whole, Half, Quarter }

}

public class RockControl : MonoBehaviour {

    //public enum KeyboardBank { Bank1, Bank2, Bank3 }

    public VideoController videoController;

    [SerializeField] VideoClip RockVideo = null;
    [SerializeField] GameObject[] sparks;
    [SerializeField] MeshRenderer[] keyIndicators;


    [Space(10)]
    [SerializeField] TextMeshPro debugText;
    [SerializeField] bool debugging = false;

    [Header("Performance Controls")]
    [Space(10)]
    [SerializeField] float boredomSpeed = 0.1f;
    [SerializeField] float hitAddition = 0.05f;
    [SerializeField] float hitMinThreshold = 0.5f;
    [Space(5)]
    [SerializeField] float winMarker = 0.75f;
    [SerializeField] AK.Wwise.Event winAKEvent;
    [Space(5)]
    [SerializeField] AK.Wwise.Event fail1AKEvent;
    [SerializeField] float fail1Marker = 0.25f;
    [Space(5)]
    [SerializeField] AK.Wwise.Event fail2AKEvent;
    [SerializeField] float fail2Marker = 0f;
    
    

    [Header("Winning Notes Controls")]
    [Space(10)]
    [SerializeField] Color selectedColor = Color.green * 1.1f;
    [SerializeField] Color upcomingColor = Color.yellow * 1.1f;
    [SerializeField] Color unselectedColor = Color.red * 1.1f;
    [Space(5)]
    [SerializeField] float quarterDuration = 0.385f;
    [SerializeField] float halfDuration = 0.785f;
    [SerializeField] float wholeDuration = 1.585f;
    [SerializeField] float doubleDuration = 3.185f;       
    [Space(5)]
    [SerializeField] UpcomingNote[] score = null;
    [Space(5)]
    [SerializeField] float sparksDuration = 1.25f;


    //[Space(10)]
    //[SerializeField] KeyboardBank keyboardBank = KeyboardBank.Bank1;

    //[SerializeField] List<AK.Wwise.Event> bank1;
    //[SerializeField] List<AK.Wwise.Event> bank2;
    //[SerializeField] List<AK.Wwise.Event> bank3;


    [Header("READ ONLY")]
    [SerializeField] [Range(0, 1)] float performance = 0.5f;
    [SerializeField] bool[] winningNotes;
    [SerializeField] UpcomingNote currentUpcoming = null;
   //[SerializeField] bool[] upcomingNotes;

    bool videoReady = false;
    bool performing = false;
    Coroutine calculatingPerformanceCo = null;

    Coroutine progressingScoreCo = null;


    void Start() {
        StartCoroutine(StartVideo());


        winningNotes = new bool[12];
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






    #region Calculating Performance


    public void StartPerformance() {

        // Start calculating performance 
        if (calculatingPerformanceCo != null) StopCoroutine(calculatingPerformanceCo);
        calculatingPerformanceCo = StartCoroutine(CalculatingPerformance());
        if (debugging) Debug.Log("[RockControl] Starting Performance, calculating crowd boredom vs keyboard.");
    }

    public void StopPerformance() {
        if (calculatingPerformanceCo != null) StopCoroutine(calculatingPerformanceCo);
        if (debugging) Debug.Log("[RockControl] Stopping Performance, no longer calculating crowd boredom vs keyboard.");

        performing = false;

    }

    public void AddToPerformance(int noteIndex) {
        if (!performing) return;

        //if (winningNotes[noteIndex]) {
        //    ActivateSparks();
        //}

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

            if (debugText != null) {
                debugText.text = "Performance = " + performance + System.Environment.NewLine +
                                    System.Environment.NewLine +
                                    "Cheer No.		> 0.75" + System.Environment.NewLine +
                                    "SmallBoo No. 	< 0.25" + System.Environment.NewLine +
                                    "LargeBoo No. 	= 0";
            }

            yield return null;

        }

    }

    #endregion



    #region Set Winning Notes

    public void ProgressMusicScore() {

        Debug.LogWarning("[RockControl] We are not progressing score at the moment! Ignoring.");
        return;

        // Start progressing the score
        if (progressingScoreCo != null) StopCoroutine(progressingScoreCo);
        progressingScoreCo = StartCoroutine(ProgressingMusicScore());
        if (debugging) Debug.Log("[RockControl] Progressing score, changing gfx based on winning / upcoming notes");

    }

    IEnumerator ProgressingMusicScore() {

        float fadeTime = 0.8f;
        float delay = 0;

        // Set all key indicators to red
        foreach (MeshRenderer rend in keyIndicators) {
            rend.material.SetColor("_EmissionColor", unselectedColor);
        }


        // Go through the entire score in order
        foreach (UpcomingNote upcomingNote in score) {


            currentUpcoming = upcomingNote;

            // Start fading the upcoming notes to yellow, if they are not already green
            for (int i = 0; i < upcomingNote.winningNotes.Length; i++) {
                if (upcomingNote.winningNotes[i] && !winningNotes[i]) {
                    StartCoroutine(PTUtilities.instance.FadeColorTo(keyIndicators[i], "_EmissionColor", upcomingColor, fadeTime, AnimationCurve.EaseInOut(0, 0, 1, 1), TimeScale.Scaled));

                    if (debugging) Debug.Log("[RockControl] Fading upcoming note index " + i + " to yellow.");
                }
            }

            // Wait until entirely green
            yield return new WaitForSeconds(fadeTime);

            
            for (int i = 0; i < upcomingNote.winningNotes.Length; i++) {

                // Set upcoming notes to green, as long as they are not already green
                if (upcomingNote.winningNotes[i] && !winningNotes[i]) {
                    keyIndicators[i].material.SetColor("_EmissionColor", selectedColor);

                    if (debugging) Debug.Log("[RockControl] Setting new note index " + i + " to green.");
                }

                // Set old notes back to red, as long as they are not still green
                else if (winningNotes[i] && !upcomingNote.winningNotes[i]) {
                    keyIndicators[i].material.SetColor("_EmissionColor", unselectedColor);

                    if (debugging) Debug.Log("[RockControl] Setting old note index " + i + " back to red.");
                }
            }

            // Set the upcoming notes as the new winning notes
            winningNotes = upcomingNote.winningNotes;

            // Convert the next bar length to seconds
            switch (upcomingNote.barLength) {
                case BarLength.Quarter:
                    delay = quarterDuration;
                    break;
                case BarLength.Half:
                    delay = halfDuration;
                    break;
                case BarLength.Whole:
                    delay = wholeDuration;
                    break;
                case BarLength.Double:
                    delay = doubleDuration;
                    break;
                default:
                    break;
            }

            if (debugging) Debug.Log("[RockControl] UpcomingNote length = " + delay);

            // Wait until next notes need to fade in
            yield return new WaitForSeconds(delay - fadeTime);

        }


        if (debugging) Debug.Log("[RockControl] Finished score.");

    }


    public void SetWinningNotes(int noteIndex1, int noteIndex2) {

        //// Reset the material colors
        //for (int i = 0; i < keyIndicators.Length; i++) {
        //    if (upcomingNotes[i]) {
        //        keyIndicators[i].material.SetColor("_EmissionColor", unselectedColor);
        //    }
        //}

        //// Set the winning notes
        //upcomingNotes = new bool[12];
        //upcomingNotes[noteIndex1] = true;
        //upcomingNotes[noteIndex2] = true;

        //// Set the new material colors
        //keyIndicators[noteIndex1].material.SetColor("_EmissionColor", selectedColor);
        //keyIndicators[noteIndex2].material.SetColor("_EmissionColor", selectedColor);

    }

    //public void SetWinningNotes(int noteIndex1, int noteIndex2, BarLength delay) {
    //    float _delay = 0;
    //    switch (delay) {
    //        case BarLength.Quarter:
    //            _delay = 0.4f;
    //            break;
    //        case BarLength.Half:
    //            _delay = 0.8f;
    //            break;
    //        case BarLength.Whole:
    //            _delay = 1.6f;
    //            break;
    //        case BarLength.Double:
    //            _delay = 3.2f;
    //            break;
    //        default:
    //            break;
    //    }
    //    StartCoroutine(SettingWinningNotes(noteIndex1, noteIndex2, _delay));
    //}


    //IEnumerator SettingWinningNotes(int noteIndex1, int noteIndex2, float delay) {

    //    StartCoroutine(PTUtilities.instance.FadeColorTo(keyIndicators[noteIndex1], selectedColor, delay, AnimationCurve.EaseInOut(0, 0, 1, 1), TimeScale.Scaled));
    //    StartCoroutine(PTUtilities.instance.FadeColorTo(keyIndicators[noteIndex2], selectedColor, delay, AnimationCurve.EaseInOut(0, 0, 1, 1), TimeScale.Scaled));

    //    yield return new WaitForSeconds(delay);

    //    // Reset previous key indicators
    //    for (int i = 0; i < keyIndicators.Length; i++) {
    //        if (winningNotes[i]) {
    //            keyIndicators[i].material.SetColor("_EmissionColor", unselectedColor);
    //        }
    //    }

    //    winningNotes = new bool[12];
    //    winningNotes[noteIndex1] = true;
    //    winningNotes[noteIndex2] = true;
    //}


    #endregion


    #region Activating Sparks

    public void ActivateSparks() {
        if (activatingSparksCo != null) {
            if (debugging) Debug.Log("[RockControl] Tried to activate sparks but they're already activating, ignoring.");
            return;
        }
        activatingSparksCo = StartCoroutine(ActivatingSparks());
    }


    Coroutine activatingSparksCo;
    IEnumerator ActivatingSparks() {

        int firstIndex = Random.Range(0, sparks.Length);
        int secondIndex = 0;
        while (secondIndex == firstIndex) {
            secondIndex = Random.Range(0, sparks.Length);
        }

        sparks[firstIndex].SetActive(true);
        sparks[secondIndex].SetActive(true);

        yield return new WaitForSeconds(sparksDuration);

        sparks[firstIndex].SetActive(false);
        sparks[secondIndex].SetActive(false);

        activatingSparksCo = null;
    }

    #endregion








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


[System.Serializable]
public class UpcomingNote {

    public string note = "";
    public bool[] winningNotes = null;
    public BarLength barLength = 0;

    public UpcomingNote(string note = "New UpcomingNote", bool[] winningNotes = null, BarLength barLength = 0) {
        this.note = note;
        this.winningNotes = winningNotes;
        this.barLength = barLength;
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