using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paperticket;

public class Wand : MonoBehaviour
{

    public VelocityEvent velocityEvent;

    public Color startingColor;
    
    public Gradient gradient;

    public float intensity = 1f;

    public GameObject wandObject = null;

    public string propertyName = "";

    public bool startActive = true;

    //List<Material> mats = null;
    public Material mat = null;

    Coroutine wandingCo = null;

    void OnEnable() {

        if (wandObject == null) wandObject = gameObject;

        //foreach (MeshRenderer renderer in wandObject.GetComponentsInChildren<MeshRenderer>()) {
        //    mats.Add(renderer.material);
        //}

        mat = mat != null ? mat : wandObject.GetComponent<MeshRenderer>().material;

        propertyName = propertyName != null ? mat.HasProperty(propertyName) ? propertyName : mat.HasProperty("_Color") ? "_Color" : mat.HasProperty("_BaseColor") ? "_BaseColor" : "" : "";
        if (propertyName == "") {
            Debug.LogError("[Wand] ERROR -> Could not find property name of mesh renderer to fade! Cancelling...");
        }


        mat.SetColor(propertyName, startingColor);

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

        //foreach (Material mat in mats) {
        //    mat.SetColor(propertyName, gradient.Evaluate(0));
        //}
        mat.SetColor(propertyName, gradient.Evaluate(0) * intensity);

        velocityEvent.enabled = true;

        while (true) {
            //foreach (Material mat in mats) {
            //    mat.SetColor(propertyName, gradient.Evaluate(velocityEvent.Progress)));
            //}
            mat.SetColor(propertyName, gradient.Evaluate(velocityEvent.Progress) * intensity);
            yield return null;
        }
        
    }

}
