using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateLighting : MonoBehaviour
{

    [Min(0.025f)] public float updateTick = 0.1f;
    [Space(5)]
    [SerializeField] ReflectionProbe[] probes = null;
    [Space(5)]
    [SerializeField] bool outputRenderTextures = false;
    [SerializeField] RenderTexture[] outputs = null;


    Coroutine UpdatingLightingCo;

    // Start is called before the first frame update
    void OnEnable()
    {
        if (outputRenderTextures && outputs.Length != probes.Length) {
            Debug.LogError("[UpdateLighting] ERROR -> Not enough output RenderTextures assigned for amount of reflection probes present! Disabling.");
            enabled = false;
        }

        UpdatingLightingCo = StartCoroutine(UpdatingLighting());

    }

    void OnDisable() {
        StopAllCoroutines();
    }

    IEnumerator UpdatingLighting() {

        Debug.Log("[UpdateLighting] Starting to update dynamic GI"); 

        while (enabled) {
            yield return new WaitForSeconds(updateTick);

            if (probes.Length > 0) {
                for (int i = 0; i < probes.Length; i++) {
                    if (outputRenderTextures) probes[i].RenderProbe(outputs[i]);
                    else probes[i].RenderProbe();
                }
            }

            DynamicGI.UpdateEnvironment();
        }

        Debug.Log("[UpdateLighting] Stopping update of dynamic GI");




    }
}
