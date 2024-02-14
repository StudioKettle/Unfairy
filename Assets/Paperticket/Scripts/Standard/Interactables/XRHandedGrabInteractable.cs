using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using Paperticket;

[SelectionBase]
[DisallowMultipleComponent]
[AddComponentMenu("Paperticket/XR Handed Grab Interactable")]
public class XRHandedGrabInteractable : XRGrabInteractable {
    [Header("Paperticket Settings")]
    [Space(10)]
    public Transform LeftHandAttachTransform = null;
    public Transform RightHandAttachTransform = null;
    public HandPose animationPose = 0;
    public HandPose hoverPose = 0;

    private XRDirectInteractor leftController = null;
    private XRDirectInteractor rightController = null;

    private Transform originalAttachTransform = null;



    private Coroutine forcingDetachCo = null;

    protected override void Awake() {

        originalAttachTransform = attachTransform;

        if (PTUtilities.instance.SetupComplete) {
            Setup();            
        } else PTUtilities.OnSetupComplete += Setup;

        base.Awake();
    }

    void Setup() {

        leftController = PTUtilities.instance.leftController.GetComponent<XRDirectInteractor>();
        rightController = PTUtilities.instance.rightController.GetComponent<XRDirectInteractor>();

        if (leftController == null || rightController == null) {
            Debug.Log("[XRHandedGrabInteractable] ERROR -> Left or right controller missing! Disabling.");
            enabled = false;
        }
    }

    //  OnSelectEntering - set attachTransform - then call base
    protected override void OnSelectEntering(SelectEnterEventArgs args) {
        if (args.interactorObject == leftController) {
            //Debug.Log($"Left hand");
            attachTransform.SetPositionAndRotation(LeftHandAttachTransform.position, LeftHandAttachTransform.rotation);
        } else if (args.interactorObject == rightController) {
            //Debug.Log($"Right hand");
            attachTransform.SetPositionAndRotation(RightHandAttachTransform.position, RightHandAttachTransform.rotation);
        } else {
            // Handle case where interactor is not left hand or right hand (socket?)
            attachTransform.SetPositionAndRotation(originalAttachTransform.position, originalAttachTransform.rotation);
        }
        base.OnSelectEntering(args);
    }


    
    public void ForceDetach() {
                
        if (forcingDetachCo != null) return;
        if (interactorsSelecting.Count == 0) return;

        forcingDetachCo = StartCoroutine(ForcingDetach(interactorsSelecting[0] as XRBaseInteractor));        
    }

    private IEnumerator ForcingDetach(XRBaseInteractor interactor) {
        if (interactor.TryGetComponent(out XRDirectInteractor directInteractor)) {
            directInteractor.enabled = false;
            yield return new WaitForSeconds(0.25f);
            directInteractor.enabled = true;
        }
    }
}