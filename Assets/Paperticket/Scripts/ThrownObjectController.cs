using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paperticket;

public class ThrownObjectController : MonoBehaviour {

    [SerializeField] Transform videoSphere;
    [SerializeField] DelayedEvent delayedEvent;
    [SerializeField] VideoEvent videoEvent;
    [SerializeField] GameObject thrownObject;

    public void EnableSphere() {
        if (isActiveAndEnabled) {
            delayedEvent.enabled = true;
        }
    }

    void OnDisable() {
        delayedEvent.enabled = false;
        videoEvent.enabled = false;
        thrownObject.SetActive(false);
    }


}
