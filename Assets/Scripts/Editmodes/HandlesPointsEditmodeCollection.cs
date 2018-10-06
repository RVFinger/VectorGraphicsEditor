using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HandlesPointsEditmodeCollection : EditmodeCollection
{
    [Header("Editmodes"), Tooltip("Order in List determines the priority of the editmodes")]
    [SerializeField] List<EditmodeHandlesPoints> handlesPointsEditmodes;

    HandlesPointsUpdater _handlePointsUpdater;

    public enum Mode
    {
        none,
        moveHandle,
        movePoint,
        select,
    }
   
    protected override void SetCollectionUpdater()
    {
        _hoverAlsoHandles = false;
        if (_handlePointsUpdater == null)
            _handlePointsUpdater = new HandlesPointsUpdater();

        _updater = _handlePointsUpdater as UpdaterBase;

        _updater.Init();

        if (EditorMenu == null)
            return;

        EditorMenu.Init(_updater);
    }

    protected override void CastEditmodeList()
    {
        modes = handlesPointsEditmodes.Cast<EditmodeBase>().ToList();
    }

    protected override void AddPathObjectHoverConditions()
    {
        _onPathObjectHoverUpdate.Add(delegate (PathObject item) { return item.OverPathObject(); });
        _onPathObjectHoverUpdate.Add(delegate (PathObject item) { return item.Selected && item.MouseOverPathHandles(); });
        _onPathObjectHoverUpdate.Add(delegate (PathObject item) { return item.Selected && item.MouseOverPathPoints(); });
    }

    protected override void AddKeyboardEventsListener()
    {
        // StartListening("delete", Program.Instance.Delete, new KeyCode[] { KeyCode.Delete });
    }

    protected override void StopListeningToKeyboardEvents()
    {
        //StopListening("delete", Program.Instance.Delete);
    }

    public override void Enter()
    {
        base.Enter();

        _handlePointsUpdater.UpdateMenu();

        Program.Instance.ShowPointsHandleRenderer(true);
    }

    public override void Exit()
    {
        base.Exit();
        Program.Instance.ShowPointsHandleRenderer(false);
        _handlePointsUpdater.OnExit();
    }

    public override void Hover(bool isHover)
    {
        if (!isHover)
            _handlePointsUpdater.UnhoverCurrentData();
    }

    protected override void SetEditmodeClickAndDragActions(EditmodeBase mode)
    {
        EditmodeHandlesPoints handlesPoints = mode as EditmodeHandlesPoints;

        switch (handlesPoints.mode)
        {
            case Mode.moveHandle:
                mode.SetClickFunction(delegate { _handlePointsUpdater.SelectPathObject(); _handlePointsUpdater.SelectBezierSegment(HandlesPointsUpdater.SelectionMode.Handle); });
                mode.SetDragFunction(delegate { _handlePointsUpdater.UpdateCurrentBezierHandles(); }); break;
            case Mode.movePoint:
                mode.SetClickFunction(delegate { _handlePointsUpdater.SelectPathObject(); _handlePointsUpdater.SelectBezierSegment(HandlesPointsUpdater.SelectionMode.Point); });
                mode.SetDragFunction(_handlePointsUpdater.MovePoint);
                break;
            case Mode.select:
                mode.SetClickFunction(_handlePointsUpdater.SelectPathObject);
                break;
        }
    }

    protected override void SetEditmodeOnUpActions(EditmodeBase mode)
    {
        EditmodeHandlesPoints handlesPoints = mode as EditmodeHandlesPoints;

        mode.SetUpFunction(delegate
        {
            _updater.UpdateInlineArea();
            _hoverData.UnHoverCurrentEverything();
            if (handlesPoints.mode == Mode.movePoint)
            {
                _handlePointsUpdater.UnselectMultiSelection();
            }
        });
    }

    protected override void SetFreeSpaceClickDragUpAction()
    {
        FreeSpaceEditmode.SetClickFunction(_handlePointsUpdater.StartSelection);
        FreeSpaceEditmode.SetDragFunction(_handlePointsUpdater.Select);
        FreeSpaceEditmode.SetUpFunction(delegate
        { _handlePointsUpdater.EndSelection(); _handlePointsUpdater.UnSelectPathObject(); });
    }
}

[System.Serializable]
public class EditmodeHandlesPoints : EditmodeBase
{
    public HandlesPointsEditmodeCollection.Mode mode;
    public override bool Suscribed { get { return true; } }
}
