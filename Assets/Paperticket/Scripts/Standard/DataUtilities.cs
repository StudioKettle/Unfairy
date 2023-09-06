using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {


    public class DataUtilities : MonoBehaviour {

        public static DataUtilities instance = null;



        [Header("CONTROLS")]

        [SerializeField] bool autoloadMainBundle = true;
        [SerializeField] System.Environment.SpecialFolder editorBundleLocation = System.Environment.SpecialFolder.MyDocuments;
        
        #region Expansion Paths

        public string ExpansionFilePath {
            get {

                if (Application.platform == RuntimePlatform.WindowsEditor) {
                    return System.Environment.GetFolderPath(editorBundleLocation) + "/Paperticket Studios/CarePlaysVR/PC Asset Bundles/";

                } else if (Application.platform == RuntimePlatform.WindowsPlayer) {
                    return Application.dataPath + "/Asset Bundles/";

                } else if (Application.platform == RuntimePlatform.Android) {
                    return "/sdcard/Android/obb/" + Application.identifier + "/";

                } else {
                    Debug.LogError("[DataUtilities] Bad platform for ExpansionFilePath!");
                    return "";
                }
            }
        }

        public string ExpansionFileName {
            get {

                if (Application.platform == RuntimePlatform.WindowsEditor) {
                    return "main." + bundleVersion + "." + Application.identifier + ".obb";

                } else if (Application.platform == RuntimePlatform.WindowsPlayer) {
                    return "main." + bundleVersion + "." + Application.identifier + ".obb";

                } else if (Application.platform == RuntimePlatform.Android) {
                    return "main." + bundleVersion + "." + Application.identifier + ".obb";

                } else {
                    Debug.LogError("[DataUtilities] Bad platform for ExpansionFileName!");
                    return "";
                }
            }
        }

        #endregion

               

        [Header("PRELOADED PROGRESS DATA")]
        [Space(20)]
        public bool loadProgressOnStart = false;

        [SerializeField] List<string> StringKeys = null;
        [SerializeField] List<ProgressFloat> FloatKeys = null;
        
        [System.Serializable]
        class ProgressFloat {
            public string String = "";
            public float Float = 0;
        }

               

        [Header("DEBUG OPTIONS")]
        [Space(20)]
        [SerializeField] bool debugging = false;
        [SerializeField] bool frameDebugging = false;
        [Space(10)]
        [SerializeField] bool loadDebugBundles = false;
        [SerializeField] List<AssetBundles> debugBundles = null;

        [Header("LIVE VARIABLES")]
        [Space(20)]
        public List<AssetBundle> loadedBundles = null;
        [Space(20)]
        public bool finishedInitialising;
        int bundleVersion = 0;

        void Awake() {

            // Set this object as the DataUtilities instance
            if (instance == null) {
                instance = this;
            } else if (instance != this) {
                Destroy(gameObject);
            }

            // Load the player's progress
            if (loadProgressOnStart) LoadPlayerProgress();

            // Set bundle version and display start debug message if applicable  
            bundleVersion = int.Parse(Application.version.Replace(".", ""));
            if (debugging) Debug.Log("[DataUtilities] Careplays Version Information -" + "\n" +
                                            "Application identifier = " + Application.identifier + "\n" +
                                            "Application version = " + Application.version + "\n" +
                                            "Bundle version = " + bundleVersion + "\n" +
                                            "Expansion File Path = " + ExpansionFilePath);

            // Load debug bundles if necessary
            if (loadDebugBundles) {
                if (debugBundles.Count > 0) {
                    StartCoroutine(LoadingDebugBundles());
                }
            }

            // If checked, load the main bundle
            if (autoloadMainBundle) LoadMainBundle();
            else finishedInitialising = true;





            //if (debugging) Debug.Log("[DataUtilities]" + _ExpansionAssetBundle == null ? " Failed to load ExpansionAssetBundle" : " ExpansionAssetBundle successfully loaded!");

        }



        #region Public loading/unloading bundle calls

        void LoadMainBundle() {
            if (debugging) Debug.Log("[DataUtilities] Attempting to load AssetBundle '" + ExpansionFileName + "'...");
            StartCoroutine(LoadingMainAssetBundle());
        }

        public void LoadAssetBundle( AssetBundles assetBundle ) {

            //AssetBundle newAssetBundle = AssetBundle.LoadFromFile(ExpansionFilePath + assetBundle.ToString());
            if (debugging) Debug.Log("[DataUtilities] Attempting to load AssetBundle '" + assetBundle.ToString() + "'...");
            StartCoroutine(LoadingAssetBundle(assetBundle));

        }

        public void UnloadAssetBundle( AssetBundles assetBundle, bool unloadAllLoadedObjects ) {

            if (debugging) Debug.Log("[DataUtilities] Attempting to unload AssetBundle '" + assetBundle.ToString() + "'...");

            foreach (AssetBundle bundle in loadedBundles) {
                if (bundle.name == assetBundle.ToString()) {
                    bundle.Unload(unloadAllLoadedObjects);
                    if (debugging) Debug.Log("[DataUtilities] AssetBundle '" + assetBundle.ToString() + "' unloaded!");
                    loadedBundles.Remove(bundle);
                    return;
                }
            }
            Debug.LogError("[DataUtilities] ERROR -> AssetBundle '" + assetBundle.ToString() + "' was not found in Loaded Bundles! Could not unload...");
        }

        public void UnloadAllBundles( bool unloadAllLoadedObjects ) {
            if (debugging) Debug.Log("[DataUtilities] Attempting to unload all loaded asset bundles (except main)");
            StartCoroutine(UnloadingAllAssetBundles(unloadAllLoadedObjects));
        }

        

        #endregion




        #region Public bundle utilities


        public bool isBundleLoaded( AssetBundles assetBundle ) {
            foreach (AssetBundle bundle in loadedBundles) {
                if (bundle == null) {
                    Debug.LogWarning("[DataUtilities] WARNING -> Loaded AssetBundle '" + assetBundle + "' returned null, returning false");
                    return false;

                } else if (bundle.name == assetBundle.ToString()) {
                    return true;
                }
            }
            return false;
        }


        public AssetBundle GetAssetBundle( AssetBundles assetBundle ) {
            foreach (AssetBundle bundle in loadedBundles) {
                if (bundle.name == assetBundle.ToString()) {
                    return bundle;
                }
            }
            if (debugging) Debug.LogWarning("[DataUtilities] Could not find AssetBundle '" + assetBundle.ToString() + "'! Returning null. ");
            return null;
        }

        public AssetBundle mainBundle {
            get {
                foreach (AssetBundle bundle in loadedBundles) {
                    if (bundle.name == "main") {
                        return bundle;
                    }
                }
                Debug.LogError("[DataUtilities] ERROR -> Main bundle could not be found! Something has gone terribly wrong!");
                return null;
            }
        }


        public AssetBundles[] GetLoadedBundles () {
            List<AssetBundles> bundleList = new List<AssetBundles>();            
            foreach(AssetBundle bundle in loadedBundles) {
                if (bundle.name == "main") {
                    if (debugging) Debug.Log("[DataUtilities] Skipping main expansion file in GetLoadBundles...");
                    continue;
                }
                bundleList.Add((AssetBundles)System.Enum.Parse(typeof(AssetBundles), bundle.name));
            }
            if (bundleList.Count > 0) return bundleList.ToArray();
            else return null;
        }

               

        //public Object LoadObjectFromBundle( AssetBundles assetBundle, string objectName) {

        //    // Make sure the bundle is loaded  
        //    if (!isBundleLoaded(assetBundle)) {
        //        Debug.LogWarning("[DataUtilties] ERROR -> Cannot load object '"+objectName+"'! Bundle '"+assetBundle.ToString()+"'is not loaded!");
        //        return null;
        //    }
                                  
        //    AssetBundle bundle = GetAssetBundle(assetBundle); //_ExpansionAssetBundle;
        //    while (bundle == null) {
        //        if (debugging) Debug.Log("[DataUtilties] Attempting to get asset bundle '" + assetBundle.ToString() + "'");
        //        bundle = GetAssetBundle(assetBundle); //_ExpansionAssetBundle;
        //        yield return null;
        //    }
        //    if (debugging) Debug.Log("[DataUtilties] Got the asset bundle '" + bundle + "'");
        //    // Load the video clip from the asset bundle and wait until it's finished
        //    var assetLoadRequest = bundle.LoadAssetAsync(objectName);
        //    yield return assetLoadRequest;

        //    // Treat the video as a VideoClip and give to the video player
        //    return assetLoadRequest.asset;


        //}



        #endregion





        #region Bundle coroutines

        IEnumerator LoadingAssetBundle(AssetBundles assetBundle) {
            var bundleLoadRequest = AssetBundle.LoadFromFileAsync(ExpansionFilePath + assetBundle.ToString() +".obb");
            yield return bundleLoadRequest;

            var loadedBundle = bundleLoadRequest.assetBundle;
            if (loadedBundle == null) {
                Debug.LogError("[DataUtilities] ERROR -> Failed to load AssetBundle '" + assetBundle.ToString() + "'! Ignoring request, this is probably a fatal error :( ");
                yield break;
            }
            loadedBundles.Add(loadedBundle);
            if (debugging) Debug.Log("[DataUtilities] AssetBundle '" + assetBundle.ToString() + "' successfully loaded!");
        }

        IEnumerator LoadingMainAssetBundle() {
            var bundleLoadRequest = AssetBundle.LoadFromFileAsync(ExpansionFilePath + ExpansionFileName);
            yield return bundleLoadRequest;

            var loadedBundle = bundleLoadRequest.assetBundle;
            if (loadedBundle == null) {
                Debug.LogError("[DataUtilities] ERROR -> Failed to load AssetBundle '"+ExpansionFileName+"'! Ignoring request, this is probably a fatal error :( ");
                yield break;
            }
            loadedBundles.Add(loadedBundle);
            if (debugging) Debug.Log("[DataUtilities] AssetBundle '"+ExpansionFileName+"' successfully loaded!");

            finishedInitialising = true;
        }


        IEnumerator UnloadingAllAssetBundles(bool unloadAllLoadedObjects) {

            for (int i = 0; i < loadedBundles.Count; i++) {

                if (loadedBundles[i].name == ExpansionFileName) continue;
                loadedBundles[i].Unload(unloadAllLoadedObjects);
                yield return null;

            }


        }


        IEnumerator LoadingDebugBundles() {
            foreach (AssetBundles newBundle in debugBundles) {
                if (!isBundleLoaded(newBundle)) {
                    LoadAssetBundle(newBundle);
                    yield return new WaitUntil(() => isBundleLoaded(newBundle));
                }
            }
        }


        #endregion






        #region Progress Keys



        void LoadPlayerProgress() {

            // Load all string progress keys
            for (int i = 0; i < StringKeys.Count; i++) {
                SetProgressKey(StringKeys[i]);
                if (debugging) Debug.Log("[DataUtilities] New string progress key added: " + StringKeys[i] + ", " + CheckProgressKey(StringKeys[i]));
            }

            // Load all float progress keys
            for (int i = 0; i < FloatKeys.Count; i++) {
                SetProgressKeyAsFloat(FloatKeys[i].String, FloatKeys[i].Float);
                if (debugging) Debug.Log("[DataUtilities] New float progress key added: " + FloatKeys[i].String + ", value = " + GetProgressKeyAsFloat(FloatKeys[i].String));
            }

        }

                          

        public bool CheckProgressKey( string keyName ) {
            if (PlayerPrefs.HasKey(keyName)) {
                return PlayerPrefs.GetInt(keyName) == 1;
            }
            if (debugging) Debug.Log("[DataUtilities] Cannot find progress key '" + keyName + "' (string)");
            return false;
        }

        public float GetProgressKeyAsFloat( string keyName ) {
            if (PlayerPrefs.HasKey(keyName)) {
                return PlayerPrefs.GetFloat(keyName);
            }
            if (debugging) Debug.Log("[DataUtilities] Cannot find progress key '" + keyName + "' (float)");
            return 0;
        }




        public void SetProgressKey( string keyName ) {
            PlayerPrefs.SetInt(keyName, 1);
            PlayerPrefs.Save();
        }

        public void SetProgressKeyAsFloat( string keyName, float value ) {
            PlayerPrefs.SetFloat(keyName, value);
            PlayerPrefs.Save();
        }



        public void ResetProgressKey( string keyName ) {
            if (PlayerPrefs.HasKey(keyName)) {
                PlayerPrefs.SetInt(keyName, 0);
                PlayerPrefs.Save();
            }
        }


        public void ClearPlayerProgress() {
            if (debugging) Debug.Log("[DataUtilities] Clearing all player progress! OwO");

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            // Load the player's progress
            if (loadProgressOnStart) {
                LoadPlayerProgress();
            }

        }

        #endregion




        #region Debugging 

        private void OnValidate() {
            if (frameDebugging && frameDebugCo == null) StartCoroutine(FrameDebugging());
            else if (!frameDebugging && frameDebugCo != null) StopCoroutine(FrameDebugging());
        }
        Coroutine frameDebugCo;
        IEnumerator FrameDebugging() {
            while (Application.isPlaying) {
                Debug.Log("[DataUtilities] AssetBundle we01: " + isBundleLoaded(AssetBundles.we01) + "\n " +
                          "[DataUtilities] AssetBundle we02: " + isBundleLoaded(AssetBundles.we02) + "\n " +
                          "[DataUtilities] AssetBundle we03: " + isBundleLoaded(AssetBundles.we03) + "\n " +
                          "[DataUtilities] AssetBundle we04: " + isBundleLoaded(AssetBundles.we04) + "\n ");
                yield return new WaitForSeconds(1f);
            }
        }

        #endregion
    }

}









//// Unless we're on PC, make sure all the videos are transfered to the persistant datapath
////if (Application.platform != RuntimePlatform.WindowsPlayer) {
////    StartCoroutine(CheckVideoStatus());
////}
//// NOT USED ATM
//IEnumerator CheckVideoStatus() {
//    if (_Debug) Debug.Log("[DataUtilities] Checking status of video files...");

//    string persistentPath = Path.Combine(Application.persistentDataPath, "Videos");

//    // Make sure that the persistant data path exists
//    if (!Directory.Exists(persistentPath)) {
//        if (_Debug) Debug.Log("[DataUtilities] Path '" + persistentPath + "' missing, creating now.");
//        Directory.CreateDirectory(persistentPath);
//    } else {
//        if (_Debug) Debug.Log("[DataUtilities] Path '" + persistentPath + "' found!");
//    }

//    // Copy each of the video files in turn
//    for (int i = 0; i < _VideoNames.Length; i++) {
//        string completeStreamingPath = Path.Combine(Application.streamingAssetsPath, _VideoNames[i]) + ".mp4";
//        string completePersistantPath = Path.Combine(persistentPath, _VideoNames[i]) + ".mp4";


//        // Skip re-copying if the file already exists
//        if (File.Exists(completePersistantPath)) {
//            if (_Debug) Debug.Log("[DataUtilities] Video '" + _VideoNames[i] + "' already in persistant path, ignoring");
//            //File.Delete(completePersistantPath);
//        } else {

//            // Put in a request for the video content
//            UnityWebRequest www = UnityWebRequest.Get(completeStreamingPath);

//            // Create the destination file in the persistant data path
//            var downloadHandler = new DownloadHandlerFile(completePersistantPath);
//            downloadHandler.removeFileOnAbort = true;

//            www.downloadHandler = downloadHandler;

//            yield return www.SendWebRequest();

//            if (www.isNetworkError || www.isHttpError)
//                Debug.LogError("[DataUtilities] ERROR -> " + www.error);
//            else
//                if (_Debug) Debug.Log("[DataUtilities] Download saved to: " + completePersistantPath.Replace("/", "\\") + "\r\n" + www.error);

//            // Close the web request and remove any resources
//            downloadHandler.Dispose();
//            www.Dispose();

//        }

//    }

//    yield return null;

//    SceneUtilities.instance.ForceUnloadUnusedAssets();

//}
