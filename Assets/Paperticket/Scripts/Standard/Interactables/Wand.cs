using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paperticket;
using UnityEngine.XR.Interaction.Toolkit;

public class Wand : MonoBehaviour {

    [Header("Required")]
    [Space(5)]
    public VelocityEvent velocityEvent;
    public GameObject wandObject = null;
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

    Coroutine wandingCo = null;
    Coroutine resettingCo = null;

    Rigidbody rb = null;

    //public delegate void WandResetStart();
    //public event WandResetStart OnWandResetStart;
    //public delegate void WandResetFinish();
    //public event WandResetFinish OnWandResetFinish;


    void OnEnable() {

        if (wandObject == null) wandObject = gameObject;

        mat = mat != null ? mat : wandObject.GetComponent<MeshRenderer>().material;

        rb = gameObject.GetComponent<Rigidbody>();

        propertyName = propertyName != null ? mat.HasProperty(propertyName) ? propertyName : mat.HasProperty("_Color") ? "_Color" : mat.HasProperty("_BaseColor") ? "_BaseColor" : "" : "";
        if (propertyName == "") {
            Debug.LogError("[Wand] ERROR -> Could not find property name of mesh renderer to fade! Cancelling...");
        }


        mat.SetColor(propertyName, startingColor);

        if (startActive) {
            Activate();
        }

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


    void Selected(SelectEnterEventArgs args) {
        if (resettingCo != null) return;
        selected = true;        
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

    public void ForceReset() {
        if (resettingCo == null && origin != null) resettingCo = StartCoroutine(Resetting(true));
    }

    IEnumerator Resetting(bool forced) {                
        //if (OnWandResetStart != null) OnWandResetStart();

        // wait and then move to origin
        if (!forced) yield return new WaitForSeconds(resetDelay);


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

        //if (OnWandResetFinish != null) OnWandResetFinish();
    }


    public void Activate() {
        active = true;

        mat.SetColor(propertyName, gradient.Evaluate(velocityEvent.Progress) * intensity);

        if (wandingCo == null) wandingCo = StartCoroutine(Wanding());
    }

    public void Deactivate() {
        active = false;

        if (wandingCo != null) StopCoroutine(wandingCo);

        mat.SetColor(propertyName, startingColor);
    }


    IEnumerator Wanding() {    

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


}
