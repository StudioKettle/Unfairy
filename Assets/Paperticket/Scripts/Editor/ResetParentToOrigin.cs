using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ResetParentToOrigin : Editor {

    [MenuItem("Paperticket/Reset Selected Transform/Reset Position")]
    public static void ResetPosition() {
        ResetToOrigin(true, false, false);
    }

    [MenuItem("Paperticket/Reset Selected Transform/Reset Rotation")]
    public static void ResetRotation() {
        ResetToOrigin(false, true, false);
    }

    [MenuItem("Paperticket/Reset Selected Transform/Reset Scale")]
    public static void ResetScale() {
        ResetToOrigin(false, false, true);
    }

    [MenuItem("Paperticket/Reset Selected Transform/Reset All")]
    public static void ResetAll() {
        ResetToOrigin(true, true, true);
    }


    public static void ResetToOrigin (bool resetPosition, bool resetRotation, bool resetScale) {

        
        Transform tempParent = new GameObject("[TempParent]").transform;

        var transforms = Selection.transforms;
        for (int i = 0; i < transforms.Length; i++) {
            Debug.Log("[ResetParentToOrigin] Selected transform = " + transforms[i].name);

            Transform activeTransform = transforms[i]; // Selection.activeTransform;

            Undo.RegisterFullObjectHierarchyUndo(activeTransform.gameObject, "[ResetParentToOrigin] Save original state of hierarchy");

            // Get all children
            List<Transform> children = new List<Transform>();
            for (int j = 0; j < activeTransform.childCount; j++) {
                children.Add(activeTransform.GetChild(j));
            }

            // Set the parent of the children to the temp object
            foreach (Transform child in children) {
                child.SetParent(tempParent, true);
                //Undo.SetTransformParent(child, tempParent, "[ResetParentToOrigin] Change childs parent to temp");
            }


            // Reset the active transform values
            if (resetPosition) activeTransform.position = Vector3.zero;
            if (resetRotation) activeTransform.rotation = Quaternion.identity;
            if (resetScale) activeTransform.localScale = Vector3.one;

            foreach (Transform child in children) {
                //Undo.SetTransformParent(child, activeTransform, "[ResetParentToOrigin] Change childs parent to original");
                child.SetParent(activeTransform, true);
            }



        }

        DestroyImmediate(tempParent.gameObject);





























        //Transform activeTransform = Selection.activeTransform;
        //Transform tempParent = new GameObject("[TempParent]").transform;

        //Undo.RegisterFullObjectHierarchyUndo(activeTransform, "[ResetParentToOrigin] Save original state of hierarchy");

        //List<Transform> children = new List<Transform>(); 
       
        //for (int i = 0; i < activeTransform.childCount; i++) {
        //    children.Add(activeTransform.GetChild(i));
        //}

        //foreach (Transform child in children) {
        //    //child.SetParent(tempParent, true);
        //    Undo.SetTransformParent(child, tempParent, "[ResetParentToOrigin] Change childs parent to temp");
        //}


        ////Debug.Log("number of children 2 = " + activeTransform.childCount);
        
        //if (resetPosition) activeTransform.position = Vector3.zero;
        //if (resetRotation) activeTransform.rotation = Quaternion.identity;
        //if (resetScale) activeTransform.localScale = Vector3.one;

        //foreach (Transform child in children) {
        //    Undo.SetTransformParent(child, activeTransform, "[ResetParentToOrigin] Change childs parent to original");
        //    //child.SetParent(activeTransform, true);
        //}

        ////Debug.Log("number of children 3 = " + activeTransform.childCount);

        //DestroyImmediate(tempParent.gameObject);

        

    }



}
