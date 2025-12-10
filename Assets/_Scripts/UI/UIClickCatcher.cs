using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickCatcher : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Transform _parentUI;
    
    private void Awake()
    {
        if (_parentUI == null) _parentUI = transform.parent;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _parentUI?.gameObject.SetActive(false);
    }
}
