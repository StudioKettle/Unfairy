using Paperticket;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket {

    public class ExitMenu : MonoBehaviour {


        [SerializeField] VideoController videoController = null;

        [Header("CONTROLS")]
        [Space(5)]
        [SerializeField] bool debugging = false;
        [Space(5)]
        [SerializeField] bool activateOnButton = true;
        [Space(15)]
        [SerializeField] UnityEvent2 OnEvent = null;
        [SerializeField] UnityEvent2 OffEvent = null;

        [Header("LIVE VARIABLES")]
        [Space(15)]
        [SerializeField] bool locked = true;
        [Space(5)]
        [SerializeField] bool controllerBeamState = false;
        [SerializeField] bool videoPlayingState = false;



        public bool Locked {
            get { return locked; }
            set { locked = value; 
                if (debugging) Debug.Log("[ExitMenu] Exit Menu " + (locked?"locked":"unlocked"));
                if (!locked) CheckFocus();
            }
        }
        bool menuActive = false;
        bool lastMenuState = false;


        /// <summary>
        /// NOTE: Re-enable these vvvvv when Oculus is installed
        /// </summary>

        //void OnEnable() {
        //    OVRManager.InputFocusLost += FocusLost;
        //    OVRManager.InputFocusAcquired += FocusAcquired;
        //}
        //void OnDisable() {
        //    OVRManager.InputFocusLost -= FocusLost;
        //    OVRManager.InputFocusAcquired -= FocusAcquired;
        //}


        void Update() {

            if (activateOnButton) {
                if (PTUtilities.instance.ControllerMenuButton && !lastMenuState) {
                    // NOTE: Re-enable this vvvvv when Oculus is installed
                    //if (!OVRManager.hasInputFocus) return;

                    if (locked) {
                        if (debugging) Debug.Log("[ExitMenu] Exit Menu is locked, ignoring menu button.");
                        return;
                    }

                    if (!menuActive) ActivateMenu();
                    else DeactivateMenu();
                }

                lastMenuState = PTUtilities.instance.ControllerMenuButton;
            }
        }


        public void ActivateMenu() {
            if (debugging) Debug.Log("[ExitMenu] Exit Menu activated! Pausing experience.");

            controllerBeamState = PTUtilities.instance.ControllerBeamActive;
            PTUtilities.instance.ControllerBeamActive = true;


            if (videoController == null) Debug.LogWarning("[ExitMenu] WARNING -> No VideoController set! Cannot get VideoPlayingState.");
            else {
                videoPlayingState = videoController.playingVideo;
                if (videoPlayingState) videoController.PauseVideo();
            }

            // Turn this on after setting AudioSource.ignoreListenerPause to true on all menu sources
            //AudioListener.pause = true;


            if (OnEvent != null) OnEvent.Invoke();

            menuActive = true;
        }

        public void DeactivateMenu() {
            if (debugging) Debug.Log("[ExitMenu] Exit Menu deactivated! Resuming experience.");

            if (OffEvent != null) OffEvent.Invoke();


            if (videoController == null) Debug.LogWarning("[ExitMenu] WARNING -> No VideoController set! Cannot set VideoPlayingState.");
            else if (videoPlayingState) videoController.PlayVideo();


            // Turn this on after setting AudioSource.ignoreListenerPause to true on all menu sources
            //AudioListener.pause = false;


            PTUtilities.instance.ControllerBeamActive = controllerBeamState;

            menuActive = false;
        }



        void FocusAcquired() {
            if (locked) return;

            if (menuActive) {
                if (activateOnButton) PTUtilities.instance.ControllerBeamActive = true;
                else DeactivateMenu();
            }
        }

        void FocusLost() {
            if (locked) return;

            if (!menuActive) {
                ActivateMenu();
            }            
            PTUtilities.instance.ControllerBeamActive = false;
        }


        /// <summary>
        /// NOTE: Re-enable this vvvvv when Oculus is installed
        /// </summary>
        void CheckFocus() {
            //if (!OVRManager.hasInputFocus) {
            //    FocusLost();
            //}
        }

    }
}
