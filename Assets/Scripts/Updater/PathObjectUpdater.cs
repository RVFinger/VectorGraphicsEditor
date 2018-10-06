using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathObjectUpdater : UpdaterBase, IMultiSelector
{
    List<ScalableObject> _currentList = new List<ScalableObject>();
    List<IColorable> _currentColorables = new List<IColorable>();
    private SelectionMode _currentSelectionMode = SelectionMode.NoObject;

    // Colorselector uses same list
    public void SynchronizeColorablesList()
    {
        PathObjectEditorMenu menu = _editorMenu as PathObjectEditorMenu;
        menu.SynchronizeColorablesList(_currentColorables);
    }

    public void Suscribe(ScalableObject scalable)
    {
        scalable.Selected = true;
        scalable.ShowBoundingRect(true);
        _currentList.Add(scalable);
        IColorable colorable = scalable as IColorable;
        _currentColorables.Add(colorable);
        if(_currentColorables.Count == 1)
        _editorMenu.UpdateMenu(colorable);
    }

    public void Unsuscribe(ScalableObject scalable)
    {
        if (scalable == null)
            return;
        scalable.Selected = false;
        scalable.ShowBoundingRect(false);
        _currentList.Remove(scalable);
        _currentColorables.Remove(scalable as IColorable);
    }

    public void UnsuscribeAll()
    {
        foreach (var item in _currentList)
        {
            item.Selected = false;
            item.ShowBoundingRect(false);
        }
        _currentList.Clear();
        _currentColorables.Clear();
    }

    public void SelectToMove()
    {
        SelectPathObject();
        SetStartPosition();
    }

    public void SelectToScale()
    {
        SelectPathObject();
        StartScale();
    }

    public void SelectPathObject()
    {
        if (_currentSelectionMode != SelectionMode.MultipleObject)
        {
            _currentSelectionMode = SelectionMode.SingleObject;

            Unsuscribe(_currentSelectedObject);

            _program.UpdateSelectedPathObject(_currentHoveredObject);

            Suscribe(_currentSelectedObject);

            UpdateMenu();
        }
    }

    public void UnSelectPathObjects()
    {
        UnsuscribeAll();

        _program.UpdateSelectedPathObject(null);
        _currentSelectionMode = SelectionMode.NoObject;
        UpdateMenu();
    }

    public void UpdateMenu()
    {
        if (_editorMenu != null)
            _editorMenu.UpdateMenu(_currentSelectionMode);
    }

    public void StartSelection()
    {
        ItemSelected = 0;
        _currentSelectionMode = SelectionMode.NoObject;

        StartSelectionPoint = _program.GetMouseCanvasPosition();
        UnsuscribeAll();
    }

    // on up after multiselect
    public void EndSelection()
    {
        Renderer.UpdateRectangle(Vector2.zero, Vector2.zero);

        if (ItemSelected > 1)
            _currentSelectionMode = SelectionMode.MultipleObject;
        else if (ItemSelected == 1)
            _currentSelectionMode = SelectionMode.SingleObject;
        else
            _currentSelectionMode = SelectionMode.NoObject;

        UpdateMenu();

        if (_currentSelectionMode == SelectionMode.NoObject)
            UnSelectPathObjects();
    }

    public override void DeleteObject()
    {
        foreach (ScalableObject item in _currentList)
        {
            _program.Delete(item);
        }
        UnsuscribeAll();
        _currentSelectionMode = SelectionMode.NoObject;
        UpdateMenu();
    }

    public void OnExit()
    {
        UnSelectPathObjects();
    }

    public void Select()
    {
        Vector2 mousePosition = _program.GetMouseCanvasPosition();

        Renderer.UpdateRectangle(StartSelectionPoint, mousePosition);
        ItemSelected = 0;
        _currentSelectionMode = SelectionMode.MultipleObject;

        ScalableObject selected = null;
        List<ScalableObject> objectList = _program.CurrentScalableList();
        UnsuscribeAll();

        for (int i = 0; i < objectList.Count; i++)
        {
            ScalableObject segment = objectList[i];

            Vector2 downLeftAnchor = segment.DownLeftAnchor;
            Vector2 upRightAnchor = segment.UpRightAnchor;

            if (mousePosition.x < downLeftAnchor.x && StartSelectionPoint.x < downLeftAnchor.x ||
                mousePosition.x > upRightAnchor.x && StartSelectionPoint.x > upRightAnchor.x ||
                mousePosition.y < downLeftAnchor.y && StartSelectionPoint.y < downLeftAnchor.y ||
                mousePosition.y > upRightAnchor.y && StartSelectionPoint.y > upRightAnchor.y)
                continue;

            Suscribe(segment);
            selected = segment;
            ItemSelected++;
        }

        if (ItemSelected == 1)
        {
            _currentSelectionMode = SelectionMode.SingleObject;
            _currentHoveredObject = selected;
        }
    }


    void ShowBoundingRect(bool show = true)
    {
        if (_currentSelectedObject != null)
            _currentSelectedObject.ShowBoundingRect(show);
    }

    public void Duplicate()
    {
        _program.Duplicate();
    }

    public void Move()
    {
        foreach (ScalableObject item in _currentList)
            item.Move();
    }

    public void UpdateInlineAreaRenderer()
    {
        foreach (ScalableObject item in _currentList)
        {
            IColorable selectable = item as IColorable;
            selectable.UpdateInlineArea();
        }
    }
    private void SetStartPosition()
    {
        foreach (ScalableObject item in _currentList)
            item.SetStartPosition();
    }

    private void StartScale()
    {
        foreach (ScalableObject item in _currentList)
            item.StartScale();
    }

    public void Scale()
    {
        foreach (ScalableObject item in _currentList)
            item.Scale();
    }


    /////////////////////////  Functions called from EditorMenu //////////////////////////

    public void SetPathThickness(float thickness)
    {
        foreach (IColorable item in _currentColorables)
            item.UpdateLineRendererThickness(thickness);
    }

    public string GetPathThickness()
    {
        float pathThickness = 0;
        string thickness = "-";
        for (int i = 0; i < _currentColorables.Count; i++)
        {
            IColorable current = _currentColorables[i];

            if (i == 0)
            {
                pathThickness = current.GetPathThickness();
                thickness = pathThickness.ToString();
            }
            else
            {
                if (current.GetPathThickness() != pathThickness)
                    return "-";
            }

        }    
        return thickness;
    }

    public int GetSiblingIndex(out int siblingsCount)
    {
        if (_currentSelectedObject == null)
        {
            siblingsCount = 0;
            return 0;
        }
        siblingsCount = _currentSelectedObject.transform.parent.childCount;
        return _currentSelectedObject.transform.GetSiblingIndex();
    }

    public void ChangeSiblingIndex(bool increase)
    {
        if (_currentSelectedObject == null)
            return;

        int currentIndex = _currentSelectedObject.transform.GetSiblingIndex();
        int siblingsCount = _currentSelectedObject.transform.parent.childCount;

        if (increase)
        {
            if (currentIndex + 1 < siblingsCount)
                _currentSelectedObject.transform.SetSiblingIndex(++currentIndex);
        }
        else
        {
            if (currentIndex - 1 >= 0)
                _currentSelectedObject.transform.SetSiblingIndex(--currentIndex);
        }

        _program.SortPathObjectList();
    }

    public void SetSiblingIndex(bool onTop)
    {
        if (_currentSelectedObject == null)
            return;

        if (onTop)
            _currentSelectedObject.transform.SetAsLastSibling();
        else
            _currentSelectedObject.transform.SetAsFirstSibling();

        _program.SortPathObjectList();

    }

    public SelectionMode GetCurrentState()
    {
        return _currentSelectionMode;
    }
}
