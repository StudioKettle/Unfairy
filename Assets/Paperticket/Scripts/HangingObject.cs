using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangingObject : MonoBehaviour {

    [SerializeField] List<Rigidbody> children;


    List<Vector3> childrenPos;
    List<Quaternion> childrenRot;

    void Awake() {

        children = new List<Rigidbody>(GetComponentsInChildren<Rigidbody>());
        childrenPos = new List<Vector3>();
        childrenRot = new List<Quaternion>();

        foreach (Rigidbody rb in children) {
            childrenPos.Add(rb.transform.localPosition);
            childrenRot.Add(rb.transform.localRotation);
        }

    }

    public void ResetObject() {

        for (int i = 0; i < children.Count; i++) {
            var rb = children[i];
            
            rb.isKinematic = true;

            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.transform.SetLocalPositionAndRotation(childrenPos[i], childrenRot[i]);

            rb.isKinematic = false;
        }

    }

}
