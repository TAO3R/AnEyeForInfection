using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum InterpolationMode
{
    Linear,
    Quadratic,
    Custom
}

public class ProceduralMove : MonoBehaviour
{
    [SerializeField] private RouteObject route;
    [SerializeField] AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private int _currentSegment;
    private float _t;   // Normalized progress within the current segment
    private int _direction; // +1 forward, -1 backward
    private Coroutine _moveCoroutine;

    private void Awake()
    {
        if (route != null)
        {
            route.Validate();
        }
    }

    public void Move(bool forward = true)
    {
        _direction = forward ? 1 : -1;
        if (_moveCoroutine == null)
        {
            // _moveCoroutine = StartCoroutine(MoveAlongRoute());
        }
    }

    // private IEnumerator MoveAlongRoute()
    // {
    //     if (route == null || route.Points.Count == 0) { yield break; }
    //
    //     yield return null;
    // }
}
