using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using Paperticket;

public class ShrinkToSphere : MonoBehaviour {

    [SerializeField] float duration = 1.0f;
    [SerializeField] float scaleAmount = 0.1f;
    [SerializeField] AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] AnimationCurve constraintCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Space(10)]
    [SerializeField] UnityEvent2 startEvents;

    ParentConstraint constraint = null;
    Coroutine shrinkingCo = null;

    // Start is called before the first frame update
    void Start() {

        constraint = GetComponent<ParentConstraint>();
        
    }

    
    public void Shrink() {
        if (shrinkingCo != null) StopCoroutine(shrinkingCo);
        shrinkingCo = StartCoroutine(Shrinking());
    }


    IEnumerator Shrinking() {        

        if (startEvents != null) startEvents.Invoke();

        // Reset the position of the target to the sphere, but leave children where they are
        Debug.Log("[ShrinkToSphere] constraint = " + constraint);
        Debug.Log("[ShrinkToSphere] GetSource(0) = " + constraint.GetSource(0));
        Debug.Log("[ShrinkToSphere] SourceTransform = " + constraint.GetSource(0).sourceTransform);
        Debug.Log("[ShrinkToSphere] Position = " + constraint.GetSource(0).sourceTransform.position);
        var movement = constraint.GetSource(0).sourceTransform.position - transform.position;        
        transform.position += movement;
        foreach (Transform child in transform) {
            child.position -= movement;
        }
        constraint.translationAtRest = transform.position;
        constraint.rotationAtRest = transform.rotation.eulerAngles;


        // Scale the sphere down over (duration) seconds
        StartCoroutine(PTUtilities.instance.ScaleTransformViaCurve(transform, scaleCurve, Vector3.one * scaleAmount, duration, TimeScale.Scaled));

        // Move the sphere to the target over (duration seconds)
        constraint.SetTranslationOffset(0, Vector3.zero);
        constraint.SetRotationOffset(0, Vector3.zero);
        constraint.constraintActive = true;
        float t = 0;
        while (constraint.weight != 1) {
            t += Time.deltaTime;
            constraint.weight = Mathf.Clamp01(Mathf.Lerp(0, 1, constraintCurve.Evaluate(t / duration)));
            yield return null;
        }

        constraint.enabled = false;


        foreach (Transform child in transform) {
            child.gameObject.SetActive(false);
        }

    }

}
