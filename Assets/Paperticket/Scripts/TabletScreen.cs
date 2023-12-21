using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletScreen : MonoBehaviour {


    [SerializeField] SpriteRenderer spriteRenderer = null;
    [SerializeField] Sprite[] tabletScreens = null;

    [Space(5)]

    [SerializeField] bool debugging = false;

    int currentIndex = 0;


    // Start is called before the first frame update
    void Start() {
        spriteRenderer = spriteRenderer ?? GetComponent<SpriteRenderer>();

        if (tabletScreens.Length == 0) {
            Debug.LogError("[TabletScreen] No tablet screens added! Disabling.");
            enabled = false;
        }
    }


    public void SetScreen(int screenIndex) {
        if (currentIndex == screenIndex) {
            Debug.LogWarning("[TabletScreen] Passed screen is the same as current screen, disregarding.");
            return;
        }
        if (screenIndex >= tabletScreens.Length) {
            Debug.LogError("[TabletScreen] Invalid screen index, disregarding.");
            return;
        }

        spriteRenderer.sprite = tabletScreens[screenIndex];
        currentIndex = screenIndex;
    }

    public void NextScreen() {
        SetScreen((currentIndex + 1) % tabletScreens.Length);
    }

    public void PreviousScreen() {
        if (currentIndex > 0) SetScreen((currentIndex - 1) % tabletScreens.Length);
        else SetScreen(tabletScreens.Length - 1);
    }

}
