using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalScreen : MonoBehaviour {

    [SerializeField] MeshRenderer meshRenderer = null;
    [SerializeField] Material[] screens = null;

    [Space(5)]

    [SerializeField] bool debugging = false;

    int currentIndex = -1;


    // Start is called before the first frame update
    void Start() {
        meshRenderer = meshRenderer ?? GetComponent<MeshRenderer>();

        if (screens.Length == 0) {
            Debug.LogError("[CrystalScreen] No screens added! Disabling.");
            enabled = false;
        }
    }


    public void SetScreen(int screenIndex) {
        if (currentIndex == screenIndex) {
            Debug.LogWarning("[CrystalScreen] Passed screen is the same as current screen, disregarding.");
            return;
        }
        if (screenIndex >= screens.Length) {
            Debug.LogError("[CrystalScreen] Invalid screen index, disregarding.");
            return;
        }

        if (screens[screenIndex] == null) {
            meshRenderer.enabled = false;
        } else {
            meshRenderer.material = screens[screenIndex];
            meshRenderer.enabled = true;
        }

        currentIndex = screenIndex;
    }

    public void NextScreen() {
        SetScreen((currentIndex + 1) % screens.Length);
    }

    public void PreviousScreen() {
        if (currentIndex > 0) SetScreen((currentIndex - 1) % screens.Length);
        else SetScreen(screens.Length - 1);
    }

}
