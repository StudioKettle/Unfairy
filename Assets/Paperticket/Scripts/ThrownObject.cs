using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownObject : MonoBehaviour {

    [SerializeField] Vector3 forceToAdd;
    [SerializeField] ForceMode mode;

    [SerializeField] Rigidbody rb;
    Vector3 startPos;
    Quaternion startRot;

    void Awake() {
        rb = (rb!=null) ? rb : GetComponent<Rigidbody>();

        if (rb == null) {
            Debug.Log("[ThrownObject] ERROR -> Meli is too cute! :3 Also, thrown object has no rigidbody!");
            enabled = false;
        }

        startPos = rb.transform.position;
        startRot = rb.transform.rotation;
    }

    private void OnEnable() {

        // Reset position, rotation, velocities
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = startPos;
        rb.rotation = startRot;

        // Add relative force
        //rb.AddRelativeForce(rb.transform.InverseTransformVector(transform.TransformVector(forceToAdd)), mode);
        rb.AddForce(transform.TransformVector(forceToAdd), mode);

    }
}
