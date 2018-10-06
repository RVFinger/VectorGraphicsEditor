using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ObjectEditmodeCollection : EditmodeCollection
{
    [Header("Editmodes"), Tooltip("Order in List determines the priority of the editmodes")]
    [SerializeField] List<EditmodePathObject> pathObjectEditmodes;
    protected static PathObjectUpdater _pathObjectUpdater;


    public enum Mode
    {
        none,
        move,
        scale,
        select,
    }


    protected override void SetCollectionUpdater()
    {
        if (_pathObjectUpdater == null)
            _pathObjectUpdater = new PathObjectUpdater();


        _updater = _pathObjectUpdater as UpdaterBase;

        _updater.Init();


        if (EditorMenu == null)
            return;

        EditorMenu.Init(_updater);

    }

    protected override void CastEditmodeList()
    {
        modes = pathObjectEditmodes.Cast<EditmodeBase>().ToList();
    }

    protected override void AddPathObjectHoverConditions()
    {
        if (UsePathObjectList)
        {
            _onPathObjectHoverUpdate.Add(delegate (PathObject item) { return item.Selected && item.MouseOverObject(); });
            _onPathObjectHoverUpdate.Add(delegate (PathObject item) { return item.OverPathObject(); });
        }
        else
        {
            _onAreaObjectHoverUpdate.Add(delegate (AreaObject item) { return item.Selected && item.MouseOverObject(); });
            _onAreaObjectHoverUpdate.Add(delegate (AreaObject item) { return item.OverPathObject(); });
        }

    }

    protected override void AddKeyboardEventsListener()
    {
        StartListening("delete", _pathObjectUpdater.DeleteObject, new KeyCode[] { KeyCode.Delete });
        StartListening("duplicate", _pathObjectUpdater.Duplicate, new KeyCode[] { KeyCode.D });
    }

    protected override void StopListeningToKeyboardEvents()
    {
        StopListening("delete", _pathObjectUpdater.DeleteObject);
        StopListening("duplicate", _pathObjectUpdater.Duplicate);
    }

    public override void Enter()
    {
        base.Enter();
        _pathObjectUpdater.UnSelectPathObjects();
    }

    public override void Exit()
    {
        base.Exit();
        _pathObjectUpdater.OnExit();
        Program.Instance.ColorPicker.ShowColorpicker(false);
    }

    public override void Hover(bool isHover)
    {
        _pathObjectUpdater.HoverPathObject(isHover);
    }

    protected override void SetEditmodeClickAndDragActions(EditmodeBase mode)
    {
        EditmodePathObject pathObject = mode as EditmodePathObject;

        switch (pathObject.mode)
        {
            case Mode.select:
                mode.SetClickFunction(_pathObjectUpdater.SelectToMove);
                mode.SetDragFunction(_pathObjectUpdater.Move); break;
            case Mode.scale:
                mode.SetClickFunction(_pathObjectUpdater.SelectToScale);
                mode.SetDragFunction(_pathObjectUpdater.Scale); break;
            case Mode.move:
                mode.SetClickFunction(_pathObjectUpdater.SelectToMove);
                mode.SetDragFunction(_pathObjectUpdater.Move); break;
        }
    }

    protected override void SetEditmodeOnUpActions(EditmodeBase mode)
    {
        mode.SetUpFunction(_pathObjectUpdater.UpdateInlineAreaRenderer);
    }

    protected override void SetFreeSpaceClickDragUpAction()
    {
        FreeSpaceEditmode.SetClickFunction(delegate
        {
            Program.Instance.ColorPicker.HideColorpicker();
            _pathObjectUpdater.StartSelection();
        });
        FreeSpaceEditmode.SetDragFunction(_pathObjectUpdater.Select);
        FreeSpaceEditmode.SetUpFunction(_pathObjectUpdater.EndSelection);
    }

    public override void ChangeState()
    {
        PathObjectUpdater.SelectionMode current = _pathObjectUpdater.GetCurrentState();

        foreach (EditmodePathObject editmode in modes)
            editmode.Suscribe(current);
    }
}

[System.Serializable]
public class EditmodePathObject : EditmodeBase
{
    public PathObjectUpdater.SelectionMode unsuscribeState;
    public ObjectEditmodeCollection.Mode mode;

    bool _suscribed;
    public override bool Suscribed { get { return _suscribed; } }

    public void Suscribe(PathObjectUpdater.SelectionMode state)
    {
        _suscribed = true;

        if (state == PathObjectUpdater.SelectionMode.NoObject && unsuscribeState == PathObjectUpdater.SelectionMode.NoObject)
            _suscribed = false;
    }
}