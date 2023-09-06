using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {
    public class TimelineManager : MonoBehaviour {

        [SerializeField] Vector3 initialPosition;
        [SerializeField] Vector3 initialRotation;

        // Start is called before the first frame update
        void OnEnable() {
            //transform.rotation = Quaternion.Euler(initialRotation);
            transform.position = PTUtilities.instance.HeadsetPosition();
            transform.rotation = PTUtilities.instance.HeadsetRotation() * Quaternion.Euler(initialRotation);
            transform.Translate(initialPosition, Space.Self);
        }




    }

}
