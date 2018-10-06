using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdaterBase
{
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
        Program.OnHoveredObjectChanged += UpdatePathObject;
        Program.OnSelectedObjectChanged += UpdateSelectedPathObject;
        _program = Program.Instance;
        Renderer = _program.SelectionRectangle;
    }

    public virtual void OnDisable()
    {
        Program.OnHoveredObjectChanged -= UpdatePathObject;
        Program.OnSelectedObjectChanged -= UpdateSelectedPathObject;
    }

    public void SetEditorMenu(EditorMenuBase editorMenu)
    {
        _editorMenu = editorMenu;
    }

    void UpdatePathObject(ScalableObject current)
    {
        _currentHoveredObject = current;
        _currentPathObject = current as PathObject;
        if (_currentPathObject != null)
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
            if (_currentHoveredObject)
                _currentHoveredObject.Hover(true, true);

            _lastHoveredObject = _currentHoveredObject;
        }
    }

    public void UpdateInlineArea()
    {
        if (_currentPathObject)
            _currentPathObject.UpdateInlineArea();
    }

    public virtual void DeleteObject()
    {

    }

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