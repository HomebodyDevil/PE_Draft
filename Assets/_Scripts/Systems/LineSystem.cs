using System;
using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class LineSystem : Singleton<LineSystem>
{
    [SerializeField] private LineRenderer lineView;
    [SerializeField] private Camera lineCamera;

    private Plane _mouseRayPlane;
    private Coroutine _whileMousePressingCoroutine;
    private Coroutine _drawCurveCoroutine;
    
    protected override void Awake()
    {
        base.Awake();
        Setup();
        
        _mouseRayPlane = new Plane(lineCamera.transform.forward, new Vector3(0, 0, ConstValue.LINE_Z));
    }

    private void Start()
    {
        SetVisible(false);
    }

    private void Setup()
    {
        if (lineView == null) transform.AssignChildVar<LineRenderer>("LineView", ref lineView);
        if (lineCamera == null) transform.AssignChildVar<Camera>("LineCamera", ref lineCamera);

        lineView.startWidth = lineView.endWidth = ConstValue.LINE_WIDTH;
    }

    private void OnEnable()
    {
        InputManager.Instance.PlayerActions.Default.MouseLeftButton.started += OnMouseLeftClickDown;
        InputManager.Instance.PlayerActions.Default.MouseLeftButton.canceled += OnMouseLeftClickUp;
    }
    
    private void OnDisable()
    {
        InputManager.Instance.PlayerActions.Default.MouseLeftButton.started -= OnMouseLeftClickDown;
        InputManager.Instance.PlayerActions.Default.MouseLeftButton.canceled -= OnMouseLeftClickUp;
    }

    private void OnMouseLeftClickDown(InputAction.CallbackContext context)
    {
        SetVisible(true);
        SetStartPoint();
        _whileMousePressingCoroutine = StartCoroutine(WhilePressingCoroutine());
    }
    
    private void OnMouseLeftClickUp(InputAction.CallbackContext context)
    {
        SetVisible(false);
        if (_whileMousePressingCoroutine != null) StopCoroutine(_whileMousePressingCoroutine);
    }

    private void SetVisible(bool visible)
    {
        lineView.enabled = visible;
    }

    private void SetStartPoint()
    {
        lineView.SetPosition(0, GetMousePointWorldPos());
    }
    
    private void SetEndPoint()
    {
        lineView.SetPosition(1, GetMousePointWorldPos());
    }
    
    private Vector3 GetMousePointWorldPos()
    {
        Ray ray = lineCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        
        if (_mouseRayPlane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        
        return Vector3.zero;
    }

    private IEnumerator WhilePressingCoroutine()
    {
        float time = 0;
        
        while (true)
        {
            if (time > 60) yield break;
            
            SetEndPoint();
            
            Debug.Log("Pressing");
            time += 0.05f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    private IEnumerator DrawCurveCoroutine()
    {
        float time = 0;
        
        while (true)
        {
            if (time > 60) yield break;
            
            SetEndPoint();
            
            Debug.Log("Pressing");
            time += 0.05f;
            yield return new WaitForSeconds(0.01f);
        }
    }

    public void StartDrawCurve()
    {
        SetStartPoint();
        SetVisible(true);
        _drawCurveCoroutine = StartCoroutine(DrawCurveCoroutine());
    }

    public void StopDrawCurve()
    {
        SetVisible(false);
        if (_drawCurveCoroutine != null) StopCoroutine(_drawCurveCoroutine);
    }
}