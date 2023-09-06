using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Paperticket {
    public class BundleEvent : MonoBehaviour {

        [SerializeField] AssetBundles requiredBundle = 0;
        [Space(10)]
        [SerializeField] float checkFrequency = 1f;
        [SerializeField] bool OneTimeUse = true;
        [SerializeField] bool debug = false;

        float timeToChange = 0;

        [Space(5)]
        [SerializeField] UnityEvent2 bundleEvent = null;

        // Start is called before the first frame update
        void OnEnable() {
            timeToChange = Time.time + checkFrequency;
        }

        void Update() {
            if (bundleEvent == null) return;
            if (Time.time > timeToChange) {
                
                // Send the event if the bundle is loaded
                if (DataUtilities.instance.isBundleLoaded(requiredBundle)) {
                    if (debug) Debug.Log("[BundleEvent] Bundle '"+requiredBundle.ToString()+"' is loaded! Sending event...");
                    bundleEvent.Invoke();

                    // Destroy this script if this is a one time use, otherwise reset
                    if (OneTimeUse ) {
                        if (GetComponents<Component>().Length > 2 || transform.childCount > 0) {
                            if (debug) Debug.Log("[TimedEvent] One time use is enabled, disabling this script");
                            Destroy(this);
                        } else {
                            if (debug) Debug.Log("[TimedEvent] One time use is enabled, destroying this object");
                            Destroy(gameObject);
                        }

                    } 
                } else {
                    if (debug) Debug.Log("[BundleEvent] Bundle '" + requiredBundle.ToString() + "' not loaded yet...");
                }

                timeToChange = Time.time + checkFrequency;
            }

        }
    }
}