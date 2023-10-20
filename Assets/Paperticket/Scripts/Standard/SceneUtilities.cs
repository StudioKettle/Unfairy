using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

namespace Paperticket {

    public class SceneUtilities : MonoBehaviour {

        public static SceneUtilities instance = null;

        public delegate void SceneAlmostReady();
        public static event SceneAlmostReady OnSceneAlmostReady;

        public delegate void SceneLoaded();
        public static event SceneLoaded OnSceneLoad;

        public delegate void SceneUnloaded();
        public static event SceneUnloaded OnSceneUnload;

        public delegate void SceneMadeActive();
        public static event SceneMadeActive OnSceneMadeActive;

        enum StartBehaviour { None, LoadFirstScene, LoadFirstSceneOverride, SetFirstSceneActive }
        enum FirstSceneName { BottleTest, WFCTest, RockTest }

        [Header("Controls")]

        [SerializeField] StartBehaviour startBehaviour;
        [SerializeField] FirstSceneName firstSceneName;

        public string _FirstSceneName = "";


        [SerializeField] bool _Debug = false;

        AsyncOperation asyncOperation = null;

        [SerializeField] bool convergeDynamicGI = false;

        string lastSceneStarted = "";


        void Awake() {
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Destroy(gameObject);
            }




            //// If this is a build and we don't wanna force override, set the first scene by build index (for Granger's benefit)
            if (Application.isEditor || startBehaviour == StartBehaviour.LoadFirstSceneOverride) {
                _FirstSceneName = firstSceneName.ToString();
            } else {
                //_FirstSceneName = SceneManager.GetSceneByBuildIndex(1).name;
                string path = SceneUtility.GetScenePathByBuildIndex(1);         
                _FirstSceneName = path.Substring(0, path.Length - 6).Substring(path.LastIndexOf('/') + 1);
            }

            if (_Debug) Debug.Log("[SceneUtilities] First scene name: " + _FirstSceneName);

            // Load first scene or make it active, if applicable
            switch (startBehaviour) {
                // Load first scene
                case StartBehaviour.LoadFirstSceneOverride:
                case StartBehaviour.LoadFirstScene:
                    StartCoroutine(LoadingFirstScene());
                    break;

                // Set first scene active
                case StartBehaviour.SetFirstSceneActive:
                    StartCoroutine(SettingFirstSceneActive());
                    break;

                // Do nothing
                case StartBehaviour.None:
                default:
                    break;
            }
           
            
        }

        IEnumerator LoadingFirstScene() {

            if (_Debug) Debug.Log("[SceneUtilities] Loading the first scene: " + _FirstSceneName);

            BeginLoadScene(_FirstSceneName);
            yield return new WaitUntil(() => lastSceneStarted == _FirstSceneName);

            // Wait a sec then finish loading the intro scene
            yield return new WaitForSeconds(0.5f);
            FinishLoadScene(true);

        }

        IEnumerator SettingFirstSceneActive() {
            if (_Debug) Debug.Log("[SceneUtilities] Setting the first scene active: " + _FirstSceneName);
            
            yield return new WaitUntil(() => SceneManager.sceneCount > 1);
            
            //// Wait until Unity knows we have more than one scene open
            //while (SceneManager.sceneCount <= 1) {
            //    if (_Debug) Debug.Log("[SceneUtilities] Waiting for scene count to catch up...");
            //    yield return new WaitForSeconds(0.1f);
            //}

            // Wait a sec then make the first scene active
            yield return new WaitForSeconds(0.5f);
            SetActiveScene(_FirstSceneName);

        }






        #region Public variables

        public bool CheckSceneLoaded( string sceneName ) {
            if (SceneManager.GetSceneByName(sceneName).isLoaded) {
                if (_Debug) Debug.Log("[SceneUtilities] " + sceneName + " is loaded!");
                return true;
            }
            return false;
        }


        public bool CheckSceneActive( string sceneName ) {
            if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName(sceneName) && DynamicGI.isConverged) {
                if (_Debug) Debug.Log("[SceneUtilities] " + sceneName + " is active!");
                return true;
            }
            return false;
        }

        public int SceneCount {
            get { return SceneManager.sceneCount; }
        }

        /// <summary>
        /// Get the progress on the current scene load
        /// </summary>
        public float GetSceneProgress {
            get {
                if (asyncOperation != null) {
                    return asyncOperation.progress;
                } else {
                    return 0;
                }
            }
        }

        #endregion


        #region Public Calls

        public void LoadScene( string sceneToLoad, bool setActive ) {
            if (_Debug) Debug.Log("[SceneUtilities] Attempting to load scene '" + sceneToLoad + "'" + (setActive ? ", and set it as the active scene" : "" ));
            StartCoroutine(LoadingScene(sceneToLoad, setActive));
        }

        public void BeginLoadScene( string sceneToLoad ) {
            if (_Debug) Debug.Log("[SceneUtilities] Attempting to begin loading scene '" + sceneToLoad + "'");
            StartCoroutine(BeginLoadingScene(sceneToLoad));
        }

        public void FinishLoadScene( bool setSceneActive ) {
            if (_Debug) Debug.Log("[SceneUtilities] Attempting to finish loading '" + lastSceneStarted + "'");
            StartCoroutine(FinishLoadingScene(setSceneActive));
        }

        public void LoadSceneExclusive( string sceneToLoad ) {

            if (_Debug) Debug.Log("[SceneUtilities] Attempting to load scene '"+sceneToLoad+"' exclusively");
            StartCoroutine(LoadingSceneExclusive(sceneToLoad));
        }

        public void UnloadScene( string scene ) {
            if (_Debug) Debug.Log("[SceneUtilities] Attempting to unload scene '"+scene+"' asynchronously (enforcing a max scene count of 2)");
            StartCoroutine(UnloadingScene(scene, 2, true));
        }

        public void UnloadScene( string scene, int maxSceneCount, bool forceSceneCleanup ) {
            if (_Debug) Debug.Log("[SceneUtilities] Attempting to unload scene '"+scene+"' asynchronously (enforcing a max scene count of "+maxSceneCount+")");
            StartCoroutine(UnloadingScene(scene, maxSceneCount, forceSceneCleanup));
        }

        public void UnloadActiveScene (int maxSceneCount, bool forceSceneCleanup ) {
            if (_Debug) Debug.Log("[SceneUtilities] Attempting to unload scene '"+SceneManager.GetActiveScene().name+"' asynchronously (enforcing a max scene count of "+maxSceneCount+")");
            StartCoroutine(UnloadingScene(SceneManager.GetActiveScene().name, maxSceneCount, forceSceneCleanup));
        }

        public void ForceUnloadUnusedAssets() {
            StartCoroutine(FlushingUnusedAssets());
        }

        public void SetActiveScene(string sceneName) {
            // Make sure the scene is loaded & not already active
            if (!CheckSceneLoaded(sceneName)) {
                Debug.LogError("[SceneUtilities] ERROR -> No scene with name '" + sceneName + "' loaded! Cancelling.'");
                return;
            } else if (CheckSceneActive(sceneName)) {
                Debug.LogWarning("[SceneUtilities] '" + sceneName + "' is already the active scene! Cancelling.'");
                return;
            } else {
                if (_Debug) Debug.Log("[SceneUtilities] Attempting to set '" + sceneName + "' as the active scene");
            }

            StartCoroutine(SettingActiveScene(sceneName));
        }

        #endregion


        #region Loading / Unloading Couroutines

        IEnumerator LoadingScene( string sceneToLoad, bool setActive ) {

            yield return BeginLoadingScene(sceneToLoad);
            yield return FinishLoadingScene(setActive);

        }


        IEnumerator BeginLoadingScene( string sceneToLoad ) {

            // Begin to load the new scene
            asyncOperation = SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Additive);
            asyncOperation.allowSceneActivation = false;
            if (_Debug) Debug.Log("[SceneUtilities] Waiting for scene '" + sceneToLoad + "' to load...");

            // Wait until the new scene is almost loaded
            yield return new WaitUntil(() => asyncOperation.progress >= 0.9f);
            lastSceneStarted = sceneToLoad;

            // Send an event out for the caller script to pick up
            if (OnSceneAlmostReady != null) {
                if (_Debug) Debug.Log("[SceneUtilities] OnSceneAlmostReady event called");
                OnSceneAlmostReady();
            }

        }

       
        IEnumerator FinishLoadingScene( bool setSceneActive ) {

            // Finish loading the new scene
            asyncOperation.allowSceneActivation = true;
            while (!asyncOperation.isDone) {
                yield return null;
            }

            // Set the new scene as active
            if (setSceneActive) {
                if (_Debug) Debug.Log("[SceneUtilities] Attempting to set '" + lastSceneStarted + "' as active");

                while (SceneManager.GetActiveScene().name != lastSceneStarted) {
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(lastSceneStarted));
                    yield return new WaitForSeconds(0.1f);
                }

                // Send an event out for anyone listening
                if (OnSceneMadeActive != null) {
                    if (_Debug) Debug.Log("[SceneUtilities] OnSceneMadeActive event called");
                    OnSceneMadeActive();
                }

                //yield return new WaitUntil(() => CheckSceneActive(lastSceneStarted));
                if (_Debug) Debug.Log("[SceneUtilities] Set '" + lastSceneStarted + "' as active!");

            }

            if (_Debug) Debug.Log("[SceneUtilities] Finished loading '" + lastSceneStarted + "'!");

            // Wait until the dynamic GI is converged
            if (convergeDynamicGI) { 
                if (_Debug) Debug.Log("Waiting for dynamic GI to update");
                yield return new WaitUntil(() => DynamicGI.isConverged);
            }

            // Send an event out for the caller script to pick up
            if (OnSceneLoad != null) {
                if (_Debug) Debug.Log("[SceneUtilities] OnSceneLoad event called");
                OnSceneLoad();
            }


        }


        IEnumerator LoadingSceneExclusive ( string sceneToLoad ) {

            yield return StartCoroutine(UnloadingScene(SceneManager.GetActiveScene().name, 1, true));
            yield return BeginLoadingScene(sceneToLoad);
            yield return FinishLoadingScene(true);
        }



        IEnumerator UnloadingScene( string scene, int maxSceneCount, bool forceSceneCleanup ) {

            // Make sure the scene is not the manager scene
            if (scene == "ManagerScene") {
                Debug.LogError("[SceneUtilities] ERROR -> Cannot unload ManagerScene! It's too important! Stop that...");
                yield break;
            }

            // Unload the scene asynchronously
            asyncOperation = SceneManager.UnloadSceneAsync(scene);
            yield return new WaitUntil(() => !SceneManager.GetSceneByName(scene).isLoaded);
            yield return null;

            // Double check there are the right number of scenes loaded
            if (SceneManager.sceneCount > maxSceneCount) {
                if (_Debug) Debug.Log("[SceneUtilities] Too many scenes, waiting for scene cleanup to complete...");
                if (forceSceneCleanup) ForceSceneCleanup(maxSceneCount);
                yield return new WaitUntil(() => SceneManager.sceneCount <= maxSceneCount);
                if (_Debug) Debug.Log("[SceneUtilities] Scenes cleanup complete!");
            }

            // Flush any unloaded assets out of memory
            if (_Debug) Debug.Log("[SceneUtilities] '" + scene + "' unloaded, flushing unused assets");
            ForceUnloadUnusedAssets();


            // Send an event out for the caller script to pick up
            if (OnSceneUnload != null) {
                if (_Debug) Debug.Log("[SceneUtilities] OnSceneUnload event called");
                OnSceneUnload();
            }


        }

        IEnumerator SettingActiveScene (string sceneName) {
            
            while (SceneManager.GetActiveScene().name != sceneName) {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
                yield return new WaitForSeconds(0.1f);
            }

            // Send an event out for anyone listening
            if (OnSceneMadeActive != null) {
                if (_Debug) Debug.Log("[SceneUtilities] OnSceneMadeActive event called");
                OnSceneMadeActive();
            }

            if (_Debug) Debug.Log("[SceneUtilities] Set '" + sceneName + "' as active!");
        }

        #endregion




        #region Cleanup functions


        IEnumerator FlushingUnusedAssets() {

            if (_Debug) Debug.Log("[SceneUtilities] Flushing unused assets from memory");

            // Flush any unloaded assets out of memory
            asyncOperation = Resources.UnloadUnusedAssets();
            while (!asyncOperation.isDone) {
                yield return null;
            }

            if (_Debug) Debug.Log("[SceneUtilities] Unused assets flushed!");

        }
                       
        void ForceSceneCleanup( int maxSceneCount) {

            // Only the current scene and the ManagerScene are loaded
            if (SceneManager.sceneCount <= maxSceneCount) return;

            // Check which scenes have to be removed
            bool currentSceneFound = false;
            for (int i = 0; i < SceneManager.sceneCount; i++) {
                Scene scene = SceneManager.GetSceneAt(i);

                // Skip the ManagerScene
                if (scene.name == "ManagerScene") continue;

                // Skip the current scene if it's the first one found
                if (scene.name == lastSceneStarted && !currentSceneFound) {
                    currentSceneFound = true;
                    continue;
                }

                // Otherwise, unload the scene
                SceneManager.UnloadSceneAsync(scene);
                
            }
        }

        #endregion
    }

}


