using UnityEngine;
using UnityEngine.Events;
using PathCreation;


public class PathTraveller : MonoBehaviour {

    [Header("REFERENCES")]
    public PathCreator pathCreator;


    [Space(10)]
    [Header("CONTROLS")]
    [SerializeField] bool autoStart = true;
    [SerializeField] EndOfPathInstruction endOfPathInstruction;
    [Space(5)]
    [SerializeField] float baseSpeed = 5;
    [SerializeField] AnimationCurve speedCurve = AnimationCurve.Constant(0, 1, 1);
    [Space(5)]
    public bool resetPositionOnEnable = true;
    public bool followRotation = true;
    [Space(10)]
    public float finishMarginTime = 0.05f;
    public UnityEvent2 pathFinished;

    Vector3 startPos;

    [Space(10)]
    [Header("READ ONLY")]
    [SerializeField] float currentTime = 0;
    [Space(5)]
    [SerializeField] float curveMultiplier = 0;
    [SerializeField] float finalSpeed = 0;
    [Space(5)]
    [SerializeField] bool eventThisLoop = false;

    float lastTime = 0;

    //float distanceTravelled;

    bool started = false;

    public float BaseSpeed {
        set { baseSpeed = value; }
    }

    public float CurrentTime {
        get { return currentTime; }
    }



    void Awake() {
        if (pathCreator == null) { Debug.LogError("[PathTraveller] No Pathcreator found! Disabling."); return; }


        startPos = transform.position = pathCreator.path.GetPointAtDistance(0);

        // Subscribed to the pathUpdated event so that we're notified if the path changes during the game
        pathCreator.pathUpdated += OnPathChanged;
    }
    void OnDestroy() {
        // Unsubscribed from the pathUpdated event
        pathCreator.pathUpdated -= OnPathChanged;
    }

    void OnEnable() {
        if (resetPositionOnEnable) {
            transform.position = startPos;
            OnPathChanged();
        }
        if (autoStart) started = true;
    }




    void Update() {
        if (pathCreator == null | !started) return;        

        //distanceTravelled += speed * Time.deltaTime;
        curveMultiplier = speedCurve.Evaluate(currentTime % 1);
        finalSpeed = baseSpeed * curveMultiplier;
        currentTime += finalSpeed * Time.deltaTime;

        //transform.position = pathCreator.path.GetPointAtDistance(distanceTravelled, endOfPathInstruction);
        transform.position = pathCreator.path.GetPointAtTime(currentTime, endOfPathInstruction);

        //if (followRotation) transform.rotation = pathCreator.path.GetRotationAtDistance(distanceTravelled, endOfPathInstruction);
        if (followRotation) transform.rotation = pathCreator.path.GetRotation(currentTime, endOfPathInstruction);

        // Send an event if we've reached the end
        if (pathFinished != null) {
            //var currentTime = pathCreator.path.GetClosestTimeOnPath(transform.position);
            if (!eventThisLoop && currentTime > 1 - finishMarginTime) {
                pathFinished.Invoke();
                eventThisLoop = true;
            } else if (currentTime < lastTime) {
                eventThisLoop = false;
            }
            lastTime = currentTime;
        }
        
    }


    // If the path changes during the game, update the distance travelled so that the follower's position on the new path
    // is as close as possible to its position on the old path
    void OnPathChanged() {
        if (pathCreator == null | !started) return;
        //distanceTravelled = pathCreator.path.GetClosestDistanceAlongPath(transform.position);
        currentTime = pathCreator.path.GetClosestTimeOnPath(transform.position);
    }

}
