using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Handanimation : MonoBehaviour
{
    [Tooltip("Don't forget to assign the prefiew animator!")]
    public Animator anim;

    [Tooltip("Set this integer to preview the animations, ranges from 0 - 14 for a total of 15 animations!")]
    public int animationindex;
  
    void Update()
    {
            AnimationIndex();
            SetAnimation();
    }

    private int AnimationIndex()
    {
        return Mathf.Clamp(animationindex,0,14);
    }

    private void SetAnimation()
    {
        anim.SetInteger("animationIndex", AnimationIndex());
    }
}
