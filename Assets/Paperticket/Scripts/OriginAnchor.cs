using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OriginAnchor : MonoBehaviour {

    [SerializeField] bool debug = false;   

    //bool changed = false;   


    void FixedUpdate() {

        //if (transform.rotation.eulerAngles != Vector3.zero) {
        //    changed = true;

        //    Physics.gravity = transform.up * -9.81f;

        //    if (debug) Debug.Log("[OriginAnchor] Gravity changed to = " + Physics.gravity);

        //} else if (changed) {
        //    changed = false;

        //    Physics.gravity = Vector3.up * -9.81f;

        //    if (debug) Debug.Log("[OriginAnchor] Gravity returned to = " + Physics.gravity);
        //}

        var strength = transform.localPosition.y + -9.81f;

        Physics.gravity = transform.up * strength;


    }

    public void Reset() {
        transform.localPosition = Vector3.zero;

    }

}
