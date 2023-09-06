using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {

    public class SceneLightSettings : MonoBehaviour {
        public static SceneLightSettings instance = null;

        [SerializeField] bool applyOnLoad = true;
        [SerializeField] int currentIndex = 0;

        [Space(5)]
        [SerializeField] bool debugging = true;

        [Space(15)]
        [SerializeField] SceneLightSettingsContainer[] settings;

        public string currentName {
            get {
                if (settings.Length > currentIndex) {
                    return settings[currentIndex].name;
                } else return "{Error}";                 
            }
        }


        public void Start () {

            CheckForActive();

            if (applyOnLoad) ApplySettings(currentIndex);
        }

        public void ApplySettings(string settingName) {
            if (debugging) Debug.Log("[SceneLightSettings] Searching for settings '" + settingName + "' to apply...");

            if (settings.Length > 0) {
                for (int i = 0; i < settings.Length; i++) {
                    if (settings[i].name == settingName) {

                        ApplySettings(settings[i]);
                        currentIndex = i;

                        return;
                    }
                }
            }
            if (debugging) Debug.LogError("[SceneLightSettings] ERROR -> Could not find matching settings '" + settingName + "'");
        }

        public void ApplySettings(int settingIndex) {
            if (debugging) Debug.Log("[SceneLightSettings] Searching for settings [" + settingIndex + "] to apply...");

            if (settings.Length > settingIndex) {

                ApplySettings(settings[settingIndex]);
                currentIndex = settingIndex;

                return;
            }
            if (debugging) Debug.LogError("[SceneLightSettings] ERROR -> Could not find matching settings [" + settingIndex + "]");
        }

        public void ApplyNextSettings() {
            if (debugging) Debug.Log("[SceneLightSettings] Attempting to apply next settings...");

            if (settings.Length > 0) {

                currentIndex = (currentIndex + 1) % settings.Length;
                ApplySettings(currentIndex);

                return;
            }
            if (debugging) Debug.LogError("[SceneLightSettings] ERROR -> Could not find any matching settings!");
        }


        public void ApplySettings(SceneLightSettingsContainer container) {
            if (debugging) Debug.Log("[SceneLightSettings] Applying matching settings '" + container.name + "'!");

            if (container.changeAmbientLight) RenderSettings.ambientLight = container.ambientLightColor;
            if (container.changeFog) {
                RenderSettings.fog = container.fogEnabled;
                RenderSettings.fogMode = container.fogMode;
                RenderSettings.fogColor = container.fogColor;
                RenderSettings.fogDensity = container.fogDensity;
                RenderSettings.fogStartDistance = container.fogStart;
                RenderSettings.fogEndDistance = container.fogEnd;
            }
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
    public class SceneLightSettingsContainer {

        [Space(10)]
        public string name;
        [Space(10)]
        public bool changeAmbientLight = false;
        [ColorUsage(false, true)] public Color ambientLightColor;
        [Space(5)]
        public bool changeFog = false;
        public bool fogEnabled = false;
        public FogMode fogMode = FogMode.Linear;
        public Color fogColor = Color.black;
        public float fogDensity = 0.05f;
        public float fogStart = 0;
        public float fogEnd = 300;

        public SceneLightSettingsContainer(string _name, bool _changeAmbientLight, Color _ambientLightColor, bool _changeFog, bool _fogEnabled, FogMode _fogMode, Color _fogColor, float _fogDensity, float _fogStart, float _fogEnd) {
            name = _name;
            changeAmbientLight = _changeAmbientLight;
            ambientLightColor = _ambientLightColor;
            
            changeFog = _changeFog;
            fogEnabled = _fogEnabled;
            fogMode = _fogMode;
            fogColor = _fogColor;
            fogDensity = _fogDensity;
            fogStart = _fogStart;
            fogEnd = _fogEnd;
    }

    }

}
