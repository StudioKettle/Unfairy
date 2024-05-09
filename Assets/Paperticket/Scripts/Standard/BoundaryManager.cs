using Paperticket;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;

public class BoundaryManager : MonoBehaviour {


    [Header("CONTROLS")]
    [Space(10)]
    [SerializeField] bool startActive = false;
    [Space(5)]
    [SerializeField] float boundaryFetchTimeout = 5;
    [Space(5)]
    [SerializeField] Mesh cornerMesh = null;
    [SerializeField] float cornerSize = 1f;
    [SerializeField] Material cornerMat = null;



    [Header("DEBUG")]
    [Space(10)]
    [SerializeField] bool debugging = false;
    [Space(5)]
    [SerializeField] bool showGizmos = false;
    [SerializeField] float gizmoSize = 1;
    [SerializeField] Color gizmoCornerColour = Color.red;
    [SerializeField] Color gizmoWallsColour = Color.magenta;
    [SerializeField] [Range(0, 1)] float gizmoOpacity = 1;
    [Space(10)]
    [SerializeField] bool forceRefresh = false;

    [Header("READ ONLY")]
    [Space(10)]
    [SerializeField] bool boundaryActive = false;
    [Space(5)]
    [SerializeField] List<Vector3> BoundaryPoints = new List<Vector3>();
    [SerializeField] List<GameObject> BoundaryCorners = new List<GameObject>();
    [SerializeField] private Mesh boundaryMesh = null;

    private XRInputSubsystem xRInputSubSystem = null;
    private Transform playerRig = null;
    private bool setupComplete = false;


    #region Setup


    // Start is called before the first frame update
    void Start() {
        StartCoroutine(Setup());
    }


    IEnumerator Setup() {

        // Wait for PTUtilities
        if (!PTUtilities.instance.SetupComplete) {
            if (debugging) Debug.Log("[BoundaryManager] Starting setup after PTUtilities...");
            yield return new WaitUntil(() => PTUtilities.instance.SetupComplete);
        }
        playerRig = PTUtilities.instance.playerRig.transform;
        
        // Grab the inputsubsystem for getting play area boundary            
        float timeSpentHere = 0;
        while (xRInputSubSystem == null) {
            // Timeout if its been too long
            if (timeSpentHere > boundaryFetchTimeout) {
                Debug.LogError("[PTUtilities] ERROR -> Could not find XRInputSubSystem for play area boundary. Ignoring.");
                break;
            }

            if (debugging) Debug.Log("[PTUtilities] Looking for XRInputSubSystem for play area boundary...");
            var loader = XRGeneralSettings.Instance?.Manager?.activeLoader;
            if (loader != null) xRInputSubSystem = loader.GetLoadedSubsystem<XRInputSubsystem>();

            //if (xRInputSubSystem == null) Debug.LogWarning("[PTUtilities] WARNING -> No XRInputSubSystem found yet, gonna keep trying but this is unusual...");                    
            yield return null;
            timeSpentHere += Time.deltaTime;
        }
        if (xRInputSubSystem != null) {
            xRInputSubSystem.boundaryChanged += RefreshBoundaries;
            if (debugging) Debug.Log("[PTUtilities] XRInputSubSystem for play area boundary found!");
        }
                
        for (int i = 0; i < 4; i++) {
            var go = new GameObject("{BoundaryCorner" + i + "}", typeof(MeshFilter), typeof(MeshRenderer));
            go.transform.parent = transform;
            go.GetComponent<MeshFilter>().mesh = cornerMesh;
            go.GetComponent<MeshRenderer>().material = cornerMat;
            go.SetActive(false);
            BoundaryCorners.Add(go);
        }

        RefreshBoundaries(xRInputSubSystem);

        setupComplete = true;

        if (startActive) SetBoundaryActive(true);
    }



    #endregion



    #region Update methods
    

    private void OnDrawGizmos() {
        if (!showGizmos || !Application.isPlaying || !setupComplete) return;
        if (BoundaryPoints == null) { Debug.LogWarning("[PTUtilities] BoundaryPoints is null! Cannot draw gizmos."); return; }
        if (BoundaryPoints.Count == 0) { Debug.LogWarning("[PTUtilities] Zero BoundaryPoints! Cannot draw gizmos."); return; }

        if (forceRefresh) {
            RefreshBoundaries(xRInputSubSystem);
            forceRefresh = false;
        }

        Gizmos.color = gizmoCornerColour.WithAlpha(gizmoCornerColour.a * gizmoOpacity); ;
        foreach (Vector3 point in BoundaryPoints) {
            Gizmos.DrawSphere(point, 0.1f * gizmoSize);
        }

        Gizmos.color = gizmoWallsColour.WithAlpha(gizmoWallsColour.a * gizmoOpacity);
        Gizmos.DrawWireMesh(boundaryMesh, playerRig.position, playerRig.rotation);
        Gizmos.color = gizmoWallsColour.WithAlpha(0.5f * gizmoWallsColour.a * gizmoOpacity);
        Gizmos.DrawMesh(boundaryMesh, playerRig.position, playerRig.rotation);
        
    }

    #endregion

    #region Public methods

    public void SetBoundaryActive(bool active) {
        if (!setupComplete) { Debug.LogError("[PTUtilities] ERROR -> Tried to set boundary active before setup is complete! Ignoring."); return; }

        foreach (GameObject go in BoundaryCorners) {
            go.SetActive(active);
        }
        boundaryActive = active;
    }

    /// <summary>
    /// Refresh the play area boundaries (mainly done automatically but can be triggered if want)
    /// </summary>
    /// <param name="inputSubsystem">The XRInputSubsystem to check again - should be set automatically but you do you</param>
    public void RefreshBoundaries(XRInputSubsystem inputSubsystem) {
        inputSubsystem = xRInputSubSystem ?? inputSubsystem;
        if (inputSubsystem == null) { Debug.LogError("[PTUtilities] ERROR -> Tried to refresh play area boundary but no XRInputSubsystem could be found! Ignoring."); return; }

        List<Vector3> currentBoundaries = new List<Vector3>();
        if (inputSubsystem.TryGetBoundaryPoints(currentBoundaries)) {
            if (currentBoundaries == null) { /*Debug.LogWarning("[PTUtilities] Tried to refresh play area boundary but no points found? Weird, ignoring.");*/ return; }

            // Update the boundaries only if the points (or the number of points) actually changed
            if (BoundaryPoints != currentBoundaries || BoundaryPoints.Count != currentBoundaries.Count) {
                BoundaryPoints = currentBoundaries;
                UpdateBoundaryMesh();
                Debug.Log("[PTUtilities] Play area boundary successfully updated!");
            }

            transform.position = playerRig.position;
            transform.rotation = playerRig.rotation;

        } else { Debug.LogError("[PTUtilities] ERROR -> Tried to refresh play area boundary but could not find any points! Ignoring."); return; }
    }
    void UpdateBoundaryMesh() {
        Vector3[] vertices = {
                        BoundaryPoints[0],
                        BoundaryPoints[1],
                        BoundaryPoints[1].WithY(2),
                        BoundaryPoints[0].WithY(2),
                        BoundaryPoints[3].WithY(2),
                        BoundaryPoints[2].WithY(2),
                        BoundaryPoints[2],
                        BoundaryPoints[3],
                    };

        int[] triangles = {
                        0, 2, 1, //face front
			            0, 3, 2,
                        2, 3, 4, //face top
			            2, 4, 5,
                        1, 2, 5, //face right
			            1, 5, 6,
                        0, 7, 4, //face left
			            0, 4, 3,
                        5, 4, 7, //face back
			            5, 7, 6,
                        0, 6, 7, //face bottom
			            0, 1, 6
                    };

        if (boundaryMesh == null) boundaryMesh = new Mesh();
        else boundaryMesh.Clear();
        boundaryMesh.vertices = vertices;
        boundaryMesh.triangles = triangles;
        boundaryMesh.Optimize();
        boundaryMesh.RecalculateNormals();


        // Move corners to fit new mesh
        BoundaryCorners[0].transform.position = BoundaryPoints[0];
        BoundaryCorners[1].transform.position = BoundaryPoints[1];
        BoundaryCorners[2].transform.position = BoundaryPoints[2];
        BoundaryCorners[3].transform.position = BoundaryPoints[3];
        
    }

    #endregion
}
