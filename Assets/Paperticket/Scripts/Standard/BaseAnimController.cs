using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket { 
    public class BaseAnimController : MonoBehaviour {

        [Header("BASE")]
        [SerializeField] protected Animator animator = null;
        [SerializeField] protected bool debugging = false;
        [SerializeField] [Tooltip("NOTE: this should be read only")] protected int currentIndex = 0;

        public virtual void Awake() {
            animator = animator ?? GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
            if (!animator) {
                Debug.LogError("[BaseAnimController] ERROR -> No animator found! Disabling animation controller...");
                gameObject.SetActive(false);
            }
        }
        public virtual void OnEnable() {
            SetAnimation(currentIndex);
        }



        #region PUBLIC FUNCTIONS

        public virtual void SetAnimation( int animationIndex ) {
            animator.SetInteger("animationIndex", animationIndex);
            currentIndex = animationIndex;
        }

        #endregion
                       



    #if UNITY_EDITOR
        public virtual void OnDrawGizmosSelected() {
            if (debugging && Application.isPlaying && animator.GetInteger("animationIndex") != currentIndex) {
                SetAnimation(currentIndex);
            }
        }
    #endif


    }
}
