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
    [SerializeField] private bool _isEventScene = false;    // Persistant Manager에서 가져와 세팅할 예정.

    [Space(20f), Header("MapNode Preset")] 
    [SerializeField] private MapNodeLayoutData _presetLayout = null;   // Persistant Manager에서 가져와 세팅할 예정.
    
    [Space(20f), Header("Prefabs")]
    [SerializeField] private GameObject _mapNodeViewPrefab;
    
    [Space(20f), Header("Grid Settings(Random Mode)")]
    [SerializeField, Range(1, 10)] private int _maxNodeCountInLevel = 5;
    [SerializeField, Min(1)] private int _maxNodesLevel = 15;

    [Space(20f), Header("Layout")]
    [SerializeField] private Transform _mapNodeStartPoint;
    [SerializeField] private Transform _mapNodeEndPoint;
    [SerializeField] private RectTransform _mapScrollView;
    [SerializeField] private RectTransform _mapNodesPanel;

    [Space(20f), Header("Randomness"), Tooltip("그리드 셀 중시으로부터 최대 랜덤 오프셋(0~0.5")] 
    [SerializeField, Range(0f, 0.5f)] private float _volatilityRatio = 0.25f;
    
    //private Dictionary<int, List<MapNode>> _mapNodes = new();
    //private Dictionary<int, List<MapNodeView>> _mapNodeViews = new();
    
    // [level][slot] -> MapNode
    private Dictionary<int, Dictionary<int, MapNode>> _mapNodes = new();
    
    // [level][slot] -> MapNodeView
    private Dictionary<int, Dictionary<int, MapNodeView>> _mapNodeViews = new();
    
    // Addressables Location 캐시
    public Dictionary<NodeType, List<IResourceLocation>> _MapNodeLocations { get; private set; } = new();

    // 차후, 계산 후 정해짐.
    private float _cellWidth;
    private float _cellHeight;
    
    // 노드 배치 영역(anchoredPosition 기준)
    private Vector2 _areaMin;
    private Vector2 _areaMax;

    private int TotalLevels = 15;
    private int MaxSlotsPerLevel = 5;
    
    // private float _nodeVerticalDistance = 60f;
    // private float _nodeHorizontalDistance = 60f;

    
    public Dictionary<NodeType, List<IResourceLocation>> MapNodeLocations { get; set; } = new();
    
    protected override void Awake()
    {
        base.Awake();
        VarSetup();
    }

    private void Start()
    {
        // Persistant Manager에서 세팅된 preset이 있다면 가져와 세팅토록 한다.
        // isEventScene 또한 마찬가지.
        // _presetLayout = ~~~

        TotalLevels = _presetLayout ? _presetLayout.totalLevels : _maxNodesLevel;
        MaxSlotsPerLevel = _presetLayout ? _presetLayout.maxNodesPerLevel : _maxNodeCountInLevel; 
        
        _cellWidth = _mapNodesPanel.sizeDelta.x / (MaxSlotsPerLevel + 1);
        _cellHeight = (_areaMax.y - _areaMin.y) / (TotalLevels + 1);
        
        if (!_isEventScene)
            StartCoroutine(BuildMap());
    }

    private void VarSetup()
    {
        if (_mapNodeStartPoint == null) transform.AssignChildVar<Transform>("MapNodeStartPoint", ref _mapNodeStartPoint);
        if (_mapNodeEndPoint == null) transform.AssignChildVar<Transform>("MapNodeEndPoint", ref _mapNodeEndPoint);
        if (_mapScrollView == null) transform.AssignChildVar<RectTransform>("MapNodeScrollView", ref _mapScrollView);
        if (_mapNodesPanel == null) transform.AssignChildVar<RectTransform>("MapNodesPanel", ref _mapNodesPanel);

        // 시작/끝 anchoredPosition 설정
        float halfH = _mapNodesPanel.sizeDelta.y * 0.5f;

        if (_mapNodeStartPoint.TryGetComponent<RectTransform>(out var rtStart))
        {
            // 세로방향 기준.
            // 가로방향을 사용한다면 차후 수정할 필요 있음.
            rtStart.anchoredPosition = new Vector2(0, -halfH + 200f);
        }

        if (_mapNodeEndPoint.TryGetComponent<RectTransform>(out var rtEnd))
        {
            rtEnd.anchoredPosition = new Vector2(0, halfH - 200f);
        }

        if (rtStart) _areaMin = rtStart.anchoredPosition;
        if (rtEnd) _areaMax = rtEnd.anchoredPosition;
    }

    private IEnumerator BuildMap()
    {
        yield return InitializeMapNodeLocations();

        if (_presetLayout)
            yield return BuildFromPreset();
        else
            yield return BuildRandom();
        
        PlaceAndDrawNodes();
    }
    
    // ====================================================================
    // 프리셋이 있다면 해당 SO를 기반으로 생성하는 함수를 사용토록 함.
    // ====================================================================
    private IEnumerator BuildFromPreset()
    {
        foreach (var entry in _presetLayout.nodeEntries)
        {
            var node = new MapNode(entry.level, entry.slot, entry.nodeType);
            yield return GetMapNodeDataAndSet(entry.nodeType, node);
            RegisterNode(node);
        }
        
        // 부모-자식 연결
        foreach (var entry in _presetLayout.nodeEntries)
        {
            if (!TryGetNode(entry.level, entry.slot, out var childNode)) continue;

            foreach (var parentCoord in entry.parentEntries)
            {
                if (TryGetNode(parentCoord.x, parentCoord.y, out var parentNode))
                    MapNode.Connect(parentNode, childNode);
            }
        }
    }
    
    // ====================================================================
    // 프리셋이 없다면 랜덤하게 생성토록 함.
    // ====================================================================
    private IEnumerator BuildRandom()
    {
        for (int level = 0; level < TotalLevels; level++)
        {
            int nodeCount = UnityEngine.Random.Range(1, MaxSlotsPerLevel + 1);
            
            List<int> slots = GetShuffledSlots(MaxSlotsPerLevel, nodeCount);
            
            foreach (var slot in slots)
            {
                NodeType type = GetRandomNodeType();
                var node = new MapNode(level, slot, type);
                yield return GetMapNodeDataAndSet(type, node);

                if (node.MapNodeStatus == null)
                {
                    Debug.LogWarning($"[MapNodeSystem] Level {level}, Slot {slot} : MapNodeData 로드 실패 - 건너뜀");
                    continue;
                }

                RegisterNode(node);
            }
        }

        ConnectRandomNodes();
    }

    /// <summary>
    /// 인접 Level 노드(들)을 연결한다.
    /// 현재 Level의 각 노드들은 다음 Level의 가장 가까운 Slot의 노드에 연결된다.
    /// </summary>
    private void ConnectRandomNodes()
    {
        for (int level = 0; level < TotalLevels - 1; level++)
        {
            if (!_mapNodes.ContainsKey(level)) continue;
            if (!_mapNodes.ContainsKey(level + 1)) continue;
            
            var currentNodes = _mapNodes[level];
            var nextNodes = _mapNodes[level + 1];
        
            foreach (var kvp in currentNodes)
            {
                var parent = kvp.Value;
        
                MapNode closestChild = null;
                int minDist = int.MaxValue;
        
                foreach (var nextKvp in nextNodes)
                {
                    int dist = Mathf.Abs(nextKvp.Key - parent.NodeSlot);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestChild = nextKvp.Value;
                    }
                }
                
                if (closestChild != null)
                    MapNode.Connect(parent, closestChild);
            }
        }
        
        // parent가 없는 노드(level > 0)에 대해 한 레벨 아래 중 가장 가까운 노드를 parent로 연결
        for (int level = 1; level < TotalLevels; level++)
        {
            if (!_mapNodes.ContainsKey(level))     continue;
            if (!_mapNodes.ContainsKey(level - 1)) continue;
 
            foreach (var kvp in _mapNodes[level])
            {
                var node = kvp.Value;
                if (node.ParentNodes.Count > 0) continue;
 
                MapNode closestParent = null;
                int minDist = int.MaxValue;
 
                foreach (var parentKvp in _mapNodes[level - 1])
                {
                    int dist = Mathf.Abs(parentKvp.Key - node.NodeSlot);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestParent = parentKvp.Value;
                    }
                }
 
                if (closestParent != null)
                    MapNode.Connect(closestParent, node);
            }
        }
    }
    
    // ====================================================================
    // (Node)View 배치 및 연결선 그리기
    // ====================================================================
    private void PlaceAndDrawNodes()
    {
        GameObject lineRoot = new GameObject("ConnectionLines");
        lineRoot.transform.SetParent(_mapNodesPanel, false);
        lineRoot.transform.SetAsFirstSibling();

        var lineRootRect = lineRoot.AddComponent<RectTransform>();
        lineRootRect.anchoredPosition = Vector2.zero;
        lineRootRect.sizeDelta = Vector2.zero;

        for (int level = 0; level < TotalLevels; level++)
        {
            if (!_mapNodes.ContainsKey(level)) continue;

            foreach (var kvp in _mapNodes[level])
            {
                int slot = kvp.Key;
                MapNode node = kvp.Value;

                Vector2 cellCenter = GetCellCenter(level, slot);
                Vector2 randomOffset = GetRandomOffset();
                Vector2 finalPos = cellCenter + randomOffset;

                GameObject go = Instantiate(_mapNodeViewPrefab, _mapNodesPanel);
                if (!go.TryGetComponent<MapNodeView>(out var view))
                    view = go.AddComponent<MapNodeView>();
                
                view.SetMapNode(node);
                view.RectTransform.anchoredPosition = finalPos;

                if (!_mapNodeViews.ContainsKey(level))
                    _mapNodeViews[level] = new();

                _mapNodeViews[level][slot] = view;
            }
        }

        DrawAllConnectionLines(lineRootRect);
    }

    private void DrawAllConnectionLines(RectTransform lineParent)
    {
        for (int level = 0; level < TotalLevels; level++)
        {
            if (!_mapNodeViews.ContainsKey(level)) continue;

            foreach (var kvp in _mapNodeViews[level])
            {
                var view = kvp.Value;
                var parentViews = new List<MapNodeView>();

                foreach (var parentNode in view.MapNode.ParentNodes)
                {
                    if (TryGetView(parentNode.NodeLevel, parentNode.NodeSlot, out var parentView))
                        parentViews.Add(parentView);
                }
                
                view.DrawConnectionLines(parentViews, lineParent);
            }
        }
    }
    
    // ====================================================================
    // Addressables
    // ====================================================================
    public IEnumerator InitializeMapNodeLocations()
    {
        var handles = new List<AsyncOperationHandle>();
        foreach (NodeType nodeType in Enum.GetValues(typeof(NodeType)))
        {
            if (!MapNodeLocations.ContainsKey(nodeType))
                MapNodeLocations[nodeType] = new List<IResourceLocation>();
            
            var handle = Addressables.LoadResourceLocationsAsync(
                new List<object> {nodeType.ToString(), "MapNodes"},
                Addressables.MergeMode.Intersection,
                typeof(MapNodeData));

            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                MapNodeLocations[nodeType].AddRange(handle.Result);
                handles.Add(handle);
            }
            else
            {
                Addressables.Release(handle);
            }
        }
    }

    private IEnumerator GetMapNodeDataAndSet(NodeType nodeType, MapNode mapNode)
    {
        if (!MapNodeLocations.ContainsKey(nodeType) || MapNodeLocations[nodeType].Count == 0)
        {
            Debug.LogWarning($"[MapNodeSystem] \"{nodeType}\"에 해당하는 MapNodeData가 없음.");
            yield break;
        }
        
        int idx = UnityEngine.Random.Range(0, MapNodeLocations[nodeType].Count);
        var handle = Addressables.LoadAssetAsync<MapNodeData>(MapNodeLocations[nodeType][idx]);

        yield return handle;
        
        if (handle.Status == AsyncOperationStatus.Succeeded)
            mapNode.SetMapNodeData(handle.Result);
        else
            Debug.LogWarning($"[MapNodeSystem] MapNodeData 로드 실패 : {nodeType}");
        
        Addressables.Release(handle);
    }
    
    // ====================================================================
    // Helpers
    // ====================================================================
    
    /// <summary>
    /// Grid Cell의 anchoredPosition 중심 좌표를 반환한다.
    /// </summary>
    private Vector2 GetCellCenter(int level, int slot)
    {
        // 가로 : Panel 좌측에서 (slot + 1) * cellWidth
        // 세로 : areaMin에서 (level + 1) * cellHeight
        
        float x = -_mapNodesPanel.sizeDelta.x * 0.5f + (slot + 1) * _cellWidth;
        float y = _areaMin.y + (level + 1) * _cellHeight;
        return new Vector2(x, y);
    }

    private Vector2 GetRandomOffset()
    {
        float maxOffsetX = _cellWidth * _volatilityRatio;
        float maxOffsetY = _cellHeight * _volatilityRatio;
        
        return new Vector2(
            UnityEngine.Random.Range(-maxOffsetX, maxOffsetX),
            UnityEngine.Random.Range(-maxOffsetY, maxOffsetY));
    }

    private void RegisterNode(MapNode node)
    {
        if (!_mapNodes.ContainsKey(node.NodeLevel))
            _mapNodes[node.NodeLevel] = new Dictionary<int, MapNode>();

        _mapNodes[node.NodeLevel][node.NodeSlot] = node;
    }

    private bool TryGetNode(int level, int slot, out MapNode node)
    {
        node = null;
        return _mapNodes.ContainsKey(level) && _mapNodes[level].TryGetValue(slot, out node);
    }

    private bool TryGetView(int level, int slot, out MapNodeView view)
    {
        view = null;
        return _mapNodeViews.ContainsKey(level) && _mapNodeViews[level].TryGetValue(slot, out view);
    }

    private NodeType GetRandomNodeType()
    {
        return (NodeType)UnityEngine.Random.Range(1, Enum.GetValues(typeof(NodeType)).Length);
    }

    private List<int> GetShuffledSlots(int maxSlot, int count)
    {
        List<int> pool = new();
        for (int i = 0; i < maxSlot; i++) pool.Add(i);

        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        
        return pool.GetRange(0, Mathf.Min(count, pool.Count));
    }
    
    // private IEnumerator CreateMapNodes()
    // {
    //     yield return InitializeMapNodeLocations();
    //     
    //     // 1. 각 단계(Level)마다 랜덤한 수의 Node를 만들도록 한다.
    //     for (int currentLevel = 0; currentLevel < _maxNodesLevel; currentLevel++)
    //     {
    //         //int randomNodeCount = UnityEngine.Random.Range(1, _maxNodeCountInLevel + 1);
    //         int randomNodeCount = 1;
    //         List<MapNode> mapNodesInLevel = new();
    //         
    //         for (int i = 0; i < randomNodeCount; i++)
    //         {
    //             NodeType randomNodeType = GetRandomNodeType();
    //             
    //             MapNode newMapNode = new(currentLevel, randomNodeType);
    //             
    //             yield return GetMapNodeDataAndSet(randomNodeType, newMapNode);
    //             if (newMapNode.MapNodeStatus == null)
    //             {
    //                 Debug.Log("Failed to Set Map Node Data");
    //                 continue;
    //             }
    //             
    //             mapNodesInLevel.Add(newMapNode);
    //         }
    //         
    //         _mapNodes[currentLevel]= mapNodesInLevel;
    //     }
    //     
    //     // 2. 만들어진 노드들을 연결해준다.
    //     ConnectMapNodes();
    // }
    //
    // private IEnumerator GetMapNodeDataAndSet(NodeType nodeType, MapNode mapNode)
    // {
    //     if (MapNodeLocations[nodeType].Count == 0)
    //     {
    //         Debug.Log($"{nodeType.ToString()} has no map nodes.");
    //         yield break;
    //     }
    //     
    //     int randomNum = UnityEngine.Random.Range(0, MapNodeLocations[nodeType].Count);
    //     var handle = Addressables.LoadAssetAsync<MapNodeData>(MapNodeLocations[nodeType][randomNum]);
    //     
    //     yield return handle;
    //
    //     if (handle.Status != AsyncOperationStatus.Succeeded)
    //     {
    //         Debug.Log("Failed to Load Map Node Data");
    //         Addressables.Release(handle);
    //         yield break;
    //     }
    //
    //     var result = handle.Result;
    //     
    //     mapNode.SetMapNodeData(handle.Result);
    //
    //     Addressables.Release(handle);
    // }
    //
    // public IEnumerator InitializeMapNodeLocations()
    // {
    //     List<AsyncOperationHandle> mapNodeLocationHandles = new();
    //     
    //     foreach (NodeType nodeType in Enum.GetValues(typeof(NodeType)))
    //     {
    //         if (!MapNodeLocations.ContainsKey(nodeType)) MapNodeLocations.Add(nodeType, new());
    //         
    //         var handle = Addressables.LoadResourceLocationsAsync(
    //             new List<object> { nodeType.ToString(), "MapNodes" },
    //             Addressables.MergeMode.Intersection,
    //             typeof(MapNodeData));
    //
    //         yield return handle;
    //         
    //         if (handle.Status != AsyncOperationStatus.Succeeded)
    //         {
    //             Addressables.Release(handle);
    //             yield break;
    //         }
    //         
    //         MapNodeLocations[nodeType].AddRange(handle.Result);
    //         mapNodeLocationHandles.Add(handle);
    //     }
    //     
    //     foreach (var handle in mapNodeLocationHandles)
    //         Addressables.Release(handle);
    // }
    //
    // private void ConnectMapNodes()
    // {
    //     // 지금은 세로로 1자로 만들어 그냥 무대포로 MapNode를 View에 할당중.
    //     Debug.Log("ConnectMapNodes도 차후 바꿔줄 필요 있음.");
    //     Vector2 initialPos = _mapNodeStartPoint.GetComponent<RectTransform>().anchoredPosition;
    //     for (int currentLevel = 0; currentLevel < _maxNodesLevel; currentLevel++)
    //     {
    //         GameObject go = Instantiate(_mapNodeViewPrefab, _mapNodesPanel);
    //         if (go.TryGetComponent<RectTransform>(out var rt))
    //         {
    //             rt.anchoredPosition = initialPos + Vector2.up * currentLevel * _nodeVerticalDistance;
    //         }
    //
    //         if (go.TryGetComponent<MapNodeView>(out var mapNodeView))
    //         {
    //             mapNodeView.SetMapNode(_mapNodes[currentLevel][0]);
    //         }
    //     }
    // }
    //
    // // 이미 생성된 상태라면, DrawMapNodeView를 사용토록 한다.
    // // 저장 데이터를 불러왔을 경우 등등...
    // private void DrawMapNodeViews()
    // {
    //     
    // }
}
