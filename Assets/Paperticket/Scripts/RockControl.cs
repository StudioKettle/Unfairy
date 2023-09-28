using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using TMPro;
using Paperticket;
using System.Linq;

public class RockControl : MonoBehaviour {

    
    [SerializeField] bool debugging = false;

    [SerializeField] GameObject audience;
    [SerializeField] GameObject sparks;
    [SerializeField] GameObject lightbeams;
    [SerializeField] TextMeshPro ambientText;

    [SerializeField] GameObject importMarimba;
    [SerializeField] GameObject calculateMarimba;


    //[SerializeField] Transform daeVideo;
    List<GameObject> daePlanes = new List<GameObject>();
    int planesIndex = 0;


    [SerializeField] GameObject atlasPlanes;

    private void Start() {

        //foreach(Transform child in daeVideo) {
        //    if (child != daeVideo) {
        //        daePlanes.Add(child.gameObject);
        //        child.gameObject.SetActive(child.gameObject == daePlanes[0]);
        //    }
        //}

        CheckForActive();
    }



    void Update() {
        if (Input.GetKeyDown(KeyCode.A)) ToggleAudience();
        else if (Input.GetKeyDown(KeyCode.S)) ToggleSparks();
        else if (Input.GetKeyDown(KeyCode.B)) ToggleLightBeams();
        else if (Input.GetKeyDown(KeyCode.L)) CycleAmbientLight();
        else if (Input.GetKeyDown(KeyCode.V)) CycleDaeVideo();
        else if (Input.GetKeyDown(KeyCode.M)) ToggleMarimbaNormals();
        else if (Input.GetKeyDown(KeyCode.P)) ToggleVideoPlanes();
    }

    void ToggleAudience () {
        if (!audience.activeSelf) {
            audience.SetActive(true);
            SceneCameraSettings.instance.ApplySettings("SetSkybox");
        } else {
            audience.SetActive(false);
            SceneCameraSettings.instance.ApplySettings("SetBlack");
        }
    }

    void ToggleSparks () {
        sparks.SetActive(!sparks.activeSelf);
    }

    void ToggleLightBeams() {
        lightbeams.SetActive(!lightbeams.activeSelf);
    }

    void ToggleMarimbaNormals() {
        importMarimba.SetActive(!importMarimba.activeSelf);
        calculateMarimba.SetActive(!calculateMarimba.activeSelf);
    }

    void CycleAmbientLight() {
        SceneLightSettings.instance.ApplyNextSettings();
        ambientText.text = SceneLightSettings.instance.currentName;
    }

    void ToggleVideoPlanes() {
        atlasPlanes.SetActive(!atlasPlanes.activeSelf);
    }

    void CycleDaeVideo() {
        daePlanes[planesIndex].gameObject.SetActive(false);

        planesIndex = (planesIndex + 1) % daePlanes.Count;
        daePlanes[planesIndex].gameObject.SetActive(true);
    }

    void CheckForActive() {
        StartCoroutine(CheckingForActive());
    }
    IEnumerator CheckingForActive() {

        if (SceneUtilities.instance.CheckSceneActive(gameObject.scene.name)) {
            ambientText.text = SceneLightSettings.instance.currentName;
        } else {
            yield return new WaitForSeconds(0.1f);
            SceneUtilities.OnSceneMadeActive += CheckForActive;
        }
    }
    void OnDisable() {
        SceneUtilities.OnSceneMadeActive -= CheckForActive;
    }

}
