using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paperticket;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

public class LockedWand : MonoBehaviour {

    [Header("REFERENCES")]
    [Space(5)]
    public VelocityEvent velocityEvent;
    [SerializeField] Material mat = null;
    [SerializeField] XRHandedGrabInteractable interactable = null;
    [SerializeField] Transform origin = null;
    [Space(15)]
    [SerializeField] bool debugging = false;
    [Header("COLOURS")]
    [Space(5)]
    public Color startingColor;
    [Space(5)]
    public Gradient gradient;
    public float intensity = 1f;
    public string propertyName = "";
    [Header("XR")]
    [Space(5)]
    [SerializeField] bool resetOnUnselect = true;
    [Space(5)]
    public float resetDelay = 1;
    public float resetDuration = 1;
    public AnimationCurve resetCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("READ ONLY")]
    [Space(10)]
    [SerializeField] bool tracking = false;
    [SerializeField] bool selected = false;

    Coroutine trackingCo = null;
    Coroutine resettingCo = null;

    Rigidbody rb = null;
    Transform attachTransform = null;

    [Space(10)]
    [SerializeField] UnityEvent2 OnLocked;
    [SerializeField] UnityEvent2 OnFinished;



    void OnEnable() {
        rb = gameObject.GetComponent<Rigidbody>();

        propertyName = propertyName != null ? mat.HasProperty(propertyName) ? propertyName : mat.HasProperty("_Color") ? "_Color" : mat.HasProperty("_BaseColor") ? "_BaseColor" : "" : "";
        if (propertyName == "") {
            Debug.LogError("[LockedWand] ERROR -> Could not find property name of mesh renderer to fade! Cancelling...");
        }
        mat.SetColor(propertyName, startingColor);


        if (interactable != null) {
            interactable.selectEntered.AddListener(Selected);
            interactable.selectExited.AddListener(Unselected);
        }

    }

    void OnDisable() {
        if (interactable != null) {
            interactable.selectEntered.RemoveListener(Selected);
            interactable.selectExited.RemoveListener(Unselected);
        }
    }






    #region On select/unselect of interactable object

    //public void PretendSelect() {
    //    var args = new SelectEnterEventArgs();
    //    Selected(args);
    //}

    void Selected(SelectEnterEventArgs args) {
        if (resettingCo != null) return;

        var interactorObject = args.interactorObject.transform.gameObject;

        if (tracking) {
            if (debugging) Debug.Log("[LockedWand] Selected by '" + interactorObject.name + "', but tracking is already enabled! Locking wand.");
            if (args.interactorObject.transform.gameObject.name.Contains("Left")) {
                ActivateWand(ControllerType.LeftController);
            } else ActivateWand(ControllerType.RightController);
            return;
        }

        if (debugging) Debug.Log("[LockedWand] Selected by '" + interactorObject.name + "'!");
        selected = true;
    }
    void Unselected(SelectExitEventArgs args) {
        var interactorObject = args.interactorObject.transform.gameObject;
        if (debugging) Debug.Log("[LockedWand] Unselected by '" + interactorObject.name + "'!");

        selected = false;
        if (resetOnUnselect && resettingCo == null && origin != null) resettingCo = StartCoroutine(Resetting(false));

        mat.SetColor(propertyName, startingColor);
    }

    #endregion







    #region Activate tracking of wand progress

    public void StartTracking() {
        if (debugging) Debug.Log("[LockedWand] Started tracking wand progress...");

        tracking = true;

        mat.SetColor(propertyName, gradient.Evaluate(velocityEvent.Progress) * intensity);

        if (selected) {
            if (debugging) Debug.Log("[LockedWand] Wand already grabbed! Locking wand.");
            if (interactable.interactorsSelecting[0].transform.gameObject.name.Contains("Left")) {
                ActivateWand(ControllerType.LeftController);
            } else ActivateWand(ControllerType.RightController);
        }
    }


    //public void StopTracking() {
    //    tracking = false;
    //    if (trackingCo != null) StopCoroutine(trackingCo);
    //    mat.SetColor(propertyName, startingColor);
    //}



    #endregion







    #region Forcing auto end on wand

    Coroutine activatingWandCo = null;
    public void ActivateWand(ControllerType controllerType) {

        // Remove listeners
        interactable.selectEntered.RemoveListener(Selected);
        interactable.selectExited.RemoveListener(Unselected);

        if (activatingWandCo != null) return;
        activatingWandCo = StartCoroutine(ActivatingWand(controllerType));
    }

    IEnumerator ActivatingWand(ControllerType controllerType) {
        Debug.Log("[LockedWand] Attempting to lock wand origin to controller...");

        // Lock the wand to controller
        yield return StartCoroutine(LockingWandToHand(controllerType));        
        if (OnLocked != null) OnLocked.Invoke();

                
        Debug.Log("[LockedWand] Wand origin locked to controller! Resetting wand to hand...");

        // Reset wand to hand (if not already selected or already resetting)
        if (!selected) {
            // Zoom back to origin
            StartCoroutine(PTUtilities.instance.RotateTransformViaCurve(transform, resetCurve, origin, 0.125f, TimeScale.Scaled));
            yield return StartCoroutine(PTUtilities.instance.MoveTransformViaCurve(transform, resetCurve, origin, 0.125f, TimeScale.Scaled));
        }
        transform.parent = origin;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;



        Debug.Log("[LockedWand] Wand reset to hand! Starting to track wand progress...");

        // Wait until velocity event is progressed
        velocityEvent.enabled = true;
        while (velocityEvent.Progress < 1) {
            mat.SetColor(propertyName, gradient.Evaluate(velocityEvent.Progress) * intensity);
            yield return null;
        }
        mat.SetColor(propertyName, gradient.Evaluate(intensity));


        Debug.Log("[LockedWand] Wand progress finished!");

        if (OnFinished != null) OnFinished.Invoke();
        
    }


    //public void LockWandToHand(ControllerType controllerType) {
    //    if (lockingWandToHandCo != null) return;
    //    lockingWandToHandCo = StartCoroutine(LockingWandToHand(controllerType));
    //}

    //Coroutine lockingWandToHandCo = null;
    IEnumerator LockingWandToHand(ControllerType controllerType) {


        // Unselect the interactable
        interactable.ForceDetach();
        //yield return new WaitForSeconds(0.25f);
        yield return null;
        interactable.enabled = false;
        yield return null;
        Destroy(interactable);

        // Set the reset origin to new controller constraint
        var autoEndWandConstraint = PTUtilities.instance.NewControllerConstraint(controllerType);
        switch (controllerType) {
            case ControllerType.LeftController:
                autoEndWandConstraint.name = "[LockedWandConstraint_Left]";
                autoEndWandConstraint.transform.position = PTUtilities.instance.leftController.transform.position;
                autoEndWandConstraint.transform.rotation = PTUtilities.instance.leftController.transform.rotation;
                origin.SetParent(autoEndWandConstraint.transform);
                origin.transform.localPosition = -interactable.LeftHandAttachTransform.localPosition;
                origin.transform.localRotation = interactable.LeftHandAttachTransform.localRotation;                
                PTUtilities.instance.leftController.GetComponentInChildren<HandAnimController>().SetHandPose(interactable.animationPose);
                break;
            case ControllerType.RightController:
                autoEndWandConstraint.name = "[LockedWandConstraint_Right]";
                autoEndWandConstraint.transform.position = PTUtilities.instance.rightController.transform.position;
                autoEndWandConstraint.transform.rotation = PTUtilities.instance.rightController.transform.rotation;
                origin.SetParent(autoEndWandConstraint.transform);
                origin.transform.localPosition = -interactable.RightHandAttachTransform.localPosition;
                origin.transform.localRotation = interactable.RightHandAttachTransform.localRotation;
                PTUtilities.instance.rightController.GetComponentInChildren<HandAnimController>().SetHandPose(interactable.animationPose);
                break;
            case ControllerType.Head:
            default:
                Debug.LogError("[LockedWand] ERROR -> Bad ControllerType defined! Did you set to Head by accident?");
                break;
        }
        origin.gameObject.SetActive(true);
        autoEndWandConstraint.SetActive(true);
        yield return null;

        // Disable + destroy interactable
        //interactable.enabled = false;
        //yield return null;
        //Destroy(interactable);
        //yield return null;

        // Turn off the colliders and rigidbody
        Destroy(rb);
        foreach (Collider col in GetComponents<Collider>()) Destroy(col);


    }


    //IEnumerator TrackingWandProgress() {

    //    while (velocityEvent.Progress < 1) {
    //        // Wait until the player is holding the wand
    //        if (interactable != null) {
    //            if (!selected) {
    //                velocityEvent.enabled = false;
    //                if (debugging) Debug.Log("[LockedWand] Waiting until we are selected");
    //                yield return new WaitUntil(() => selected);
    //            } else if (!velocityEvent.enabled) {
    //                velocityEvent.enabled = true;
    //            }
    //        }
    //        mat.SetColor(propertyName, gradient.Evaluate(velocityEvent.Progress) * intensity);
    //        yield return null;
    //    }

    //    mat.SetColor(propertyName, gradient.Evaluate(intensity));

    //}






    public void AutoProgressWand(float duration) {
        if (autoProgressingWandCo != null) return;
        autoProgressingWandCo = StartCoroutine(AutoProgressingWand(duration));
    }

    Coroutine autoProgressingWandCo = null;
    IEnumerator AutoProgressingWand(float duration) {
        velocityEvent.Disabled = true;
        float startValue = velocityEvent.Progress;
        float t = 0;
        while (t < 1) {
            velocityEvent.Progress = Mathf.Lerp(startValue, 1, t);
            t += Time.deltaTime / duration.Min(0.01f);
            yield return null;
        }
        velocityEvent.Progress = 1;
    }



    public void AdjustProgressMultiplier(float newValue, float duration) {
        if (adjustingProgMultCo != null) return;
        adjustingProgMultCo = StartCoroutine(adjustingProgressMultiplier(newValue, duration));
    }


    Coroutine adjustingProgMultCo = null;
    IEnumerator adjustingProgressMultiplier(float newValue, float duration) {
        float startValue = velocityEvent.ProgressMultiplier;
        float t = 0;
        while(t < 1) {
            velocityEvent.ProgressMultiplier = Mathf.Lerp(startValue, newValue, t);
            t += Time.deltaTime / duration.Min(0.01f);
            yield return null;
        }
        velocityEvent.ProgressMultiplier = newValue;
    }



    #endregion




    #region Reset wand to origin

    public void ForceReset() {
        if (resettingCo == null && origin != null) resettingCo = StartCoroutine(Resetting(true));
    }

    IEnumerator Resetting(bool forced) {

        // wait and then move to origin
        if (!forced) yield return new WaitForSeconds(resetDelay);


        //if (OnWandResetStart != null) OnWandResetStart.Invoke();

        // Turn off the colliders and rigidbody
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        foreach (Collider col in GetComponents<Collider>()) {
            col.enabled = false;
        }

        // Zoom back to origin
        StartCoroutine(PTUtilities.instance.RotateTransformViaCurve(transform, resetCurve, origin, resetDuration, TimeScale.Scaled));
        yield return StartCoroutine(PTUtilities.instance.MoveTransformViaCurve(transform, resetCurve, origin, resetDuration, TimeScale.Scaled));


        // Turn back on the colliders and rigidbody        
        foreach (Collider col in GetComponents<Collider>()) {
            col.enabled = true;
        }
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        resettingCo = null;

        //if (OnWandResetFinish != null) OnWandResetFinish.Invoke();
    }

    #endregion




}
