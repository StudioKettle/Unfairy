using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Paperticket {
    public enum HandPose { None, Idle, Fist, Grab_big, Grab_small, Point, Spread, Thumbs_up, Ok, Peace, Rock, Hold_gun, Fire_gun, Hold_sword, Cast_spell, Dragonball};

    public class HandAnimController : MonoBehaviour {
        

        [Header("REFERENCES")]
        [SerializeField] Animator animator;

        [Header("CONTROLS")]
        [Space(10)]
        [SerializeField] HandPose defaultPose;
        [SerializeField] HandPose hoverPose;
        
        [Header("READ ONLY")]
        [Space(10)]
        [SerializeField] HandPose handPose;

        XRDirectInteractor interactor;

        #region Setup

        void Awake() {
            animator = animator ?? GetComponent<Animator>();
            if (animator == null) {
                Debug.LogError("[HandAnimController] ERROR -> No animator found! Disabling hand...");
                gameObject.SetActive(false);
            }

            interactor = GetComponentInParent<XRDirectInteractor>(true);
            if (interactor == null) {
                Debug.LogError("[HandAnimController] ERROR -> No interactor found! Disabling hand...");
                gameObject.SetActive(false);
            }

            interactor.hoverEntered.AddListener(Hovered);
            interactor.hoverExited.AddListener(Unhovered);
            interactor.selectEntered.AddListener(Selected);
            interactor.selectExited.AddListener(Unselected);

        }
        void OnEnable() {
            SetHandPose(defaultPose);    
        }

        #endregion

        void Hovered(HoverEnterEventArgs args) {
            if (interactor.hasSelection) return;

            var interactable = args.interactableObject.transform.gameObject;

            // If XRHandedGrabInteractable, grab its hover pose
            if (interactable.HasComponent<XRHandedGrabInteractable>()) {
                if (interactable.GetComponent<XRHandedGrabInteractable>().hoverPose != HandPose.None) {
                    SetHandPose(interactable.GetComponent<XRHandedGrabInteractable>().hoverPose);
                    return;
                }

            } else if (interactable.HasComponent<XRExtendedInteractable>()) {
                if (interactable.GetComponent<XRExtendedInteractable>().hoverPose != HandPose.None) {
                    SetHandPose(interactable.GetComponent<XRExtendedInteractable>().hoverPose);
                    return;
                }
            } 

            SetHandPose(hoverPose);
            
        }

        void Unhovered(HoverExitEventArgs args) {
            if (!interactor.hasHover && !interactor.hasSelection) {
                SetHandPose(defaultPose);
            }
        }

        void Selected(SelectEnterEventArgs args) {
            var interactable = args.interactableObject.transform.gameObject;

            if (interactable.HasComponent<XRHandedGrabInteractable>()) {
                SetHandPose(interactable.GetComponent<XRHandedGrabInteractable>().animationPose);

            } else if (interactable.HasComponent<XRExtendedInteractable>()) {
                SetHandPose(interactable.GetComponent<XRExtendedInteractable>().animationPose);

            }
        }

        void Unselected(SelectExitEventArgs args) {
            if (!interactor.hasSelection) {
                if (interactor.hasHover) {
                    SetHandPose(hoverPose);
                    return;
                }
                SetHandPose(defaultPose);
            }
        }



        #region PUBLIC FUNCTIONS

        public void SetHandPose (HandPose newHandPose) {
            if (newHandPose == HandPose.None) return;
            handPose = newHandPose;

            animator.SetInteger("animationIndex", (int)newHandPose);

            //Debug.Log("HandPose = " + newHandPose.ToString() + ", index = " + (int)newHandPose);
        }


        public void SetHandLayer(LayerMask layer) {
            gameObject.SetLayer(layer);
            for (int i = 0; i < transform.childCount; i++) {
                transform.GetChild(i).gameObject.SetLayer(layer);
            }
        }


        #endregion






//#if UNITY_EDITOR
//        void OnDrawGizmosSelected() {
//            if (Application.isPlaying && handPose != pose) {
//                SetHandPose(pose);
//            }
//        }
//#endif

    }
}