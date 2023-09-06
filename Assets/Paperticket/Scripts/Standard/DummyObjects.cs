using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyObjects : MonoBehaviour
{

    [SerializeField] Object[] dummyObjects;


    public void Awake() {
        string dummyString = "[DummyObjects] Dummy objects registered: \n";

        for (int i = 0; i < dummyObjects.Length; i++) {
            dummyString += "Object[" + i + "/" + (dummyObjects.Length - 1) + "] = " + dummyObjects[i].name + "\n";
        }

        Debug.Log(dummyString);
    }

}
