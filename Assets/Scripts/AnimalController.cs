using UnityEngine;
using System.Collections;

public class WaypointMover : MonoBehaviour
{
    // ---------------------------------------------------------------
    //  WAYPOINT SETUP
    //  Drag your 4 empty GameObjects into these slots in the Inspector
    // ---------------------------------------------------------------
    [Header("Waypoints")]
    public Transform waypointA;   // Waypoint 1
    public Transform waypointB;   // Waypoint 2
    public Transform waypointC;   // Waypoint 3
    public Transform waypointD;   // Waypoint 4

    // ---------------------------------------------------------------
    //  TIME INTERVALS  <-- EDIT THESE VALUES
    //  How many seconds the object WAITS at each waypoint
    //  before moving to the next one.
    // ---------------------------------------------------------------
    [Header("Wait Time at Each Waypoint (seconds)")]
    public float waitAtA = 1f;   // Wait 1 sec at Waypoint A
    public float waitAtB = 2f;   // Wait 2 sec at Waypoint B
    public float waitAtC = 1f;   // Wait 1 sec at Waypoint C
    public float waitAtD = 3f;   // Wait 3 sec at Waypoint D

    // ---------------------------------------------------------------
    //  MOVEMENT SETTINGS
    // ---------------------------------------------------------------
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float reachDistance = 0.1f;

    [Header("Options")]
    public bool loop = true;      // Loop back to A after D
    public bool pingPong = false; // Reverse: A->B->C->D->C->B->A

    // ---------------------------------------------------------------
    //  INTERNAL — do not edit
    // ---------------------------------------------------------------
    private Transform[] waypoints;
    private float[] waitTimes;
    private int currentIndex = 0;
    private int direction = 1;
    private bool isWaiting = false;

    void Start()
    {
        // Build arrays from the named fields so the Inspector stays readable
        waypoints = new Transform[] { waypointA, waypointB, waypointC, waypointD };
        waitTimes = new float[] { waitAtA, waitAtB, waitAtC, waitAtD };

        // Start at first waypoint
        if (waypoints[0] != null)
            transform.position = waypoints[0].position;
    }

    void Update()
    {
        if (isWaiting) return;

        Transform target = waypoints[currentIndex];
        if (target == null) return;

        // Move toward the current waypoint
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        // Reached it? Start the wait timer
        if (Vector3.Distance(transform.position, target.position) <= reachDistance)
        {
            transform.position = target.position;
            StartCoroutine(WaitThenMove());
        }
    }

    IEnumerator WaitThenMove()
    {
        isWaiting = true;

        float waitDuration = waitTimes[currentIndex]; // Grab this waypoint's wait time
        Debug.Log($"Reached Waypoint {(char)('A' + currentIndex)} — waiting {waitDuration}s");

        yield return new WaitForSeconds(waitDuration); // <-- THE PAUSE HAPPENS HERE

        AdvanceToNextWaypoint();
        isWaiting = false;
    }

    void AdvanceToNextWaypoint()
    {
        if (pingPong)
        {
            int next = currentIndex + direction;
            if (next >= waypoints.Length || next < 0)
            {
                direction *= -1;
                next = currentIndex + direction;
            }
            currentIndex = next;
        }
        else if (loop)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
        else
        {
            // Stop at the last waypoint
            if (currentIndex < waypoints.Length - 1)
                currentIndex++;
        }
    }

    // Scene view path visualization
    void OnDrawGizmos()
    {
        Transform[] pts = { waypointA, waypointB, waypointC, waypointD };
        string[] labels = { "A", "B", "C", "D" };

        for (int i = 0; i < pts.Length; i++)
        {
            if (pts[i] == null) continue;

            Gizmos.color = (i == currentIndex) ? Color.yellow : Color.cyan;
            Gizmos.DrawSphere(pts[i].position, 0.2f);

#if UNITY_EDITOR
            // Show wait time label next to each waypoint in the Scene view
            float[] times = { waitAtA, waitAtB, waitAtC, waitAtD };
            UnityEditor.Handles.Label(
                pts[i].position + Vector3.up * 0.4f,
                $"[{labels[i]}] wait: {times[i]}s"
            );
#endif

            int next = (i + 1) % pts.Length;
            if (pts[next] != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(pts[i].position, pts[next].position);
            }
        }
    }
}