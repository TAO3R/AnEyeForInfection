using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RouteObject", menuName = "Scriptable Objects/RouteObject")]
public class RouteObject : ScriptableObject
{
    [Tooltip("List of Transforms the route passes through (in order). Assign in the inspector.")] [SerializeField]
    private List<Transform> points;

    public List<Transform> Points => points;

    [Tooltip("Duration (in seconds) to travel each segment. Must be (points.Count - 1) elements. Assign in the inspector.")]
    [SerializeField]
    private List<float> segmentDurations;
    
    // Cumulative distance
    private float[] _cumulativeDistances;
    private float _totalLength;
    public float TotalLength => _totalLength;

    /// <summary>
    /// Ensures the segment duration list matches the point count
    /// </summary>
    public void Validate()
    {
        if (segmentDurations.Count != Mathf.Max(0, points.Count - 1))
        {
            segmentDurations = new List<float>(new float[Mathf.Max(0, points.Count - 1)]);
            for (int i = 0; i < segmentDurations.Count; i++)
                segmentDurations[i] = 0.1f; // default .1s per segment
        }
    }
    
    /// <summary>
    /// Recalculate segment lengths and cumulative distances.
    /// Call this if points are modified.
    /// </summary>
    public void Recalculate()
    {
        if (points == null || points.Count < 2)
        {
            _cumulativeDistances = new float[0];
            _totalLength = 0f;
            return;
        }

        _cumulativeDistances = new float[points.Count];
        _totalLength = 0f;

        _cumulativeDistances[0] = 0f;
        for (int i = 1; i < points.Count; i++)
        {
            _totalLength += Vector3.Distance(points[i - 1].position, points[i].position);
            _cumulativeDistances[i] = _totalLength;
        }
    }
    
    /// <summary>
    /// Get world position along the route at a given distance
    /// </summary>
    /// <param name="distance">
    /// cumulative distance from the start
    /// </param>
    /// <returns>
    /// Vector3 representing world position
    /// </returns>
    public Vector3 GetPointAtDistance(float distance)
    {
        if (points == null || points.Count == 0)
            return Vector3.zero;    // No route
        if (points.Count == 1 || distance <= 0f)
            return points[0].position;  // Start
        if (distance >= _totalLength)
            return points[^1].position; // End
        
        // Find segment index
        int i = 1;
        while (i < _cumulativeDistances.Length && _cumulativeDistances[i] < distance)
            i++;

        float segStart = _cumulativeDistances[i - 1];
        float segEnd = _cumulativeDistances[i];
        float segT = (distance - segStart) / (segEnd - segStart);

        return Vector3.Lerp(points[i - 1].position, points[i].position, segT);
    }
}
