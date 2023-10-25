using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Paperticket {
    //[RequireComponent(typeof(ConfigurableJoint))]
    public class PhysicsConstraint : MonoBehaviour {
        enum TargetType { Transform, LeftController, RightController, Head }

        [SerializeField] bool debugging = false;
        [Space(10)]
        [SerializeField] TargetType targetType = TargetType.Transform;
        [Space(5)]
        [SerializeField] Vector3 positionOffset;
        [SerializeField] Vector3 rotationOffset;
        [Space(5)]
        [SerializeField] float maxDist = 1f;
        [SerializeField] float maxVelocity = 1f;
        [SerializeField] Transform transformTarget = null;

        ConfigurableJoint joint = null;
        Rigidbody rigidbody = null;

        Transform follower = null;
        Rigidbody followerRigidbody = null;

        // Start is called before the first frame update
        void Start() {

            rigidbody = GetComponent<Rigidbody>();

            switch (targetType) {
                case TargetType.Transform:
                    if (transformTarget == null) {
                        Debug.Log("[PhysicsConstraint] ERROR -> Set to Target but no target transform defined! Disabling.");
                        enabled = false;
                        return;
                    }
                    break;
                case TargetType.LeftController:
                    transformTarget = PTUtilities.instance.leftController.transform;
                    break;
                case TargetType.RightController:
                    transformTarget = PTUtilities.instance.rightController.transform;
                    break;
                case TargetType.Head:
                    transformTarget = PTUtilities.instance.headProxy;
                    break;
                default:
                    Debug.LogError("[ConstrainToController] ERROR -> Bad TargetType passed as target transform! Cancelling.");
                    return;
            }

            if (debugging) Debug.Log("[PhysicsConstraint] Physics constraining + '" + gameObject.name + "' to " + transformTarget.name);



            //joint = GetComponent<ConfigurableJoint>();

            //follower = new GameObject("[PhysicsConstraintTarget] '" + transformTarget + "' Follower", typeof(Rigidbody)).transform;
            //followerRigidbody = follower.GetComponent<Rigidbody>();
            //followerRigidbody.isKinematic = true;
            //followerRigidbody.useGravity = false;
            //joint.connectedBody = followerRigidbody;
        }

        // Update is called once per frame
        void FixedUpdate() {
            //follower.position = transformTarget.position + positionOffset;
            //follower.rotation = Quaternion.Euler(transformTarget.rotation.eulerAngles + rotationOffset);

            var dist = Vector3.Distance(transformTarget.position, transform.position);
            if (dist < maxDist) {
                rigidbody.velocity = (transformTarget.position - transform.position).normalized * (maxVelocity * (1.0f - dist / maxDist));
            } else {
                rigidbody.velocity = Vector3.zero;
            }
        }
    }
}