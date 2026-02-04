using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class MapNodeSystem : Singleton<MapNodeSystem>
{
    [Header("Scene Type")] 
    [SerializeField] private bool _isEventScene = false;
    
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
    
    public Dictionary<NodeType, List<IResourceLocation>> MapNodeLocations { get; set; } = new();
    //private List<
    
    protected override void Awake()
    {
        base.Awake();

        VarSetup();
    }

    private void Start()
    {
        if (!_isEventScene)
            StartCoroutine(CreateMapNodes());
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

    private IEnumerator CreateMapNodes()
    {
        yield return InitializeMapNodeLocations();
        
        // 1. 각 단계(Level)마다 랜덤한 수의 Node를 만들도록 한다.
        for (int currentLevel = 0; currentLevel < _maxNodesLevel; currentLevel++)
        {
            //int randomNodeCount = UnityEngine.Random.Range(1, _maxNodeCountInLevel + 1);
            int randomNodeCount = 1;
            List<MapNode> mapNodesInLevel = new();
            
            for (int i = 0; i < randomNodeCount; i++)
            {
                NodeType randomNodeType = GetRandomNodeType();
                
                MapNode newMapNode = new(currentLevel, randomNodeType);
                
                yield return GetMapNodeDataAndSet(randomNodeType, newMapNode);
                if (newMapNode.MapNodeStatus == null)
                {
                    Debug.Log("Failed to Set Map Node Data");
                    continue;
                }
                
                mapNodesInLevel.Add(newMapNode);
            }
            
            _mapNodes[currentLevel]= mapNodesInLevel;
        }
        
        // 2. 만들어진 노드들을 연결해준다.
        ConnectMapNodes();
    }
    
    private IEnumerator GetMapNodeDataAndSet(NodeType nodeType, MapNode mapNode)
    {
        if (MapNodeLocations[nodeType].Count == 0)
        {
            Debug.Log($"{nodeType.ToString()} has no map nodes.");
            yield break;
        }
        
        int randomNum = UnityEngine.Random.Range(0, MapNodeLocations[nodeType].Count);
        var handle = Addressables.LoadAssetAsync<MapNodeData>(MapNodeLocations[nodeType][randomNum]);
        
        yield return handle;

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Failed to Load Map Node Data");
            Addressables.Release(handle);
            yield break;
        }

        var result = handle.Result;
        // Debug.Log($"enemies : {result.BattleEnemiesData.RuntimeKeyIsValid() && !string.IsNullOrEmpty(result.BattleEnemiesData.AssetGUID)}");
        // Debug.Log($"events : {result.EventData.RuntimeKeyIsValid() && !string.IsNullOrEmpty(result.EventData.AssetGUID)}");
        // Debug.Log($"timeline : {result.StartTimeline.RuntimeKeyIsValid() && !string.IsNullOrEmpty(result.StartTimeline.AssetGUID)}");
        
        mapNode.SetMapNodeData(handle.Result);

        Addressables.Release(handle);
    }

    // /// <summary>
    // /// 해당 NodeType으로 세팅된 SO의 location(Addressables)를 가져온다.
    // /// </summary>
    // /// <param name="nodeType">이 타입의 정보들을 가져온다.</param>
    // /// <param name="resourceLocations">이 리스트에 할당하게 된다.</param>
    // /// <returns></returns>
    // private IEnumerator GetMapNodeDataLocations(NodeType nodeType, List<IResourceLocation> resourceLocations)
    // {
    //     Debug.Log("nodeType에 따라 적절한 에셋(MapNodeData)를 로드할 수 있도록 수정하자.");
    //     // 아래의 string. label?을 사용해서 해보까.
    //     //string label = $"{nodeType.ToString()}NodeData";
    //     
    //     var handle = Addressables.LoadResourceLocationsAsync(
    //         new List<object> { nodeType.ToString(), "MapNodes" },
    //         Addressables.MergeMode.Intersection,
    //         typeof(MapNodeData));
    //     
    //     yield return handle;
    //
    //     if (handle.Status != AsyncOperationStatus.Succeeded)
    //     {
    //         Addressables.Release(handle);
    //         yield break;
    //     }
    //     resourceLocations.AddRange(handle.Result);
    //     
    //     Addressables.Release(handle);
    // }

    public IEnumerator InitializeMapNodeLocations()
    {
        List<AsyncOperationHandle> mapNodeLocationHandles = new();
        
        foreach (NodeType nodeType in Enum.GetValues(typeof(NodeType)))
        {
            if (!MapNodeLocations.ContainsKey(nodeType)) MapNodeLocations.Add(nodeType, new());
            
            var handle = Addressables.LoadResourceLocationsAsync(
                new List<object> { nodeType.ToString(), "MapNodes" },
                Addressables.MergeMode.Intersection,
                typeof(MapNodeData));

            yield return handle;
            
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                Addressables.Release(handle);
                yield break;
            }
            
            MapNodeLocations[nodeType].AddRange(handle.Result);
            mapNodeLocationHandles.Add(handle);
        }
        
        foreach (var handle in mapNodeLocationHandles)
            Addressables.Release(handle);
    }

    private void ConnectMapNodes()
    {
        // 지금은 세로로 1자로 만들어 그냥 무대포로 MapNode를 View에 할당중.
        Debug.Log("ConnectMapNodes도 차후 바꿔줄 필요 있음.");
        Vector2 initialPos = _mapNodeStartPoint.GetComponent<RectTransform>().anchoredPosition;
        for (int currentLevel = 0; currentLevel < _maxNodesLevel; currentLevel++)
        {
            GameObject go = Instantiate(_mapNodeViewPrefab, _mapNodesPanel);
            if (go.TryGetComponent<RectTransform>(out var rt))
            {
                rt.anchoredPosition = initialPos + Vector2.up * currentLevel * _nodeVerticalDistance;
            }

            if (go.TryGetComponent<MapNodeView>(out var mapNodeView))
            {
                mapNodeView.SetMapNode(_mapNodes[currentLevel][0]);
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
