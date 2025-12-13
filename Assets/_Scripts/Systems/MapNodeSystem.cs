using System;
using System.Collections.Generic;
using UnityEngine;

public class MapNodeSystem : Singleton<MapNodeSystem>
{
    [SerializeField] private GameObject _mapNodeViewPrefab;
    //[SerializeField, Range(3, 6)] private int _verticalNodeCount = 4;
    [SerializeField, Range(1, 10)] private int _maxNodeCountInLevel = 5;
    [SerializeField, Min(1)] private int _maxNodesLevel = 15;

    [SerializeField] private Transform _mapNodeStartPoint;
    [SerializeField] private Transform _mapNodeEndPoint;
    [SerializeField] private RectTransform _mapScrollView;
    [SerializeField] private RectTransform _mapNodesPanel;
    
    private float _nodeVerticalDistance = 60f;
    private float _nodeHorizontalDistance = 60f;
    private float _volatility = 10f;

    private Dictionary<int, List<MapNode>> _mapNodes = new();
    private Dictionary<int, List<MapNodeView>> _mapNodeViews = new();
    
    protected override void Awake()
    {
        base.Awake();

        VarSetup();
    }

    private void Start()
    {
        CreateMapNodes();
    }

    private void VarSetup()
    {
        if (_mapNodeStartPoint == null) transform.AssignChildVar<Transform>("MapNodeStartPoint", ref _mapNodeStartPoint);
        if (_mapNodeEndPoint == null) transform.AssignChildVar<Transform>("MapNodeEndPoint", ref _mapNodeEndPoint);
        if (_mapScrollView == null) transform.AssignChildVar<RectTransform>("MapNodeScrollView", ref _mapScrollView);
        if (_mapNodesPanel == null) transform.AssignChildVar<RectTransform>("MapNodesPanel", ref _mapNodesPanel);
        
        if (_mapNodeStartPoint.TryGetComponent<RectTransform>(out var rt))
            rt.anchoredPosition = new Vector2(0, _mapNodesPanel.sizeDelta.y / 2 * (-1) + 200f);

        if (_mapNodeEndPoint.TryGetComponent<RectTransform>(out rt))
        {
            rt.anchoredPosition = new Vector2(0, _mapNodesPanel.sizeDelta.y / 2 - 200f);
        }
        
        float viewHeight =
            _mapNodeEndPoint.GetComponent<RectTransform>().anchoredPosition.y -
            _mapNodeStartPoint.GetComponent<RectTransform>().anchoredPosition.y;
        
        float viewWidth = _mapNodesPanel.sizeDelta.x;
        _nodeVerticalDistance = viewHeight / (_maxNodesLevel + 1);
        _nodeHorizontalDistance = viewWidth / (_maxNodeCountInLevel + 1);

        if (_mapNodeViewPrefab.TryGetComponent<RectTransform>(out rt))
        {
            _volatility = Mathf.Max(rt.rect.height, rt.rect.width) * 0.8f;
        }
    }

    private void CreateMapNodes()
    {
        // 1. 각 단계(Level)마다 랜덤한 수의 Node를 만들도록 한다.
        for (int currentLevel = 0; currentLevel < _maxNodesLevel; currentLevel++)
        {
            //int randomNodeCount = UnityEngine.Random.Range(1, _maxNodeCountInLevel + 1);
            int randomNodeCount = 1;
            List<MapNode> mapNodesInLevel = new();
            
            for (int i = 0; i < randomNodeCount; i++)
            {
                MapNode newMapNode = new(currentLevel, GetRandomNodeType());
                mapNodesInLevel.Add(newMapNode);
            }
            
            _mapNodes[currentLevel]= mapNodesInLevel;
        }

        // 2. 만들어진 노드들을 연결해준다.
        ConnectMapNodes();
    }

    private void ConnectMapNodes()
    {
        Vector2 initialPos = _mapNodeStartPoint.GetComponent<RectTransform>().anchoredPosition;
        for (int currentLevel = 0; currentLevel < _maxNodesLevel; currentLevel++)
        {
            GameObject go = Instantiate(_mapNodeViewPrefab, _mapNodesPanel);
            if (go.TryGetComponent<RectTransform>(out var rt))
            {
                rt.anchoredPosition = initialPos + Vector2.up * currentLevel * _nodeVerticalDistance;
            }
        }
    }

    // 이미 생성된 상태라면, DrawMapNodeView를 사용토록 한다.
    // 저장 데이터를 불러왔을 경우 등등...
    private void DrawMapNodeViews()
    {
        
    }

    private NodeType GetRandomNodeType()
    {
        return (NodeType)UnityEngine.Random.Range(1, Enum.GetValues(typeof(NodeType)).Length);
    }
}
