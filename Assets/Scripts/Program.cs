using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;


public class Program : Singleton<Program>
{
    protected Program() { }
    [SerializeField] Transform _pathObjectParent;
    [SerializeField] Transform _areaObjectParent;
    [SerializeField] GameObject _pathObjectPrefab;
    [SerializeField] GameObject _areaObjectPrefab;
    [SerializeField] List<EditmodeCollection> _editModes;
    [SerializeField] HorizontalLayoutGroup _layout;
    [SerializeField] RectTransform _rectTransform;
    [SerializeField] Transform _editor;
    [SerializeField] MultiSelectionRectRenderer _selectRectangle;
    public MultiSelectionRectRenderer SelectionRectangle => _selectRectangle;
    [SerializeField] ColorSelector _colorSelector;
    [SerializeField] Colorpicker _colorPicker;
    [SerializeField] Canvas _canvas;
    public Colorpicker ColorPicker => _colorPicker;

    public Vector3 DownLeft;
    public Vector3 UpRight;

    public int SEGMENTS_PER_CURVE;

    private EditmodeBase _currentEditmode = null;
    private List<EditmodeBase> _currentModes = new List<EditmodeBase>();
    private EditmodeCollection _currentCollection = null;

    ScalableObject _hoveredObject;

    private Vector2 _cursorPosition = new Vector2(30, 20);
    private Camera _camera;
    private float _leftEditorBorder;
    private float _rightEditorBorder;
    private float _upEditorBorder;
    private float _downEditorBorder;
    public Vector3[] Corners;

    private bool _isDragging;
    private bool _hovered = true;

    // Called when Object hovered
    public delegate void CurrentHoveredObjectChanged(ScalableObject current);
    public static event CurrentHoveredObjectChanged OnHoveredObjectChanged;

    public delegate void CurrentSelectedObjectChanged(ScalableObject current);
    public static event CurrentSelectedObjectChanged OnSelectedObjectChanged;

    DrawareaObjectsUpdater _drawAreaObjectsUpdater = new DrawareaObjectsUpdater();
    PathObjectsUpdater _pathObjectsUpdater = new PathObjectsUpdater();
    ObjectListUpdaterBase _currentUpdater;

    public void Awake()
    {
        HoverData _hoverData = new HoverData();
        foreach (var item in _editModes)
        {
            item.Init(_hoverData);
        }

        _colorSelector.Init(_colorPicker);

        _layout.childControlHeight = false;
        _layout.childControlWidth = false;

        if (_colorPicker == null)
            _colorPicker = FindObjectOfType<Colorpicker>();
        _colorPicker.Init();
    }

    public void Start()
    {
        if (_camera == null)
            _camera = FindObjectOfType<Camera>();

        _canvas.renderMode = RenderMode.ScreenSpaceCamera;
        _canvas.worldCamera = _camera;
        SetEditorBorders();

        _drawAreaObjectsUpdater.Init(_areaObjectPrefab, _areaObjectParent);
        _drawAreaObjectsUpdater.NewObject(true);
        _pathObjectsUpdater.Init(_pathObjectPrefab, _pathObjectParent);
    }

    public void SetEditorBorders()
    {
        Corners = new Vector3[4];

        _rectTransform.GetWorldCorners(Corners);

        DownLeft = _editor.InverseTransformPoint(Corners[0]);
        UpRight = _editor.InverseTransformPoint(Corners[2]);

        _leftEditorBorder = Corners[0].x;
        _rightEditorBorder = Corners[2].x;
        _downEditorBorder = Corners[0].y;
        _upEditorBorder = Corners[2].y;
    }


    public void UpdateHoveredPathObject(ScalableObject scalableObject)
    {
        if (_currentUpdater != null)
            _currentUpdater.SetLastSelected(scalableObject);

        OnHoveredObjectChanged(scalableObject);

        ChangeState();
    }

    public void UpdateSelectedPathObject(ScalableObject scalableObject)
    {
        if (scalableObject != null)
            OnSelectedObjectChanged(scalableObject);
    }

    public bool PathObjectHovered()
    {
        return _hoveredObject != null;
    }


    void Update()
    {
        if (_currentModes.Count == 0 || _isDragging)
            return;

        EditmodeBase mode = _currentEditmode;

        if (CursorNotOverEditor())
        {
            SetCursorTexture(null);
            _currentEditmode = null;
            return;
        }

        bool overObject;

        if (_currentUpdater.OtherObjectHovered(out _hoveredObject, out overObject))
            UpdateHoveredPathObject(_hoveredObject);

        if (overObject)
        {
            _hovered = true;

            for (int i = 0; i < _currentModes.Count; i++)
            {
                if (_currentModes[i].Suscribed && _currentModes[i].CheckCondition(_hoveredObject))
                {
                    mode = _currentModes[i];

                    break;
                }
            }
        }
        else
        {
            if (_hovered)
            {
                _currentCollection.Hover(false);
                _hovered = false;
            }
            mode = _currentCollection.FreeSpaceEditmode;
        }

        if (_currentEditmode != mode)
        {
            _currentEditmode = mode;
            Cursor.SetCursor(_currentEditmode.cursor, _cursorPosition, CursorMode.Auto);

            SetCursorTexture(_currentEditmode.cursor);
        }

        _currentCollection.CheckKeyboardInput();
    }

    void SetCursorTexture(Texture2D texture)
    {
        Cursor.SetCursor(texture, _cursorPosition, CursorMode.Auto);
    }

    public void UseCollection(EditmodeCollection collection)
    {
        _hovered = false;

        _currentModes = collection.Modes;
        if (_currentCollection != null)
            _currentCollection.Exit();
        _currentCollection = collection;

        if (_currentCollection.UsePathObjectList)
        {
            _currentUpdater = _pathObjectsUpdater;
            _pathObjectsUpdater.OnHoverUpdate = _currentCollection.OnPathObjectHoverUpdate;
        }
        else
        {
            _currentUpdater = _drawAreaObjectsUpdater;
            _drawAreaObjectsUpdater.OnHoverUpdate = _currentCollection.OnAreaObjectHoverUpdate;
        }
        _currentUpdater.Unselect();

        _currentCollection.Enter();
        UpdateSelectedPathObject(null);
        _currentUpdater.IsPathObjectUpdateAllowed = true;
        ChangeState();
    }

    public void ChangeState()
    {
        if (_currentCollection != null)
            _currentCollection.ChangeState();
    }


    //////////////////////////// ObjectListUpdater ///////////////////////////

    public ScalableObject GetAreaObject()
    {
        ScalableObject drawArea = _drawAreaObjectsUpdater.GetFirstSelectedObject();
        if (drawArea == null)
        {
            drawArea = _drawAreaObjectsUpdater.NewObject(true);
        }

        return drawArea;
    }

    public void AllowPathObjectUpdate()
    {
        _currentUpdater.Unselect();
        ChangeState();
    }

    public bool IsPathObjectUpdateAllowed()
    {
        return _pathObjectsUpdater.IsPathObjectUpdateAllowed;
    }

    public void SetPathObjectUpdate(bool isAllowed)
    {
        _pathObjectsUpdater.IsPathObjectUpdateAllowed = isAllowed;
    }

    public void NewPathObject()
    {
        _currentUpdater.NewObject(_currentUpdater != _pathObjectsUpdater);
    }

    public void Delete(ScalableObject scalableObject)
    {
        _currentUpdater.DeleteObject(scalableObject);
    }

    public void ShowPointsHandleRenderer(bool enable)
    {
        _pathObjectsUpdater.ShowPointsHandleRenderer(enable);
    }

    public void Duplicate()
    {
        _currentUpdater.DuplicateObject();
    }

    public List<ScalableObject> CurrentScalableList()
    {
        return _currentUpdater.ObjectList;
    }

    public void SetBoundingBoxPoint(int index)
    {
        _currentUpdater.SetBoundingBoxPoint(index);
    }

    public void SortPathObjectList()
    {
        _currentUpdater.SortObjectList();
    }

    public void HideAllBoundingBoxes()
    {
        _drawAreaObjectsUpdater.HideAllBoundingBoxes();
        _pathObjectsUpdater.HideAllBoundingBoxes();
    }

    //////////////////////////// User Input ///////////////////////////


    public void OnInitDrag(BaseEventData data)
    {
        _isDragging = true;

        if (_currentEditmode == null)
            return;

        PointerEventData pointerEventData = data as PointerEventData;

        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            _currentEditmode.CallOnDown();

        }
    }

    public void OnDrag(BaseEventData data)
    {
        PointerEventData pointerEventData = data as PointerEventData;

        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            if (_currentEditmode != null)
                _currentEditmode.CallDrag();
        }
    }

    public void OnUp(BaseEventData data)
    {
        PointerEventData pointerEventData = data as PointerEventData;
        if (pointerEventData.button == PointerEventData.InputButton.Left)
        {
            if (_currentEditmode != null)
                _currentEditmode.CallOnUp();
        }
        else if (pointerEventData.button == PointerEventData.InputButton.Right)
        {
            AllowPathObjectUpdate();
        }
        _isDragging = false;
    }

    public Vector2 GetMouseCanvasPosition()
    {
        return _editor.InverseTransformPoint(WorldPosition());
    }

    private Vector2 WorldPosition()
    {
        Vector3 mousePosition = Input.mousePosition;

        mousePosition.z = 100;
        return _camera.ScreenToWorldPoint(mousePosition);
    }

    public bool CursorNotOverEditor()
    {
        Vector3 mousePosition = WorldPosition();

        return mousePosition.x < _leftEditorBorder || mousePosition.x > _rightEditorBorder || mousePosition.y > _upEditorBorder || mousePosition.y < _downEditorBorder;
    }
}


public class HoverData
{
    public BezierSegment HoveredBeziersegment;
    public int HoveredSegmentIndex;
    public int HoveredSegmentPointIndex;
    public bool IsSecondHandleHovered;

    /// <summary>
    /// Event is raised when User hovers over a BezierSegment
    /// </summary>
    /// <param name="data">HoverData is used by Updater class</param>
    public delegate void CurrentSegmentChanged(HoverData data);
    public static event CurrentSegmentChanged BezierSegmentChanged;

    public void UpdateBezierSegment(BezierSegment current)
    {
        HoveredBeziersegment = current;
        BezierSegmentChanged(this);
    }

    public void UnHoverCurrentEverything()
    {
        if (HoveredBeziersegment == null)
            return;

        UpdateBezierSegment(null);
    }
}

