using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using Paperticket;

public class ShrinkToSphere : MonoBehaviour {

    [SerializeField] float duration = 1.0f;
    [SerializeField] float scaleAmount = 0.1f;
    [SerializeField] AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
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

        // Scale the sphere down over (duration) seconds
        StartCoroutine(PTUtilities.instance.ScaleTransformViaCurve(transform, curve, Vector3.one * scaleAmount, duration, TimeScale.Scaled));

        // Move the sphere to the target over (duration seconds)
        constraint.constraintActive = true;
        float t = 0;
        while (constraint.weight != 1) {
            t += duration * Time.deltaTime;
            constraint.weight = Mathf.Clamp01(Mathf.Lerp(0, 1, curve.Evaluate(t)));
            yield return null;
        }

    }

}
