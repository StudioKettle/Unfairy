using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paperticket;

public class Wand : MonoBehaviour
{

    public VelocityEvent velocityEvent;
    
    public Gradient gradient;

    public GameObject wandObject = null;

    string propertyName = "";

    public bool startActive = true;

    Material mat = null;

    Coroutine wandingCo = null;

    void OnEnable() {

        if (wandObject == null) wandObject = gameObject;

        mat = wandObject.GetComponent<MeshRenderer>().material;
                
        propertyName = mat.HasProperty("_Color") ? "_Color" : mat.HasProperty("_BaseColor") ? "_BaseColor" : "";
        if (propertyName == "") {
            Debug.LogError("[Wand] ERROR -> Could not find property name of mesh renderer to fade! Cancelling...");
        }

        if (startActive) {
            Activate();
        }

    }

    public void Activate() {
        Deactivate();
        wandingCo = StartCoroutine(Wanding());
    }

    public void Deactivate() {
        if (wandingCo != null) StopCoroutine(wandingCo);
    }


    IEnumerator Wanding() {

        mat.SetColor(propertyName, gradient.Evaluate(0));

        velocityEvent.enabled = true;

        while (true) {
            mat.SetColor(propertyName, gradient.Evaluate(velocityEvent.Progress));
            yield return null;
        }
        
    }

}
