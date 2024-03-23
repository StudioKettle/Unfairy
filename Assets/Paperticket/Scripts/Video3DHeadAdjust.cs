using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paperticket;

public class Video3DHeadAdjust : MonoBehaviour {

    [SerializeField] Transform videoSphere;
    [SerializeField] Vector3 offset;
    [Space(10)]
    [SerializeField] float xMultiplier = 1;
    [SerializeField] float yMultiplier = 1;
    [SerializeField] float zMultiplier = 1;

    

    // Update is called once per frame
    void Update() {
        if (!PTUtilities.instance.SetupComplete) return;

        var headPos = PTUtilities.instance.HeadsetPosition();

        // Gotta use the height of the IRL camera
        // var heightFix = yMultiplier * cameraIRLHeight;

        videoSphere.localPosition = offset + (Vector3.right * (headPos.x * -xMultiplier)) 
                                         + Vector3.up * (headPos.y * -yMultiplier)
                                         + Vector3.forward * (headPos.z * -zMultiplier);

    }
}
