using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paperticket;

public class ResetToOrigin : MonoBehaviour {

    [SerializeField] bool debugging = false;

    [Header("REFERENCES")]
    [Space(5)]
    [SerializeField] Transform optionalOrigin = null;
    [SerializeField] Vector3 positionOffset = Vector3.zero;
    [SerializeField] Vector3 rotationOffset = Vector3.zero;
    [SerializeField] Space space = Space.Self;
    [Space(10)]
    [Header("CONTROLS")]
    [Space(5)]
    public float resetDelay = 1;
    public float resetDuration = 1;
    public AnimationCurve resetCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Space(5)]
    public bool keepKinematic = false;
    public bool keepGravity = false;

    [Header("READ ONLY")]
    [Space(10)]
    public bool resetting = false;


    public delegate void StartedReset();
    public event StartedReset OnStartedReset;

    public delegate void EndedReset();
    public event EndedReset OnEndedReset;


    Coroutine resettingCo = null;

    Rigidbody rb = null;
    Vector3 defaultPosition = Vector3.zero;
    Quaternion defaultRotation = Quaternion.identity;



    private void Awake() {
        rb = gameObject.GetComponent<Rigidbody>();
        defaultPosition = transform.position;
        defaultRotation = transform.rotation;
    }


    public void Activate() {
        if (resettingCo != null) {
            Debug.LogWarning("[ResetToOrigin] Could not reset to origin because we are already resetting! Ignoring...");
            return;
        }

        resettingCo = StartCoroutine(Resetting());
    }




    IEnumerator Resetting() {

        // wait and then move to origin
        yield return new WaitForSeconds(resetDelay);


        // Send starting event out for listeners to pick up
        if (OnStartedReset != null) {
            if (debugging) Debug.Log("[ResetToOrigin] OnStartedReset called.");
            OnStartedReset();
        }


        // Turn off the colliders and rigidbody
        var cols = new List<Collider>();
        var isKinematic = false;
        var useGravity = false;
        if (rb != null) { 
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            if (keepKinematic) isKinematic = rb.isKinematic;
            if (keepGravity) useGravity = rb.useGravity;
            rb.isKinematic = true;
            foreach (Collider col in GetComponents<Collider>()) {
                if (col.enabled) {
                    col.enabled = false;
                    cols.Add(col);
                }            
            }
        }


        // Calculate target position + rotation
        Vector3 targetPosition = (optionalOrigin != null) ? optionalOrigin.position : defaultPosition;
        Quaternion targetRotation = (optionalOrigin != null) ? optionalOrigin.rotation : defaultRotation;

        //targetPosition += (space == Space.Self) ? transform.TransformVector(positionOffset) : positionOffset;
        //targetRotation = (space == Space.Self) ? (targetRotation * Quaternion.Euler(rotationOffset)) : (Quaternion.Euler(rotationOffset) * targetRotation);

        // Transform target pos + rot into amounts to move + rotate
        //var localMoveAmount = transform.parent.InverseTransformPoint(targetPosition) - transform.localPosition;
        //var localRotateAmount = Quaternion.FromToRotation(transform.localRotation.eulerAngles, (Quaternion.Inverse(transform.parent.rotation) * targetRotation).eulerAngles).eulerAngles;


        StartCoroutine(PTUtilities.instance.RotateTransformViaCurve(transform, resetCurve, optionalOrigin, resetDuration, TimeScale.Scaled));
        yield return StartCoroutine(PTUtilities.instance.MoveTransformViaCurve(transform, resetCurve, optionalOrigin, resetDuration, TimeScale.Scaled));

        // Zoom back to origin
        //StartCoroutine(PTUtilities.instance.RotateTransformViaCurve(transform, resetCurve, localRotateAmount, resetDuration, TimeScale.Scaled));
        //yield return StartCoroutine(PTUtilities.instance.MoveTransformViaCurve(transform, resetCurve, localMoveAmount, resetDuration, TimeScale.Scaled));


        // Turn back on the colliders and rigidbody
        if (rb != null) {
            foreach (Collider col in cols) col.enabled = true;
            rb.isKinematic = (keepKinematic) ? isKinematic : true;
            rb.useGravity = (keepGravity) ? useGravity : false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }


        // Send ending event out for listeners to pick up
        if (OnEndedReset != null) {
            if (debugging) Debug.Log("[ResetToOrigin] OnEndedReset called.");
            OnEndedReset();
        }


        resettingCo = null;
    }

}
