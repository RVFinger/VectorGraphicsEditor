using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdaterBase {

    protected PathObject _currentPathObject;
    protected PathObject _currentSelectedPathObject;
    protected ScalableObject _currentHoveredObject;
    protected ScalableObject _currentSelectedObject;
    protected List<BezierSegment> _segments;
    protected List<BezierSegment> _selectedSegments;
    protected EditorMenuBase _editorMenu;
    protected ScalableObject _lastHoveredObject;
    protected Program _program;
    public int ItemSelected { get; set; }
    public Vector2 StartSelectionPoint { get; set; }
    public MultiSelectionRectRenderer Renderer { get; set; }
    //protected int _itemSelected;
    //protected Vector2 _multiSelectionStartPoint;
    //protected MultiSelectionRectRenderer _multiSelectionRenderer;
    //protected Vector2 _currentMousePosition;

    public enum SelectionMode
    {
        Point,
        MultiplePoints,
        NoSelection,
        Handle,
        SingleObject,
        MultipleObject,
        NoObject,
        None,
    }

    public virtual void Init()
    {
        Program.OnHoveredPathObjectChanged += UpdatePathObject;
        Program.OnSelectedPathObjectChanged += UpdateSelectedPathObject;
        _program = Program.Instance;
        Renderer = _program.SelectionRectangle;

    }

    public virtual void OnDisable()
    {
        Program.OnHoveredPathObjectChanged -= UpdatePathObject;
        Program.OnSelectedPathObjectChanged -= UpdateSelectedPathObject;
    }

    public void SetEditorMenu(EditorMenuBase editorMenu)
    {
        _editorMenu = editorMenu;
    }

    void UpdatePathObject(ScalableObject current)
    {
        _currentHoveredObject = current;
        _currentPathObject = current as PathObject;
        if(_currentPathObject != null)
        _segments = _currentPathObject.BezierCurve;
    }

    void UpdateSelectedPathObject(ScalableObject current)
    {
        _currentSelectedObject = current;
        _currentSelectedPathObject = current as PathObject;
    }

    public void HoverPathObject(bool isHover)
    {
        if (_lastHoveredObject != null && _lastHoveredObject.Hovered)
            _lastHoveredObject.Hover(false); ;

        if (isHover)
        {
            if(_currentHoveredObject)
                _currentHoveredObject.Hover(true, true);

            _lastHoveredObject = _currentHoveredObject;
        }
    }

    public void UpdateInlineArea()
    {
        _currentPathObject.UpdateInlineArea();
    }
    public virtual void DeleteObject()
    {
        
    }
        //public virtual void StartSelection()
        //{

        //}

        //public virtual void MinimizeSelectionRectangle()
        //{

        //}

        //public virtual void MultiSelection()
        //{
        //    _currentMousePosition = _program.GetMouseCanvasPosition();

        //    _multiSelectionRenderer.UpdateRectangle(_multiSelectionStartPoint, _currentMousePosition);
        //    _itemSelected = 0;

        //}

    }


public interface IMultiSelector
{
    int ItemSelected { get; set; }
    Vector2 StartSelectionPoint { get; set; }
    MultiSelectionRectRenderer Renderer { get; set; }

    void StartSelection();
    void Select();
    void EndSelection();
}