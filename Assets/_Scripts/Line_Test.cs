using System;
using UnityEngine;

public class Line_Test : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float rad = 3f;
    [SerializeField, Range(1f, 10f)] private float duration = 1f;

    private float _currentTime = 0f;
    private float _x = 0f;
    private float _y = 0f;
    private float _ratio = 0f;
    
    [SerializeField] private Transform first;
    [SerializeField] private Transform firstSecond;
    [SerializeField] private Transform second;
    
    private void Awake()
    {
        transform.AssignChildVar<Transform>("First", ref first);
        transform.AssignChildVar<Transform>("First_Second", ref firstSecond);
        transform.AssignChildVar<Transform>("Second", ref second);
        
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
        
        TryGetComponent<LineRenderer>(out lineRenderer);
    }

    private void Update()
    {
        _ratio = _currentTime / duration;
        
        _x = Mathf.Cos(_ratio * Mathf.PI * 2f)  * rad;
        _y = -Mathf.Sin(_ratio * Mathf.PI * 2f) * rad;
        
        lineRenderer.SetPosition(1, new(_x, _y));

        _currentTime = (_currentTime + Time.deltaTime) % duration;
    }
}
