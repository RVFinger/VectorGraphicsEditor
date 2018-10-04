using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HandlesPointsUpdater : UpdaterBase, IMultiSelector
{
    private BezierSegment _currentBezierSegment;
    private BezierSegment _currentHoveredSegment;
    private HoverData _currentHoverData;

    private SelectionMode _currentSelectionMode = SelectionMode.NoSelection;
    private PathObject _lastSelectedObject;


    public override void Init()
    {
        base.Init();

        HoverData.BezierSegmentChanged += SetBezierSegmentData;
    }

    public override void OnDisable()
    {
        base.OnDisable();

        HoverData.BezierSegmentChanged -= SetBezierSegmentData;
    }

    void SetBezierSegmentData(HoverData data)
    {
        UnhoverCurrentData();
        _currentHoverData = data;
        _currentHoveredSegment = data.HoveredBeziersegment;
    }

    public void UnhoverCurrentData()
    {
        if (_currentHoveredSegment == null)
            return;
        _currentHoveredSegment.PointHovered = false;
        _currentHoveredSegment.FirstHandleHovered = false;
        _currentHoveredSegment.SecondHandleHovered = false;
    }

    public void UpdateCurrentBezierHandles()
    {
        Vector2 position = _program.GetMouseCanvasPosition();

        _currentBezierSegment.MoveHandle(position, _currentHoverData.IsSecondHandleHovered);
        _currentPathObject.UpdatePointsHandlesRenderer();
    }

    public void SelectPathObject()
    {
        if (_currentPathObject == null)
            return;
        _currentPathObject.Selected = true;
        _currentPathObject.EnablePointsHandlesRenderer(true);
        _program.UpdateSelectedPathObject(_currentPathObject);
        if (_lastSelectedObject && _lastSelectedObject != _currentPathObject)
        {
            _lastSelectedObject.Selected = false;
            _lastSelectedObject.EnablePointsHandlesRenderer(false);
            for (int i = 0; i < _selectedSegments.Count; i++)
                _selectedSegments[i].PointSelected = false;
        }
        _selectedSegments = _currentPathObject.BezierCurve;

        _lastSelectedObject = _currentPathObject;
    }

    public void UnSelectPathObject()
    {
        if (_currentSelectionMode == SelectionMode.NoSelection)
        {
            if (_lastSelectedObject)
            {
                _lastSelectedObject.Selected = false;
                _lastSelectedObject.EnablePointsHandlesRenderer(false);
            }
            _program.UpdateSelectedPathObject(null);
        }
    }

    public void OnExit()
    {
        if (_lastSelectedObject)
        {
            _lastSelectedObject.Selected = false;
            _lastSelectedObject.EnablePointsHandlesRenderer(false);
        }
        _program.UpdateSelectedPathObject(null);
        _lastSelectedObject = null;

        if(_selectedSegments != null)
        {
            for (int i = 0; i < _selectedSegments.Count; i++)
                _selectedSegments[i].PointSelected = false;
        }

    }

    public void MovePoint()
    {
        BezierSegment current = _currentHoverData.HoveredBeziersegment;
        current.PointSelected = true;

        Vector2 newPosition = _program.GetMouseCanvasPosition() - current.Point;

        for (int i = 0; i < _segments.Count; i++)
            if (_segments[i].PointSelected)
                _segments[i].MovePoints(newPosition);

        _currentPathObject.UpdatePointsHandlesRenderer();
    }

    public void UnselectBezierSegment()
    {
        if (_currentHoveredObject == null)
            return;

        _currentSelectionMode = SelectionMode.NoSelection;

        for (int i = 0; i < _segments.Count; i++)
            _selectedSegments[i].PointSelected = false;
        _currentBezierSegment = null;

        //if(_currentPathObject != null)
        _currentPathObject.UpdatePointsHandlesRenderer();
    }

    // Click on point or handle
    public void SelectBezierSegment(SelectionMode mode)
    {
        if (_currentHoverData == null || _currentHoverData.HoveredBeziersegment == null)
            return;

        BezierSegment previousSelected = _currentBezierSegment;
        _currentBezierSegment = _currentHoverData.HoveredBeziersegment;

        if ((_currentSelectionMode == SelectionMode.MultiplePoints && mode == SelectionMode.Point))
            return;

        _currentSelectionMode = mode;

        if (previousSelected != null)
            previousSelected.PointSelected = false;
        _currentBezierSegment.PointSelected = true;

        _currentPathObject.UpdatePointsHandlesRenderer();

        UpdateMenu();
    }

    // Click on Free Space
    public void StartSelection()
    {
        if (_currentHoveredObject == null)
            return;
        ItemSelected = 0;
        _currentSelectionMode = SelectionMode.NoSelection;

        StartSelectionPoint = _program.GetMouseCanvasPosition();

        for (int i = 0; i < _segments.Count; i++)
            _segments[i].PointSelected = false;
    }

    public void UpdateMenu()
    {

        _editorMenu.UpdateMenu(_currentSelectionMode);
        //_editorMenu.UpdateMenu(_currentSelectionMode != SelectionMode.NoSelection);
    }

    // on up after multiselect
    public void EndSelection()
    {
        Renderer.UpdateRectangle(Vector2.zero, Vector2.zero);

        if (ItemSelected > 1)
            _currentSelectionMode = SelectionMode.MultiplePoints;
        else if (ItemSelected == 1)
            _currentSelectionMode = SelectionMode.Point;
        else
            _currentSelectionMode = SelectionMode.NoSelection;

        UpdateMenu();

        if (_currentPathObject != null)
            _currentPathObject.UpdatePointsHandlesRenderer();
    }

    // on up after move
    public void UnselectMultiSelection()
    {
        if (_currentSelectionMode != SelectionMode.Point)
            UnselectBezierSegment();
        UpdateMenu();
    }

    public void Select()
    {
        if (_currentSelectedPathObject == null)
            return;
        Vector2 mousePosition = _program.GetMouseCanvasPosition();

        Renderer.UpdateRectangle(StartSelectionPoint, mousePosition);
        ItemSelected = 0;

        _currentSelectionMode = SelectionMode.MultiplePoints;



        BezierSegment selected = null;
        List<BezierSegment> segments = _currentSelectedPathObject.BezierCurve;
        for (int i = 0; i < segments.Count; i++)
        {
            BezierSegment segment = segments[i];

            Vector2 segmentPoint = segment.Point;
            segment.PointSelected = false;

            if ((segmentPoint.x < mousePosition.x && segmentPoint.x < StartSelectionPoint.x)
                || (segmentPoint.x > mousePosition.x && segmentPoint.x > StartSelectionPoint.x)
                || (segmentPoint.y < mousePosition.y && segmentPoint.y < StartSelectionPoint.y)
                || (segmentPoint.y > mousePosition.y && segmentPoint.y > StartSelectionPoint.y))
                continue;
            segment.PointSelected = true;
            selected = segment;
            ItemSelected++;
        }
        if (ItemSelected == 1)
            _currentBezierSegment = selected;
        _currentPathObject.UpdatePointsHandlesRenderer();
    }


    /////////////////////////  Functions called from EditorMenu //////////////////////////

    public bool Conjunct()
    {
        if (_currentBezierSegment == null)
            return false;
        return _currentBezierSegment.Conjunct;
    }

    public void SetConjunct(bool isConjunct)
    {
        for (int i = 0; i < _segments.Count; i++)
            if (_selectedSegments[i].PointSelected)
                _selectedSegments[i].SetConjunct(isConjunct);

        _currentSelectedPathObject.UpdatePointsHandlesRenderer();
    }

    public void OneHandle(bool isSecondHandle)
    {
        for (int i = 0; i < _segments.Count; i++)
            if (_selectedSegments[i].PointSelected)
                _selectedSegments[i].OneHandle(isSecondHandle);
        _currentSelectedPathObject.UpdatePointsHandlesRenderer();
        UpdateInlineArea();
    }

    public void NoHandle()
    {
        for (int i = 0; i < _segments.Count; i++)
            if (_selectedSegments[i].PointSelected)
                _selectedSegments[i].NoHandles();
        _currentSelectedPathObject.UpdatePointsHandlesRenderer();
        UpdateInlineArea();
    }

    public void Handles()
    {
        for (int i = 0; i < _segments.Count; i++)
            if (_selectedSegments[i].PointSelected)
                _selectedSegments[i].Handles();
        _currentSelectedPathObject.UpdatePointsHandlesRenderer();
        UpdateInlineArea();
    }
}

