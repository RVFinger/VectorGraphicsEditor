using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Linq;

/*
    The EditmodeCollection Class is responsible for handling the UserInput
    Each Collection (Path, PathObject, HandlesPoints) holds a List of Editmodes, 
    which are set and executed if their Conditions are met 
    (e.g. if Collection Path is active the Editmode Add PathPoint is triggered,
    if the mouse cursor is close to a path and the user clicks.
*/
public class EditmodeCollection : MonoBehaviour
{
    [Tooltip("Editormenu (submenu on the right side), can be null")]
    [SerializeField] EditorMenuBase _editorMenu;
    [Tooltip("Toggle which sets Collection")]
    [SerializeField] Toggle _toggle;
    [SerializeField] bool _usePathObjectList;

    public bool UsePathObjectList => _usePathObjectList;
    [Tooltip("Editmode is set if all conditions of the modes in the list are not met")]
    [SerializeField] EditmodeBase _freeSpaceEditmode;
    public EditmodeBase FreeSpaceEditmode => _freeSpaceEditmode;
    protected EditorMenuBase EditorMenu => _editorMenu;
    // Conditions which must be met in order to hover the PathObject (hovered shows blue hoverpath)
    protected List<Func<AreaObject, bool>> _onAreaObjectHoverUpdate = new List<Func<AreaObject, bool>>();
    public List<Func<AreaObject, bool>> OnAreaObjectHoverUpdate => _onAreaObjectHoverUpdate;

    protected List<Func<PathObject, bool>> _onPathObjectHoverUpdate = new List<Func<PathObject, bool>>();
    public List<Func<PathObject, bool>> OnPathObjectHoverUpdate => _onPathObjectHoverUpdate;

    protected List<EditmodeBase> modes;
    public List<EditmodeBase> Modes => modes;

    // Each Collection has its own updater, PathObjectCollection a PathObjectUpdater, PathCollection a PathUpdater etc.
    protected UpdaterBase _updater;

    // Every Collection has a set of Keyboard Inputs which it is listening to.
    protected Dictionary<string, UnityEvent> _eventDictionary = new Dictionary<string, UnityEvent>();
    protected Dictionary<string, KeyCode[]> _usedKeyCodes = new Dictionary<string, KeyCode[]>();

    protected HoverData _hoverData;
    protected bool _isInitialized;

    protected bool _hoverAlsoHandles;
    public bool HoverAlsoHandles => _hoverAlsoHandles;

    public enum PathCondition
    {
        CursorNotOverEditor,
        CloseToPath,
        CloseToPathPoints,
        CloseToOtherEndPoint,
        CloseToEndPoints,
        CloseToPathHandles,
        FreeSpace,
        OverPathObject,
        CloseToBoundingBoxUp,
        CloseToBoundingBoxRight,
        CloseToBoundingBoxDiagonal,
        CloseToBoundingBoxDiagonalInv,
        OverBouncingBox,
        PathObjectHovered,
    }

    /// <summary>
    /// All Conditions and Actions are set in Init
    /// </summary>
    /// <param name="hoverData">Program is handing over hoverdata</param>
    public virtual void Init(HoverData hoverData)
    {

            _hoverData = hoverData;

            AddListenerToToggle();
            CastEditmodeList();
            SetCollectionUpdater();
            SetEditmodeActions();

            AddPathObjectHoverConditions();
            AddKeyboardEventsListener();

        _isInitialized = true;
    }

    private void OnDisable()
    {
        StopListeningToKeyboardEvents();
        if(_updater != null)
        _updater.OnDisable();
    }

    /// <summary>
    /// User sets current collection by clicking the matching toggle in the Scene 
    /// </summary>
    private void AddListenerToToggle()
    {
        _toggle.onValueChanged.AddListener((value) => { if (value) Program.Instance.UseCollection(this); });
    }

    private void SetEditmodeActions()
    {
        SetEditmodeCondition(_freeSpaceEditmode);
        SetFreeSpaceClickDragUpAction();

        foreach (var item in Modes)
        {
            SetEditmodeCondition(item);
            SetEditmodeClickAndDragActions(item);
            SetEditmodeOnUpActions(item);
        }
    }

    /// <summary>
    /// Each Collection has its own updater, PathObjectCollection a PathObjectUpdater, PathCollection a PathUpdater etc.
    /// </summary>
    protected virtual void SetCollectionUpdater()
    {
        
    }

    protected virtual void CastEditmodeList() { }

    protected virtual void AddPathObjectHoverConditions() { }

    protected virtual void AddKeyboardEventsListener() { }

    protected virtual void StopListeningToKeyboardEvents() { }

    /// <summary>
    /// Is called when collection is set
    /// </summary>
    public virtual void Enter()
    {
        if (_editorMenu)
            _editorMenu.ShowMenu(true);
    }
    /// <summary>
    /// Is called before other collection is set
    /// </summary>
    public virtual void Exit()
    {
        if (_editorMenu)
            _editorMenu.ShowMenu(false);
    }

    public virtual void Hover(bool isHover) { }

    /// <summary>
    /// Every mode has its own Condition which must be met before the mode's action is called.
    /// </summary>
    /// <param name="mode"></param>
    private void SetEditmodeCondition(EditmodeBase mode)
    {
        switch (mode.condition)
        {
            case PathCondition.CloseToPath:
                mode.SetPointer(delegate (PathObject item) { return item.MouseOverPath(ref _hoverData); }); break;
            case PathCondition.CloseToOtherEndPoint:
                mode.SetPointer(delegate (PathObject item) { return item.CloseToOtherEndPoint(ref _hoverData); }); break;
            case PathCondition.CloseToEndPoints:
                mode.SetPointer(delegate (PathObject item) { return item.CloseToEndPoints(ref _hoverData); }); break;
            case PathCondition.CloseToPathPoints:
                mode.SetPointer(delegate (PathObject item) { return item.MouseOverPathPoints(ref _hoverData); }); break;
            case PathCondition.CloseToPathHandles:
                mode.SetPointer(delegate (PathObject item) { return item.Selected && item.MouseOverPathHandles(ref _hoverData); }); break;
            case PathCondition.OverPathObject:
                mode.SetPointer(delegate (ScalableObject item) { return item.OverPathObject(); }); break;
            case PathCondition.CloseToBoundingBoxUp:
                mode.SetPointer(delegate (ScalableObject item) { return item.CloseToBoundingBoxPoints(5, 7); }); break;
            case PathCondition.CloseToBoundingBoxRight:
                mode.SetPointer(delegate (ScalableObject item) { return item.CloseToBoundingBoxPoints(4, 6); }); break;
            case PathCondition.CloseToBoundingBoxDiagonal:
                mode.SetPointer(delegate (ScalableObject item) { return item.CloseToBoundingBoxPoints(0, 1); }); break;
            case PathCondition.CloseToBoundingBoxDiagonalInv:
                mode.SetPointer(delegate (ScalableObject item) { return item.CloseToBoundingBoxPoints(2, 3); }); break;
            case PathCondition.OverBouncingBox:
                mode.SetPointer(delegate (ScalableObject item) { return item.MouseOverObject(); }); break;
            case PathCondition.FreeSpace:
                mode.SetPointer(delegate (ScalableObject item) { _hoverData.UnHoverCurrentEverything(); return true; }); break;
            case PathCondition.PathObjectHovered:
                mode.SetPointer(delegate (ScalableObject item) { _hoverData.UnHoverCurrentEverything(); return Program.Instance.PathObjectHovered(); }); break;
        }
    }

    protected virtual void SetEditmodeClickAndDragActions(EditmodeBase mode) { }

    protected virtual void SetEditmodeOnUpActions(EditmodeBase mode) { }

    protected virtual void SetFreeSpaceClickDragUpAction() { }

    public virtual void ChangeState() { }

    //bool _hovered =  true;
    //public void CheckHoverState()
    //{
    //    if (!_hovered)
    //    {
    //        Hover(true);
    //        _hovered = true;
    //    }else
    //    {
    //        Hover(false);
    //        _hovered = false;
    //    }
    //}

    public void CheckKeyboardInput()
    {
        if (!Input.anyKey)
            return;

        for (int i = 0; i < _usedKeyCodes.Count; i++)
        {
            var item = _usedKeyCodes.ElementAt(i);
            var itemKey = item.Key;
            KeyCode[] combo = item.Value;

            if (combo.Length == 2)
            {
                if (Input.GetKey(combo[0]) && Input.GetKeyDown(combo[1]))
                    TriggerEvent(itemKey);
            }

            if (combo.Length == 1)
                if (Input.GetKeyDown(combo[0]))
                    TriggerEvent(itemKey);
        }
    }

    // credits to https://unity3d.com/de/learn/tutorials/topics/scripting/events-creating-simple-messaging-system
    protected void StartListening(string eventName, UnityAction listener, KeyCode[] keycodes)
    {
        _usedKeyCodes.Add(eventName, keycodes);

        UnityEvent thisEvent = null;
        if (_eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent();
            thisEvent.AddListener(listener);
            _eventDictionary.Add(eventName, thisEvent);
        }
    }

    protected void StopListening(string eventName, UnityAction listener)
    {
        UnityEvent thisEvent = null;
        if (_eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }

        KeyCode[] thisKeyCode = null;
        if (_usedKeyCodes.TryGetValue(eventName, out thisKeyCode))
        {
            _usedKeyCodes.Remove(eventName);
        }
    }

    private void TriggerEvent(string eventName)
    {
        UnityEvent thisEvent = null;
        if (_eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke();
        }
    }
}


[System.Serializable]
public class EditmodeBase : System.Object
{
    public EditmodeCollection.PathCondition condition;
    public Texture2D cursor;

    Func<PathObject, bool> checkCondition;
    Func<ScalableObject, bool> checkBaseCondition;
    Action onClickFunction;
    Action onDragFunction = null;
    Action onUpFunction = null;

    public virtual bool Suscribed { get { return true; } }

    public void SetPointer(Func<PathObject, bool> condition)
    {
        checkCondition = condition;
       // checkBaseCondition = condition;
    }
    public void SetPointer(Func<ScalableObject, bool> condition)
    {
        checkCondition = condition;
        checkBaseCondition = condition;
    }
    // Some Function are called on mouse click...
    public void SetClickFunction(Action function)
    {
        onClickFunction = function;
    }
    // others on mouse drag and 
    public void SetDragFunction(Action function)
    {
        onDragFunction = function;
    }
    // others on mouse up
    public void SetUpFunction(Action function)
    {
        onUpFunction = function;
    }

    public bool CheckCondition(PathObject pathObject)
    {
        return checkCondition.Invoke(pathObject);
    }
    public bool CheckCondition(ScalableObject pathObject)
    {
        //if(pathObject is AreaObject)
        if(checkBaseCondition != null)
        return checkBaseCondition.Invoke(pathObject);
        else
            return checkCondition.Invoke(pathObject as PathObject);

    }
    public void CallOnDown()
    {
        if (onClickFunction != null)
            onClickFunction.Invoke();
    }

    public void CallDrag()
    {
        if (onDragFunction != null)
            onDragFunction.Invoke();
    }

    public void CallOnUp()
    {
        if (onUpFunction != null)
            onUpFunction.Invoke();
    }

}
