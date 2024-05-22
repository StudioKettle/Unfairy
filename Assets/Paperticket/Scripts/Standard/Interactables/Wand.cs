using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paperticket;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

public class Wand : MonoBehaviour {

    [Header("Required")]
    [Space(5)]
    public VelocityEvent velocityEvent;
    //public GameObject wandObject = null;
    [Header("Optional")]
    [Space(5)]
    [SerializeField] Material mat = null;
    [SerializeField] XRHandedGrabInteractable interactable = null;
    [SerializeField] Transform origin;
    [Space(15)]
    public bool startActive = true;
    [Header("COLOURS")]
    [Space(5)]
    public Color startingColor;
    [Space(5)]
    public Gradient gradient;
    public float intensity = 1f;
    public string propertyName = "";
    [Header("XR")]
    [Space(5)]
    [SerializeField] bool activateOnSelect = false;
    [SerializeField] bool deactivateOnUnselect = false;
    [SerializeField] bool deprogressOnUnselect = false;
    [SerializeField] bool resetOnUnselect = false;
    [Header("ORIGIN")]
    [Space(5)]
    public float resetDelay = 1;
    public float resetDuration = 1;
    public AnimationCurve resetCurve = AnimationCurve.Linear(0,0,1,1);

    [Header("READ ONLY")]
    [Space(10)]
    [SerializeField] bool active = false;
    [SerializeField] bool selected = false;

    Coroutine trackingCo = null;
    Coroutine resettingCo = null;

    Rigidbody rb = null;


    [SerializeField] UnityEvent2 OnWandResetStart;
    [SerializeField] UnityEvent2 OnWandResetFinish;
    [Space(10)]
    [SerializeField] UnityEvent2 OnWandSelectUnactivated;
    [SerializeField] UnityEvent2 OnWandSelectActivated;



    void OnEnable() {

        //if (wandObject == null) wandObject = gameObject;

        //mat = mat != null ? mat : wandObject.GetComponent<MeshRenderer>().material;

        rb = gameObject.GetComponent<Rigidbody>();

        propertyName = propertyName != null ? mat.HasProperty(propertyName) ? propertyName : mat.HasProperty("_Color") ? "_Color" : mat.HasProperty("_BaseColor") ? "_BaseColor" : "" : "";
        if (propertyName == "") {
            Debug.LogError("[Wand] ERROR -> Could not find property name of mesh renderer to fade! Cancelling...");
        }
        mat.SetColor(propertyName, startingColor);


        if (interactable != null) {
            interactable.selectEntered.AddListener(Selected);
            interactable.selectExited.AddListener(Unselected);
        }


        if (startActive) {
            Activate();
        }
    }

    void OnDisable() {
        if (interactable != null) {
            interactable.selectEntered.RemoveListener(Selected);
            interactable.selectExited.RemoveListener(Unselected);
        }
    }






    #region On select/unselect of interactable object

    public void PretendSelect() {
        var args = new SelectEnterEventArgs();
        Selected(args);
    }

    void Selected(SelectEnterEventArgs args) {
        if (resettingCo != null) return;
        selected = true;

        if (active && OnWandSelectActivated != null) OnWandSelectActivated.Invoke();
        else if (!active && OnWandSelectUnactivated != null) OnWandSelectUnactivated.Invoke();


        if (activateOnSelect) Activate();
    }
    void Unselected(SelectExitEventArgs args) {
        selected = false;
        if (deactivateOnUnselect) Deactivate();
        if (deprogressOnUnselect) velocityEvent.Progress = 0;
        if (resetOnUnselect) {
            if (resettingCo == null && origin != null) resettingCo = StartCoroutine(Resetting(false));
        }

        if (active) mat.SetColor(propertyName, gradient.Evaluate(velocityEvent.Progress) * intensity);
        else mat.SetColor(propertyName, startingColor);
    }

    #endregion







    #region Reset wand to origin

    public void ForceReset() {
        if (resettingCo == null && origin != null) resettingCo = StartCoroutine(Resetting(true));
    }

    IEnumerator Resetting(bool forced) {                

        // wait and then move to origin
        if (!forced) yield return new WaitForSeconds(resetDelay);


        if (OnWandResetStart != null) OnWandResetStart.Invoke();

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

        if (OnWandResetFinish != null) OnWandResetFinish.Invoke();
    }

    #endregion







    #region Activate tracking of wand progress

    public void Activate() {
        active = true;
        
        if (selected && OnWandSelectActivated != null) OnWandSelectActivated.Invoke();

        mat.SetColor(propertyName, gradient.Evaluate(velocityEvent.Progress) * intensity);

        if (trackingCo == null) trackingCo = StartCoroutine(TrackingwandProgress());
    }

    public void Deactivate() {
        active = false;

        if (trackingCo != null) StopCoroutine(trackingCo);

        mat.SetColor(propertyName, startingColor);
    }


    IEnumerator TrackingwandProgress() {    

        while (velocityEvent.Progress < 1) {
            // Wait until the player is holding the wand
            if (interactable != null) {
                if (!selected) {
                    velocityEvent.enabled = false;
                    yield return new WaitUntil(() => selected);
                } else if (!velocityEvent.enabled) {
                    velocityEvent.enabled = true;
                }
            }
            mat.SetColor(propertyName, gradient.Evaluate(velocityEvent.Progress) * intensity);
            yield return null;
        }

        mat.SetColor(propertyName, gradient.Evaluate(intensity));

    }

    #endregion



    #region Forcing auto end on wand

    public void ForceWandToHand() {
        if (forcingWandToHandCo != null) return;
        forcingWandToHandCo = StartCoroutine(ForcingWandToHand());
    }

    Coroutine forcingWandToHandCo = null;
    IEnumerator ForcingWandToHand() {

        // Unselect the interactable
        interactable.ForceDetach();
        yield return null;

        // Set the reset origin to new left constraint
        var autoEndWandConstraint = PTUtilities.instance.NewControllerConstraint(ControllerType.LeftController);
        autoEndWandConstraint.name = "[AutoEndWandConstraint]";
        autoEndWandConstraint.transform.position = origin.transform.position = PTUtilities.instance.leftController.transform.position;
        autoEndWandConstraint.transform.rotation = origin.transform.rotation = PTUtilities.instance.leftController.transform.rotation;
        origin.SetParent(autoEndWandConstraint.transform);
        origin.gameObject.SetActive(true);
        autoEndWandConstraint.SetActive(true);
        yield return null;

        // Disable + destroy interactable
        interactable.enabled = false;
        yield return null;
        Destroy(interactable);
        yield return null;

        // Reset wand to left controller
        ForceReset();
    }


    public void AutoProgressWand(float duration) {
        if (autoProgressingWandCo != null) return;
        autoProgressingWandCo = StartCoroutine(AutoProgressingWand(duration));
    }

    Coroutine autoProgressingWandCo = null;
    IEnumerator AutoProgressingWand(float duration) {
        velocityEvent.Disabled = true;
        while (velocityEvent.Progress < 1) {
            velocityEvent.Progress += Time.deltaTime / duration.Min(0.01f);
            yield return null;
        }
    }





    #endregion

}
