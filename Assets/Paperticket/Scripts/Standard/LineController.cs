using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineController : MonoBehaviour {

    [SerializeField] LineRenderer lineRenderer;

    [SerializeField] AnimationCurve startWidth = AnimationCurve.Constant(0, 1, 1);
    [SerializeField] AnimationCurve endWidth = AnimationCurve.Constant(0, 1, 1);

    [SerializeField] [Min(0.01f)] float duration = 1f;

    float currentTime = 0;



    // Update is called once per frame
    void Update() {

        currentTime += Time.deltaTime / duration;

        lineRenderer.startWidth = startWidth.Evaluate(currentTime);
        lineRenderer.endWidth = endWidth.Evaluate(currentTime);
        
    }
}
