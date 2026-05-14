using UnityEngine;
using System.Collections;

public class WaypointMover : MonoBehaviour
{
    [Header("Waypoints")]
    public Transform waypointA;
    public Transform waypointB;
    public Transform waypointC;
    public Transform waypointD;

    [Header("Rotation at Each Waypoint (Y axis only)")]
    public float rotationAtA = 0f;
    public float rotationAtB = 90f;
    public float rotationAtC = 180f;
    public float rotationAtD = 270f;

    [Header("Wait Time at Each Waypoint (seconds)")]
    public float waitAtA = 1f;
    public float waitAtB = 2f;
    public float waitAtC = 1f;
    public float waitAtD = 3f;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float reachDistance = 0.1f;
    public float rotationSpeed = 8f;

    [Header("Options")]
    public bool loop = true;
    public bool pingPong = false;

    private Transform[] waypoints;
    private float[] waitTimes;
    private float[] rotations;
    private int currentIndex = 0;
    private int direction = 1;
    private bool isWaiting = false;

    // Store original X and Z so we NEVER change them
    private float originalX;
    private float originalZ;

    void Start()
    {
        waypoints = new Transform[] { waypointA, waypointB, waypointC, waypointD };
        waitTimes = new float[] { waitAtA, waitAtB, waitAtC, waitAtD };
        rotations = new float[] { rotationAtA, rotationAtB, rotationAtC, rotationAtD };

        // Save the X and Z rotation exactly as set in Inspector — never touch them again
        originalX = transform.eulerAngles.x;
        originalZ = transform.eulerAngles.z;

        if (waypoints[0] != null)
            transform.position = waypoints[0].position;
    }

    void Update()
    {
        if (isWaiting) return;

        Transform target = waypoints[currentIndex];
        if (target == null) return;

        // Rotate toward next waypoint — Y axis only, X and Z locked to original
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            float targetY = Quaternion.LookRotation(dir).eulerAngles.y;
            float smoothY = Mathf.LerpAngle(
                transform.eulerAngles.y,
                targetY,
                rotationSpeed * Time.deltaTime
            );
            // Apply only Y, keep original X and Z
            transform.rotation = Quaternion.Euler(originalX, smoothY, originalZ);
        }

        // Move
        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) <= reachDistance)
        {
            transform.position = target.position;
            StartCoroutine(WaitThenMove());
        }
    }

    IEnumerator WaitThenMove()
    {
        isWaiting = true;

        // Rotate to custom Y at this waypoint — X and Z never change
        float customY = rotations[currentIndex];
        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, customY)) > 0.5f)
        {
            float smoothY = Mathf.LerpAngle(
                transform.eulerAngles.y,
                customY,
                rotationSpeed * Time.deltaTime
            );
            transform.rotation = Quaternion.Euler(originalX, smoothY, originalZ);
            yield return null;
        }
        transform.rotation = Quaternion.Euler(originalX, customY, originalZ);

        yield return new WaitForSeconds(waitTimes[currentIndex]);

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
            if (currentIndex < waypoints.Length - 1)
                currentIndex++;
        }
    }

    void OnDrawGizmos()
    {
        Transform[] pts = { waypointA, waypointB, waypointC, waypointD };
        for (int i = 0; i < pts.Length; i++)
        {
            if (pts[i] == null) continue;
            Gizmos.color = (i == currentIndex) ? Color.yellow : Color.cyan;
            Gizmos.DrawSphere(pts[i].position, 0.2f);
        }
    }
}