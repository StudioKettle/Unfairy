using UnityEngine;
using UnityEngine.Events;

namespace PathCreation.Examples
{
    // Moves along a path at constant speed.
    // Depending on the end of path instruction, will either loop, reverse, or stop at the end of the path.
    public class PathFollower : MonoBehaviour {
        public PathCreator pathCreator;
        public EndOfPathInstruction endOfPathInstruction;
        public float speed = 5;
        public bool resetPositionOnEnable = true;
        float distanceTravelled;

        public bool followRotation = true;

        [Space(10)]
        public float finishMarginTime = 0.05f;
        public UnityEvent2 pathFinished;

        Vector3 startPos;

        public float Speed {
            set { speed = value; }
        }


        void Awake() {
            startPos = transform.position = pathCreator.path.GetPointAtDistance(0);
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

        bool eventThisLoop = false;
        float lastTime = 0;
        void Update() {
            if (pathCreator != null) {
                distanceTravelled += speed * Time.deltaTime;
                transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
                if (followRotation) transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);

                // Send an event if we've reached the end
                if (pathFinished != null) {
                    var currentTime = pathCreator.path.GetClosestTimeOnPath(transform.position);
                    if (!eventThisLoop && currentTime > 1 - finishMarginTime) {
                        pathFinished.Invoke();
                        eventThisLoop = true;
                    } else if (currentTime < lastTime) {
                        eventThisLoop = false;
                    }
                    lastTime = currentTime;
                }
            }
        }

        // If the path changes during the game, update the distance travelled so that the follower's position on the new path
        // is as close as possible to its position on the old path
        void OnPathChanged() {
            distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        }

    }
}