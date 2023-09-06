using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsObject : MonoBehaviour {

    Vector3 localPos = Vector3.zero;
    Quaternion localRot = Quaternion.identity;

    Rigidbody rb = null;

    void Awake() {

        localPos = transform.localPosition;
        localRot = transform.localRotation;

        rb = GetComponent<Rigidbody>();

    }

    public void ResetObject() {

        rb.isKinematic = true;

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.transform.SetLocalPositionAndRotation(localPos, localRot);

        rb.isKinematic = false;

    }
}
