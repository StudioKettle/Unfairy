using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Paperticket;

public class Wand : MonoBehaviour
{

    public VelocityEvent velocityEvent;
    
    public Gradient gradient;

    string propertyName = "";
    Material mat;

    void OnEnable() {

        mat = GetComponent<MeshRenderer>().material;
                
        propertyName = mat.HasProperty("_Color") ? "_Color" : mat.HasProperty("_BaseColor") ? "_BaseColor" : "";
        if (propertyName == "") {
            Debug.LogError("[Wand] ERROR -> Could not find property name of mesh renderer to fade! Cancelling...");
        }



    }

    private void Update() {
        mat.SetColor(propertyName, gradient.Evaluate(velocityEvent.Progress));
    }


    //IEnumerator ChangeColor() {



    //    mat.SetColor(propertyName, gradient.Evaluate(progress));

    //    for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration) {            
            
    //        yield return null;
    //    }
    //    mat.SetColor(propertyName, gradient.Evaluate(1));
    //    yield return null;


    //}

}
