using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket {
    public static class PTExtensions {


        /// <summary>
        /// Test if layer is included in this layermask
        /// </summary>
        public static bool Test (this LayerMask layerMask, int layer) {
            if (((1 << layer) & layerMask) != 0) return true;
            else return false;
        }

        /// <summary>
        /// Test if this gameobject is has the provided layer and tag
        /// </summary>
        public static bool CheckLayerAndTag(this GameObject gameObject, LayerMask layers, string tag, bool debugging = false) {
            if (!layers.Test(gameObject.layer)) {
                if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' layer '" + LayerMask.LayerToName(gameObject.layer) + "' is invalid, returning false.");
                return false;
            }
            if (tag.Length > 0) {
                string[] splitTags = tag.Split(',');
                foreach (string splitTag in splitTags) {
                    if (gameObject.tag == splitTag) {
                        if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' has valid layer and tag, returning true.");
                        return true;
                    }
                }
            } else {
                if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' has valid layer and tag, returning true.");
                return true;
            }
            if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' tag '" + gameObject.tag + "' is invalid, returning false.");
            return false;
                //if (tag.Contains(",")) {
                //    string[] splitTags = tag.Split(',');
                //    foreach (string splitTag in splitTags) {
                //        if (splitTag == tag) {
                //            if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' has valid layer and tag, returning true.");
                //            return true;
                //        }
                //    }
                //    if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' tag '" + gameObject.tag + "' is invalid, returning false.");
                //    return false;
                //} else if (gameObject.tag != tag) {
                //    if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' tag '" + gameObject.tag + "' is invalid, returning false.");
                //    return false;
                //}                       
            //if (debugging) Debug.Log("[CheckLayerAndTag] GameObject '" + gameObject.name + "' has valid layer and tag, returning true.");
            //return true;
        }

        /// <summary>
        /// Destroy this gameobject
        /// </summary>
        public static void DestroyMe(this GameObject gameObject) {
            GameObject.Destroy(gameObject);
        }


    }
}
