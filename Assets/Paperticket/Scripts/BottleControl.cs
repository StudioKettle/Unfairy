using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Events;
using TMPro;
using Paperticket;

public class BottleControl : MonoBehaviour {

    public enum Panning { Center, Left, Right }
    [SerializeField] bool debugging = false;
    [Space(10)]
    [Header("REFERENCES")]
    [SerializeField] VideoController videoController = null;
    [SerializeField] VideoClip bottleVideo = null;
    [Space(5)]
    [SerializeField] AK.Wwise.RTPC rTPC_EarpiecePan = null;
    [Space(10)]
    [Header("CONTROLS")]
    [Space(5)]
    [SerializeField] UnityEvent2 earpieceEvent = null;
    [SerializeField] float earGap = 0.8f;
    [SerializeField] float earMod = 0.075f;
    [Space(5)]
    [SerializeField] UnityEvent2 bazAudio4aEvent = null;
    [SerializeField] float baz4aLength = 9.7f;
    [SerializeField] float baz4aGap = 1.5f;
    [SerializeField] float baz4aMod = 0.09f;
    [Space(5)]
    [SerializeField] UnityEvent2 bazAudio4bEvent = null;
    [SerializeField] float baz4bLength = 2.85f;
    [SerializeField] float baz4bGap = 1.5f;
    [SerializeField] float baz4bMod = 0.19f;
    [Space(5)]
    [SerializeField] UnityEvent2 bazAudio4cEvent = null;
    [SerializeField] float baz4cLength = 10.7f;
    [SerializeField] float baz4cGap = 2f;
    [SerializeField] float baz4cMod = 0.19f;
    [Space(5)]
    [SerializeField] UnityEvent2 bazAudio4dEvent = null;
    [SerializeField] float baz4dLength = 11.68f;
    [SerializeField] float baz4dGap = 2.5f;
    [SerializeField] float baz4dMod = 0.38f;
    [Space(5)]
    [SerializeField] UnityEvent2 bazAudio5Event = null;

    [Space(10)]
    [Header("READ ONLY")]
    [Space(5)]
    [SerializeField] float videoOffset = 0;
    [SerializeField] bool startCounting = false;
    [SerializeField] int counter = 0;


    [Space(10)]
    [Header("READ ONLY")]
    [SerializeField] bool videoReady = false;

    void Start() {
        StartCoroutine(StartVideo());
    }

    IEnumerator StartVideo() {
        yield return new WaitForSeconds(0.5f);

        videoController.OnVideoStarted += VideoReady;
        videoController.SetNextVideo(bottleVideo);

        yield return new WaitUntil(()=> videoReady);
        videoController.OnVideoStarted -= VideoReady;

        // Create a callback flag to enable retrieving current time of audio
        //AkCallbackType enablePlayPosition = AkCallbackType.AK_EnableGetSourcePlayPosition;
        // NOTE -> Doing this in inspector, shouldn't be, but we are

        // Start the audio 
        //audioPlayingId = bottleAudio.Post(gameObject, callbackFlags, null);
                
        //yield return new WaitForSeconds(0.25f);

        //int uPosition = 0;
        //AkSoundEngine.GetSourcePlayPosition(audioPlayingId, out uPosition);
        //float time = uPosition / 1000;
        //Debug.Log("Time = " + time);
        //videoController.SetTime(time);
    }

    void VideoReady() {
        videoReady = true;
    }


    public void StartBazAudio4(Transform earpiece) {

        // Check which side to pan audio, then destroy earpiece
        if (PTUtilities.instance.headProxy.InverseTransformPoint(earpiece.position).x < 0) {
            SetEarpiecePan(Panning.Left);
        } else {
            SetEarpiecePan(Panning.Right);
        }
        earpiece.gameObject.DestroyMe();

        // Start waiting for voice lines
        StartCoroutine(AdjustingBazAudio());
    }

    
    //void StartCheckingUnlock() {
    //    StartCoroutine(CheckingUnlock());
    //}

    //IEnumerator CheckingUnlock() {
    //    float timeRemaining = baz4dLength;
    //    while (timeRemaining > 0) {

    //    }

    //}


    //public void AddToVideoLoopCounter() {
    //    if (!startCounting) return;
    //    counter += 1;
    //    //if (debugging) Debug.Log("[BottleControl] AddToVideoLoopCounter incremented, counter = " + counter);
    //}





    IEnumerator AdjustingBazAudio() {

        // VideoOffset to work out how fast / slow we are
        videoOffset = videoController.currentVideoTime;

        string debugString = "";
        if (debugging) debugString += "[BottleControl] Start of AdjustingBazAudio, video time = " + videoController.currentVideoTime + System.Environment.NewLine;


        // If x< 1.3 || x > 4.9, speed up by up to 3.6 sec

        // if (x < 1.3) x += 7.2(makes 4.9 - 8.5)
        // x = LinearRemap(4.9, 8.5, 0, 1)
        //  else slow down by up to 3.6 sec
        // x = LinearRemap(1.3, 4.9, -1, 0)

        // Set videoOffset to how far from 4.9 we are in either direction (-1 to 1)? <- nah do real numbers i.e. -3.6 to 3.6
        if (videoOffset < 1.3f || videoOffset > 4.9) {
            videoOffset += (videoOffset < 1.3f) ? 7.2f : 0;
            //videoOffset.LinearRemap(4.9f, 8.5f, 0, 1);
            if (debugging) debugString += "< 1.3 or > 4.9, I.e. we need to speed up ";
        } else {
            //videoOffset.LinearRemap(1.3f, 4.9f, -1, 0);
            if (debugging) debugString += "Between 1.3 and 4.9, I.e. we need to slow down ";
        }
        videoOffset -= 4.9f;

        if (debugging) {
            debugString += "by " + videoOffset;
            Debug.Log(debugString);
        }



        if (earpieceEvent != null) earpieceEvent.Invoke();
        if (debugging) Debug.Log("[BottleControl] Earpiece go in");
        yield return new WaitForSeconds(earGap + (earMod * videoOffset));

        if (debugging) Debug.Log("[BottleControl] Earpiece connect");
        if (earpieceEvent != null) earpieceEvent.Invoke();
        yield return new WaitForSeconds(earGap + (earMod * videoOffset));


        if (debugging) Debug.Log("[BottleControl] Sending BazAudio4aEvent then waiting "+baz4aGap+" + an extra " + (baz4aMod * videoOffset));
        if (bazAudio4aEvent != null) bazAudio4aEvent.Invoke();
        yield return new WaitForSeconds(baz4aLength + baz4aGap + (baz4aMod * videoOffset));

        if (debugging) Debug.Log("[BottleControl] Sending BazAudio4bEvent then waiting " + baz4bGap + " + an extra " + (baz4bMod * videoOffset));
        if (bazAudio4bEvent != null) bazAudio4bEvent.Invoke();
        yield return new WaitForSeconds(baz4bLength + baz4bGap + (baz4bMod * videoOffset));

        if (debugging) Debug.Log("[BottleControl] Sending BazAudio4cEvent then waiting " + baz4cGap + " + an extra " + (baz4cMod * videoOffset));
        if (bazAudio4cEvent != null) bazAudio4cEvent.Invoke();
        yield return new WaitForSeconds(baz4cLength + baz4cGap + (baz4cMod * videoOffset));

        if (debugging) Debug.Log("[BottleControl] Sending BazAudio4dEvent then waiting " + baz4dGap + " + an extra " + (baz4dMod * videoOffset));
        if (bazAudio4dEvent != null) bazAudio4dEvent.Invoke();
        yield return new WaitForSeconds(baz4dLength + baz4dGap + (baz4dMod * videoOffset));


        if (bazAudio5Event != null) bazAudio5Event.Invoke();

    }



    //IEnumerator DelayingBazAudio() {


    //    //if (delay < 1.9)
    //    //    wait 0.95, connect, wait 0.95
    //    //else if (delay < 5.3)
    //    //    wait 0.25, connect, wait 0.25
    //    //else
    //    //    wait(9.1 - delay) / 2, connect, wait(9.1 - delay) / 2

    //    //baz4 + start count
    //    //end count + unlock
    //    //wait 3.3
    //    //wait 2



    //    // Set delay based on where in the video loop we are
    //    videoOffset = videoController.currentVideoTime.Clamp(0, 7.19f);

    //    if (debugging) Debug.Log("[BottleControl] Start of DelayingBazAudio, loop time = " + videoController.currentVideoTime + ", delay = " + videoOffset);

    //    earpieceConnect.Post(PTUtilities.instance.gameObject);

    //    if (videoOffset < 1.9f) {
    //        yield return new WaitForSeconds(0.95f);
    //        earpieceConnect.Post(PTUtilities.instance.gameObject);
    //        if (debugging) Debug.Log("[BottleControl] Delay < 1.9, loop time = " + videoController.currentVideoTime);
    //        yield return new WaitForSeconds(0.95f);
    //    } else if (videoOffset < 5.3f) {
    //        yield return new WaitForSeconds(0.25f);
    //        earpieceConnect.Post(PTUtilities.instance.gameObject);
    //        if (debugging) Debug.Log("[BottleControl] 1.9 < Delay < 5.3, loop time = " + videoController.currentVideoTime);
    //        yield return new WaitForSeconds(0.25f);
    //    } else {
    //        yield return new WaitForSeconds((9.1f - videoOffset)/2);
    //        earpieceConnect.Post(PTUtilities.instance.gameObject);
    //        if (debugging) Debug.Log("[BottleControl] Delay > 5.3, loop time = " + videoController.currentVideoTime);
    //        yield return new WaitForSeconds((9.1f - videoOffset) / 2);
    //    }

    //    // Play BazAudio4
    //    //var baz4Time = videoController.currentVideoTime;
    //    if (debugging) Debug.Log("[BottleControl] Sending BazAudio4Event, baz4time = " + videoController.currentVideoTime + ", delay = " + videoOffset);
    //    if (bazAudio4Event != null) bazAudio4Event.Invoke();

    //    // Unlock video after 5 cycles of the video
    //    //startCounting = true;
    //    //yield return new WaitUntil(() => counter == 5);
    //    yield return new WaitForSeconds(39.3f);

    //    if (debugging) Debug.Log("[BottleControl] BazAudio4 finish at loop time = " + videoController.currentVideoTime);

    //    //if (baz4Time + 3.3f > 5.3f) {
    //    //    yield return new WaitUntil(() => counter == 6);
    //    // yield return new WaitForSeconds(baz4Time + 3.3f);
    //    //} 

    //    if (videoController.currentVideoTime > 5.3f) {
    //        if (debugging) Debug.Log("[BottleControl] Unlocking after this loop, loop time = " + videoController.currentVideoTime + " is > 5.3");
    //        yield return new WaitUntil(() => videoController.currentVideoTime < 5.3f);
    //        if (unlockEvent != null) unlockEvent.Invoke();
    //        yield return new WaitForSeconds(videoController.currentVideoTime.LinearRemap(5.3f, 7.2f, 1.5f, 2f));
    //    } else {
    //        if (debugging) Debug.Log("[BottleControl] Unlocking now, loop time = " + videoController.currentVideoTime + " is < 5.3");
    //        if (unlockEvent != null) unlockEvent.Invoke();
    //        yield return new WaitForSeconds(videoController.currentVideoTime.LinearRemap(0, 5.3f, 2f, 1.5f));
    //    }

    //    //startCounting = false;
    //    //if (debugging) Debug.Log("[BottleControl] Counters finished, loop time = " + videoController.currentVideoTime);

    //    //yield return new WaitForSeconds(baz4Time + 3.3f);
    //    //if (debugging) Debug.Log("[BottleControl] BazAudio4 finish at loop time = " + videoController.currentVideoTime);

    //    if (debugging) Debug.Log("[BottleControl] Unlocking after 5 vid cycles, loop time = " + videoController.currentVideoTime + ", waiting 2 sec"); 
    //    if (unlockEvent != null) unlockEvent.Invoke();
                
    //    // Linear remap (0, 7.2) > (1.5, 3.5)

    //    yield return new WaitForSeconds(videoController.currentVideoTime.LinearRemap(0, 7.2f, 1.5f, 2.5f));
    //    //yield return new WaitForSeconds(2f);

    //    // Play BazAudio5 at least 2 seconds after unlock
    //    //yield return new WaitForSeconds(1f);
    //    //if (debugging) Debug.Log("[BottleControl] Sending BazAudio5Event after 2 seconds, loop time = " + videoController.currentVideoTime);
    //    if (debugging) Debug.Log("[BottleControl] BazAudio5 go");
    //    if (bazAudio5Event != null) bazAudio5Event.Invoke();

    //}




    // this one too
    //IEnumerator DelayingBazAudio() {

    //    // Set delay based on where in the video loop we are
    //    delay = videoController.currentVideoTime.Clamp(0, 7.19f);

    //    if (debugging) Debug.Log("[BottleControl] Start of DelayingBazAudio, loop time = " + videoController.currentVideoTime + ", delay = " + delay);

    //    earpieceConnect.Post(PTUtilities.instance.gameObject);

    //    //if (videoController.currentVideoTime <= 3.4f) {
    //    //    if (debugging) Debug.Log("[BottleControl] Less than 3.4 seconds :. Waiting normally");
    //    //    yield return new WaitUntil(() => videoController.currentVideoTime >= 3.4f);
    //    //} else {
    //    //    counter -= 1;
    //    //    delay = (7.2f - videoController.currentVideoTime + 3.4f) / 2;
    //    //    if (debugging) Debug.Log("[BottleControl] Over 3.4 seconds :. Adding allowance for another loop to compensate");
    //    //    yield return new WaitForSeconds(delay);
    //    //}

    //    // Linear remap (0, 7.2) > (0, 1)
    //    var connectionDelay = (7.19f - delay) + delay.LinearRemap(0, 7.19f, 0, 1.69f);
    //    yield return new WaitForSeconds(connectionDelay / 2);
    //    earpieceConnect.Post(PTUtilities.instance.gameObject);
    //    if (debugging) Debug.Log("[BottleControl] Earpiece connect audio, loop time = " + videoController.currentVideoTime + ", connectionDelay = " + connectionDelay);
    //    yield return new WaitForSeconds(connectionDelay / 2);


    //    // Play BazAudio4
    //    if (debugging) Debug.Log("[BottleControl] Sending BazAudio4Event, loop time = " + videoController.currentVideoTime + ", delay = " + delay);
    //    var baz4Time = videoController.currentVideoTime;
    //    if (bazAudio4Event != null) bazAudio4Event.Invoke();

    //    // Unlock video after 5 cycles of the video
    //    startCounting = true;
    //    yield return new WaitUntil(() => counter == 5);
    //    if (debugging) Debug.Log("[BottleControl] Counters finished, delay = " + delay);
    //    startCounting = false;

    //    if (debugging) Debug.Log("[BottleControl] Unlocking after 5 vid cycles");
    //    if (unlockEvent != null) unlockEvent.Invoke();

    //    yield return new WaitForSeconds(baz4Time + 3.3f);
    //    // Linear remap (0, 7.2) > (1.5, 3.5)
    //    yield return new WaitForSeconds(delay.LinearRemap(0, 7.19f, 2.5f, 1.5f));
    //    //yield return new WaitForSeconds(2f);

    //    // Play BazAudio5 at least 2 seconds after unlock
    //    //yield return new WaitForSeconds(1f);
    //    //if (debugging) Debug.Log("[BottleControl] Sending BazAudio5Event after 2 seconds, loop time = " + videoController.currentVideoTime);
    //    if (bazAudio5Event != null) bazAudio5Event.Invoke();

    //}


    // this one
    //IEnumerator WaitingForBazAudio4() {

    //    if (debugging) Debug.Log("[BottleControl] Start of WaitingForBazAudio4, loop time = " + videoController.currentVideoTime);

    //    // Set delay based on where in the video loop we are
    //    if (videoController.currentVideoTime <= 3.4f) {
    //        if (debugging) Debug.Log("[BottleControl] Less than 3.4 seconds :. Waiting.");
    //        yield return new WaitUntil(() => videoController.currentVideoTime >= 3.4f);
    //    } else {
    //        delay = ((7.2f - videoController.currentVideoTime + 3.4f) / 2);
    //        if (debugging) Debug.Log("[BottleControl] Over 3.4 seconds :. Waiting " + delay + " seconds so we're back at 3.4 seconds in loop");
    //        yield return new WaitForSeconds(delay);
    //    }

    //    // Play BazAudio4
    //    if (debugging) Debug.Log("[BottleControl] Sending BazAudio4Event, loop time = " + videoController.currentVideoTime);
    //    if (bazAudio4Event != null) bazAudio4Event.Invoke();

    //    // Unlock the video after BazAudio4 is finished
    //    yield return new WaitForSeconds(39.4f + delay);
    //    yield return new WaitUntil(() => videoController.currentVideoTime < 3.6f);
    //    if (debugging) Debug.Log("[BottleControl] Unlocking after " + (39.4f + delay) + " seconds, loop time = " + videoController.currentVideoTime);
    //    if (unlockEvent != null) unlockEvent.Invoke();

    //    // Play BazAudio5 at least 2 seconds after unlock
    //    yield return new WaitForSeconds(2f);
    //    if (debugging) Debug.Log("[BottleControl] Sending BazAudio5Event after 2 seconds, loop time = " + videoController.currentVideoTime);
    //    if (bazAudio5Event != null) bazAudio5Event.Invoke();

    //}


    //IEnumerator WaitingForUnlock() {

    //yield return new WaitForSeconds(39.4f + delay);

    //// Send the event in inspector
    //if (unlockEvent != null) unlockEvent.Invoke();

    //yield return new WaitForSeconds(delay.Min(2f));

    //// Send the event in inspector
    //if (bazAudio5Event != null) bazAudio5Event.Invoke();

    //StartCoroutine(WaitingForBazAudio5());
    //}

    //IEnumerator WaitingForBazAudio5() {

    //    if (delay > 2) {
    //        yield return new WaitForSeconds(Mathf.Min(timeToDelayBaz5,2));
    //    } else {
    //        yield return new WaitForSeconds(2 - delay);
    //    }


    //    if (bazAudio5Event != null) bazAudio5Event.Invoke();
    //}






    public void SetEarpiecePan(Panning panning) {

        switch (panning) {
            case Panning.Center:
                rTPC_EarpiecePan.SetGlobalValue(0);
                break;
            case Panning.Left:
                rTPC_EarpiecePan.SetGlobalValue(-1);
                break;
            case Panning.Right:
                rTPC_EarpiecePan.SetGlobalValue(1);
                break;
            default:
                Debug.LogError("[BottleControl] Could not SetEapiecePan! Bad Panning value set, ignoring.");
                return;
        }
        
        if (debugging) Debug.Log("[BottleControl] SetEapiecePan to " + panning.ToString());
    }
}
