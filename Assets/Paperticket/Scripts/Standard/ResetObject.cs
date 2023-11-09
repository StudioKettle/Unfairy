using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetObject : MonoBehaviour {

    [SerializeField] bool useDistance = false;
    [SerializeField] float maxDistance = 10f;
    [Space(5)]
    [SerializeField] bool usePosition = false;
    [SerializeField] Vector2 minMaxWorldX = Vector2.zero;
    [SerializeField] Vector2 minMaxWorldY = Vector2.zero;
    [SerializeField] Vector2 minMaxWorldZ = Vector2.zero;

    Vector3 initialPos = Vector3.zero;
    Quaternion initialRot = Quaternion.identity;

    // Start is called before the first frame update
    void Start() {
        initialPos = transform.position;
        initialRot = transform.rotation;
    }

    // Update is called once per frame
    void Update() {
        
        if (useDistance && (transform.position - initialPos).magnitude > maxDistance) {
            ResetPosAndRot();
        }
        else if (usePosition) {

            if (transform.position.x < minMaxWorldX.x || transform.position.x > minMaxWorldX.y) {
                ResetPosAndRot();
            }
            if (transform.position.y < minMaxWorldY.x || transform.position.y > minMaxWorldY.y) {
                ResetPosAndRot();
            }
            if (transform.position.z < minMaxWorldZ.x || transform.position.z > minMaxWorldZ.y) {
                ResetPosAndRot();
            }

        }

    }

    void ResetPosAndRot() {
        transform.position = initialPos;
        transform.rotation = initialRot;
    }
}
