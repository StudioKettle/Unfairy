using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paperticket;

public class RageGhostPlane : MonoBehaviour {

    GameObject child;

    [SerializeField] float duration = 4f;

    //[Header("READ ONLY")]
    //[Space(10)]
    //[SerializeField] bool planeActive = false;


    void Start() {

        child = transform.GetChild(0).gameObject;
        
    }


    public void Play() {
        //if (planeActive) {
        //    Debug.LogError("[RageGhostPlane] ERROR -> Cannot play as plane already active, ignoring.");
        //    return;
        //}

        if (child.activeSelf) {
            child.gameObject.SetActive(false);
            StopAllCoroutines();
        }

        transform.position = PTUtilities.instance.HeadsetPosition();
        transform.rotation = PTUtilities.instance.HeadsetRotation();

        child.gameObject.SetActive(true);

        //planeActive = true;

        StartCoroutine(Playing());
    }

    IEnumerator Playing() {
        yield return new WaitForSeconds(duration);

        child.SetActive(false);
    }

}
