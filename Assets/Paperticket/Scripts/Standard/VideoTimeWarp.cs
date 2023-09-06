using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {
    public class VideoTimeWarp : MonoBehaviour {
        [Serializable] enum WarpMode { Limit, Advance }
        
        [Header("REFERENCES")]
        [SerializeField] VideoController videoController = null;

        [Header("CONTROLS")]
        [Space(10)]
        [SerializeField] WarpMode videoMode = WarpMode.Limit;
        [SerializeField] [Min(0)] float targetTime = 0;

        [Header("LIMIT MODE")]
        [Space(10)]
        [SerializeField] AnimationCurve limitCurve = AnimationCurve.Linear(0,0,1,1);

        [Header("ADVANCE MODE")]
        [Space(10)]
        [SerializeField] [Min(0)] float advanceDuration = 1;
        [SerializeField] [Min(0)] float maxPlaybackSpeed = 8;

        Coroutine timeWarpCo = null;

        void OnEnable() {

            if (videoController.currentVideoTime < targetTime) {

                if (timeWarpCo != null) StopCoroutine(timeWarpCo);

                switch (videoMode) {
                    case WarpMode.Limit: StartCoroutine(Limiting());
                        break;
                    case WarpMode.Advance: StartCoroutine(Advancing());
                        break;
                    default:
                        break;
                }

            }
        }


        IEnumerator Limiting() {            
            float currentTime = videoController.currentVideoTime;
            float startTime = currentTime;                       

            while (currentTime < targetTime) {
                yield return null;
                currentTime = videoController.currentVideoTime;

                videoController.SetSpeed(limitCurve.Evaluate( (targetTime - currentTime) / (targetTime - startTime)) );

            }
        }



        IEnumerator Advancing() {
            float currentTime = videoController.currentVideoTime;

            while (currentTime < targetTime - 1) { 
                yield return null;
                currentTime = videoController.currentVideoTime;

                videoController.SetSpeed(Mathf.Min(maxPlaybackSpeed, (targetTime - currentTime) / advanceDuration));                

            }

            videoController.SetSpeed(1);
        }

    }
}