using UnityEngine;

namespace PathCreation.Examples
{
    // Moves along a path at constant speed.
    // Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
    public class PathFollower : MonoBehaviour
    {
        public PathCreator pathCreator;
        public EndOfPathInstruction endOfPathInstruction;
        public float speed = 5;
        public bool resetPositionOnEnable = true;
        //bool active;
        float distanceTravelled;

        public bool followRotation = true;

        Vector3 startPos;

        void Awake() {
            startPos = transform.position;
            if (pathCreator != null) {
                // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
                pathCreator.pathUpdated += OnPathChanged;
            }
        }

        void OnEnable() {
            if (resetPositionOnEnable) {
                transform.position = startPos;
                OnPathChanged();
            }
        }

        void Update() {
            if (pathCreator != null)
            {
                distanceTravelled += speed * Time.deltaTime;
                transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                if (followRotation) transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
            }
        }

        // If the path changes during the game, update the distance travelled so that the follower's position on the new path
        // is as close as possible to its position on the old path
        void OnPathChanged() {
            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }

        //public void Activate() {
        //    active = true;
        //}

        //public void Deactivate() {
        //    active = false;
        //    if (resetOnDeactivate) distanceTravelled = 0;
        //}
    }
}