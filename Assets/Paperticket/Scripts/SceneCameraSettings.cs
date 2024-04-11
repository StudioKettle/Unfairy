using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {

    public class SceneCameraSettings : MonoBehaviour {
        public static SceneCameraSettings instance = null;

        [SerializeField] bool applyOnLoad = true;
        [SerializeField] int currentIndex = 0;

        [Space(5)]
        [SerializeField] bool debugging = true;

        [Space(15)]
        [SerializeField] SceneCameraSettingsContainer[] settings;

        public string currentName {
            get {
                if (settings.Length > currentIndex) {
                    return settings[currentIndex].name;
                } else return "{Error}";
            }
        }


        public void Start() {

            CheckForActive();

            if (applyOnLoad) ApplySettings(currentIndex);
        }


        public void ApplySettings(string settingName) {
            if (debugging) Debug.Log("[SceneCameraSettings] Searching for settings '" + settingName + "' to apply...");
            
            if (settings.Length > 0) {
                for (int i = 0; i < settings.Length; i++) {
                    if (settings[i].name == settingName) {

                        ApplySettings(settings[i]);
                        currentIndex = i;

                        return;
                    }
                }
            }
            if (debugging) Debug.LogError("[SceneCameraSettings] ERROR -> Could not find matching settings '" + settingName + "'");
        }

        public void ApplySettings(int settingIndex) {
            if (debugging) Debug.Log("[SceneCameraSettings] Searching for settings [" + settingIndex + "] to apply...");

            if (settings.Length > settingIndex) {

                ApplySettings(settings[settingIndex]);
                currentIndex = settingIndex;

                return;                
            }
            if (debugging) Debug.LogError("[SceneCameraSettings] ERROR -> Could not find matching settings [" + settingIndex + "]");
        }

        public void ApplyNextSettings() {
            if (debugging) Debug.Log("[SceneCameraSettings] Attempting to apply next settings...");

            if (settings.Length > 0) {

                currentIndex = (currentIndex + 1) % settings.Length;
                ApplySettings(currentIndex);

                return;
            }
            if (debugging) Debug.LogError("[SceneCameraSettings] ERROR -> Could not find any matching settings!");
        }




        public void ApplySettings(SceneCameraSettingsContainer container) {
            if (debugging) Debug.Log("[SceneCameraSettings] Applying matching settings '" + container.name + "'! (Called from "+gameObject.name+")");

            if (container.changeClearFlags) Camera.main.clearFlags = container.clearFlags;
            if (container.changeBackground) Camera.main.backgroundColor = container.background;
            if (container.changeCullingMask) Camera.main.cullingMask = container.cullingMask;
        }





        void CheckForActive() {
            StartCoroutine(CheckingForActive());
        }
        IEnumerator CheckingForActive() {

            if (SceneUtilities.instance.CheckSceneActive(gameObject.scene.name)) {
                if (instance != null) Debug.LogWarning("Switching to the latest SceneCameraSettings in scene '" + gameObject.scene.name + "'");
                instance = this;
            } else {
                yield return new WaitForSeconds(0.1f);
                SceneUtilities.OnSceneMadeActive += CheckForActive;
            }
        }
        void OnDisable() {
            SceneUtilities.OnSceneMadeActive -= CheckForActive;
        }

    }




    [Serializable]
    public class SceneCameraSettingsContainer {

        [Space(10)]
        public string name;
        [Space(10)]
        public bool changeClearFlags = false;
        public CameraClearFlags clearFlags;
        [Space(5)]
        public bool changeBackground = false;
        public Color background;
        [Space(5)]
        public bool changeCullingMask = false;
        public LayerMask cullingMask;

        public SceneCameraSettingsContainer(string _name, bool _changeClearFlags, CameraClearFlags _clearFlags, bool _changeBackground, Color _background, bool _changeCullingMask, LayerMask _cullingMask) {
            name = _name;
            changeClearFlags = _changeClearFlags;
            clearFlags = _clearFlags;
            changeBackground = _changeBackground;
            background = _background;
            changeCullingMask = _changeCullingMask;
            cullingMask = _cullingMask;
        }

    }
}
