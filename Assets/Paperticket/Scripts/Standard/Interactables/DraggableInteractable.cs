using Paperticket;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class DraggableInteractable : ButtonInteractable {

    [Header("Dragging")]

    [SerializeField] bool useHomeSocket;
    [SerializeField] XRSocketInteractor homeSocket;
    [SerializeField] float scaleWhenHeld;
    [SerializeField] float distance;

    [Header("Dragging (Read Only)")]

    [SerializeField] bool homeSocketed;

    Coroutine followingCo;

    Vector3 initialPos;
    Quaternion initialRot;
    Vector3 initialScale;

    protected override void Initialise() {
        base.Initialise();
        initialPos = transform.position;
        initialRot = transform.rotation;
        initialScale = transform.localScale;

        if (useHomeSocket && homeSocket != null) {

            Collider[] colliders = baseInteractable.colliders.ToArray();

            foreach (XRSocketInteractor socket in GameObject.FindObjectsOfType<XRSocketInteractor>()) {
                if (socket != homeSocket) {
                    foreach (Collider col in colliders) {
                        Physics.IgnoreCollision(socket.GetComponent<Collider>(), col);
                    }
                }
            }

        }

    }

    public override void Select() {  
        base.Select();
        if (debugging) Debug.LogWarning("[DraggableInteractable] WARNING -> Drag won't work with this version of Select()");
    }
    public override void Select( XRBaseInteractor interactor ) {
        base.Select(interactor);

        // Start following coroutine if the interactor is the controller
        //XRRayInteractor rayInteractor = interactor as XRRayInteractor;
        if (interactor as XRRayInteractor != null) {
            if (debugging) Debug.Log("[DraggableInteractable] Ray Interactor ("+interactor.name+") detected!");
            if (followingCo != null) StopCoroutine(followingCo);
            followingCo = StartCoroutine(Follow(interactor));

            transform.localScale = Vector3.one * scaleWhenHeld;

        } else if (interactor as XRSocketInteractor != null) {
            if (debugging) Debug.Log("[DraggableInteractable] Socket Interactor ("+interactor.name+") detected!");
            if (useHomeSocket) { 
                if (interactor as XRSocketInteractor == homeSocket) {
                    if (debugging) Debug.Log("[DraggableInteractable] Home socket found, disabled self");
                    foreach (Collider col in baseInteractable.colliders) {
                        Destroy(col);
                        homeSocketed = true;
                        StopAllCoroutines();

                        if ((baseInteractable as XRGrabInteractable) != null) {
                            (baseInteractable as XRGrabInteractable).trackPosition = false;
                            (baseInteractable as XRGrabInteractable).trackRotation = false;
                        }
                        transform.parent = homeSocket.attachTransform;
                        transform.position = homeSocket.attachTransform.position;
                        transform.rotation = homeSocket.attachTransform.rotation;
                        transform.localScale = Vector3.one * scaleWhenHeld;
                    }
                } else {
                    if (debugging) Debug.LogError("[DraggableInteractable] Not my home socket! Shouldn't be possible!");
                    transform.position = initialPos;
                    transform.rotation = initialRot;
                    transform.localScale = initialScale;
                }
            }
        } else {
            if (debugging) Debug.LogError("[DraggableInteractable] Not sure what's got me, returning home");
            transform.position = initialPos;
            transform.rotation = initialRot;
            transform.localScale = initialScale;
        }


    }


    public override void Deselect() { 
        base.Deselect();
        if (debugging) Debug.LogWarning("[DraggableInteractable] WARNING -> Drag won't work with this version of Deselect()");
    }
    public override void Deselect( XRBaseInteractor interactor ) {
        base.Deselect(interactor);

        // Stop following coroutine is the interactor is the controller
        XRRayInteractor rayInteractor = interactor as XRRayInteractor;
        if (rayInteractor != null) {
            StopCoroutine(followingCo);
            followingCo = null;
            
            // Reset the controller attach point to default position
            interactor.attachTransform.localPosition = Vector3.zero;

            if (!homeSocketed) {

                transform.position = initialPos;
                transform.rotation = initialRot;
                transform.localScale = initialScale;

            }

        } else {
            Debug.Log("[DraggableInteractable] Socket gone!");
        }
        
    }


    public override void HoverOn() { 
        base.HoverOn();
        if (debugging) Debug.LogWarning("[DraggableInteractable] WARNING -> Drag won't work with this version of HoverOn()");
    }
    public override void HoverOn( XRBaseInteractor interactor ) {
        base.HoverOn(interactor);
    }


    public override void HoverOff() { 
        base.HoverOff();
        if (debugging) Debug.LogWarning("[DraggableInteractable] WARNING -> Drag won't work with this version of HoverOff()");
    }

    public override void HoverOff( XRBaseInteractor interactor ) {
        base.HoverOff(interactor);
    }


    IEnumerator Follow( XRBaseInteractor interactor ) {


        XRGrabInteractable grabInteractable = baseInteractable.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null) {

            Transform controller = interactor.attachTransform;
            Transform headset = PTUtilities.instance.headProxy;
            //distance = (transform.position - controller.position).magnitude;
            controller.position += (controller.forward * distance);

            while (true) {
                //transform.position = controller.position + (controller.forward * distance);
                transform.rotation = Quaternion.LookRotation(transform.position - headset.position, Vector3.up);
                
                yield return null;
            }

        }        

    }


}
