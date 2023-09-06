using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace Paperticket {

    public class VideoController : MonoBehaviour {



        [Header("REFERENCES")]

        //Grab the video player and audio source
        public VideoPlayer videoPlayer = null;
        //[SerializeField] AssetBundles requiredBundle = 0;
        private string completePath;


        [Header("VIDEO CONTROLS")]
        [Space(10)]
        [SerializeField] string currentVideoName ="";
        [SerializeField] bool autoLoad = true;
        [SerializeField] bool autoPlay = true;
        public bool AutoPlay {
            get { return autoPlay; }
            set { autoPlay = value; }
        }
        [SerializeField] bool skipFramesOnDrop = true;


        [Header("PRE-START CONTROLS")]
        [Space(10)]
       // [SerializeField] bool moveToHead = false;
       // [SerializeField] bool rotateToHead = false;
        [SerializeField] Vector3 initialRotation = Vector3.zero;
        [Space(10)]
        [SerializeField] bool rePrepareOnError = true;
        [SerializeField] [Min(0)] float rePrepareDelay = 0.5f;


        [Header("AUDIO CONTROLS")]
        [Space(10)]
        [SerializeField] bool externalAudio = false;
        [SerializeField] AudioSource externalAudioSource = null;


        [Header("FINISH CONTROLS")]
        [Space(10)]
        [SerializeField] float earlyFinishDuration = 0.5f;
        [SerializeField] bool pauseVideoOnFinish = true;
        [Space(10)]
        [SerializeField] bool _UseFinishEvents = false;
        [SerializeField] UnityEvent2[] _FinishEvents = null;
        
        int finishIndexNo = 0;


        [Header("READ ONLY")]
        [Space(5)]

        [SerializeField] string datapath = "";
        [Space(10)]
        public bool videoLoaded = false;
        public bool videoStarted = false;
        public bool playingVideo = false;
        public bool videoEnded = false;


        [Header("DEBUGGING")]
        [Space(10)]
        [SerializeField] bool debugging = true;
        [Space(10)]
        public float currentVideoTime = 0;                // in seconds
        public float currentSpeed = 1;
        [SerializeField] private long currentFrames = 0;
        [SerializeField] private long endFrames = 0;


        #region Enable and Update

        private void Awake() {
            videoPlayer.loopPointReached += VideoLooped;
        }

        // Use this for initialization
        void OnEnable() {


            //if (!debugging) debugging = true;

            // Set the video sphere position and rotation (if applicable)
          //  if (moveToHead) transform.position = PTUtilities.instance.HeadsetPosition();
          //  if (rotateToHead) transform.rotation = PTUtilities.instance.HeadsetRotation() * Quaternion.Euler(initialRotation);
          //  else 
          //      transform.rotation = Quaternion.Euler(initialRotation);

            //Make sure the video doesn't skip frames (to keep the audio in sync)
            videoPlayer.skipOnDrop = skipFramesOnDrop;

            videoPlayer.source = VideoSource.VideoClip;

            //     if (autoLoad) StartCoroutine(LoadVideoClipFromBundle(currentVideoName));

            

        }


        // Update is called once per frame
        void Update() {

            if (videoPlayer.isPrepared) {

                // Shows if the video is currently playing
                playingVideo = videoPlayer.isPlaying;

                // The current time of the video playthrough
                currentVideoTime = (float)videoPlayer.time;
                currentFrames = videoPlayer.frame;
                currentSpeed = videoPlayer.playbackSpeed;

            }            

            if (!videoPlayer.isLooping && videoStarted && !videoEnded && (videoPlayer.frame >= endFrames)) {
                FinishVideo();
            }

        }

        #endregion


        #region PUBLIC VIDEO FUNCTIONS


        //-----------------------------------------------------------
        // PUBLIC VIDEO CONTROLS
        //-----------------------------------------------------------

        // Reset the video controller and get a new video lined up
        public void SetNextVideo( string newVideoName ) {

            if (debugging) Debug.Log("[VideoController] Switching the video to " + newVideoName);

            // Stop the video
            videoPlayer.Stop();
            if (externalAudio) externalAudioSource.Stop();

            // Set the bools to false
            videoStarted = false;
            videoLoaded = false;
            videoEnded = false;
            currentVideoTime = 0;

            // Set position and rotation if applicable
          //  if (moveToHead) transform.position = PTUtilities.instance.HeadsetPosition();
          //  if (rotateToHead) transform.rotation = PTUtilities.instance.HeadsetRotation() * Quaternion.Euler(initialRotation);
          //  else 
                transform.rotation = Quaternion.Euler(initialRotation);

            // Load the prepare the next video
            StartCoroutine(LoadVideoClipFromBundle(newVideoName));

        }
        
        // Plays the video if it's not already playing 
        public void PlayVideo() {

            // Cancel if there is no video asssigned yet
            if (!videoLoaded || videoEnded) {
                if (debugging) Debug.LogWarning("[VideoController] Cannot play video! The video has ended, or no video has been loaded yet.");
                return;
            }

            // If the video hasn't started yet...
            if (!videoStarted) {
                videoStarted = true;

                // Play the video
                videoPlayer.Play();
                if (debugging) Debug.Log("[VideoController] Playing the video");

                // If it exists, set audio time to 0 and play the audio
                if (externalAudio) {
                    externalAudioSource.time = 0f;
                    externalAudioSource.Play();
                }

            // If the video hasn't ended yet...
            } else {

                // If we aren't playing, play the video
                if (!playingVideo) {

                    videoPlayer.Play();
                    if (debugging) Debug.Log("[VideoController] Playing the video again (was the video interrupted somehow?)");                

                    // Force the external audio time to match the video time
                    if (externalAudio) {
                        externalAudioSource.time = (float)videoPlayer.time;
                        // Also, play the audio If not playing
                        externalAudioSource.Play();
                    }

                }
            }
        }

        // Set the time of the video (only seems to work when playing)
        public void SetTime( float time ) {
            if (debugging) Debug.Log("[VideoController] Atempting to set time to " + time + " seconds...");

            // Check if the provided time is viable
            if (time < (endFrames / videoPlayer.frameRate)) {

                videoPlayer.time = time;
                if (externalAudio) externalAudioSource.time = time;

            } else Debug.LogError("[VideoController] ERROR -> Cannot SetTime to value greater than video duration!");

        }

        // Add or subtract a certain amount of time from the video
        public void AddTimeStep( float timeStep ) {
            if (debugging) Debug.Log("[VideoController] Atempting to step forward " + timeStep + " seconds...");

            // Set up an end buffer of 5 seconds
            float totalTime = endFrames - 5f;

            if (videoPlayer.time < (totalTime - timeStep)) {

                float newTime = Mathf.Clamp((float)videoPlayer.time + timeStep, 0f, totalTime - 10f);

                videoPlayer.time = newTime;
                if (externalAudio) externalAudioSource.time = newTime;
            }
        }

        // Pause the video if it is playing
        public void PauseVideo() {

            if (!playingVideo) return;

            if (debugging) Debug.Log("[VideoController] Pausing the video");
            videoPlayer.Pause();

            if (externalAudio) {
                externalAudioSource.Pause();
            }

        }
        
        // A stop method so that it can be invoked on command
        public void StopVideo() {
            videoPlayer.Stop();
            if (externalAudio) {
                externalAudioSource.Stop();
            }

            videoEnded = true;
        }

        public void SetSpeed( float playbackSpeed ) {
            if (!playingVideo) return;

            //if (debugging) Debug.Log("[VideoController] Setting the video speed to: " + playbackSpeed);

            videoPlayer.playbackSpeed = Mathf.Max(0, playbackSpeed);
            if (externalAudio) externalAudioSource.pitch = Mathf.Max(0, playbackSpeed);

        }



        #endregion


        #region OTHER PUBLIC FUNCTIONS


        public void SetAudioVolume( float volume ) {

            if (debugging) Debug.Log("[VideoController] Setting audio volume");

            if (externalAudio) {
                externalAudioSource.volume = Mathf.Clamp01(volume);
            } else if (videoPlayer.audioOutputMode == VideoAudioOutputMode.AudioSource) {
                videoPlayer.GetTargetAudioSource(0).volume = Mathf.Clamp01(volume);
            } else {
                videoPlayer.SetDirectAudioVolume(0, Mathf.Clamp01(volume));
            }

        }

        public void SetInitialRotation (Vector3 rotation ) {

            initialRotation = rotation;

        }


        public void SyncExternalAudio() {
            if (!externalAudio) {
                if (debugging) Debug.LogWarning("[VideoController] Not using external audio, ignoring Sync External Audio");
                return;
            }
            externalAudioSource.time = (float)videoPlayer.time;
        }

        #endregion


        #region INTERNAL FUNCTIONS

        //-----------------------------------------------------------
        // EVENT HANDLERS
        //-----------------------------------------------------------

        // Let us know the video has finished playing (from event in update)
        void FinishVideo() {
            Debug.Log("Finish");
            if (!videoPlayer.isLooping) {
                videoEnded = true;
                videoStarted = false;

                if (pauseVideoOnFinish) {
                    videoPlayer.Pause();
                    if (externalAudio) externalAudioSource.Pause();
                }

                if (_UseFinishEvents && _FinishEvents.Length > 0 && finishIndexNo < _FinishEvents.Length) {

                    _FinishEvents[finishIndexNo].Invoke();
                    finishIndexNo++;

                }
            } else {
                Invoke("StopVideo", 1f);
            }
        }


        void VideoLooped(VideoPlayer source) {

            if (pauseVideoOnFinish) {
                videoPlayer.Pause();
            }

            if (_UseFinishEvents && _FinishEvents.Length > 0) {

                _FinishEvents[finishIndexNo].Invoke();

                finishIndexNo = (finishIndexNo + 1) % _FinishEvents.Length;

                if(debugging) Debug.Log("[VideoController] Video looped, index = " + finishIndexNo);
            }

        }

        void VideoPlayerErrorReceived( VideoPlayer source, string message ) {
            Debug.LogError("[VideoController] VideoPlayer on '" + source.gameObject.name + "' error received! Error = " + message);
            videoPlayer.errorReceived -= VideoPlayerErrorReceived;

            if (rePrepareOnError) StartCoroutine(RepreparingOnError());
        }

        #endregion


        #region INTERNAL COROUTINES

        // Set the clip of the new video to be played
        [SerializeField] AssetBundle videoBundle;
        IEnumerator LoadVideoClipFromBundle( string clipName ) {
            if (debugging) Debug.Log("[VideoController] Setting the video clip");

            //Let other scripts know the video hasn't started yet
            videoStarted = false;

            // Load the asset bundle from the above path
            //videoBundle = DataUtilities.instance.GetAssetBundle(requiredBundle); //_ExpansionAssetBundle;
            while (videoBundle == null) {
           //     if (debugging) Debug.Log("[VideoController] Attempting to get asset bundle '"+requiredBundle.ToString()+"'");
           //     videoBundle = DataUtilities.instance.GetAssetBundle(requiredBundle); //_ExpansionAssetBundle;
                yield return null;
            }
            if (debugging) Debug.Log("[VideoController] Got the asset bundle '" + videoBundle + "'");
            // Load the video clip from the asset bundle and wait until it's finished
            var assetLoadRequest = videoBundle.LoadAssetAsync<VideoClip>(clipName);
            yield return assetLoadRequest;

            // Treat the video as a VideoClip and give to the video player
            VideoClip clip = assetLoadRequest.asset as VideoClip;
            videoPlayer.clip = clip;
            currentVideoName = clipName;

            if (debugging) Debug.Log("[VideoController] Video clip set to '" + clip.name + "'");


            // Prepare the video, stopping any current preparation attempts in case of error
            if (preppingVideoCo != null) StopCoroutine(preppingVideoCo);
            preppingVideoCo = StartCoroutine(PreparingVideo());

        }

        // Start preparing the video and waits till its done
        Coroutine preppingVideoCo;
        IEnumerator PreparingVideo() {

            if (debugging) Debug.Log("[VideoController] Starting video preparation");

            // Setup error checking
            videoPlayer.errorReceived += VideoPlayerErrorReceived;

            //Prepare next video
            videoPlayer.Prepare();

            //Wait until video is prepared			
            if (debugging) Debug.Log("[VideoController] Preparing video");
            while (!videoPlayer.isPrepared) {
                yield return null;

            }
            if (debugging) Debug.Log("[VideoController] Video prepared!");

            videoLoaded = true;

            endFrames = (long)(videoPlayer.frameCount - (earlyFinishDuration * videoPlayer.frameRate)); // 15

            if (autoPlay) {
                if (debugging) Debug.Log("[VideoController] Autoplay is on, playing video!");
                PlayVideo();
            }
        }


        IEnumerator RepreparingOnError() {
            yield return new WaitForSeconds(rePrepareDelay);
            Debug.LogWarning("[VideoController] Attempting to prepare video again after receiving error...");
            if (preppingVideoCo != null) StopCoroutine(preppingVideoCo);
            preppingVideoCo = StartCoroutine(PreparingVideo());
        }


        #endregion



    }

}




