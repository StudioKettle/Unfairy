using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paperticket;
using UnityEngine.Events;

public class RagePlane : MonoBehaviour {

    [SerializeField] MeshRenderer mRenderer = null;
    [SerializeField] bool autoActivate = false;
    [Space(10)]
    [SerializeField] float opacity = 0.5f;
    [SerializeField] bool useAlpha = false;
    [SerializeField] Vector2 alphaOffset = Vector2.zero;
    [Space(10)]
    [SerializeField] float duration = 0;
    [SerializeField] AnimationCurve curve = AnimationCurve.Linear(0,0,1,1);
    [SerializeField] TimeScale timescale = TimeScale.Scaled;
    [Space(10)]
    [SerializeField] float fadeIn = 0;
    [SerializeField] float fadeOut = 0;

    [SerializeField] UnityEvent2 startEvent = null;
    [SerializeField] UnityEvent2 finishEvent = null;

    Coroutine activatingCo = null;

    // Start is called before the first frame update
    void Start() {

        if (autoActivate) Activate();
    }


    public void Activate() {
        if (activatingCo != null) return;
        
        SetMatStuff();

        activatingCo = StartCoroutine(Activating());
    }

    IEnumerator Activating() {
        if (startEvent != null) startEvent.Invoke();

        yield return StartCoroutine(PTUtilities.instance.FadeMeshFloatPropTo(mRenderer, "_Alpha", opacity, fadeIn, curve, timescale));

        if (timescale == TimeScale.Scaled)
            yield return new WaitForSeconds(duration - fadeIn - fadeOut);
        else
            yield return new WaitForSecondsRealtime(duration - fadeIn - fadeOut);

        yield return StartCoroutine(PTUtilities.instance.FadeMeshFloatPropTo(mRenderer, "_Alpha", 0, fadeOut, curve, timescale));

        if (finishEvent != null) finishEvent.Invoke();
    }


    void SetMatStuff() {
        //Debug.Log("[RagePlane] SetMatStuff");
        mRenderer.material.SetFloat("_UseAlphaOffset", useAlpha ? 1f : 0);
        mRenderer.material.SetVector("_AlphaOffset", alphaOffset);
    }

}
