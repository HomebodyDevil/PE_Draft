using System;
using System.Collections.Generic;
using SerializeReferenceEditor;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MapNodeView : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _nodeIcon;
    [SerializeField] private TextMeshProUGUI _nodeLevelText;
    
    // 연결선을 그릴 Line의 Prefab
    [SerializeField] private GameObject _connectionLinePrefab;
    
    public MapNode MapNode { get; private set; }
    public RectTransform RectTransform { get; private set; }
    
    public MapNodeView(MapNode mapNode)
    {
        MapNode = mapNode;
    }

    private void Awake()
    {
        if (!_nodeIcon) transform.AssignChildVar<Image>("MapNodeImage", ref _nodeIcon);
        if (!RectTransform) RectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {

    }

    public void OnClick()
    {
        Debug.Log("현재 MapNodeView에선, Click시, BattleScene으로만 전환.");
        SceneType sceneType = SceneService.Instance.GetSceneTypeBasedOnNodeType(MapNode.NodeType);
        SceneService.Instance.ChangeScene(sceneType);
        if (MapNode == null)
        {
            Debug.Log("MapNodeView: MapNode is null");
            return;
        }

        Debug.Log($"MapNode Type: {MapNode.NodeType.ToString()}");
        Debug.Log("PlayerStatusService의 MapNodeStatus Set.");

        MapNode.MapNodeStatus.CheckIsValid(out var timeline, out var enemiesDataCheck, out var eventDataCheck);
        Debug.LogError($"SetMapNodeStatus\ntimeline : {timeline}\nenemies : {enemiesDataCheck}\nevents : {eventDataCheck}");
        
        PlayerStatusService.Instance.CurrentMapNodeStatus = MapNode.MapNodeStatus;
    }
    
    public void SetMapNode(MapNode mapNode)
    {
        MapNode = mapNode;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (MapNode == null) return;

        // if (_nodeLevelText)
        //     _nodeLevelText.text = $"Lv.{MapNode.NodeLevel}";
        
        if (_nodeIcon)
            _nodeIcon.color = GetColorByType(MapNode.NodeType);
    }

    private Color GetColorByType(NodeType type)
    {
        Debug.Log("Test : MapNodeView 아이콘 색 설정.");
        Color color = type switch
        {
            NodeType.Battle => Color.red,
            NodeType.None => Color.black,
            NodeType.Elite => Color.blue,
            NodeType.Rest => Color.yellow,
            NodeType.Event => Color.white,
            _ => Color.violet,
        };
        
        return color;
    }

    public void DrawConnectionLines(List<MapNodeView> parentViews, Transform lineParent)
    {
        if (_connectionLinePrefab == null || parentViews == null || parentViews.Count == 0)
        {
            Debug.Log("Cant ConnectionLines");
            return;
        }

        foreach (var parentView in parentViews)
        {
            if (parentView == null) continue;
            DrawLine(parentView.RectTransform.anchoredPosition,
                RectTransform.anchoredPosition,
                lineParent);
        }
    }

    private void DrawLine(Vector2 from, Vector2 to, Transform parent)
    {
        GameObject lineGo = Instantiate(_connectionLinePrefab, parent);
        if (!lineGo.TryGetComponent<RectTransform>(out var rt)) return;

        Vector2 dir = to - from;
        float dist = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        rt.anchoredPosition = from + dir * 0.5f;
        rt.sizeDelta = new Vector2(dist, 4f);   // 두께 = 4px
        rt.localRotation = Quaternion.Euler(0, 0, angle);
    }
}
