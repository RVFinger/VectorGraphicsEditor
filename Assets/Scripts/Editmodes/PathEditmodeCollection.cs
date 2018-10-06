using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class PathEditmodeCollection : EditmodeCollection
{
    [Header("Editmodes"), Tooltip("Order in List determines the priority of the editmodes")]
    [SerializeField] List<EditmodePath> pathEditmodes;
    [SerializeField] EditmodePath freeSpaceEditmode;

    PathUpdater _pathUpdater;

    public enum Mode
    {
        none,
        add,
        delete,
        close,
        selectEndPoint,
    }

    protected override void SetCollectionUpdater()
    {
        _hoverAlsoHandles = true;

        if (_pathUpdater == null)
            _pathUpdater = new PathUpdater();


        _updater = _pathUpdater as UpdaterBase;

        _updater.Init();
    }

    protected override void CastEditmodeList()
    {
        modes = pathEditmodes.Cast<EditmodeBase>().ToList();
    }

    protected override void AddPathObjectHoverConditions()
    {
        _onPathObjectHoverUpdate.Add(delegate (PathObject item) { return item.MouseOverPathPoints(); });
        _onPathObjectHoverUpdate.Add(delegate (PathObject item) { return item.MouseOverPath(); });
    }

    protected override void AddKeyboardEventsListener()
    {
        StartListening("delete", _pathUpdater.DeleteObject, new KeyCode[] { KeyCode.Delete });
    }

    protected override void StopListeningToKeyboardEvents()
    {
        StopListening("delete", _pathUpdater.DeleteObject);
    }

    public override void Enter()
    {
        base.Enter();
        Program.Instance.ShowPointsHandleRenderer(true);
    }

    public override void Exit()
    {
        base.Exit();
        Program.Instance.AllowPathObjectUpdate();
        Program.Instance.ShowPointsHandleRenderer(false);
    }

    public override void Hover(bool isHover)
    {
        _pathUpdater.HoverPathObject(isHover);

        if (!isHover)
            _pathUpdater.UnhoverCurrentData();
    }

    protected override void SetEditmodeClickAndDragActions(EditmodeBase mode)
    {
        // pathupdater must be inialized before!
        EditmodePath path = mode as EditmodePath;

        switch (path.mode)
        {
            case Mode.delete:
                mode.SetClickFunction(_pathUpdater.DeleteBezierSegment); break;
            case Mode.add:
                mode.SetClickFunction(_pathUpdater.InsertBezierSegment); break;
            case Mode.close:
                mode.SetClickFunction(_pathUpdater.CloseCurve); break;
            case Mode.selectEndPoint:
                mode.SetClickFunction(_pathUpdater.SelectEndPoint); break;
        }
    }

    protected override void SetEditmodeOnUpActions(EditmodeBase mode)
    {
        mode.SetUpFunction(_updater.UpdateInlineArea);
    }

    protected override void SetFreeSpaceClickDragUpAction()
    {
        FreeSpaceEditmode.SetClickFunction(_pathUpdater.NewBezierSegment);
        FreeSpaceEditmode.SetDragFunction(_pathUpdater.UpdateCurrentBezierHandles);
        FreeSpaceEditmode.SetUpFunction(delegate
        {
            _updater.UpdateInlineArea();
            _hoverData.UnHoverCurrentEverything();
        });
    }

    public override void ChangeState()
    {
        PathUpdater.State current = _pathUpdater.GetCurrentState();

        foreach (EditmodePath editmode in modes)
            editmode.Suscribe(current);
    }

}

[System.Serializable]
public class EditmodePath : EditmodeBase
{
    public PathEditmodeCollection.Mode mode;

    public PathUpdater.State suscribeState;
    public PathUpdater.State unsuscribeState;

    bool _suscribed;
    public override bool Suscribed { get { return _suscribed; } }

    public void Suscribe(PathUpdater.State state)
    {
        _suscribed = false;

        if (state == PathUpdater.State.Closed && unsuscribeState == PathUpdater.State.Closed)
            return;

        if ((int)state >= (int)suscribeState && (int)state < (int)unsuscribeState)
            _suscribed = true;
    }
}
