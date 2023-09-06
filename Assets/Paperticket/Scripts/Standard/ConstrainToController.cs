using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.XR;

namespace Paperticket {
    [RequireComponent(typeof(ParentConstraint))]
    public class ConstrainToController : MonoBehaviour {

        enum controllerType { LeftController, RightController, Head}

        ParentConstraint constraint;

        [SerializeField] bool debugging = false;
        [Space(10)]
        [SerializeField] controllerType controller = controllerType.LeftController;
        [Space(5)]
        [SerializeField] Vector3 positionOffset;
        [SerializeField] Vector3 rotationOffset;
        [Space(15)]
        [SerializeField] bool AffectXPosition = true;
        [SerializeField] bool AffectYPosition = true;
        [SerializeField] bool AffectZPosition = true;
        [Space(15)]
        [SerializeField] bool AffectXRotation = true;
        [SerializeField] bool AffectYRotation = true;
        [SerializeField] bool AffectZRotation = true;

        //List<Renderer> renderers = new List<Renderer>();


        // Start is called before the first frame update
        void Start() {

            if (debugging) Debug.Log("[ConstrainToController] Constraining + '" + gameObject.name + "' to " + controller.ToString());

            constraint = GetComponent<ParentConstraint>();

            ConstraintSource source = new ConstraintSource();
            switch (controller) {
                case controllerType.LeftController:
                    source.sourceTransform = PTUtilities.instance.leftController.transform;
                    break;
                case controllerType.RightController:
                    source.sourceTransform = PTUtilities.instance.rightController.transform;
                    break;
                case controllerType.Head:
                    source.sourceTransform = PTUtilities.instance.headProxy;
                    break;
                default:
                    Debug.LogError("[ConstrainToController] ERROR -> Bad ControllerType passed as constraint transform! Cancelling");
                    return;
            }
        

            source.weight = 1;

            constraint.AddSource(source);
            constraint.SetTranslationOffset(0, positionOffset);
            constraint.SetRotationOffset(0, rotationOffset);
            
            // Check which translation axis to include
            if (AffectXPosition) {
                if (AffectYPosition) {
                    if (AffectZPosition) constraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
                    else constraint.translationAxis = Axis.X | Axis.Y;                         
                } else if (AffectZPosition) constraint.translationAxis = Axis.X | Axis.Z;
                else constraint.translationAxis = Axis.X;
            } else if (AffectYPosition) {
                if (AffectZPosition) constraint.translationAxis = Axis.Y | Axis.Z;
                else constraint.translationAxis = Axis.Y;
            } else if (AffectZPosition) constraint.translationAxis = Axis.Z;
            else constraint.translationAxis = Axis.None;

            // Check which rotation axis to include
            if (AffectXRotation) {
                if (AffectYRotation) {
                    if (AffectZRotation) constraint.rotationAxis = Axis.X | Axis.Y | Axis.Z;
                    else constraint.rotationAxis = Axis.X | Axis.Y;
                } else if (AffectZRotation) constraint.rotationAxis = Axis.X | Axis.Z;
                else constraint.rotationAxis = Axis.X;
            } else if (AffectYRotation) {
                if (AffectZRotation) constraint.rotationAxis = Axis.Y | Axis.Z;
                else constraint.rotationAxis = Axis.Y;
            } else if (AffectZRotation) constraint.rotationAxis = Axis.Z;
            else constraint.rotationAxis = Axis.None;


            constraint.constraintActive = true;




        }

        /// <summary>
        /// NOTE: Re-enable these vvvvv when Oculus is installed
        /// </summary>

        //void OnEnable() {
        //    OVRManager.InputFocusLost += QuestFocusLost;
        //    OVRManager.InputFocusAcquired += QuestFocusAcquired;
        //}

        //void OnDisable() {
        //    OVRManager.InputFocusLost -= QuestFocusLost;
        //    OVRManager.InputFocusAcquired -= QuestFocusAcquired;
        //}


        //bool focusLost = false;


        void QuestFocusLost() {
            if (/*!focusLost && */controller != controllerType.Head) {
                constraint.constraintActive = false;

                //// Get renderers on hands to disable them if Oculus input focus is lost
                //foreach (Renderer rend in GetComponentsInChildren<Renderer>(false)) {
                //    renderers.Add(rend);
                //    rend.enabled = false;
                //    if (debugging) Debug.Log("[ConstrainToController] Focus lost! Hiding renderer '" + rend.name + "'");
                //}
                //focusLost = true;
            }
        }

        void QuestFocusAcquired() {
            constraint.constraintActive = true;

            //// Enable any renderers that were disabled when oculus input focus was lost
            //if (focusLost && controller != controllerType.Head && renderers.Count > 0) {
            //    foreach (Renderer rend in renderers) {
            //        rend.enabled = true;
            //        renderers.Remove(rend);
            //        if (debugging) Debug.Log("[ConstrainToController] Focus lost! Showing renderer '" + rend.name + "'");
            //    }
            //}
            //focusLost = false;
        }

    }
}