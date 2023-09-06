using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLighting : MonoBehaviour
{

    [Min(0.025f)] public float updateTick = 0.1f;

    Coroutine UpdatingLightingCo;

    // Start is called before the first frame update
    void OnEnable()
    {
        UpdatingLightingCo = StartCoroutine(UpdatingLighting());

    }

    IEnumerator UpdatingLighting() {

        Debug.Log("[UpdateLighting] Starting to update dynamic GI"); 

        while (enabled) {
            yield return new WaitForSeconds(updateTick);
            DynamicGI.UpdateEnvironment();
        }

        Debug.Log("[UpdateLighting] Stopping update of dynamic GI");




    }
}
