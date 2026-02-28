using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    [SerializeField] Transform[] waypoints;
    int currentIndex;

    public Transform GetNext()
    {
        if (waypoints == null || waypoints.Length == 0) return transform;
        Transform wp = waypoints[currentIndex];
        currentIndex = (currentIndex + 1) % waypoints.Length;
        return wp;
    }

    void OnDrawGizmos()
    {
        if (waypoints == null) return;
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            Gizmos.DrawSphere(waypoints[i].position, 0.3f);
            int next = (i + 1) % waypoints.Length;
            if (waypoints[next] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[next].position);
        }
    }
}
