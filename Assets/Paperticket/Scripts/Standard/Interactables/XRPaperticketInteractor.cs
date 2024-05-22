using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using Paperticket;

[SelectionBase]
[DisallowMultipleComponent]
[AddComponentMenu("Paperticket/XR Paperticket Interactor")]
public class XRPaperticketInteractor : XRDirectInteractor {
    [Header("Paperticket Settings")]
    [Space(10)]
    public float forcingDetachTime = 0.25f;


    protected override void Awake() {

        if (PTUtilities.instance != null && PTUtilities.instance.SetupComplete) {
            Setup();            
        } else PTUtilities.OnSetupComplete += Setup;

        base.Awake();
    }

    void Setup() {

    }

    
    public void ForceDetach() {
                
        //if (forcingDetachCo != null) return;
        if (interactablesSelected.Count == 0) return;

        PTUtilities.instance.StartCoroutine(PTUtilities.instance.ToggleComponent(this, forcingDetachTime, TimeScale.Unscaled));

        //forcingDetachCo = StartCoroutine(ForcingDetach(interactablesSelected[0] as XRGrabInteractable));        
    }

    //Coroutine forcingDetachCo = null;
    //IEnumerator ForcingDetach(XRBaseInteractable interactable) {
    //    directInteractor.enabled = false;
    //    yield return new WaitForSeconds(forcingDetachTime.Min(Time.deltaTime));
    //    directInteractor.enabled = true;
    //}
}