using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectListUpdaterBase
{
    List<ScalableObject> _sorted;
    protected List<ScalableObject> objectList = new List<ScalableObject>();
    public bool _isPathObjectUpdateAllowed;
    protected ScalableObject _lastHoveredObject;
    protected ScalableObject _lastSelectedObject;
    List<ScalableObject> newObjects = new List<ScalableObject>();
    protected Transform _prefabParent;
    protected GameObject _prefab;
    protected bool _objectUpdateAfterNewObject;

    public List<ScalableObject> ObjectList { get { return objectList; } set { objectList = value; } }

    public void Init(GameObject prefab, Transform prefabParent)
    {
        _prefab = prefab;
        _prefabParent = prefabParent;
    }

    public virtual void SetLastSelected(ScalableObject scalableObject)
    {
        _lastSelectedObject = scalableObject;
    }

    public void AddObject(ScalableObject scaleableObject)
    {
        ObjectList.Add(scaleableObject);
    }

    public void RemoveObject(ScalableObject scaleableObject)
    {
        ObjectList.Remove(scaleableObject);
    }

    public ScalableObject NewObject(bool objectUpdateAfter)
    {
        GameObject newPathObject = GameObject.Instantiate(_prefab, _prefabParent);
        _lastSelectedObject = newPathObject.GetComponent<ScalableObject>();
        _lastSelectedObject.Init();
        AddObject(_lastSelectedObject);

        Program.Instance.UpdateHoveredPathObject(_lastSelectedObject);
        _isPathObjectUpdateAllowed = objectUpdateAfter;
        SortObjectList();

        return _lastSelectedObject;
    }

    public void HideAllBoundingBoxes()
    {
        foreach (ScalableObject item in ObjectList)
        {
            item.ShowBoundingRect(false);
            item.Hover(false);
            //item.Hovered = false;
        }
    }

    public void DeleteObject(ScalableObject scalableObject)
    {
        if (scalableObject == null)
            return;
        GameObject.Destroy(scalableObject.gameObject);
        RemoveObject(scalableObject);
    }

    public void DuplicateObject()
    {
        if (_lastSelectedObject == null)
            return;

        bool downLeftset = false;
        Vector2 downLeft = Vector2.zero;
        Vector2 oldDownLeft = Vector2.zero;
        Vector2 translate = Vector2.zero;

        for (int i = ObjectList.Count - 1; i >= 0; i--)
        {
            ScalableObject currentObject = ObjectList[i];
            if (currentObject.Selected)
            {
                if (!downLeftset)
                {
                    oldDownLeft = currentObject.DownLeftAnchor;
                    downLeft = new Vector2(currentObject.Width() / 2f, currentObject.Height() / 2f) - Vector2.zero;
                    translate = downLeft;
                    downLeftset = true;
                }
                else
                {
                    translate = downLeft;
                    translate += oldDownLeft;
                    translate -= currentObject.DownLeftAnchor;
                }

                GameObject newPathObject = GameObject.Instantiate(_lastSelectedObject.gameObject, _prefabParent);
                ScalableObject dublicate = newPathObject.GetComponent<ScalableObject>();
                dublicate.Clone(currentObject, translate);

                newObjects.Add(dublicate);
            }
        }

        foreach (var item in newObjects)
        {
            ObjectList.Add(item);
        }
        newObjects.Clear();
        SortObjectList();
    }

    public void SortObjectList()
    {
        _sorted = new List<ScalableObject>(ObjectList);

        for (int i = 0; i < ObjectList.Count; i++)
        {
            int index = ObjectList.Count - 1 - ObjectList[i].transform.GetSiblingIndex();
            _sorted[index] = ObjectList[i];
        }

        for (int i = 0; i < _sorted.Count; i++)
        {
            ObjectList[i] = _sorted[i];
        }
        _sorted.Clear();
    }

    public virtual void CheckConditions(out bool overObject)
    {
        overObject = false;
    }

    public virtual bool CheckCondition(out ScalableObject otherObject, out bool newObject)
    {
        otherObject = null;
        newObject = false;
        return false;
    }

    public void Unselect()
    {
        if (_lastSelectedObject == null)
            return;
        _lastSelectedObject.Unselect();
        _lastSelectedObject.Hover(false);
        // _lastSelectedObject.ShowBeziersegments(false);
        _lastSelectedObject = null;
        _isPathObjectUpdateAllowed = true;
    }

    public bool OtherObjectHovered(out ScalableObject hoveredObject, out bool overObject)
    {
        if (!_isPathObjectUpdateAllowed && _lastSelectedObject != null)
        {
            overObject = false;

            CheckConditions(out overObject);
            _lastSelectedObject.Hover(overObject, true);
   
            hoveredObject = _lastSelectedObject;
            return false;
        }

        bool otherObjectHovered;
        if (CheckCondition(out hoveredObject, out otherObjectHovered))
        {
            overObject = true;

            if (otherObjectHovered)
            {
                if (_lastHoveredObject != null && _lastHoveredObject.Hovered)
                {
                    _lastHoveredObject.Hover(false);
                }
                _lastHoveredObject = hoveredObject;
                hoveredObject = _lastHoveredObject;
                return true;
            }
            if (!_lastHoveredObject.Hovered)
            {
                _lastHoveredObject.Hover(true, true);
            }

            hoveredObject = _lastHoveredObject;

            return otherObjectHovered;
        }

        hoveredObject = null;
        overObject = false;
        if (_lastHoveredObject != null && _lastHoveredObject.Hovered)
        {
            _lastHoveredObject.Hover(false);
        }
        return false;
    }

    public void SetBoundingBoxPoint(int index)
    {
        foreach (var item in ObjectList)
            item.BoundingBoundIndex(index);
    }
}

public class DrawareaObjectsUpdater : ObjectListUpdaterBase
{
    public List<Func<AreaObject, bool>> OnHoverUpdate { get { return _onHoverUpdate; } set { _onHoverUpdate = value; } }
    private List<Func<AreaObject, bool>> _onHoverUpdate = new List<Func<AreaObject, bool>>();
    private AreaObject _lastSelected;

    public ScalableObject GetFirstSelectedObject()
    {
        foreach (ScalableObject item in objectList)
        {
            if (item.Selected)
                return item;
        }
        if (objectList.Count > 0)
            return objectList[0];
        return null;
    }

    public override void SetLastSelected(ScalableObject scalableObject)
    {
        base.SetLastSelected(scalableObject);
        _lastSelected = scalableObject as AreaObject;
    }

    public override void CheckConditions(out bool overObject)
    {
        overObject = false;
        foreach (Func<AreaObject, bool> func in _onHoverUpdate)
        {
            if (func.Invoke(_lastSelected))
            {
                overObject = true;
            }
        }
    }
    public override bool CheckCondition(out ScalableObject hoveredObject, out bool otherObjectHovered)
    {
        hoveredObject = _lastHoveredObject;
        foreach (AreaObject pathObject in objectList)
        {
            foreach (Func<AreaObject, bool> func in _onHoverUpdate)
            {
                if (func.Invoke(pathObject))
                {
                    hoveredObject = pathObject;
                    otherObjectHovered = pathObject != _lastHoveredObject;
                    return true;
                }
            }
        }
        otherObjectHovered = false;
        return false;
    }
}


public class PathObjectsUpdater : ObjectListUpdaterBase
{
    public List<Func<PathObject, bool>> OnHoverUpdate { get { return _onHoverUpdate; } set { _onHoverUpdate = value; } }
    private List<Func<PathObject, bool>> _onHoverUpdate = new List<Func<PathObject, bool>>();

    private PathObject _lastSelected;

    public override void SetLastSelected(ScalableObject scalableObject)
    {
        base.SetLastSelected(scalableObject);
        _lastSelected = scalableObject as PathObject;
    }

    public void ShowPointsHandleRenderer(bool enable)
    {
        foreach (PathObject item in ObjectList)
        {

            item.ShowPointsHandles(enable);
        }
    }

    public override void CheckConditions(out bool overObject)
    {
        overObject = false;
        foreach (Func<PathObject, bool> func in _onHoverUpdate)
        {
            if (func.Invoke(_lastSelected))
            {
                overObject = true;
            }
        }
    }
    public override bool CheckCondition(out ScalableObject hoveredObject, out bool otherObjectHovered)
    {
        hoveredObject = _lastHoveredObject;
        foreach (PathObject pathObject in objectList)
        {
            foreach (Func<PathObject, bool> func in _onHoverUpdate)
            {
                if (func.Invoke(pathObject))
                {
                    hoveredObject = pathObject;
                    otherObjectHovered = pathObject != _lastHoveredObject;
                    return true;
                }
            }
        }
        otherObjectHovered = false;
        return false;
    }
}
