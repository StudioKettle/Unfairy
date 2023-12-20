using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.XR;
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.Events;

namespace Paperticket {

    public enum TimeScale { Scaled, Unscaled }
    public enum GameEventOption { OnButtonDown, OnButtonUp, OnTriggerPress, OnTriggerUp }
    public enum Hand { Left, Right, Both }

    public class PTUtilities : MonoBehaviour {
        //enum Handedness { Left, Right, Both }

        public static PTUtilities instance = null;

        [Header("REFERENCES")]
        public XROrigin playerRig = null;
        public SpriteRenderer headGfx = null;
        public AudioSource globalAudioSource = null;


        [Header("CONTROLS")]        
        [Space(10)]
        public string masterMixerName = "CareplaysMaster";
        [Space(5)]
        [SerializeField] AnimationCurve shakeTransformCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [Space(5)]
        public AnimationCurve easeInCurve = AnimationCurve.Linear(0,0,1,1);
        public AnimationCurve easeOutCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [Space(5)]
        //[SerializeField] AnimationCurve audioMixerRolloff;
        [SerializeField] bool _Debug = false;



        [Header("LIVE VARIABLES")]
        [Space(10)]
        public Transform headProxy = null;
        [Space(5)]
        public XRController leftController = null;
        public XRController rightController = null;
        [Space(5)]
        XRRayInteractor rightRayInteractor = null;
        XRInteractorLineVisual rightRayVisual = null;
        [Space(5)]
        public bool ControllerTriggerButton = false;
        public bool ControllerPrimaryButton = false;
        public bool ControllerMenuButton = false;
        [Space(5)]
        public Vector3 ControllerVelocity = Vector3.zero;
        public Vector3 ControllerAngularVelocity = Vector3.zero;
        public Vector3 ControllerAcceleration = Vector3.zero;
        public Vector3 ControllerAngularAcceleration = Vector3.zero;
        [Space(5)]
        public AudioMixer audioMaster = null;


        [HideInInspector] public bool SetupComplete = false;        




        #region Public variables


        public Vector3 HeadsetPosition() {
            return headProxy.position;
        }

        public Vector3 HeadsetForward() {
            return headProxy.forward;
        }

        public Quaternion HeadsetRotation() {
            return Quaternion.Euler(new Vector3(0, headProxy.rotation.eulerAngles.y, 0));
        }
        public Vector3 HeadsetRotationAsEuler() {
            return new Vector3(0, headProxy.rotation.eulerAngles.y, 0);
        }

        public bool ControllerBeamActive {
            get {
                return rightRayInteractor.enabled;
            }
            set {
                rightRayInteractor.enabled = value;
                rightRayVisual.enabled = value;
            }
        }

        public LayerMask ControllerBeamLayerMask {
            get {
                return rightRayInteractor.raycastMask;
            }
            set {
                rightRayInteractor.raycastMask = value;
            }
        }

        public float TimeScale {
            get {
                return Time.timeScale;
            }
            set {
                Time.timeScale = value;
                Time.fixedDeltaTime = fixedTimeScaleDefault * Time.timeScale;
                if (_Debug) Debug.Log("[PTUtilities] Time scale set to: " + Time.timeScale + "\n Fixed Deltatime set to: "+Time.fixedDeltaTime);
            }
        }

        float fixedTimeScaleDefault = 0;


        public static event Action OnSetupComplete = delegate { };

        #endregion




        #region Startup 


        void Awake() {

            // Create an instanced version of this script, or destroy it if one already exists
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Destroy(gameObject);
            }
                       
            StartCoroutine(Setup());
            
        }
        IEnumerator Setup() {
            if (_Debug) Debug.Log("[PTUtilities] Starting setup...");

            // Make sure our player rig is defined
            while (playerRig == null) {
                Debug.LogError("[PTUtilities] ERROR -> No Player Rig defined! Something has gone wrong!");
                yield return null;
            }


            // Grab the player's head camera            
            while (headProxy == null) {
                if (_Debug) Debug.Log("[PTUtilities] Looking for Head Proxy object...");
                headProxy = playerRig.Camera.transform;
                yield return null;
                //enabled = false;
            }
            if (_Debug) Debug.Log("[PTUtilities] Head Proxy found!");


            // Grab the right controller
            while (rightController == null) {
                if (_Debug) Debug.Log("[PTUtilities] Looking for Right Controller object...");
                foreach (XRController cont in playerRig.GetComponentsInChildren<XRController>()) {
                    if (cont.controllerNode == XRNode.RightHand) rightController = cont;
                }
                yield return null;
            }
            if (_Debug) Debug.Log("[PTUtilities] Right Controller found!");

            // Grab the left controller
            while (leftController == null) {
                if (_Debug) Debug.Log("[PTUtilities] Looking for Left Controller object...");
                foreach (XRController cont in playerRig.GetComponentsInChildren<XRController>()) {
                    if (cont.controllerNode == XRNode.LeftHand) leftController = cont;
                }
                yield return null;
            }
            if (_Debug) Debug.Log("[PTUtilities] Left Controller found!");

            //// Grab the right controller's XR ray interactor            
            //while (rightRayInteractor == null) {
            //    if (_Debug) Debug.Log("[PTUtilities] Looking for Right Ray Interactor object...");
            //    rightRayInteractor = rightController.GetComponent<XRRayInteractor>();
            //    yield return null;
            //}
            //if (_Debug) Debug.Log("[PTUtilities] Right Ray Interactor found!");

            //// Grab the right controller's XR ray visual          
            //while (rightRayVisual == null) {
            //    if (_Debug) Debug.Log("[PTUtilities] Looking for Right Ray Visual object...");
            //    rightRayVisual = rightController.GetComponent<XRInteractorLineVisual>();
            //    yield return null;
            //}
            //if (_Debug) Debug.Log("[PTUtilities] Right Ray Visual found!");


            /// NOTE: Re-enable this when DataUtilities is implemented
            //// Grab the audio mixer after the Main bundle has been loaded
            //while (audioMaster == null) {
            //    if (_Debug) Debug.Log("[PTUtilities] Looking for Audio Master mixer object...");
            //    if (DataUtilities.instance.finishedInitialising) {

            //        // Load the asset from the Main bundle and wait until it's finished
            //        var assetLoadRequest = DataUtilities.instance.mainBundle.LoadAssetAsync<AudioMixer>(masterMixerName);
            //        yield return assetLoadRequest;

            //        // Save the asset as an audio mixer
            //        audioMaster = assetLoadRequest.asset as AudioMixer;
            //    } 
            //    yield return null;
            //}
            //if (_Debug) Debug.Log("[PTUtilities] Audio Master mixer found!");

            // Grab the original fixed delta time for hen we are applying changes to time scale
            fixedTimeScaleDefault = Time.fixedDeltaTime;

            // Finish setup
            SetupComplete = true;
            if (_Debug) Debug.Log("[PTUtilities] Setup complete!");

            if (OnSetupComplete != null) OnSetupComplete.Invoke();
        }


        #endregion


        #region Update methods

        void FixedUpdate() {
            if (!SetupComplete) return;


            List<XRNodeState> nodes = new List<XRNodeState>();
            InputTracking.GetNodeStates(nodes);

            foreach (XRNodeState ns in nodes) {
                if (ns.nodeType == XRNode.RightHand) { 
                    ns.TryGetVelocity(out ControllerVelocity);
                    ns.TryGetAngularVelocity(out ControllerAngularVelocity);                    
                    ns.TryGetAcceleration(out ControllerAcceleration);                    
                    ns.TryGetAngularAcceleration(out ControllerAngularAcceleration);
                    
                }
            }

            //velocityTest = new Vector3 (Mathf.Round(velocityTest.x * 100f) / 100f, Mathf.Round(velocityTest.y * 100f) / 100f, Mathf.Round(velocityTest.z * 100f) / 100f);
            //angularTest = new Vector3(Mathf.Round(angularTest.x * 100f) / 100f, Mathf.Round(angularTest.y * 100f) / 100f, Mathf.Round(angularTest.z * 100f) / 100f);
            //accelerationTest = new Vector3(Mathf.Round(accelerationTest.x * 100f) / 100f, Mathf.Round(accelerationTest.y * 100f) / 100f, Mathf.Round(accelerationTest.z * 100f) / 100f);
            //angularAccelerationTest = new Vector3(Mathf.Round(angularAccelerationTest.x * 100f) / 100f, Mathf.Round(angularAccelerationTest.y * 100f) / 100f, Mathf.Round(angularAccelerationTest.z * 100f) / 100f);

            //velocitySensitivity = Mathf.Max(0.1f, velocitySensitivity);
            //angularSensitivity = Mathf.Max(0.1f, angularSensitivity);
            //accelerationSensitivity = Mathf.Max(0.1f, accelerationSensitivity);
            //angularAccelerationSensitivity = Mathf.Max(0.1f, angularAccelerationSensitivity);

            //Color finalCol = (Color.red * Mathf.Clamp01(velocityTest.magnitude / velocitySensitivity)) + (Color.blue * Mathf.Clamp01(angularTest.magnitude / angularSensitivity));
            //controllerRenderer.material.color = Color.Lerp(Color.white, finalCol, finalCol.maxColorComponent);


        }


        bool lastTriggerState;
        bool lastPrimaryState;
        bool lastMenuState;
        void Update() {
            if (!SetupComplete) return;            


            if (rightController.inputDevice.TryGetFeatureValue(CommonUsages.triggerButton, out ControllerTriggerButton) && ControllerTriggerButton) {                
                if (!lastTriggerState) {
                    // send event          
                    if (_Debug) Debug.Log("[PTUtilities] Trigger button was just pressed.");
                }
            }
            lastTriggerState = ControllerTriggerButton;


            if (rightController.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out ControllerPrimaryButton) && ControllerPrimaryButton) {
                if (!lastPrimaryState) {
                    // send event          
                    if (_Debug) Debug.Log("[PTUtilities] Primary button was just pressed.");
                }
            }
            lastPrimaryState = ControllerPrimaryButton;


            if (leftController.inputDevice.TryGetFeatureValue(CommonUsages.menuButton, out ControllerMenuButton) && ControllerMenuButton) {
                if (!lastMenuState) {
                    // send event          
                    if (_Debug) Debug.Log("[PTUtilities] Menu button was just pressed.");
                }
            }
            lastMenuState = ControllerMenuButton;

        }

        #endregion



        /// --------------------------------------- PUBLIC CALLS --------------------------------------- \\\
        // A list of general calls that perform common actions in the project


        #region Public calls

        /// <summary>
        /// Teleport the player to the specified position and rotation, taking into account camera position/rotation in rig
        /// </summary>
        /// <param name="position">The position to teleport to</param>
        /// <param name="forwardDirection">The foward direction to teleport to</param>
        public void TeleportPlayer( Vector3 position, Vector3 forwardDirection ) {
            
            playerRig.MoveCameraToWorldLocation(position);
            playerRig.MatchOriginUpCameraForward(Vector3.up, forwardDirection);

        }
        /// <summary>
        /// Teleport the player to the specified position, taking into account camera position in rig
        /// </summary>
        /// <param name="position">The position to teleport to</param>
        public void TeleportPlayer( Vector3 position) {
            playerRig.MoveCameraToWorldLocation(position);
        }
        /// <summary>
        /// Teleport the player to match the specified transform, taking into account camera position in rig
        /// </summary>
        /// <param name="target">The transform to teleport to</param>
        public void TeleportPlayer (Transform target ) {
            TeleportPlayer(target.position, target.forward);
        }
        /// <summary>
        /// Teleport the player to match the specified transform, taking into account camera position in rig
        /// </summary>
        /// <param name="target">The transform to teleport to</param>
        /// <param name="rotatePlayer">Whether to rotate the player to match the transform or not</param>
        public void TeleportPlayer( Transform target, bool rotatePlayer) {
            if (rotatePlayer) TeleportPlayer(target.position, target.forward);
            else TeleportPlayer(target.position);
        }



        HapticCapabilities capabilities;

        public void DoHaptics( Hand hand, float strength, float duration ) {
            if (_Debug) Debug.Log("[PTUTilities] Starting haptics on '"+hand.ToString()+"' hand(s). Strength = " + strength + ", duration = " + duration);

            if (hand != Hand.Right) {
                if (leftController.inputDevice.TryGetHapticCapabilities(out capabilities)) {
                    if (capabilities.supportsImpulse) {
                        uint channel = 0;
                        leftController.inputDevice.SendHapticImpulse(channel, strength, duration);
                    } else Debug.LogError("[PTUtilities] ERROR -> Left controller input device does not support haptic impulse :(");
                } else Debug.LogError("[PTUtilities] ERROR -> Could not get HapticCapabilities of the Left controller input device :(");
                //leftController.SendHapticImpulse(strength, duration);
            }

            if (hand != Hand.Left) {
                if (rightController.inputDevice.TryGetHapticCapabilities(out capabilities)) {
                    if (capabilities.supportsImpulse) {
                        uint channel = 0;
                        rightController.inputDevice.SendHapticImpulse(channel, strength, duration);
                    } else Debug.LogError("[PTUtilities] ERROR -> Right controller input device does not support haptic impulse :(");
                } else Debug.LogError("[PTUtilities] ERROR -> Could not get HapticCapabilities of the Right controller input device :(");
                //rightController.SendHapticImpulse(strength, duration);
            }


           
        }




        Coroutine playingGlobalAudioCo = null;
        public void PlayAudioClip(AudioClip clip, float volume) {
            if (playingGlobalAudioCo != null) StopCoroutine(playingGlobalAudioCo);

            if (!globalAudioSource.gameObject.activeSelf) {
                globalAudioSource.gameObject.SetActive(true);
            }
            globalAudioSource.PlayOneShot(clip, volume);

            playingGlobalAudioCo = StartCoroutine(TurningOffGlobalAudio(clip.length));
        }
        IEnumerator TurningOffGlobalAudio(float timeToWait) {
            yield return new WaitForSeconds(timeToWait);
            globalAudioSource.gameObject.SetActive(false);
        }



        public void PostAudioEvent(AK.Wwise.Event audioEvent, GameObject go) {
            audioEvent.Post(go);
        }

        #endregion




        // --------------------------------------- UTILITIES --------------------------------------- \\
        // The generalised helper ienumerators which change each setting over time

        #region Utilities

        // Helper coroutine for fading the alpha of a sprite
        public IEnumerator FadeAlphaTo( SpriteRenderer sprite, float targetAlpha, float duration, AnimationCurve animCurve, TimeScale timeScale) {

            if (sprite.color.a != targetAlpha) {

                if (_Debug) Debug.Log("[PTUtilities] Fading Sprite " + sprite.name + " to alpha " + targetAlpha);

                if (!sprite.enabled) {
                    sprite.enabled = true;
                }

                float alpha = sprite.color.a;
                for (float t = 0.0f; t < 1.0f; t += (timeScale==0?Time.deltaTime:Time.unscaledDeltaTime) / duration) {
                    sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, Mathf.Lerp(alpha, targetAlpha, animCurve.Evaluate(t)));
                    yield return null;
                }
                sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, targetAlpha);

                if (targetAlpha == 0f) {
                    sprite.enabled = false;
                }
                yield return null;

                if (_Debug) Debug.Log("[PTUtilities] Sprite " + sprite.name + " successfully faded to alpha " + alpha);

            } else {
                if (_Debug) Debug.LogWarning("[PTUtilities] Sprite " + sprite.name + " already at alpha " + targetAlpha + ", cancelling fade");
            }

        }
        // Helper coroutine for fading the alpha of text
        public IEnumerator FadeAlphaTo( TextMeshPro textmesh, float targetAlpha, float duration, AnimationCurve animCurve, TimeScale timeScale ) {

            if (textmesh.color.a != targetAlpha) {

                if (_Debug) Debug.Log("[PTUtilities] Fading TextMesh " + textmesh.name + "to alpha " + targetAlpha);

                if (!textmesh.enabled) {
                    textmesh.enabled = true;
                }

                float alpha = textmesh.color.a;
                for (float t = 0.0f; t < 1.0f; t += (timeScale==0?Time.deltaTime:Time.unscaledDeltaTime) / duration) {
                    textmesh.color = new Color(textmesh.color.r, textmesh.color.g, textmesh.color.b, Mathf.Lerp(alpha, targetAlpha, animCurve.Evaluate(t)));
                    yield return null;
                }
                textmesh.color = new Color(textmesh.color.r, textmesh.color.g, textmesh.color.b, targetAlpha);

                if (targetAlpha == 0f) {
                    textmesh.enabled = false;
                }
                yield return null;

                if (_Debug) Debug.Log("[PTUtilities] TextMesh " + textmesh.name + "successfully faded to alpha " + targetAlpha);

            } else {
                if (_Debug) Debug.LogWarning("[PTUtilities] TextMesh " + textmesh.name + " already at alpha " + targetAlpha + ", cancelling fade");
            }

        }
        // Helper coroutine for fading the alpha of mesh renderer
        public IEnumerator FadeAlphaTo( MeshRenderer mRenderer, float targetAlpha, float duration, AnimationCurve animCurve, TimeScale timeScale ) {

            Material mat = mRenderer.material;

            string propertyName = "";
            propertyName = mat.HasProperty("_BaseColor") ? "_BaseColor" : mat.HasProperty("_Color") ? "_Color" : "";
            if (propertyName == "") {
                Debug.LogError("[PTUtilities] ERROR -> Could not find property name of mesh renderer to fade! Cancelling...");
            }

            Color col = mat.GetColor(propertyName);

            if (col.a != targetAlpha) {

                if (_Debug) Debug.Log("[PTUtilities] Fading MeshRenderer " + mRenderer.name + "to alpha " + targetAlpha);

                if (!mRenderer.enabled) {
                    mRenderer.enabled = true;
                }

                float alpha = col.a;
                for (float t = 0.0f; t < 1.0f; t += (timeScale==0?Time.deltaTime:Time.unscaledDeltaTime) / duration) {
                    Color newColor = new Color(col.r, col.g, col.b, Mathf.Lerp(alpha, targetAlpha, animCurve.Evaluate(t)));
                    mat.SetColor(propertyName, newColor);
                    yield return null;
                }
                mat.SetColor(propertyName, new Color(col.r, col.g, col.b, targetAlpha));


                if (targetAlpha == 0f) {
                    mRenderer.enabled = false;
                }
                yield return null;

                if (_Debug) Debug.Log("[PTUtilities] MeshRenderer " + mRenderer.name + " successfully faded to alpha " + targetAlpha);

            } else {
                if (_Debug) Debug.LogWarning("[PTUtilities] MeshRenderer " + mRenderer.name + " already at alpha " + targetAlpha + ", cancelling fade");
            }
        }
        // Helper coroutine for fading the alpha of mesh renderer
        public IEnumerator FadeAlphaTo(MeshRenderer mRenderer, string propertyName, float targetAlpha, float duration, AnimationCurve animCurve, TimeScale timeScale) {

            Material mat = mRenderer.material;

            propertyName = mat.HasProperty(propertyName) ? propertyName : "";
            if (propertyName == "") {
                Debug.LogError("[PTUtilities] ERROR -> Could not find property name '" + propertyName + "' in mesh renderer to fade! Cancelling...");
            }

            Color col = mat.GetColor(propertyName);

            if (col.a != targetAlpha) {

                if (_Debug) Debug.Log("[PTUtilities] Fading MeshRenderer " + mRenderer.name + "to alpha " + targetAlpha);

                if (!mRenderer.enabled) {
                    mRenderer.enabled = true;
                }

                float alpha = col.a;
                for (float t = 0.0f; t < 1.0f; t += (timeScale == 0 ? Time.deltaTime : Time.unscaledDeltaTime) / duration) {
                    Color newColor = new Color(col.r, col.g, col.b, Mathf.Lerp(alpha, targetAlpha, animCurve.Evaluate(t)));
                    mat.SetColor(propertyName, newColor);
                    yield return null;
                }
                mat.SetColor(propertyName, new Color(col.r, col.g, col.b, targetAlpha));


                if (targetAlpha == 0f) {
                    mRenderer.enabled = false;
                }
                yield return null;

                if (_Debug) Debug.Log("[PTUtilities] MeshRenderer " + mRenderer.name + " successfully faded to alpha " + targetAlpha);

            } else {
                if (_Debug) Debug.LogWarning("[PTUtilities] MeshRenderer " + mRenderer.name + " already at alpha " + targetAlpha + ", cancelling fade");
            }
        }
        // Helper coroutine for fading the alpha of an image
        public IEnumerator FadeAlphaTo( Image image, float targetAlpha, float duration, AnimationCurve animCurve, TimeScale timeScale ) {

            if (image.color.a != targetAlpha) {

                if (_Debug) Debug.Log("[PTUtilities] Fading Image " + image.name + " to alpha " + targetAlpha);

                if (!image.enabled) {
                    image.enabled = true;
                }

                float alpha = image.color.a;
                for (float t = 0.0f; t < 1.0f; t += (timeScale==0?Time.deltaTime:Time.unscaledDeltaTime) / duration) {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(alpha, targetAlpha, animCurve.Evaluate(t)));
                    yield return null;
                }
                image.color = new Color(image.color.r, image.color.g, image.color.b, targetAlpha);

                if (targetAlpha == 0f) {
                    image.enabled = false;
                }
                yield return null;

                if (_Debug) Debug.Log("[PTUtilities] Image " + image.name + " successfully faded to alpha " + alpha);

            } else {
                if (_Debug) Debug.LogWarning("[PTUtilities] Image " + image.name + " already at alpha " + targetAlpha + ", cancelling fade");
            }

        }


        // Helper coroutine for fading the color of a sprite
        public IEnumerator FadeColorTo( SpriteRenderer sprite, Color targetColor, float duration, AnimationCurve animCurve, TimeScale timeScale ) {

            if (sprite.color != targetColor) {

                if (_Debug) Debug.Log("[PTUtilities] Fading Sprite " + sprite.name + "to color " + targetColor);

                if (!sprite.enabled) {
                    sprite.enabled = true;
                }

                Color color = sprite.color;
                for (float t = 0.0f; t < 1.0f; t += (timeScale==0?Time.deltaTime:Time.unscaledDeltaTime) / duration) {
                    sprite.color = Color.Lerp(color, targetColor, animCurve.Evaluate(t));
                yield return null;
                }
                sprite.color = targetColor;

                if (targetColor.a == 0f) {
                    sprite.enabled = false;
                }
                yield return null;

                if (_Debug) Debug.Log("[PTUtilities] Sprite " + sprite.name + "successfully faded to " + color);

            } else {
                if (_Debug) Debug.LogWarning("[PTUtilities] Sprite " + sprite.name + " already at color " + targetColor + ", cancelling fade");
            }

        }
        // Helper coroutine for fading the color of a sprite
        public IEnumerator FadeColorTo( MeshRenderer mRenderer, Color targetColor, float duration, AnimationCurve animCurve, TimeScale timeScale ) {

            Material mat = mRenderer.material;

            string propertyName = "";            
            propertyName = mat.HasProperty("_Color") ? "_Color" : mat.HasProperty("_BaseColor") ? "_BaseColor" : "";
            if (propertyName == "") {
                Debug.LogError("[PTUtilities] ERROR -> Could not find property name of mesh renderer to fade! Cancelling...");
            }

            if (mat.GetColor(propertyName) != targetColor) {

                if (_Debug) Debug.Log("[PTUtilities] Fading Sprite " + mRenderer.name + "to color " + targetColor);

                if (!mRenderer.enabled) {
                    mRenderer.enabled = true;
                }


                Color color = mat.GetColor(propertyName);
                for (float t = 0.0f; t < 1.0f; t += (timeScale==0?Time.deltaTime:Time.unscaledDeltaTime) / duration) {
                    Color newColor = Color.Lerp(color, targetColor, animCurve.Evaluate(t));
                    mat.SetColor(propertyName, newColor);
                    yield return null;
                }
                mat.SetColor(propertyName, targetColor);

                if (targetColor.a == 0f) {
                    mRenderer.enabled = false;
                }
                yield return null;

                if (_Debug) Debug.Log("[PTUtilities] MeshRenderer " + mRenderer.name + "successfully faded to " + color);

            } else {
                if (_Debug) Debug.LogWarning("[PTUtilities] MeshRenderer " + mRenderer.name + " already at color " + targetColor + ", cancelling fade");
            }

        }
        // Helper coroutine for fading the color of a sprite
        public IEnumerator FadeColorTo(MeshRenderer mRenderer, string propertyName, Color targetColor, float duration, AnimationCurve animCurve, TimeScale timeScale) {

            Material mat = mRenderer.material;
                        
            propertyName = mat.HasProperty(propertyName) ? propertyName : "";
            if (propertyName == "") {
                Debug.LogError("[PTUtilities] ERROR -> Could not find property name '"+propertyName+"' in mesh renderer to fade! Cancelling...");
            }

            if (mat.GetColor(propertyName) != targetColor) {

                if (_Debug) Debug.Log("[PTUtilities] Fading Sprite " + mRenderer.name + "to color " + targetColor);

                if (!mRenderer.enabled) {
                    mRenderer.enabled = true;
                }


                Color color = mat.GetColor(propertyName);
                for (float t = 0.0f; t < 1.0f; t += (timeScale == 0 ? Time.deltaTime : Time.unscaledDeltaTime) / duration) {
                    Color newColor = Color.Lerp(color, targetColor, animCurve.Evaluate(t));
                    mat.SetColor(propertyName, newColor);
                    yield return null;
                }
                mat.SetColor(propertyName, targetColor);

                if (targetColor.a == 0f) {
                    mRenderer.enabled = false;
                }
                yield return null;

                if (_Debug) Debug.Log("[PTUtilities] MeshRenderer " + mRenderer.name + "successfully faded to " + color);

            } else {
                if (_Debug) Debug.LogWarning("[PTUtilities] MeshRenderer " + mRenderer.name + " already at color " + targetColor + ", cancelling fade");
            }

        }
        // Helper coroutine for fading the color of a text mesh
        public IEnumerator FadeColorTo( TextMeshPro textMesh, Color targetColor, float duration, AnimationCurve animCurve, TimeScale timeScale ) {

            if (textMesh.color != targetColor) {

                if (_Debug) Debug.Log("[PTUtilities] Fading Text " + textMesh.name + "to color " + targetColor);

                if (!textMesh.enabled) {
                    textMesh.enabled = true;
                }

                Color color = textMesh.color;
                for (float t = 0.0f; t < 1.0f; t += (timeScale==0?Time.deltaTime:Time.unscaledDeltaTime) / duration) {
                    textMesh.color = Color.Lerp(color, targetColor, animCurve.Evaluate(t));
                    yield return null;
                }
                textMesh.color = targetColor;

                if (targetColor.a == 0f) {
                    textMesh.enabled = false;
                }
                yield return null;

                if (_Debug) Debug.Log("[PTUtilities] Text " + textMesh.name + "successfully faded to " + color);

            } else {
                if (_Debug) Debug.LogWarning("[PTUtilities] Text " + textMesh.name + " already at color " + targetColor + ", cancelling fade");
            }

        }
        // Helper coroutine for fading the color of an image
        public IEnumerator FadeColorTo( Image image, Color targetColor, float duration, AnimationCurve animCurve, TimeScale timeScale ) {

            if (image.color != targetColor) {

                if (_Debug) Debug.Log("[PTUtilities] Fading Image " + image.name + "to color " + targetColor);

                if (!image.enabled) {
                    image.enabled = true;
                }

                Color color = image.color;
                for (float t = 0.0f; t < 1.0f; t += (timeScale==0?Time.deltaTime:Time.unscaledDeltaTime) / duration) {
                    image.color = Color.Lerp(color, targetColor, animCurve.Evaluate(t));
                    yield return null;
                }
                image.color = targetColor;

                if (targetColor.a == 0f) {
                    image.enabled = false;
                }
                yield return null;

                if (_Debug) Debug.Log("[PTUtilities] Image " + image.name + "successfully faded to " + color);

            } else {
                if (_Debug) Debug.LogWarning("[PTUtilities] Image " + image.name + " already at color " + targetColor + ", cancelling fade");
            }

        }


        // Helper coroutine for fading volume weight
        public IEnumerator FadePostVolumeTo( Volume volume, float targetWeight, float duration, AnimationCurve animCurve, TimeScale timeScale ) {
            float weight = volume.weight;
            targetWeight = Mathf.Clamp01(targetWeight);

            if (!volume.enabled) volume.enabled = true;


            for (float t = 0.0f; t < 1.0f; t += (timeScale==0?Time.deltaTime:Time.unscaledDeltaTime) / duration) {
                float newWeight = Mathf.Lerp(weight, targetWeight, animCurve.Evaluate(t)); 
                volume.weight = newWeight;
                yield return null;
            }
            volume.weight = targetWeight;
            if (targetWeight == 0) {
                volume.enabled = false;
            }
        }

        // Helper coroutine for fading volume weight
        public IEnumerator FadePostVolumeTo(PostProcessVolume volume, float targetWeight, float duration, AnimationCurve animCurve, TimeScale timeScale) {
            float weight = volume.weight;
            targetWeight = Mathf.Clamp01(targetWeight);

            if (!volume.enabled) volume.enabled = true;


            for (float t = 0.0f; t < 1.0f; t += (timeScale == 0 ? Time.deltaTime : Time.unscaledDeltaTime) / duration) {
                float newWeight = Mathf.Lerp(weight, targetWeight, animCurve.Evaluate(t));
                volume.weight = newWeight;
                yield return null;
            }
            volume.weight = targetWeight;
            if (targetWeight == 0) {
                volume.enabled = false;
            }
        }

       

        // Helper coroutine for fading the intensity of an light
        public IEnumerator FadeLightIntensityTo(Light light, float targetIntensity, float duration, AnimationCurve animCurve, TimeScale timeScale) {

            if (light.intensity != targetIntensity) {

                if (_Debug) Debug.Log("[PTUtilities] Fading light " + light.name + " to intensity " + targetIntensity);

                if (!light.enabled) {
                    light.enabled = true;
                }

                float intensity = light.intensity;
                for (float t = 0.0f; t < 1.0f; t += (timeScale == 0 ? Time.deltaTime : Time.unscaledDeltaTime) / duration) {
                    light.intensity = Mathf.Lerp(intensity, targetIntensity, animCurve.Evaluate(t));
                    yield return null;
                }
                light.intensity = targetIntensity;

                if (targetIntensity == 0f) {
                    light.enabled = false;
                }
                yield return null;

                if (_Debug) Debug.Log("[PTUtilities] Light " + light.name + " successfully faded to intensity " + targetIntensity);

            } else {
                if (_Debug) Debug.LogWarning("[PTUtilities] Light " + light.name + " already at intensity " + targetIntensity + ", cancelling fade");
            }

        }

        // Helper coroutine for shifting the range of an light
        public IEnumerator ShiftRangeTo(Light light, float targetRange, float duration, AnimationCurve animCurve, TimeScale timeScale) {

            if (light.range != targetRange) {

                if (_Debug) Debug.Log("[PTUtilities] Shifting light " + light.name + " to range " + targetRange);

                if (!light.enabled) {
                    light.enabled = true;
                }

                float range = light.range;
                for (float t = 0.0f; t < 1.0f; t += (timeScale == 0 ? Time.deltaTime : Time.unscaledDeltaTime) / duration) {
                    light.intensity = Mathf.Lerp(range, targetRange, animCurve.Evaluate(t));
                    yield return null;
                }
                light.range = targetRange;

                if (targetRange == 0f) {
                    light.enabled = false;
                }
                yield return null;

                if (_Debug) Debug.Log("[PTUtilities] Light " + light.name + " successfully shifted to range " + targetRange);

            } else {
                if (_Debug) Debug.LogWarning("[PTUtilities] Light " + light.name + " already at range " + targetRange + ", cancelling shift");
            }

        }
        
        // Helper coroutine for fading the color of an light
        public IEnumerator FadeColorTo(Light light, Color targetColor, float duration, AnimationCurve animCurve, TimeScale timeScale) {

            if (light.color != targetColor) {

                if (_Debug) Debug.Log("[PTUtilities] Fading light " + light.name + "to color " + targetColor);

                if (!light.enabled) {
                    light.enabled = true;
                }

                Color color = light.color;
                for (float t = 0.0f; t < 1.0f; t += (timeScale == 0 ? Time.deltaTime : Time.unscaledDeltaTime) / duration) {
                    light.color = Color.Lerp(color, targetColor, animCurve.Evaluate(t));
                    yield return null;
                }
                light.color = targetColor;

                if (targetColor.a == 0f) {
                    light.enabled = false;
                }
                yield return null;

                if (_Debug) Debug.Log("[PTUtilities] Light " + light.name + "successfully faded to " + color);

            } else {
                if (_Debug) Debug.LogWarning("[PTUtilities] Light " + light.name + " already at color " + targetColor + ", cancelling fade");
            }

        }

        // Helper coroutine for fading audio source volume
        public IEnumerator FadeAudioTo( AudioSource audio, float targetVolume, float duration, TimeScale timeScale ) {
            float volume = audio.volume;
            if (!audio.enabled) audio.enabled = true;
            if (!audio.isPlaying) audio.Play();
        
            for (float t = 0.0f; t < 1.0f; t += (timeScale==0?Time.deltaTime:Time.unscaledDeltaTime) / duration) {
                float newVolume = Mathf.Lerp(volume, targetVolume, t);
                audio.volume = newVolume;
                yield return null;
            }
            audio.volume = targetVolume;
            if (targetVolume == 0) {
                audio.Stop();
                audio.enabled = false;
            }

        }
        // Helper coroutine for fading the master audio mixer volume
        public IEnumerator FadeAudioMasterTo( float targetVolume, float duration, TimeScale timeScale ) {

            if (_Debug) Debug.Log("Fading master volume...");

            yield return FadeAudioMixerTo(audioMaster, "MasterVolume", targetVolume, duration, timeScale);

            if (_Debug) Debug.Log("Finished fading master volume...");

            //audioMaster.GetFloat("MasterVolume", out float currentDB);
            //float targetDB = (targetVolume - 1) * 80;

            //if (_Debug) Debug.Log("Fading Master audio mixer, currentDB = "+currentDB+", targetDB = " + targetDB);

            //for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration) {
            //    float newDB = Mathf.Lerp(currentDB, targetDB, t);
            //    audioMaster.SetFloat("MasterVolume", newDB);
            //    if (_Debug) Debug.Log("MasterVolume = " + newDB);
            //    yield return null;
            //}
            //audioMaster.SetFloat("MasterVolume", targetDB);

            //if (_Debug) Debug.Log("Finished fading Master audio mixer, newDB = " + targetDB);

        }

        // Helper coroutine for fading an audio mixer parametre
        public IEnumerator FadeAudioMixerTo( AudioMixer mixer, string floatName, float targetValue, float duration, TimeScale timeScale ) {

            mixer.GetFloat(floatName, out float currentDB);
            AnimationCurve volumeCurve = AnimationCurve.Linear(0, -80, 1, 2f);
            float targetDB = volumeCurve.Evaluate(targetValue); // (targetValue - 1) * 80;

            if (_Debug) Debug.Log("Fading mixer '" + mixer.name + "', currentDB = " + currentDB + ", targetDB = " + targetDB);

            for (float t = 0.0f; t < 1.0f; t += timeScale==0?Time.deltaTime:Time.unscaledDeltaTime / duration) {
                float newDB = Mathf.Lerp(currentDB, targetDB, t);
                mixer.SetFloat(floatName, newDB);
                //if (_Debug) Debug.Log(floatName + " = " + newDB);
                yield return null;
            }
            audioMaster.SetFloat(floatName, targetDB);

            if (_Debug) Debug.Log("Finished fading mixer '" + mixer.name + "', newDB = " + targetDB);
        }




        // Helper coroutine for moving a transform
        public IEnumerator MoveTransformViaCurve( Transform target, AnimationCurve animCurve, Vector3 moveAmount, float duration, TimeScale timeScale ) {

            float t = 0;
            Vector3 initialPos = target.localPosition;
            //float curveTime = shakeTransformCurve.keys[shakeTransformCurve.length - 1].time;

            if (_Debug) Debug.Log("[PTUtilities] Moving transform " + target.name);

            while (t < duration) {
                yield return null;
                target.localPosition = initialPos + (moveAmount * animCurve.Evaluate(t / duration));
                t += timeScale==0?Time.deltaTime:Time.unscaledDeltaTime;
            }
            target.localPosition = initialPos + moveAmount;

            if (_Debug) Debug.Log("[PTUtilities] Finished moving " + target.name);

        }  
        // Helper coroutine for rotating a transform
        public IEnumerator RotateTransformViaCurve( Transform target, AnimationCurve animCurve, Vector3 rotateAmount, float duration, TimeScale timeScale ) {

            float t = 0;
            Vector3 initialRot = target.localRotation.eulerAngles;

            if (_Debug) Debug.Log("[PTUtilities] Rotating transform " + target.name);

            while (t < duration) {
                yield return null;
                target.localRotation = Quaternion.Euler(initialRot + (rotateAmount * animCurve.Evaluate(t / duration)));
                t += timeScale == 0 ? Time.deltaTime : Time.unscaledDeltaTime;
            }
            target.localRotation = Quaternion.Euler(initialRot + rotateAmount);

            if (_Debug) Debug.Log("[PTUtilities] Finished rotating " + target.name);

        }
        // Helper coroutine for scaling a transform
        public IEnumerator ScaleTransformViaCurve( Transform target, AnimationCurve animCurve, Vector3 scaleAmount, float duration, TimeScale timeScale ) {

            float t = 0;
            Vector3 initialScale = target.localScale;
            Vector3 finalScale = Vector3.Scale(initialScale, scaleAmount);
            //float curveTime = shakeTransformCurve.keys[shakeTransformCurve.length - 1].time;

            if (_Debug) Debug.Log("[PTUtilities] Scaling transform " + target.name);

            while (t < duration) {
                yield return null;
                //target.localScale = initialScale + Vector3.Scale(initialScale, scaleAmount) * animCurve.Evaluate(t / duration);
                target.localScale = Vector3.Lerp(initialScale, finalScale, animCurve.Evaluate(t / duration));
                t += timeScale==0?Time.deltaTime:Time.unscaledDeltaTime;
            }
            target.localScale = finalScale;

            if (_Debug) Debug.Log("[PTUtilities] Finished scaling " + target.name);

        }
        
        // Helper coroutine for shaking a transform (should be deprecated really)
        public IEnumerator ShakeTransform( Transform target, Vector3 shakePosition, Vector3 shakeRotation, float duration, TimeScale timeScale ) {

            float t = 0;
            Vector3 initialPos = target.localPosition;
            Vector3 initialRot = target.localRotation.eulerAngles;
            //float curveTime = shakeTransformCurve.keys[shakeTransformCurve.length - 1].time;

            if (_Debug) Debug.Log("[PTUtilities] Shaking transform " + target.name);

            while (t < duration) {
                yield return null;
                target.localPosition = initialPos + (shakePosition * shakeTransformCurve.Evaluate(t / duration));
                target.localRotation = Quaternion.Euler(initialRot + (shakeRotation * shakeTransformCurve.Evaluate(t / duration)));
                t += timeScale == 0 ? Time.deltaTime : Time.unscaledDeltaTime;
            }
            target.localPosition = initialPos;
            target.localRotation = Quaternion.Euler(initialRot);

            if (_Debug) Debug.Log("[PTUtilities] Finished shaking " + target.name);

        }

        // Helper coroutine for shaking a transform (should be deprecated really)
        public IEnumerator ShakeRotation(Transform target, Vector3 shakeAmount, float duration, TimeScale timeScale) {

            float t = 0;
            Vector3 initialRot = target.localRotation.eulerAngles;
            //float curveTime = shakeTransformCurve.keys[shakeTransformCurve.length - 1].time;

            if (_Debug) Debug.Log("[PTUtilities] Shaking rotation of transform " + target.name);

            while (t < duration) {
                yield return null;
                target.localRotation = Quaternion.Euler(initialRot + (shakeAmount * shakeTransformCurve.Evaluate(t / duration)));
                t += timeScale == 0 ? Time.deltaTime : Time.unscaledDeltaTime;
            }
            target.localRotation = Quaternion.Euler(initialRot);

            if (_Debug) Debug.Log("[PTUtilities] Finished shaking rotation of " + target.name);

        }




        #endregion


        // Universal ienumerators of which only one copy must be kept

        #region Universal utilities

        bool fadingAudioListener;
        Coroutine fadeAudioListenerCoroutine;

        /// <summary>
        /// Fades the volume of the Audio Listener to the target value over the duration
        /// </summary>
        /// <param name="volume">The target volume to fade to</param>
        /// <param name="duration">The duration of the fade in seconds</param>
        public void FadeAudioListener( float volume, float duration ) {

            // If an audio listener fade is already happening, cancel it and start the new one
            if (fadingAudioListener) StopCoroutine(fadeAudioListenerCoroutine);
            fadeAudioListenerCoroutine = StartCoroutine(FadeAudioListenerTo(volume, duration));

        }


        // Helper coroutine for fading audio listener volume
        IEnumerator FadeAudioListenerTo( float targetVolume, float duration ) {
            fadingAudioListener = true;


            float volume = AudioListener.volume;
            if (_Debug) Debug.Log("FadeAudioListenerTo Starting");

            for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / duration) {
                float newVolume = Mathf.Lerp(volume, targetVolume, t);
                AudioListener.volume = newVolume;

                if (_Debug) Debug.Log("AudioListener.volume = " + AudioListener.volume);
                yield return null;
            }
            AudioListener.volume = targetVolume;


            if (_Debug) Debug.Log("FadeAudioListenerTo Finished");
            fadingAudioListener = false;
        }



        #endregion

    }


    [Serializable]
    public class ProgressEvent  {

        public string name;
        [Range(0, 1)] public float threshold;
        public UnityEvent2 progressEvent;

        public ProgressEvent( float progressThreshold, UnityEvent2 eventToSend ) {
            name = progressThreshold.ToString();
            threshold = progressThreshold;
            progressEvent = eventToSend;
        }

    }

}
