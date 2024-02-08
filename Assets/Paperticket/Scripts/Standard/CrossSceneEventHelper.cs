using System.Collections;
using UnityEngine;
using TMPro;
using Paperticket;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;

public class CrossSceneEventHelper : MonoBehaviour {


    [System.Serializable]
    public enum CurveType { Constant, Linear, EaseInOut, EaseIn, EaseOut }

    AnimationCurve convertedCurve(CurveType curveType) {
        switch (curveType) {
            case CurveType.Constant: return AnimationCurve.Constant(0, 1, 1);
            case CurveType.Linear: return AnimationCurve.Linear(0, 0, 1, 1);
            case CurveType.EaseInOut: return AnimationCurve.EaseInOut(0, 0, 1, 1);
            case CurveType.EaseIn: return PTUtilities.instance.easeInCurve;
            case CurveType.EaseOut: return PTUtilities.instance.easeOutCurve;
            default: Debug.LogError("[{0}] ERROR -> Bad CurveType recieved in ConvertedCurve!!"); return null;
        }
    }

    [System.Serializable]
    public enum TrackingType { Head, LeftController, RightController, BetweenControllers }



    //#region Careplays calls

    //public void LoadCareScene( CareScene sceneToLoad ) {
    //    Debug.LogWarning("[CrossSceneEventHelper] Attempting to load new care scene: " + sceneToLoad.ToString());
    //    CareplaysManager.instance.LoadCareScene(sceneToLoad);
    //}

    //public void CompleteInductionModule( CareScene moduleToComplete ) {
    //    switch (moduleToComplete) {

    //        case CareScene.IN02_Choice:
    //            CareplaysManager.instance.IN01HonestyComplete = true;
    //            CareplaysManager.instance.IN01VideoIndex += 1;
    //            break;
    //        case CareScene.IN03_Reporting:
    //            CareplaysManager.instance.IN01ReportComplete = true;
    //            CareplaysManager.instance.IN01VideoIndex += 1;
    //            break;
    //        case CareScene.IN04_Cigarette:
    //            CareplaysManager.instance.IN01ChoiceComplete = true;
    //            CareplaysManager.instance.IN01VideoIndex += 1;
    //            break;
    //        case CareScene.IN05_Family:
    //            CareplaysManager.instance.IN01CulturalComplete = true;
    //            CareplaysManager.instance.IN01VideoIndex += 1;
    //            break;
    //        case CareScene.IN06_Privacy:
    //            CareplaysManager.instance.IN01PrivacyComplete = true;
    //            CareplaysManager.instance.IN01VideoIndex += 1;
    //            break;
    //        case CareScene.IN01_Modules:
    //        case CareScene.DesertMenu:
    //        case CareScene.WE01_Onboarding:
    //        case CareScene.WE02_Jetty:
    //        case CareScene.WE03_Dawn:
    //        case CareScene.WE04_Finale:
    //        default:
    //            Debug.LogError("CrossSceneEventHelper] ERROR -> Bad Induction module received: " + moduleToComplete);
    //            break;
    //    }
    //}

    //public void SetWE02VideoIndex(int index) {
    //    CareplaysManager.instance.WE02VideoIndex = index;
    //}

    //public void SetWE03VideoIndex( int index ) {
    //    CareplaysManager.instance.WE03VideoIndex = index;
    //}

    //public void ResetExperience() {
    //    CareplaysManager.instance.IN01ChoiceComplete = false;
    //    CareplaysManager.instance.IN01CulturalComplete = false;
    //    CareplaysManager.instance.IN01HonestyComplete = false;
    //    CareplaysManager.instance.IN01PrivacyComplete = false;
    //    CareplaysManager.instance.IN01ReportComplete = false;
    //    CareplaysManager.instance.IN01VideoIndex = 0;
    //    CareplaysManager.instance.WE02VideoIndex = 0;
    //    CareplaysManager.instance.WE03VideoIndex = 0;
    //}


    //#endregion

    #region Scene loading/unloading calls


    // NOTE: You shouldn't need to use these direct scene calls anymore
    // All loading/unloading is done with the above LoadCareScene function
    // (This also applies to the bundle loading/unloading further down)

    public void SwitchToScene(string sceneName, float invokeTime) {
        StartCoroutine(WaitThenSwitchToScene(sceneName, invokeTime));
    }
    IEnumerator WaitThenSwitchToScene(string sceneName, float invokeTime) {
        yield return new WaitForSeconds(invokeTime);
        SwitchToScene(sceneName);
    }

    public void SwitchToScene(string sceneName) {
        SceneUtilities.instance.LoadSceneExclusive(sceneName);
    }



    public void LoadNextScene(string sceneName, float invokeTime) {
        StartCoroutine(WaitThenLoadNextScene(sceneName, invokeTime));
    }
    IEnumerator WaitThenLoadNextScene(string sceneName, float invokeTime) {
        yield return new WaitForSeconds(invokeTime);
        LoadNextScene(sceneName);
    }


    public void LoadNextScene(string sceneName) {
        SceneUtilities.instance.BeginLoadScene(sceneName);
        SceneUtilities.OnSceneAlmostReady += LoadSceneCallback;

    }
    void LoadSceneCallback() {

        SceneUtilities.OnSceneAlmostReady -= LoadSceneCallback;
        SceneUtilities.instance.FinishLoadScene(true);
        SceneUtilities.instance.UnloadScene(gameObject.scene.name);

    }

    public void SetActiveScene(string sceneName) {
        SceneUtilities.instance.SetActiveScene(sceneName);
    }


    #endregion


    #region Headset / controller calls

    public void SetControllerBeam(bool toggle) {
        PTUtilities.instance.ControllerBeamActive = toggle;
    }

    public void SetControllerInteractionLayers(LayerMask layerMask) {
        PTUtilities.instance.ControllerBeamLayerMask = layerMask;
    }


    /// <summary>
    /// NOTE: Re-enable this vvvvv when Oculus is installed
    /// </summary>
    public void DoHaptics(Hand hand, float strength, float duration) {

        //PTUtilities.instance.DoHaptics(hand, strength, duration);
        // StartCoroutine(OculusUtilities.instance.DoingHaptics(strength, strength, duration, hand));

    }

    public void SetHandLayer(Hand hand, LayerMask layer) {

        // Set right or both hands
        if (hand != Hand.Left) {
            PTUtilities.instance.rightController.GetComponentInChildren<HandAnimController>().SetHandLayer(layer);
        }
        // Set left or both hands
        if (hand != Hand.Right) {
            PTUtilities.instance.leftController.GetComponentInChildren<HandAnimController>().SetHandLayer(layer);
        }

    }




    public void MatchHeadsetTransform(Transform target) {

        target.position = PTUtilities.instance.HeadsetPosition();
        target.rotation = PTUtilities.instance.HeadsetRotation();

    }

    public void FaceHeadsetTransform(Transform target, float distance) {

        target.position = PTUtilities.instance.HeadsetPosition();
        target.rotation = PTUtilities.instance.HeadsetRotation();
        target.Rotate(0, 180, 0, Space.Self);
        target.Translate(Vector3.Scale(Vector3.back * distance, target.localScale), Space.Self);

    }

    [System.Serializable]
    public enum FaceValue { KeepCurrent, FaceHeadset, FaceTarget }
    public void InterposeWithHeadset(Transform intercepter, Transform target, FaceValue facing, float distance) {
        InterposeWithHeadset(intercepter, target.position, facing, distance);
    }
    public void InterposeWithHeadset(Transform intercepter, Vector3 target, FaceValue facing, float distance) {
        var headPos = PTUtilities.instance.HeadsetPosition();

        intercepter.position = Vector3.MoveTowards(headPos, target, distance);

        if (facing == FaceValue.FaceHeadset) {
            intercepter.LookAt(headPos);
        } else if (facing == FaceValue.FaceTarget) {
            intercepter.LookAt(target);
        }
    }



    Coroutine headFadeCo;
    public void FadeHeadsetColor(Color color, float duration) {
        if (headFadeCo != null) StopCoroutine(headFadeCo);
        headFadeCo = StartCoroutine(PTUtilities.instance.FadeColorTo(PTUtilities.instance.headGfx, color, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeHeadsetColor(Color color, float duration, CurveType curveType) {
        if (headFadeCo != null) StopCoroutine(headFadeCo);
        headFadeCo = StartCoroutine(PTUtilities.instance.FadeColorTo(PTUtilities.instance.headGfx, color, duration, convertedCurve(curveType), TimeScale.Scaled));
    }


    public void FadeHeadsetIn(float duration) {
        if (headFadeCo != null) StopCoroutine(headFadeCo);
        headFadeCo = StartCoroutine(PTUtilities.instance.FadeAlphaTo(PTUtilities.instance.headGfx, 0f, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeHeadsetIn(float duration, CurveType curveType) {
        if (headFadeCo != null) StopCoroutine(headFadeCo);
        headFadeCo = StartCoroutine(PTUtilities.instance.FadeAlphaTo(PTUtilities.instance.headGfx, 0f, duration, convertedCurve(curveType), TimeScale.Scaled));
    }

    public void FadeHeadsetOut(float duration) {
        if (headFadeCo != null) StopCoroutine(headFadeCo);
        headFadeCo = StartCoroutine(PTUtilities.instance.FadeAlphaTo(PTUtilities.instance.headGfx, 1f, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeHeadsetOut(float duration, CurveType curveType) {
        if (headFadeCo != null) StopCoroutine(headFadeCo);
        headFadeCo = StartCoroutine(PTUtilities.instance.FadeAlphaTo(PTUtilities.instance.headGfx, 1f, duration, convertedCurve(curveType), TimeScale.Scaled));
    }


    public void FadeHeadsetToBlack(float duration) {
        FadeHeadsetColor(Color.black, duration);
    }

    public void FadeHeadsetToWhite(float duration) {
        FadeHeadsetColor(Color.white, duration);
    }


    public void UnscaledFadeHeadsetColor(Color color, float duration) {
        if (headFadeCo != null) StopCoroutine(headFadeCo);
        headFadeCo = StartCoroutine(PTUtilities.instance.FadeColorTo(PTUtilities.instance.headGfx, color, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Unscaled));
    }





    public void TeleportPlayer(Vector3 worldPosition, Vector3 forwardDirection) {
        PTUtilities.instance.TeleportPlayer(worldPosition, forwardDirection);
    }

    public void TeleportPlayer(Transform targetTransform) {
        PTUtilities.instance.TeleportPlayer(targetTransform);
    }
    public void TeleportPlayer(Transform targetTransform, bool rotatePlayer) {
        PTUtilities.instance.TeleportPlayer(targetTransform, rotatePlayer);
    }


    #endregion



    #region Oculus calls







    #endregion




    #region General purpose calls

    public void CreateGameObject(GameObject prefab, Vector3 position, Vector3 rotation) {
        Instantiate(prefab, position, Quaternion.Euler(rotation));
    }

    public void CreateGameObject(GameObject prefab, Transform parent, Vector3 position, Vector3 rotation) {
        Instantiate(prefab, position, Quaternion.Euler(rotation), parent);
    }

    public void CreateGameObject(GameObject prefab, Transform copyTransform) {
        Instantiate(prefab, copyTransform.position, copyTransform.rotation);
    }

    public void CreateGameObject(GameObject prefab, Transform parent, Transform copyTransform) {
        Instantiate(prefab, copyTransform.position, copyTransform.rotation, parent);
    }

    public void CreateGameObject(GameObject prefab, bool spawnInManagerScene, Vector3 position, Vector3 rotation) {
        GameObject createdObject = null;
        if (spawnInManagerScene) {
            createdObject = Instantiate(prefab, position, Quaternion.Euler(rotation), PTUtilities.instance.transform);
        } else {
            createdObject = Instantiate(prefab, position, Quaternion.Euler(rotation), transform);
        }
        createdObject.transform.parent = null;
    }

    public void DestroyGameObject(GameObject objectToDestroy) {
        Destroy(objectToDestroy);
    }

    public void DestroyComponent(Component componentToDestroy) {
        Destroy(componentToDestroy);
    }




    public void SetTimeScale(float timeScale) {
        PTUtilities.instance.TimeScale = timeScale;
    }




    public void SetLayer(GameObject gameObject, LayerMask layerMask) {
        gameObject.layer = (int)Mathf.Log(layerMask.value, 2);
    }

    public void SetLayer(GameObject gameObject, LayerMask layerMask, bool includeChildren) {
        gameObject.layer = (int)Mathf.Log(layerMask.value, 2);
        if (includeChildren) {

            foreach (Transform child in gameObject.transform.GetAllChildren()) {
                child.gameObject.SetLayer(layerMask);
            }
        }
    }

    #endregion




    #region Transform calls



    public void ShakeTransform(Transform target, Vector3 shakePosition, Vector3 shakeRotation, float duration) {
        StartCoroutine(PTUtilities.instance.ShakeTransform(target, shakePosition, shakeRotation, duration, TimeScale.Scaled));
    }

    public void ShakeRotation(Transform target, Vector3 shakeAmount, float duration) {
        StartCoroutine(PTUtilities.instance.ShakeRotation(target, shakeAmount, duration, TimeScale.Scaled));
    }


    public void MoveTransformViaCurve(Transform target, CurveType curveType, Vector3 moveAmount, float duration) {
        StartCoroutine(PTUtilities.instance.MoveTransformViaCurve(target, convertedCurve(curveType), moveAmount, duration, TimeScale.Scaled));
    }
    public void MoveTransformViaCurve(Transform target, CurveType curveType, Transform matchTransform, float duration) {
        StartCoroutine(PTUtilities.instance.MoveTransformViaCurve(target, convertedCurve(curveType), matchTransform, duration, TimeScale.Scaled));
    }

    public void RotateTransformViaCurve(Transform target, CurveType curveType, Vector3 rotateAmount, float duration) {
        StartCoroutine(PTUtilities.instance.RotateTransformViaCurve(target, convertedCurve(curveType), rotateAmount, duration, TimeScale.Scaled));
    }
    public void RotateTransformViaCurve(Transform target, CurveType curveType, Transform matchTransform, float duration) {
        StartCoroutine(PTUtilities.instance.RotateTransformViaCurve(target, convertedCurve(curveType), matchTransform, duration, TimeScale.Scaled));
    }

    public void ScaleTransformViaCurve(Transform target, CurveType curveType, Vector3 scaleAmount, float duration) {
        StartCoroutine(PTUtilities.instance.ScaleTransformViaCurve(target, convertedCurve(curveType), scaleAmount, duration, TimeScale.Scaled));
    }
    public void ScaleTransformViaCurve(Transform target, CurveType curveType, Transform matchTransform, float duration) {
        StartCoroutine(PTUtilities.instance.ScaleTransformViaCurve(target, convertedCurve(curveType), matchTransform, duration, TimeScale.Scaled));
    }





    public void UnscaledMoveTransformViaCurve(Transform target, CurveType curveType, Vector3 moveAmount, float duration) {
        StartCoroutine(PTUtilities.instance.MoveTransformViaCurve(target, convertedCurve(curveType), moveAmount, duration, TimeScale.Unscaled));

    }

    public void UnscaledScaleTransformViaCurve(Transform target, CurveType curveType, Vector3 scaleAmount, float duration) {
        StartCoroutine(PTUtilities.instance.ScaleTransformViaCurve(target, convertedCurve(curveType), scaleAmount, duration, TimeScale.Unscaled));

    }

    public void UnscaledRotateTransformViaCurve(Transform target, CurveType curveType, Vector3 rotateAmount, float duration) {
        StartCoroutine(PTUtilities.instance.RotateTransformViaCurve(target, convertedCurve(curveType), rotateAmount, duration, TimeScale.Unscaled));

    }




    public void TeleportGameObject(GameObject targetObject, Transform targetTransform) {
        targetObject.transform.position = targetTransform.position;
        targetObject.transform.rotation = targetTransform.rotation;
    }
    public void TeleportGameObject(GameObject targetObject, Transform targetTransform, bool rotateObject) {
        targetObject.transform.position = targetTransform.position;
        if (rotateObject) targetObject.transform.rotation = targetTransform.rotation;
    }




    public void LookAt(Transform looker, Transform target) {
        looker.LookAt(target);
    }

    public void LookAwayFrom(Transform looker, Transform target) {
        looker.LookAwayFrom(target);
    }



    public void MoveToIntercept(Transform intercepter, Transform origin, Transform target, float maxDelta) {
        
       intercepter.position = Vector3.MoveTowards(intercepter.position,intercepter.position.NearestPointOnLine(origin.position, target.position), maxDelta);

    }




    #endregion




    #region Mesh calls
    public void FadeMeshIn( MeshRenderer mesh, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(mesh, 1, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeMeshOut( MeshRenderer mesh, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(mesh, 0, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeMesh( MeshRenderer mesh, float alpha, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(mesh, alpha, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeMesh(MeshRenderer mesh, string propertyName, float alpha, float duration) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(mesh, propertyName, alpha, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeMeshColor( MeshRenderer mesh, Color color, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeColorTo(mesh, color, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeMeshColor(MeshRenderer mesh, string propertyName, Color color, float duration) {
        StartCoroutine(PTUtilities.instance.FadeColorTo(mesh, propertyName, color, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeMeshColor(MeshRenderer mesh, string propertyName, Color color, float intensity, float duration) {
        StartCoroutine(PTUtilities.instance.FadeColorTo(mesh, propertyName, color * intensity, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeMeshFloatProp(MeshRenderer mesh, string propertyName, float targetValue, float duration, CurveType curveType) {
        StartCoroutine(PTUtilities.instance.FadeMeshFloatPropTo(mesh, propertyName, targetValue, duration, convertedCurve(curveType), TimeScale.Scaled));
    }

    public void SetMeshAlpha( MeshRenderer mesh, float alpha ) {

        Material mat = mesh.material;

        string propertyName = mat.HasProperty("_BaseColor") ? "_BaseColor" : mat.HasProperty("_Color") ? "_Color" : "";
        if (propertyName == "") {
            Debug.LogError("[CrossSceneEventHelper] ERROR -> Could not find property name of material! Cancelling set mesh alpha.");
            return;
        }

        if (mat.GetColor(propertyName).a != alpha) {
            Color col = mat.GetColor(propertyName);
            mat.SetColor(propertyName, new Color(col.r, col.g, col.b, alpha));
        }

        if (alpha == 0) mesh.enabled = false;
        else mesh.enabled = true;

    }



    public void UnscaledFadeMesh( MeshRenderer mesh, float alpha, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(mesh, alpha, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Unscaled));
    }
    public void UnscaledFadeMeshColor( MeshRenderer mesh, Color color, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeColorTo(mesh, color, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Unscaled));
    }


    #endregion


    #region Sprite calls

    public void FadeSpriteIn( SpriteRenderer sprite, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(sprite, 1, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeSpriteOut( SpriteRenderer sprite, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(sprite, 0, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }

    public void FadeSprite( SpriteRenderer sprite, float alpha, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(sprite, alpha, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeSpriteColor( SpriteRenderer sprite, Color color, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeColorTo(sprite, color, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }


    public void UnscaledFadeSprite( SpriteRenderer sprite, float alpha, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(sprite, alpha, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Unscaled));
    }
    public void UnscaledFadeSpriteColor( SpriteRenderer sprite, Color color, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeColorTo(sprite, color, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Unscaled));
    }


    #endregion


    #region Text calls
        
    public void FadeTextIn(TextMeshPro text, float duration) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(text, 1, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeTextOut(TextMeshPro text, float duration) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(text, 0, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    
    public void FadeText( TextMeshPro text, float alpha, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(text, alpha, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeTextColor( TextMeshPro text, Color color, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeColorTo(text, color, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }


    public void UnscaledFadeText( TextMeshPro text, float alpha, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(text, alpha, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Unscaled));
    }
    public void UnscaledFadeTextColor( TextMeshPro text, Color color, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeColorTo(text, color, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Unscaled));
    }

    #endregion


    #region UI Image calls 


    public void FadeImageIn( Image image, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(image, 1, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeImageOut( Image image, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(image, 0, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }

    public void FadeImage( Image image, float alpha, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(image, alpha, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadeImageColor( Image image, Color color, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeColorTo(image, color, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }


    public void UnscaledFadeImage( Image image, float alpha, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeAlphaTo(image, alpha, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Unscaled));
    }
    public void UnscaledFadeImageColor( Image image, Color color, float duration ) {
        StartCoroutine(PTUtilities.instance.FadeColorTo(image, color, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Unscaled));
    }


    #endregion


    #region Light calls

    public void FadeLightColor(Light light, Color color, float duration) {
        StartCoroutine(PTUtilities.instance.FadeColorTo(light, color, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }

    public void FadeLightIntensity(Light light, float intensity, float duration) {
        StartCoroutine(PTUtilities.instance.FadeLightIntensityTo(light, intensity, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }

    public void ShiftLightRange(Light light, float range, float duration) {
        StartCoroutine(PTUtilities.instance.ShiftRangeTo(light, range, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }

    public void SetLightCullingMask(Light light, LayerMask layerMask) {
        light.cullingMask = (int)Mathf.Log(layerMask.value, 2);
    }



    #endregion


    #region Volume calls


    public void FadePostVolume( Volume volume, float targetWeight, float duration ) {
        StartCoroutine(PTUtilities.instance.FadePostVolumeTo(volume, targetWeight, duration, AnimationCurve.Linear(0,0,1,1), TimeScale.Scaled));
    }
    public void FadePostVolume(Volume volume, float targetWeight, float duration, CurveType curveType) {
        StartCoroutine(PTUtilities.instance.FadePostVolumeTo(volume, targetWeight, duration, convertedCurve(curveType), TimeScale.Scaled));
    }
    public void FadePostVolume(PostProcessVolume volume, float targetWeight, float duration) {
        StartCoroutine(PTUtilities.instance.FadePostVolumeTo(volume, targetWeight, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Scaled));
    }
    public void FadePostVolume(PostProcessVolume volume, float targetWeight, float duration, CurveType curveType) {
        
        StartCoroutine(PTUtilities.instance.FadePostVolumeTo(volume, targetWeight, duration, convertedCurve(curveType), TimeScale.Scaled));
    }

    public void UnscaledFadePostVolume( Volume volume, float targetWeight, float duration ) {
        StartCoroutine(PTUtilities.instance.FadePostVolumeTo(volume, targetWeight, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Unscaled));
    }
    public void UnscaledFadePostVolume(Volume volume, float targetWeight, float duration, CurveType curveType) {
        StartCoroutine(PTUtilities.instance.FadePostVolumeTo(volume, targetWeight, duration, convertedCurve(curveType), TimeScale.Unscaled));
    }
    public void UnscaledFadePostVolume(PostProcessVolume volume, float targetWeight, float duration) {
        StartCoroutine(PTUtilities.instance.FadePostVolumeTo(volume, targetWeight, duration, AnimationCurve.Linear(0, 0, 1, 1), TimeScale.Unscaled));
    }
    public void UnscaledFadePostVolume(PostProcessVolume volume, float targetWeight, float duration, CurveType curveType) {
        StartCoroutine(PTUtilities.instance.FadePostVolumeTo(volume, targetWeight, duration, convertedCurve(curveType), TimeScale.Unscaled));
    }

    #endregion




    #region Audio calls

    public void PlayAudioClip( AudioClip clip, float volume ) {
        PTUtilities.instance.PlayAudioClip(clip, volume);
    }


    public void PlayAudioClip( AudioClip clip, Vector3 worldPosition, float volume ) {
        AudioSource.PlayClipAtPoint(clip, worldPosition, volume);

    }

    public void PlayAudioClip( AudioClip clip, Transform worldPosition, float volume ) {
        PlayAudioClip(clip, worldPosition.position, volume);
    }



    public void PlayAudioClip( AudioClip clip, Vector3 worldPosition, float volume, float minDistance, float maxDistance ) {

        AudioSource tempAudio = new GameObject("[tempaudio]", typeof(AudioSource)).GetComponent<AudioSource>();
        tempAudio.transform.position = worldPosition;

        tempAudio.clip = clip;
        tempAudio.spatialBlend = 1.0f;
        tempAudio.volume = volume;
        tempAudio.minDistance = minDistance;
        tempAudio.maxDistance = maxDistance;
        //tempAudio.outputAudioMixerGroup = PTUtilities.instance.audioMaster.FindMatchingGroups("Master")[0];

        tempAudio.Play();

        Destroy(tempAudio.gameObject, clip.length);
    }

    public void PlayAudioClip(AudioClip clip, Transform worldPosition, float volume, float minDistance, float maxDistance ) {
        PlayAudioClip(clip, worldPosition.position, volume, minDistance, maxDistance);
    }

    public void PlayAudioClip( AudioClip clip, TrackingType trackingType, float volume, float minDistance, float maxDistance ) {

        Vector3 position;
        switch (trackingType) {
            case TrackingType.Head:
                position = PTUtilities.instance.HeadsetPosition();
                break;
            case TrackingType.LeftController:
                position = PTUtilities.instance.leftController.transform.position;
                break;
            case TrackingType.RightController:
                position = PTUtilities.instance.rightController.transform.position;
                break;
            case TrackingType.BetweenControllers:
                position = PTUtilities.instance.rightController.transform.position + 
                                                (PTUtilities.instance.leftController.transform.position -
                                                    PTUtilities.instance.rightController.transform.position) / 2f;
                break;
            default:
                Debug.LogError("[CrossSceneEventHelper] ERROR -> Bad TrackingType passed into PlayAudioClip! Cancelling.");
                return;
        }

        PlayAudioClip(clip, position, volume, minDistance, maxDistance);

    }






    //public void FadeAudioSourceIn( AudioSource source ) {
    //    StartCoroutine(PTUtilities.instance.FadeAudioTo(source, 1f, 0.5f, TimeScale.Scaled));
    //}

    //public void FadeAudioSourceOut( AudioSource source ) {
    //    StartCoroutine(PTUtilities.instance.FadeAudioTo(source, 0f, 0.5f, TimeScale.Scaled));
    //}
    //public void FadeAudioSource( AudioSource source, float volume, float duration ) {
    //    StartCoroutine(PTUtilities.instance.FadeAudioTo(source, volume, duration, TimeScale.Scaled));
    //}


    //public void UnscaledFadeAudioSource( AudioSource source, float volume, float duration ) {
    //    StartCoroutine(PTUtilities.instance.FadeAudioTo(source, volume, duration, TimeScale.Unscaled));
    //}




    //Coroutine mixerFadeCo;
    //public void FadeAudioMasterIn( float duration ) {
    //    if (mixerFadeCo != null) StopCoroutine(mixerFadeCo);
    //    mixerFadeCo = StartCoroutine(PTUtilities.instance.FadeAudioMasterTo(1, duration, TimeScale.Scaled));
    //}

    //public void FadeAudioMasterOut( float duration ) {
    //    if (mixerFadeCo != null) StopCoroutine(mixerFadeCo);
    //    mixerFadeCo = StartCoroutine(PTUtilities.instance.FadeAudioMasterTo(0, duration, TimeScale.Scaled));
    //}

    //public void FadeAudioMaster (float volume, float duration ) {
    //    if (mixerFadeCo != null) StopCoroutine(mixerFadeCo);
    //    mixerFadeCo = StartCoroutine(PTUtilities.instance.FadeAudioMasterTo(volume, duration, TimeScale.Scaled));
    //}



    //public void UnscaledFadeAudioMaster( float volume, float duration ) {
    //    if (mixerFadeCo != null) StopCoroutine(mixerFadeCo);
    //    mixerFadeCo = StartCoroutine(PTUtilities.instance.FadeAudioMasterTo(volume, duration, TimeScale.Unscaled));
    //}

    #endregion

    #region Wwise calls
    public void PostAudioEvent(AK.Wwise.Event audioEvent, GameObject go) {
        PTUtilities.instance.PostAudioEvent(audioEvent, go);
    }

    public void PostAudioEvent(string audioEvent, GameObject go) {
        AkSoundEngine.PostEvent(audioEvent, go);
    }



    

    #endregion


    #region Asset bundle calls



    public void LoadAssetBundle(AssetBundles assetBundle ) {
        if (DataUtilities.instance.isBundleLoaded(assetBundle)) {
            Debug.Log("[CrossSceneEventHelper] AssetBundle '"+assetBundle+"' was already loaded, disregarding load request.");
            return;
        }
        DataUtilities.instance.LoadAssetBundle(assetBundle);
    }

    public void UnloadAssetBundle( AssetBundles assetBundle, bool unloadAllLoadedAssets ) {
        if (!DataUtilities.instance.isBundleLoaded(assetBundle)) {
            Debug.Log("[CrossSceneEventHelper] AssetBundle '" + assetBundle + "' is not loaded, disregarding unload request.");
            return;
        }
        DataUtilities.instance.UnloadAssetBundle(assetBundle, unloadAllLoadedAssets);
    }

    #endregion

}
