using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[SelectionBase]
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
[AddComponentMenu("Paperticket/XR/XR Extended Interactable")]
public class XRExtendedInteractable : XRBaseInteractable {

    public UnityEvent2 ExtOnHoverEnter = null;
    public UnityEvent2 ExtOnHoverExit = null;

    public UnityEvent2 ExtOnSelectEnter = null;
    public UnityEvent2 ExtOnSelectExit = null;

    public UnityEvent2 ExtOnActivate = null;
    public UnityEvent2 ExtOnDeactivate = null;

    

    /// <summary>This method is called by the interaction manager 
    /// when the interactor first initiates hovering over an interactable.</summary>
    /// <param name="interactor">Interactor that is initiating the hover.</param>
    protected override void OnHoverEntered(HoverEnterEventArgs args ) {
        base.OnHoverEntered(args);
        if (ExtOnHoverEnter != null) ExtOnHoverEnter.Invoke();
    }

    /// <summary>This method is called by the interaction manager 
    /// when the interactor ends hovering over an interactable.</summary>
    /// <param name="interactor">Interactor that is ending the hover.</param>
    protected override void OnHoverExited(HoverExitEventArgs args ) {
        base.OnHoverExited(args);
        if (ExtOnHoverExit != null) ExtOnHoverExit.Invoke();
    }

    /// <summary>This method is called by the interaction manager 
    /// when the interactor first initiates selection of an interactable.</summary>
    /// <param name="interactor">Interactor that is initiating the selection.</param>
    protected override void OnSelectEntered( SelectEnterEventArgs args) {
        base.OnSelectEntered(args);
        if (ExtOnSelectEnter != null) ExtOnSelectEnter.Invoke();
    }

    /// <summary>This method is called by the interaction manager 
    /// when the interactor ends selection of an interactable.</summary>
    /// <param name="interactor">Interactor that is ending the selection.</param>
    protected override void OnSelectExited( SelectExitEventArgs args ) {
        base.OnSelectExited(args);
        if (ExtOnSelectExit != null) ExtOnSelectExit.Invoke();
    }

    /// <summary>This method is called by the interaction manager 
    /// when the interactor sends an activation event down to an interactable.</summary>
    /// <param name="interactor">Interactor that is sending the activation event.</param>
    protected override void OnActivated( ActivateEventArgs args ) {
        base.OnActivated(args);
        if (ExtOnActivate != null) ExtOnActivate.Invoke();
    }

    protected override void OnDeactivated( DeactivateEventArgs args ) {
        base.OnDeactivated(args);
        if (ExtOnDeactivate != null) ExtOnDeactivate.Invoke();
    }

}
