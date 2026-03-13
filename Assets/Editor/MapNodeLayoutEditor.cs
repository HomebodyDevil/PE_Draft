// 반드시 Assets/Editor/ 폴더 아래에 위치시켜야 합니다.
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// MapNodeLayoutData SO를 시각적으로 편집하는 EditorWindow.
/// 메뉴: Tools > MapNode Layout Editor
/// </summary>
public class MapNodeLayoutEditor : EditorWindow
{
    // ─────────────────────────────────────────────
    // Constants
    // ─────────────────────────────────────────────
    private const float TOOLBAR_HEIGHT    = 80f;
    private const float INSPECTOR_WIDTH_MIN = 160f;
    private const float INSPECTOR_WIDTH_MAX = 500f;
    private const float RESIZE_HANDLE_WIDTH = 5f;
    private const float NODE_RADIUS       = 18f;
    private const float NODE_DIAMETER     = NODE_RADIUS * 2f;
    private const float GRID_PADDING_X    = 60f;
    private const float GRID_PADDING_Y    = 60f;
    private const float LINE_THICKNESS    = 2f;

    private static readonly Color COL_GRID_BG      = new Color(0.15f, 0.15f, 0.15f);
    private static readonly Color COL_GRID_LINE     = new Color(0.25f, 0.25f, 0.25f);
    private static readonly Color COL_CELL_CENTER   = new Color(0.3f,  0.3f,  0.3f,  0.4f);
    private static readonly Color COL_CONN_LINE     = new Color(0.7f,  0.7f,  0.3f,  0.9f);
    private static readonly Color COL_CONN_PENDING  = new Color(1f,    0.6f,  0.1f,  0.8f);
    private static readonly Color COL_SELECTED      = new Color(1f,    0.85f, 0.2f);
    private static readonly Color COL_HOVERED       = new Color(0.6f,  0.8f,  1f);

    private static readonly Dictionary<NodeType, Color> NODE_COLORS = new()
    {
        { NodeType.None,   new Color(0.45f, 0.45f, 0.45f) },
        { NodeType.Battle, new Color(0.85f, 0.25f, 0.25f) },
        { NodeType.Elite,  new Color(0.85f, 0.50f, 0.10f) },
        { NodeType.Event,  new Color(0.25f, 0.55f, 0.90f) },
        { NodeType.Rest,   new Color(0.25f, 0.75f, 0.40f) },
    };

    // ─────────────────────────────────────────────
    // State
    // ─────────────────────────────────────────────
    private MapNodeLayoutData _target;

    // 그리드 캔버스 스크롤/줌
    private Vector2 _scrollOffset = Vector2.zero;
    private float   _zoom         = 1f;

    // 셀 크기 (줌 반영 전)
    private float _cellW;
    private float _cellH;
    private float _cellHOverride = 80f; // 사용자가 직접 조절하는 세로 간격

    // Inspector 패널 너비 (드래그로 조절)
    private float _inspectorWidth = 220f;
    private bool  _isResizing     = false;

    // 선택/호버
    private MapNodeLayoutData.NodeEntry _selectedEntry;
    private MapNodeLayoutData.NodeEntry _hoveredEntry;

    // 연결선 드래그: 우클릭 컨텍스트 → "연결 추가" 선택 후 드래그
    private bool  _connectMode   = false;
    private MapNodeLayoutData.NodeEntry _connectSource;

    // 노드 드래그 이동
    private bool  _isDraggingNode = false;
    private MapNodeLayoutData.NodeEntry _draggingEntry;
    private Vector2 _draggingCanvasPos; // 드래그 중 마우스 캔버스 좌표 (미리보기용)

    // 인스펙터 스크롤
    private Vector2 _inspectorScroll;

    // ─────────────────────────────────────────────
    // Open
    // ─────────────────────────────────────────────
    [MenuItem("Tools/MapNode Layout Editor")]
    public static void Open()
    {
        var win = GetWindow<MapNodeLayoutEditor>("MapNode Layout Editor");
        win.minSize = new Vector2(700, 500);
    }

    /// <summary>Project 창에서 SO를 더블클릭하면 이 창에서 열린다.</summary>
    public static void OpenWithAsset(MapNodeLayoutData data)
    {
        var win = GetWindow<MapNodeLayoutEditor>("MapNode Layout Editor");
        win.minSize = new Vector2(700, 500);
        win._target = data;
        win.ResetView();
    }

    // ─────────────────────────────────────────────
    // GUI Entry
    // ─────────────────────────────────────────────
    private void OnGUI()
    {
        DrawToolbar();

        if (_target == null)
        {
            DrawEmptyState();
            return;
        }

        RecalcCellSize();

        Rect canvasRect = new Rect(0, TOOLBAR_HEIGHT,
            position.width - _inspectorWidth - RESIZE_HANDLE_WIDTH,
            position.height - TOOLBAR_HEIGHT);

        Rect resizeHandleRect = new Rect(
            position.width - _inspectorWidth - RESIZE_HANDLE_WIDTH, TOOLBAR_HEIGHT,
            RESIZE_HANDLE_WIDTH, position.height - TOOLBAR_HEIGHT);

        Rect inspectorRect = new Rect(position.width - _inspectorWidth, TOOLBAR_HEIGHT,
            _inspectorWidth, position.height - TOOLBAR_HEIGHT);

        DrawCanvas(canvasRect);
        DrawInspectorPanel(inspectorRect);
        DrawResizeHandle(resizeHandleRect);
        HandleResizeInput(resizeHandleRect);
        HandleCanvasInput(canvasRect);
    }

    // ─────────────────────────────────────────────
    // Toolbar
    // ─────────────────────────────────────────────
    private void DrawToolbar()
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, TOOLBAR_HEIGHT),
            new Color(0.2f, 0.2f, 0.2f));

        GUILayout.BeginArea(new Rect(8, 4, position.width - 16, TOOLBAR_HEIGHT - 8));
        EditorGUILayout.BeginHorizontal();

        // SO 필드
        EditorGUILayout.LabelField("Layout SO", GUILayout.Width(70));
        var prev = _target;
        _target = (MapNodeLayoutData)EditorGUILayout.ObjectField(
            _target, typeof(MapNodeLayoutData), false, GUILayout.Width(200));
        if (_target != prev) { _selectedEntry = null; ResetView(); }

        GUILayout.Space(12);

        if (GUILayout.Button("New SO", GUILayout.Width(70)))
            CreateNewSO();

        GUILayout.Space(20);

        if (_target != null)
        {
            // Grid 설정
            EditorGUILayout.LabelField("Levels", GUILayout.Width(42));
            int newLevels = EditorGUILayout.IntField(_target.totalLevels, GUILayout.Width(36));
            if (newLevels != _target.totalLevels)
            {
                Undo.RecordObject(_target, "Change TotalLevels");
                _target.totalLevels = Mathf.Max(1, newLevels);
                EditorUtility.SetDirty(_target);
            }

            GUILayout.Space(8);
            EditorGUILayout.LabelField("MaxPerLevel", GUILayout.Width(80));
            int newMax = EditorGUILayout.IntField(_target.maxNodesPerLevel, GUILayout.Width(36));
            if (newMax != _target.maxNodesPerLevel)
            {
                Undo.RecordObject(_target, "Change MaxNodesPerLevel");
                _target.maxNodesPerLevel = Mathf.Max(1, newMax);
                EditorUtility.SetDirty(_target);
            }

            GUILayout.Space(20);

            if (GUILayout.Button("Reset View", GUILayout.Width(80))) ResetView();
            if (GUILayout.Button("Clear All",  GUILayout.Width(72)))
            {
                if (EditorUtility.DisplayDialog("Clear All", "모든 노드를 삭제할까요?", "삭제", "취소"))
                {
                    Undo.RecordObject(_target, "Clear All Nodes");
                    _target.nodeEntries.Clear();
                    _selectedEntry = null;
                    EditorUtility.SetDirty(_target);
                }
            }
            if (GUILayout.Button("Save", GUILayout.Width(56)))
            {
                EditorUtility.SetDirty(_target);
                AssetDatabase.SaveAssets();
            }
        }

        EditorGUILayout.EndHorizontal();

        // 줌 슬라이더
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Zoom", GUILayout.Width(36));
        _zoom = EditorGUILayout.Slider(_zoom, 0.4f, 2.0f, GUILayout.Width(160));
        EditorGUILayout.LabelField($"{_zoom:F1}x", GUILayout.Width(34));
        GUILayout.Space(20);
        EditorGUILayout.LabelField("Row Height", GUILayout.Width(68));
        _cellHOverride = EditorGUILayout.Slider(_cellHOverride, 40f, 200f, GUILayout.Width(160));
        EditorGUILayout.LabelField($"{_cellHOverride:F0}px", GUILayout.Width(38));
        EditorGUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    // ─────────────────────────────────────────────
    // Canvas
    // ─────────────────────────────────────────────
    private void DrawCanvas(Rect canvasRect)
    {
        GUI.BeginClip(canvasRect);
        EditorGUI.DrawRect(new Rect(0, 0, canvasRect.width, canvasRect.height), COL_GRID_BG);
        DrawGridLines(canvasRect);
        DrawCellCenterDots();
        DrawConnectionLines();
        DrawConnectModePreview();
        DrawNodes();
        GUI.EndClip();
    }

    private void DrawGridLines(Rect canvasRect)
    {
        Handles.color = COL_GRID_LINE;
        int cols = _target.maxNodesPerLevel + 1;
        int rows = _target.totalLevels + 1;

        for (int c = 0; c <= cols; c++)
        {
            float x = GRID_PADDING_X + c * _cellW * _zoom + _scrollOffset.x;
            Handles.DrawLine(new Vector3(x, 0), new Vector3(x, canvasRect.height));
        }
        for (int r = 0; r <= rows; r++)
        {
            float y = GRID_PADDING_Y + r * _cellH * _zoom + _scrollOffset.y;
            Handles.DrawLine(new Vector3(0, y), new Vector3(canvasRect.width, y));
        }
    }

    private void DrawCellCenterDots()
    {
        for (int lv = 0; lv < _target.totalLevels; lv++)
        {
            for (int sl = 0; sl < _target.maxNodesPerLevel; sl++)
            {
                Vector2 pos = GridToCanvas(lv, sl);
                EditorGUI.DrawRect(
                    new Rect(pos.x - 3, pos.y - 3, 6, 6),
                    COL_CELL_CENTER);
            }
        }
    }

    private void DrawConnectionLines()
    {
        foreach (var entry in _target.nodeEntries)
        {
            Vector2 childPos = GridToCanvas(entry.level, entry.slot);
            foreach (var p in entry.parentEntries)
            {
                var parentEntry = FindEntry(p.x, p.y);
                if (parentEntry == null) continue;
                Vector2 parentPos = GridToCanvas(parentEntry.level, parentEntry.slot);
                DrawArrow(parentPos, childPos, COL_CONN_LINE);
            }
        }
    }

    private void DrawConnectModePreview()
    {
        if (!_connectMode || _connectSource == null) return;
        Vector2 from = GridToCanvas(_connectSource.level, _connectSource.slot);
        Vector2 to   = Event.current.mousePosition;
        DrawArrow(from, to, COL_CONN_PENDING);
        Repaint();
    }

    private void DrawNodes()
    {
        foreach (var entry in _target.nodeEntries)
        {
            // 드래그 중인 노드는 원래 위치 대신 마우스 위치에 그린다
            bool isDragging = _isDraggingNode && entry == _draggingEntry;
            Vector2 pos = isDragging ? _draggingCanvasPos : GridToCanvas(entry.level, entry.slot);
            float   r   = NODE_RADIUS * _zoom;

            bool isSel = entry == _selectedEntry;
            bool isHov = entry == _hoveredEntry;

            // 드래그 중이면 스냅될 셀 하이라이트
            if (isDragging)
            {
                Vector2Int snapCell = CanvasToGrid(_draggingCanvasPos);
                bool validSnap = snapCell.x >= 0 && snapCell.x < _target.totalLevels &&
                                 snapCell.y >= 0 && snapCell.y < _target.maxNodesPerLevel;
                if (validSnap)
                {
                    Vector2 snapPos  = GridToCanvas(snapCell.x, snapCell.y);
                    bool    occupied = FindEntry(snapCell.x, snapCell.y) != null &&
                                       FindEntry(snapCell.x, snapCell.y) != entry;
                    Color snapColor  = occupied
                        ? new Color(1f, 0.3f, 0.3f, 0.35f)   // 충돌 → 빨강
                        : new Color(0.3f, 1f, 0.5f, 0.35f);  // 이동 가능 → 초록
                    DrawDisc(snapPos, r + 6f * _zoom, snapColor);
                }
            }

            // 외곽 링
            Color rimColor = isDragging  ? new Color(1f, 1f, 0.3f) :
                             isSel       ? COL_SELECTED :
                             isHov       ? COL_HOVERED  : Color.black;
            DrawDisc(pos, r + 3f * _zoom, rimColor);

            // 노드 본체 (드래그 중엔 반투명)
            Color bodyColor = NODE_COLORS.TryGetValue(entry.nodeType, out var c) ? c : Color.gray;
            if (isDragging) bodyColor.a = 0.75f;
            DrawDisc(pos, r, bodyColor);

            // 레이블
            GUIStyle style = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal    = { textColor = Color.white },
                fontSize  = Mathf.RoundToInt(9 * _zoom),
                fontStyle = FontStyle.Bold,
            };
            string label = entry.nodeType == NodeType.None
                ? "?"
                : entry.nodeType.ToString()[..Mathf.Min(2, entry.nodeType.ToString().Length)];
            GUI.Label(new Rect(pos.x - r, pos.y - r, r * 2, r * 2), label, style);

            // level / slot 작은 텍스트
            GUIStyle subStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.UpperCenter,
                normal    = { textColor = new Color(1, 1, 1, 0.6f) },
                fontSize  = Mathf.RoundToInt(8 * _zoom),
            };
            GUI.Label(new Rect(pos.x - 30, pos.y + r + 2, 60, 14),
                $"L{entry.level} S{entry.slot}", subStyle);
        }
    }

    // ─────────────────────────────────────────────
    // Inspector Panel
    // ─────────────────────────────────────────────
    private void DrawInspectorPanel(Rect rect)
    {
        EditorGUI.DrawRect(rect, new Color(0.18f, 0.18f, 0.18f));
        // 구분선
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), new Color(0.4f, 0.4f, 0.4f));

        GUILayout.BeginArea(new Rect(rect.x + 6, rect.y + 6, rect.width - 12, rect.height - 12));
        _inspectorScroll = EditorGUILayout.BeginScrollView(_inspectorScroll);

        if (_selectedEntry == null)
        {
            EditorGUILayout.HelpBox("노드를 클릭하면 여기서 편집할 수 있어요.", MessageType.Info);
        }
        else
        {
            DrawEntryInspector(_selectedEntry);
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndArea();
    }

    private void DrawEntryInspector(MapNodeLayoutData.NodeEntry entry)
    {
        GUIStyle header = new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 };
        EditorGUILayout.LabelField($"Node  L{entry.level} · S{entry.slot}", header);
        EditorGUILayout.Space(4);

        // NodeType
        Undo.RecordObject(_target, "Edit NodeType");
        var newType = (NodeType)EditorGUILayout.EnumPopup("Type", entry.nodeType);
        if (newType != entry.nodeType)
        {
            entry.nodeType = newType;
            EditorUtility.SetDirty(_target);
        }

        EditorGUILayout.Space(4);

        // MapNodeData
        var newData = (MapNodeData)EditorGUILayout.ObjectField(
            new GUIContent("MapNodeData", "비워두면 런타임에 Addressables에서 랜덤 로드"),
            entry.mapNodeData, typeof(MapNodeData), false);
        if (newData != entry.mapNodeData)
        {
            Undo.RecordObject(_target, "Edit MapNodeData");
            entry.mapNodeData = newData;
            // mapNodeData가 할당되면 nodeType을 자동으로 맞춰주지 않음 —
            // 사용자가 의도적으로 다르게 설정할 수 있으므로 수동 유지
            EditorUtility.SetDirty(_target);
        }

        // mapNodeData 미할당 시 안내
        if (entry.mapNodeData == null)
        {
            EditorGUILayout.HelpBox("미할당 시 Addressables에서 타입에 맞는 SO를 랜덤 로드합니다.", MessageType.None);
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Parent Connections", EditorStyles.boldLabel);

        for (int i = 0; i < entry.parentEntries.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            var p = entry.parentEntries[i];
            EditorGUILayout.LabelField($"  L{p.x} · S{p.y}", GUILayout.ExpandWidth(true));
            if (GUILayout.Button("✕", GUILayout.Width(24)))
            {
                Undo.RecordObject(_target, "Remove Parent");
                entry.parentEntries.RemoveAt(i);
                EditorUtility.SetDirty(_target);
                break;
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(4);
        if (GUILayout.Button("+ 연결 추가 (드래그)"))
        {
            _connectMode   = true;
            _connectSource = entry;
        }

        EditorGUILayout.Space(12);
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("노드 삭제"))
        {
            Undo.RecordObject(_target, "Delete Node");
            _target.nodeEntries.Remove(entry);
            _selectedEntry = null;
            EditorUtility.SetDirty(_target);
        }
        GUI.backgroundColor = Color.white;
    }

    // ─────────────────────────────────────────────
    // Input Handling
    // ─────────────────────────────────────────────
    private void HandleCanvasInput(Rect canvasRect)
    {
        Event e = Event.current;
        if (!canvasRect.Contains(e.mousePosition)) return;

        // 캔버스 로컬 좌표
        Vector2 localMouse = e.mousePosition - new Vector2(canvasRect.x, canvasRect.y);

        // 호버 업데이트
        _hoveredEntry = GetEntryAtPosition(localMouse);

        switch (e.type)
        {
            case EventType.MouseDown:
                HandleMouseDown(e, localMouse);
                break;

            case EventType.MouseUp:
                HandleMouseUp(e, localMouse);
                break;

            case EventType.MouseDrag when e.button == 2: // 미들 드래그: 패닝
                _scrollOffset += e.delta;
                e.Use();
                Repaint();
                break;

            case EventType.MouseDrag when e.button == 0 && _isDraggingNode:
                _draggingCanvasPos = localMouse;
                e.Use();
                Repaint();
                break;

            case EventType.ScrollWheel: // 줌
                _zoom = Mathf.Clamp(_zoom - e.delta.y * 0.05f, 0.4f, 2.0f);
                e.Use();
                Repaint();
                break;

            case EventType.MouseMove:
                Repaint();
                break;

            case EventType.KeyDown when e.keyCode == KeyCode.Delete:
                if (_selectedEntry != null)
                {
                    Undo.RecordObject(_target, "Delete Node");
                    foreach (var e2 in _target.nodeEntries)
                        e2.parentEntries.RemoveAll(p => p.x == _selectedEntry.level && p.y == _selectedEntry.slot);
                    _target.nodeEntries.Remove(_selectedEntry);
                    _selectedEntry = null;
                    EditorUtility.SetDirty(_target);
                    e.Use();
                    Repaint();
                }
                break;
        }
    }

    private void HandleMouseDown(Event e, Vector2 localMouse)
    {
        var hit = GetEntryAtPosition(localMouse);

        // 연결 모드 중 좌클릭: 연결 취소
        if (_connectMode && e.button == 0 && hit == null)
        {
            _connectMode   = false;
            _connectSource = null;
            e.Use();
            return;
        }

        if (e.button == 0) // 좌클릭
        {
            if (_connectMode && hit != null && hit != _connectSource)
            {
                // 연결 완료: hit를 source의 parent로 추가
                AddParentConnection(_connectSource, hit);
                _connectMode   = false;
                _connectSource = null;
            }
            else if (!_connectMode && hit != null)
            {
                _selectedEntry     = hit;
                _isDraggingNode    = true;
                _draggingEntry     = hit;
                _draggingCanvasPos = localMouse;
            }
            else
            {
                _selectedEntry = hit; // null이면 선택 해제
            }
            e.Use();
            Repaint();
        }
        else if (e.button == 1) // 우클릭
        {
            if (hit != null)
                ShowNodeContextMenu(hit);
            else
                ShowCanvasContextMenu(localMouse);
            e.Use();
        }
    }

    private void HandleMouseUp(Event e, Vector2 localMouse)
    {
        // 연결 모드 드래그 릴리즈
        if (_connectMode && e.button == 0)
        {
            var hit = GetEntryAtPosition(localMouse);
            if (hit != null && hit != _connectSource)
                AddParentConnection(_connectSource, hit);

            _connectMode   = false;
            _connectSource = null;
            e.Use();
            Repaint();
            return;
        }

        // 노드 드래그 릴리즈 → 그리드 셀에 스냅
        if (_isDraggingNode && e.button == 0 && _draggingEntry != null)
        {
            Vector2Int targetCell = CanvasToGrid(localMouse);
            bool validCell = targetCell.x >= 0 && targetCell.x < _target.totalLevels &&
                             targetCell.y >= 0 && targetCell.y < _target.maxNodesPerLevel;

            if (validCell)
            {
                // 목적지에 이미 다른 노드가 있는지 확인
                var occupant = FindEntry(targetCell.x, targetCell.y);
                if (occupant != null && occupant != _draggingEntry)
                {
                    // 충돌 → 이동 취소 (원위치 유지)
                    Debug.Log($"[MapNodeLayoutEditor] 이미 노드가 있는 셀입니다 (L{targetCell.x} S{targetCell.y}).");
                }
                else if (targetCell.x != _draggingEntry.level || targetCell.y != _draggingEntry.slot)
                {
                    Undo.RecordObject(_target, "Move Node");

                    int oldLevel = _draggingEntry.level;
                    int oldSlot  = _draggingEntry.slot;

                    // 다른 노드의 parentEntries 중 이 노드를 참조하는 것도 갱신
                    foreach (var entry in _target.nodeEntries)
                    {
                        for (int i = 0; i < entry.parentEntries.Count; i++)
                        {
                            if (entry.parentEntries[i].x == oldLevel &&
                                entry.parentEntries[i].y == oldSlot)
                                entry.parentEntries[i] = new Vector2Int(targetCell.x, targetCell.y);
                        }
                    }

                    _draggingEntry.level = targetCell.x;
                    _draggingEntry.slot  = targetCell.y;
                    EditorUtility.SetDirty(_target);
                }
            }

            _isDraggingNode = false;
            _draggingEntry  = null;
            e.Use();
            Repaint();
        }
    }

    // ─────────────────────────────────────────────
    // Context Menus
    // ─────────────────────────────────────────────
    private void ShowNodeContextMenu(MapNodeLayoutData.NodeEntry entry)
    {
        var menu = new GenericMenu();

        menu.AddItem(new GUIContent("선택"), false, () =>
        {
            _selectedEntry = entry;
            Repaint();
        });

        menu.AddSeparator("");

        // NodeType 변경
        foreach (NodeType t in Enum.GetValues(typeof(NodeType)))
        {
            NodeType captured = t;
            bool isCurrent = entry.nodeType == t;
            menu.AddItem(new GUIContent($"타입 변경/{t}"), isCurrent, () =>
            {
                Undo.RecordObject(_target, "Change NodeType");
                entry.nodeType = captured;
                EditorUtility.SetDirty(_target);
                Repaint();
            });
        }

        menu.AddSeparator("");

        menu.AddItem(new GUIContent("연결 추가 (드래그)"), false, () =>
        {
            _connectMode   = true;
            _connectSource = entry;
            Repaint();
        });

        menu.AddItem(new GUIContent("연결 모두 제거"), false, () =>
        {
            Undo.RecordObject(_target, "Clear Connections");
            entry.parentEntries.Clear();
            // 이 노드를 parent로 가지는 다른 entry의 연결도 제거
            foreach (var e2 in _target.nodeEntries)
                e2.parentEntries.RemoveAll(p => p.x == entry.level && p.y == entry.slot);
            EditorUtility.SetDirty(_target);
            Repaint();
        });

        menu.AddSeparator("");

        menu.AddItem(new GUIContent("노드 삭제"), false, () =>
        {
            Undo.RecordObject(_target, "Delete Node");
            if (_selectedEntry == entry) _selectedEntry = null;
            _target.nodeEntries.Remove(entry);
            // 이 노드를 참조하는 다른 연결도 정리
            foreach (var e2 in _target.nodeEntries)
                e2.parentEntries.RemoveAll(p => p.x == entry.level && p.y == entry.slot);
            EditorUtility.SetDirty(_target);
            Repaint();
        });

        menu.ShowAsContext();
    }

    private void ShowCanvasContextMenu(Vector2 localMouse)
    {
        Vector2Int cell = CanvasToGrid(localMouse);
        if (cell.x < 0 || cell.x >= _target.totalLevels ||
            cell.y < 0 || cell.y >= _target.maxNodesPerLevel)
            return;

        var menu = new GenericMenu();

        if (FindEntry(cell.x, cell.y) == null)
        {
            menu.AddItem(new GUIContent("노드 추가"), false, () =>
            {
                Undo.RecordObject(_target, "Add Node");
                var newEntry = new MapNodeLayoutData.NodeEntry
                    { level = cell.x, slot = cell.y, nodeType = NodeType.Battle };
                _target.nodeEntries.Add(newEntry);
                _selectedEntry = newEntry;
                EditorUtility.SetDirty(_target);
                Repaint();
            });

            // 타입 선택하며 추가
            foreach (NodeType t in Enum.GetValues(typeof(NodeType)))
            {
                if (t == NodeType.None) continue;
                NodeType captured = t;
                menu.AddItem(new GUIContent($"노드 추가 ({t})"), false, () =>
                {
                    Undo.RecordObject(_target, "Add Node");
                    var newEntry = new MapNodeLayoutData.NodeEntry
                        { level = cell.x, slot = cell.y, nodeType = captured };
                    _target.nodeEntries.Add(newEntry);
                    _selectedEntry = newEntry;
                    EditorUtility.SetDirty(_target);
                    Repaint();
                });
            }
        }
        else
        {
            menu.AddDisabledItem(new GUIContent("이미 노드가 있습니다"));
        }

        menu.ShowAsContext();
    }

    // ─────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────
    private void RecalcCellSize()
    {
        float canvasW = position.width - _inspectorWidth - RESIZE_HANDLE_WIDTH;
        _cellW = (canvasW - GRID_PADDING_X * 2f) / (_target.maxNodesPerLevel + 1);
        _cellH = _cellHOverride;
    }

    private void DrawResizeHandle(Rect handleRect)
    {
        // 핸들 배경
        Color col = _isResizing
            ? new Color(0.4f, 0.7f, 1f, 0.9f)
            : new Color(0.35f, 0.35f, 0.35f, 1f);
        EditorGUI.DrawRect(handleRect, col);

        // 중앙 점선 장식
        float cx  = handleRect.x + handleRect.width * 0.5f;
        float gap = 6f;
        Handles.color = new Color(0.7f, 0.7f, 0.7f, 0.6f);
        for (float y = handleRect.y + gap; y < handleRect.yMax - gap; y += gap * 1.5f)
            Handles.DrawLine(new Vector3(cx - 1, y), new Vector3(cx + 1, y + 2f));

        // 커서 변경
        EditorGUIUtility.AddCursorRect(handleRect, MouseCursor.ResizeHorizontal);
    }

    private void HandleResizeInput(Rect handleRect)
    {
        Event e = Event.current;

        switch (e.type)
        {
            case EventType.MouseDown when e.button == 0 && handleRect.Contains(e.mousePosition):
                _isResizing = true;
                e.Use();
                break;

            case EventType.MouseDrag when _isResizing:
                // 마우스를 왼쪽으로 드래그 → Inspector 넓어짐
                _inspectorWidth = Mathf.Clamp(
                    _inspectorWidth - e.delta.x,
                    INSPECTOR_WIDTH_MIN,
                    INSPECTOR_WIDTH_MAX);
                e.Use();
                Repaint();
                break;

            case EventType.MouseUp when _isResizing:
                _isResizing = false;
                e.Use();
                break;
        }
    }

    /// <summary>grid(level, slot) → canvas 로컬 좌표</summary>
    private Vector2 GridToCanvas(int level, int slot)
    {
        // level 0 = 아래쪽 (y가 큼), totalLevels-1 = 위쪽
        int displayRow = (_target.totalLevels - 1) - level;
        float x = GRID_PADDING_X + (slot + 1) * _cellW * _zoom + _scrollOffset.x;
        float y = GRID_PADDING_Y + (displayRow + 1) * _cellH * _zoom + _scrollOffset.y;
        return new Vector2(x, y);
    }

    /// <summary>canvas 로컬 좌표 → grid(level, slot). 범위 밖이면 (-1,-1)</summary>
    private Vector2Int CanvasToGrid(Vector2 canvasPos)
    {
        float gx = (canvasPos.x - _scrollOffset.x - GRID_PADDING_X) / (_cellW * _zoom) - 1f;
        float gy = (canvasPos.y - _scrollOffset.y - GRID_PADDING_Y) / (_cellH * _zoom) - 1f;

        int displayRow = Mathf.RoundToInt(gy);
        int slot       = Mathf.RoundToInt(gx);
        int level      = (_target.totalLevels - 1) - displayRow;

        return new Vector2Int(level, slot);
    }

    private MapNodeLayoutData.NodeEntry GetEntryAtPosition(Vector2 canvasPos)
    {
        float threshold = NODE_RADIUS * _zoom * 1.3f;
        foreach (var entry in _target.nodeEntries)
        {
            Vector2 nodePos = GridToCanvas(entry.level, entry.slot);
            if (Vector2.Distance(canvasPos, nodePos) <= threshold)
                return entry;
        }
        return null;
    }

    private MapNodeLayoutData.NodeEntry FindEntry(int level, int slot)
    {
        return _target.nodeEntries.Find(e => e.level == level && e.slot == slot);
    }

    private void AddParentConnection(MapNodeLayoutData.NodeEntry child, MapNodeLayoutData.NodeEntry parent)
    {
        var key = new Vector2Int(parent.level, parent.slot);
        if (child.parentEntries.Contains(key)) return;
        if (parent.level >= child.level)
        {
            EditorUtility.DisplayDialog("연결 오류",
                "Parent는 반드시 더 낮은 level이어야 합니다.", "확인");
            return;
        }
        Undo.RecordObject(_target, "Add Parent Connection");
        child.parentEntries.Add(key);
        EditorUtility.SetDirty(_target);
        Repaint();
    }

    private void DrawDisc(Vector2 center, float radius, Color color)
    {
        Handles.color = color;
        Handles.DrawSolidDisc(new Vector3(center.x, center.y, 0), Vector3.forward, radius);
    }

    private void DrawArrow(Vector2 from, Vector2 to, Color color)
    {
        Handles.color = color;
        Vector2 dir  = (to - from).normalized;
        Vector2 end  = to - dir * NODE_RADIUS * _zoom;

        // 선
        Handles.DrawAAPolyLine(LINE_THICKNESS * _zoom, from, end);

        // 화살촉
        float   arrowSize = 8f * _zoom;
        Vector3 right3 = new Vector3(-dir.y, dir.x, 0f);
        Vector3 dir3   = new Vector3(dir.x, dir.y, 0f);
        Vector3 tip    = new Vector3(end.x, end.y, 0f);
        Vector3 bl     = tip - dir3 * arrowSize + right3 * arrowSize * 0.5f;
        Vector3 br     = tip - dir3 * arrowSize - right3 * arrowSize * 0.5f;
        Handles.DrawAAConvexPolygon(tip, bl, br);
    }

    private void ResetView()
    {
        _scrollOffset   = Vector2.zero;
        _zoom           = 1f;
        _selectedEntry  = null;
        _connectMode    = false;
        _connectSource  = null;
        _isDraggingNode = false;
        _draggingEntry  = null;
        Repaint();
    }

    private void CreateNewSO()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "새 MapNodeLayoutData 저장",
            "NewMapNodeLayout",
            "asset",
            "저장 위치를 선택하세요");

        if (string.IsNullOrEmpty(path)) return;

        var asset = CreateInstance<MapNodeLayoutData>();
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        _target = asset;
        ResetView();
    }

    private void DrawEmptyState()
    {
        GUIStyle style = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { fontSize = 13 };
        GUI.Label(new Rect(0, TOOLBAR_HEIGHT, position.width, position.height - TOOLBAR_HEIGHT),
            "위에서 MapNodeLayoutData SO를 선택하거나\n'New SO' 버튼으로 새로 만드세요.", style);
    }
}

// ─────────────────────────────────────────────────────
// Asset Open Handler: SO 더블클릭 시 에디터 자동 오픈
// ─────────────────────────────────────────────────────
public static class MapNodeLayoutAssetHandler
{
    [UnityEditor.Callbacks.OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        var obj = EditorUtility.InstanceIDToObject(instanceID);
        if (obj is MapNodeLayoutData data)
        {
            MapNodeLayoutEditor.OpenWithAsset(data);
            return true;
        }
        return false;
    }
}
#endif