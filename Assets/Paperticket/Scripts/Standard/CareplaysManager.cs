using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket {

    #region public enums
    public enum AssetBundles { desert, menu, menuscene, we01, we01scene, we02, we02scene, we03, we03scene, we04, we04scene,
                                characters, in01, in01scene, in02, in02scene, in03, in03scene, in04, in04scene, in05,
                                 in05scene, in06, in06scene }
    public enum CareScene { DesertMenu, WE01_Onboarding, WE02_Jetty, WE03_Dawn, WE04_Finale, IN01_Modules, IN02_Choice, 
                             IN03_Reporting, IN04_Cigarette, IN05_Family, IN06_Privacy }
    #endregion

    public class CareplaysManager : MonoBehaviour {

        public static CareplaysManager instance = null;


        [Header("CONTROLS")]
        [SerializeField] bool loadFirstScene = true;
        public CareScene firstScene = CareScene.DesertMenu;
        [Space(10)]
        public List<CareSceneInfo> careSceneManifest = null;
        [Space(20)]
        [SerializeField] UnityEvent2 startLoading;
        [SerializeField] UnityEvent2 finishLoading;

        //[SerializeField] GameObject LoadingGraphic = null;
        //[SerializeField] [Min(0)] float loadingGraphicDelay = 2f;
        //SpriteRenderer[] loadingSprites = null;

        [Space(10)] 
        [SerializeField] bool debugging = false;


    [Header("LIVE VARIABLES")]
        [Space(20)]
        public bool IN01HonestyComplete = false;
        public bool IN01ChoiceComplete = false;
        public bool IN01CulturalComplete = false;
        public bool IN01PrivacyComplete = false;
        public bool IN01ReportComplete = false;
        public int IN01VideoIndex = 0;
        public int WE02VideoIndex = 0;
        public int WE03VideoIndex = 0;

        Coroutine loadingCareSceneCo = null;
        //Coroutine watchingLoadTimeCo = null;
        //bool loadGfxActive = false;

        void Awake() {
            StartCoroutine(Initialising());          

        }
        

        #region Public functions

        public void LoadCareScene( CareScene careScene ) {
            if (debugging) Debug.Log("[CareplaysManager] Attempting to load CareScene '"+careScene.ToString()+"'...");

            // Unload the current scene and old bundles, load the next set
            if (loadingCareSceneCo != null) StopCoroutine(loadingCareSceneCo);
            loadingCareSceneCo = StartCoroutine(LoadingCareScene(careScene));

            //// Watch the loading times and display loading graphics if necessary
            //if (watchingLoadTimeCo != null) StopCoroutine(watchingLoadTimeCo);
            //watchingLoadTimeCo = StartCoroutine(WatchingLoadTime());
        }

        #endregion


        #region Internal Coroutines

        IEnumerator Initialising() {

            if (!debugging) debugging = true;

            // Set as the CareplaysManager static instance
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Destroy(gameObject);
            }

            // Wait for other utility scripts to be ready
            yield return new WaitUntil(() => PTUtilities.instance != null);
            yield return new WaitUntil(() => SceneUtilities.instance != null);
            yield return new WaitUntil(() => DataUtilities.instance != null);
            yield return new WaitUntil(() => DataUtilities.instance.finishedInitialising = true);

            // Load the first careplays scene
            if (loadFirstScene) LoadCareScene(firstScene);
        }

        IEnumerator LoadingCareScene( CareScene careScene ) {
            CareSceneInfo newSceneInfo = null;
            string newSceneName = "";
            List<AssetBundles> newBundles = null;

            // Grab the scene info for the new scene from the manifest
            foreach (CareSceneInfo sceneInfo in careSceneManifest) {
                if (sceneInfo.careScene == careScene) {
                    newSceneInfo = sceneInfo;
                    break;
                }
            }
            if (newSceneInfo == null) {
                Debug.LogError("[CareplaysManager] ERROR -> No scene info found for CareScene '"+careScene.ToString()+"'! This is a fatal error :( ");
                yield break;
            }

            // Extract the scene info
            newSceneName = newSceneInfo.sceneName;
            newBundles = newSceneInfo.requiredBundles;

            if (debugging) Debug.Log("[CareplaysManager] CareScene '" + careScene.ToString() + "' info loaded! \n" +
                                     "CareScene name = " + newSceneName + "\n" +
                                     "Required bundles = " + newBundles.ToString());

            // Send an event to signify that loading has started
            if (startLoading != null) startLoading.Invoke();

            // Unload the current active scene            
            if (SceneUtilities.instance.SceneCount > 1) {
                if (debugging) Debug.Log("[CareplaysManager] Unloading the active scene...");
                SceneUtilities.instance.UnloadActiveScene(1, true);
                yield return new WaitUntil(() => SceneUtilities.instance.SceneCount == 1);
            } else if (debugging) Debug.Log("[CareplaysManager] No scenes to unload, skipping...");

            // Unload any asset bundles that are not required for the new scene
            if (DataUtilities.instance.GetLoadedBundles() != null) {
                if (debugging) Debug.Log("[CareplaysManager] Unloading the old bundles...");
                foreach (AssetBundles oldBundle in DataUtilities.instance.GetLoadedBundles()) {
                    if (!newBundles.Contains(oldBundle)) {
                        DataUtilities.instance.UnloadAssetBundle(oldBundle, true);
                        yield return new WaitUntil(() => !DataUtilities.instance.isBundleLoaded(oldBundle));
                    }
                    yield return null;
                }
            } else if (debugging) Debug.Log("[CareplaysManager] No old bundles to unload, skipping...");

            // Load any asset bundles that are required for the new scene
            foreach (AssetBundles newBundle in newBundles) {
                if (!DataUtilities.instance.isBundleLoaded(newBundle)) {
                    DataUtilities.instance.LoadAssetBundle(newBundle);
                    yield return new WaitUntil(() => DataUtilities.instance.isBundleLoaded(newBundle));
                }                
            }

            // Begin loading the next scene
            if (debugging) Debug.Log("[CareplaysManager] Loading the new scene...");
            SceneUtilities.instance.LoadScene(newSceneName, true);
            yield return new WaitUntil(() => SceneUtilities.instance.CheckSceneLoaded(newSceneName));

            // Send an event to signify that loading has finished
            if (finishLoading != null) finishLoading.Invoke();


            loadingCareSceneCo = null;
        }


        //IEnumerator WatchingLoadTime() {
            
        //    yield return new WaitForSeconds(loadingGraphicDelay);
        //    loadGfxActive = true;



        //}


        #endregion

    }


    #region CareSceneInfo class

    [Serializable]
    public class CareSceneInfo {

        public string sceneName;
        public CareScene careScene;
        public List<AssetBundles> requiredBundles;

        public CareSceneInfo() {
            sceneName = "Insert Scene Name";
            careScene = 0;
            requiredBundles = null;
        }
    }

    #endregion

}