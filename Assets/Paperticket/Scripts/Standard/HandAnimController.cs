using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {
    public class HandAnimController : MonoBehaviour {
        public enum HandPose { Idle, Fist, Grab_big, Grab_small, Point, Spread, Thumbs_up, Ok, Peace, Rock, Hold_gun, Fire_gun, Hold_sword, Cast_spell, Dragonball };

        [Header("REFERENCES")]
        [SerializeField] Animator animator;

        [Header("CONTROLS")]
        [Space(10)]
        [SerializeField] HandPose pose;
        HandPose handPose;

        void Awake() {
            animator = animator ?? GetComponent<Animator>();
            if (!animator) {
                Debug.LogError("[HandAnimController] ERROR -> No animator found! Disabling hand...");
                gameObject.SetActive(false);
            }
        }
        void OnEnable() {
            SetHandPose(pose);    
        }

        #region PUBLIC FUNCTIONS

        public void SetHandPose (HandPose newHandPose) {
            handPose = pose = newHandPose;
            //AnimationIndex = (int)newHandPose;

            animator.SetInteger("animationIndex", (int)newHandPose);
        }

        #endregion






#if UNITY_EDITOR
        void OnDrawGizmosSelected() {
            if (Application.isPlaying && handPose != pose) {
                SetHandPose(pose);
            }
        }
#endif

    }
}