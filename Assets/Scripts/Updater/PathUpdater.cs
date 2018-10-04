using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathUpdater : UpdaterBase
{
    BezierSegment _currentHoveredSegment;
    HoverData _currentHoverData;

    public enum State
    {
        Blank,
        OnePoint,
        TwoPoints,
        ThreePoints,
        FurtherPoints,
        Closed,
        NoCurveHovered,
        None,
    }

    public enum Add
    {
        First,
        ContinueAtStart,
        ContinueAtEnd,
    }

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
        if (data.HoveredBeziersegment == null)
            UnhoverCurrentData();
        else
        {
            _currentHoverData = data;
            _currentHoveredSegment = _currentHoverData.HoveredBeziersegment;
        }
    }

    public void UnhoverCurrentData()
    {
        if (_currentHoveredSegment == null)
            return;
        _currentHoveredSegment.PointHovered = false;
    }

    public void NewBezierSegment()
    {
        if (_program.IsPathObjectUpdateAllowed())
            _program.NewPathObject();

        Add add = _currentPathObject.SegmentCount == 0 ? Add.First : _segments[_segments.Count - 1].PointSelected ? Add.ContinueAtEnd :
            Add.ContinueAtStart;

        BezierSegment bezierSegment = NewBezierSegment(add);

        _currentPathObject.EndPointSelected = true;

        if (add == Add.First)
        {
            _segments.Add(bezierSegment);
        }
        else if (add == Add.ContinueAtStart)
        {
            _segments[0].isFirst = false;
            _segments.Insert(0, bezierSegment);
            _segments[0].isFirst = true;

        }
        else // ContinueAtEnd
        {
            _segments.Add(bezierSegment);
        }
        _currentHoveredSegment = bezierSegment;

        _currentPathObject.UpdatePointsHandlesRenderer();
        _currentPathObject.HandOverPointsToRenderer();
        _currentPathObject.SegmentCount++;

        UpdateCurrentBezierHandles();
    }

    BezierSegment NewBezierSegment(Add add)
    {
        BezierSegment bezierSegment = new BezierSegment(_currentPathObject);
        bezierSegment.SegmentCount = _program.SEGMENTS_PER_CURVE;
        bezierSegment.Point = _program.GetMouseCanvasPosition();
        bezierSegment.PointSelected = true;

        if (add == Add.First)
        {
            bezierSegment.AddCurvePoints(true);
            bezierSegment.isFirst = true;
        }
        else if (add == Add.ContinueAtStart)
        {
            _segments[0].SegmentCount = _program.SEGMENTS_PER_CURVE;
            _segments[0].Delete();
            _segments[0].AddCurvePoints();

            bezierSegment.AddCurvePoints(true);
            OrganizeNeighbors(bezierSegment, _segments[0]);
            _currentHoveredSegment.PointSelected = false;
        }
        else // ContinueAtEnd
        {
            bezierSegment.AddCurvePoints();
            OrganizeNeighbors(_segments[_segments.Count - 1], bezierSegment);
            _currentHoveredSegment.PointSelected = false;
        }
        return bezierSegment;
    }

    public void OrganizeNeighbors(BezierSegment neighbor, BezierSegment otherNeighbor)
    {
        neighbor.OtherNeighbor = otherNeighbor;
        otherNeighbor.Neighbor = neighbor;
    }

    public void CloseCurve()
    {
        BezierSegment first = _segments[0];
        first.isFirst = false;
        first.SegmentCount = _program.SEGMENTS_PER_CURVE;
        first.Delete();
        first.AddCurvePoints();
        BezierSegment last = _segments[_segments.Count - 1];
        OrganizeNeighbors(last, _segments[0]);
        first.Update();

        first.PointSelected = false;
        last.PointSelected = false;
        _currentPathObject.IsClosed = true;

        _currentPathObject.UpdatePointsHandlesRenderer();
        _program.AllowPathObjectUpdate();
    }

    public void SelectEndPoint()
    {
        _currentHoveredSegment.PointSelected = true;
        _currentPathObject.UpdatePointsHandlesRenderer();
        _currentPathObject.EndPointSelected = true;
        _program.SetPathObjectUpdate(false);
    }

    public void UpdateCurrentBezierHandles()
    {
        Vector2 position = _program.GetMouseCanvasPosition();

        bool isSecondHandle = _segments[_segments.Count - 1].PointSelected;
        _currentHoveredSegment.MoveHandle(position, isSecondHandle, true);

        _currentPathObject.UpdatePointsHandlesRenderer();
    }

    void SetInsertedHandlesPosition(BezierSegment insertedSegment, BezierSegment neighborSegment, BezierSegment segment)
    {
        int index = _currentHoverData.HoveredSegmentPointIndex;

        Vector2 tangent = segment.Tangent(index - 1);
        float segmentCount = segment.SegmentCount;
        float quotient = (index / segmentCount);

        Vector2 b = segment.SegmentPoints[index - 1];
        insertedSegment.Point = b;

        Vector2 q = CurveMath.GetPointOnLine(neighborSegment.SecondHandle, segment.FirstHandle, quotient);

        Vector2 q0 = neighborSegment.MoveHandleCloserToPoint(1 - quotient, true);
        segment.MoveHandleCloserToPoint(quotient, false);

        Vector2 r0 = CurveMath.GetIntersectionPointCoordinates(q0, q - q0, b, tangent);

        insertedSegment.SecondHandle = b + (b - r0) / index * (segmentCount - index);
        insertedSegment.FirstHandle = r0;

        _currentPathObject.UpdatePointsHandlesRenderer();
    }

    void InsertBezierSegment(BezierSegment insertedSegment, BezierSegment neighborSegment, BezierSegment segment)
    {
        int index = _currentHoverData.HoveredSegmentPointIndex;

        insertedSegment.CopySegmentsPoint(segment.SegmentPoints, index);
        segment.RemoveSegmentPoints(index);

        int currentRange = _currentHoverData.HoveredSegmentIndex;
        if (currentRange != 0)
            _segments.Insert(currentRange, insertedSegment);
        else
            _segments.Add(insertedSegment);

        OrganizeNeighbors(insertedSegment, segment);
        OrganizeNeighbors(neighborSegment, insertedSegment);

        insertedSegment.Update();
        segment.Update();

        _currentPathObject.SegmentCount++;
    }

    public void InsertBezierSegment()
    {
        BezierSegment insertedBezierSegment = new BezierSegment(_currentPathObject);
        BezierSegment neighborSegment = _currentHoverData.HoveredBeziersegment.Neighbor;
        BezierSegment segment = _currentHoverData.HoveredBeziersegment;

        SetInsertedHandlesPosition(insertedBezierSegment, neighborSegment, segment);
        InsertBezierSegment(insertedBezierSegment, neighborSegment, segment);
    }

    public void DeleteBezierSegment()
    {
        _segments.RemoveAt(_currentHoverData.HoveredSegmentIndex);

        OrganizeNeighbors(_currentHoveredSegment.Neighbor, _currentHoveredSegment.OtherNeighbor);
        _currentHoveredSegment.OtherNeighbor.Update();
        _currentHoveredSegment.Delete();

        _currentPathObject.UpdatePointsHandlesRenderer();
        _currentPathObject.SegmentCount--;
    }

    public override void DeleteObject()
    {
        _currentHoverData.UpdateBezierSegment(null);
        _program.Delete(_currentPathObject);
        _currentHoveredSegment = null;
        _program.SetPathObjectUpdate(true);
    }

    public State GetCurrentState()
    {
        State current = State.Closed;

        if (_currentPathObject == null)
        {
            return State.Blank;
        }

        switch (_currentPathObject.SegmentCount)
        {
            case 1:
                current = State.OnePoint; break;
            case 2:
                current = State.TwoPoints; break;
            case 3:
                current = State.ThreePoints; break;
        }
        if (_currentPathObject.SegmentCount > 3)
            current = State.FurtherPoints;
        if (_currentPathObject.IsClosed)
            current = State.Closed;

        return current;
    }
}
