using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrownObject : MonoBehaviour {

    [SerializeField] Vector3 forceToAdd;
    [SerializeField] ForceMode mode;

    Rigidbody rb;
    Vector3 startPos;
    Quaternion startRot;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        startRot = transform.rotation;
    }

    private void OnEnable() {

        // Reset position, rotation, velocities
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.position = startPos;
        rb.rotation = startRot;

        // Add relative force
        rb.AddRelativeForce(forceToAdd, mode);
    
    }
}
