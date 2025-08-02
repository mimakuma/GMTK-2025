using System.Collections.Generic;
using UnityEngine;

public class RopeTrail : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float minDistance = 0.1f;

    private List<Vector3> points = new List<Vector3>();

    void Start()
    {
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        Vector3 currentPos = transform.position;
        if (points.Count == 0 || Vector3.Distance(currentPos, points[points.Count - 1]) > minDistance)
        {
            points.Add(currentPos);
            lineRenderer.positionCount = points.Count;
            lineRenderer.SetPosition(points.Count - 1, currentPos);
        }
    }
}
