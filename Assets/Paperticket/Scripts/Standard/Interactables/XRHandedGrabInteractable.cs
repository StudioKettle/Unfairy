using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using Paperticket;

[AddComponentMenu("Paperticket/XR/XR Handed Grab Interactable")]
public class XRHandedGrabInteractable : XRGrabInteractable {
    [Header("Paperticket Settings")]
    [Space(10)]
    [SerializeField] private Transform LeftHandAttachTransform;
    [SerializeField] private Transform RightHandAttachTransform;


    private XRDirectInteractor leftController;
    private XRDirectInteractor rightController;

    private Transform m_OriginalAttachTransform;

    protected override void Awake() {

        m_OriginalAttachTransform = attachTransform;

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
            Debug.Log($"Left hand");
            attachTransform.SetPositionAndRotation(LeftHandAttachTransform.position, LeftHandAttachTransform.rotation);
        } else if (args.interactorObject == rightController) {
            Debug.Log($"Right hand");
            attachTransform.SetPositionAndRotation(RightHandAttachTransform.position, RightHandAttachTransform.rotation);
        } else {
            // Handle case where interactor is not left hand or right hand (socket?)
            attachTransform.SetPositionAndRotation(m_OriginalAttachTransform.position, m_OriginalAttachTransform.rotation);
        }
        base.OnSelectEntering(args);
    }
}