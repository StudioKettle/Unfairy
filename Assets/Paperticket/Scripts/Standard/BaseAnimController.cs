using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {
    public class BaseAnimController : MonoBehaviour {

        [SerializeField] protected Animator animator = null;
        [SerializeField] protected bool debugging = false;

        [Header("CONTROLS")]
        [Space(10)]
        [SerializeField] protected string indexName = "animIndex";
        [SerializeField] protected bool useTriggerAsFlag = false;
        [SerializeField] protected string flagName = "animChanged";

        [Header("READ ONLY")]
        [Space(10)]
        [SerializeField] [Tooltip("NOTE: this should be read only")] protected int currentIndex = 0;

        Coroutine fadingFloatCo = null;

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
            animator.SetInteger(indexName, animationIndex);
            currentIndex = animationIndex;
            if (useTriggerAsFlag) animator.SetTrigger(flagName);
        }

        public virtual void SetTrigger(string name) {
            animator.SetTrigger(name);            
        }
        public virtual void SetInteger(string name, int value) {
            animator.SetInteger(name, value);
            if (useTriggerAsFlag) animator.SetTrigger(flagName);
        }
        public virtual void SetBool(string name, bool value) {
            animator.SetBool(name, value);
            if (useTriggerAsFlag) animator.SetTrigger(flagName);
        }
        public virtual void SetFloat(string name, float value) {
            animator.SetFloat(name, value);
            if (useTriggerAsFlag) animator.SetTrigger(flagName);
        }


        public virtual void FadeFloat(string name, float value, float duration) {
            if (fadingFloatCo != null) StopCoroutine(fadingFloatCo);
            fadingFloatCo = StartCoroutine(FadingFloat(name, value, duration));
        }
        IEnumerator FadingFloat(string name, float targetValue, float duration) {
            float currentValue = animator.GetFloat(name);

            if (currentValue == targetValue)  yield break;

            if (useTriggerAsFlag) animator.SetTrigger(flagName);

            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration) {
                animator.SetFloat(name, Mathf.Lerp(currentValue, targetValue, t));
                yield return null;
            }
            animator.SetFloat(name, targetValue);

            yield return null;
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
