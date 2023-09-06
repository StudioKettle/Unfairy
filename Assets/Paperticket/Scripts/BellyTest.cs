using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BellyTest : MonoBehaviour {

    [SerializeField] Transform target;
    [SerializeField] float duration;
    [SerializeField] float pauseDuration;
    [SerializeField] Vector3 startRot;        
    [SerializeField] Vector3 endRot;

    Coroutine rotatingCo;

    // Start is called before the first frame update
    void Start()
    {
        // Start the coroutine
        rotatingCo = StartCoroutine(Rotating());
    }


    IEnumerator Rotating() {

        float t = 0;
        Quaternion startRotQ = Quaternion.Euler(startRot);
        Quaternion endRotQ = Quaternion.Euler(endRot);

        // This runs until this object is destroyed / disabled
        while (true) {
           
            // Reset time, rotate from StartRot to EndRot
            t = 0;
            while (t <= 1) {
                t += Time.deltaTime / duration;
                target.rotation = Quaternion.Lerp(startRotQ, endRotQ, Mathf.SmoothStep(0,1,t));
                yield return null;
            }

            target.rotation = endRotQ;

            // Pause for a sec
            yield return new WaitForSeconds(pauseDuration);

            // Reset time, rotate from EndRot to StartRot
            t = 0;
            while (t <= 1) {
                t += Time.deltaTime / duration;
                target.rotation = Quaternion.Lerp(endRotQ, startRotQ, Mathf.SmoothStep(0, 1, t));
                yield return null;
            }

            target.rotation = startRotQ;

            // Pause for a sec
            yield return new WaitForSeconds(pauseDuration);

        }
    }
}
