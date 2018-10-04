using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PathObject : ScalableObject, IColorable
{
    [SerializeField] PointsHandlesRenderer _pointsHandlesRenderer;
    [SerializeField] InlineAreaRenderer _inLineAreaRenderer;
    [SerializeField] BezierSegmentRenderer _pathRenderer;
    [SerializeField] BezierSegmentRenderer _pathSelectionRenderer;

    List<BezierSegment> _bezier = new List<BezierSegment>();
    public List<BezierSegment> BezierCurve { get { return _bezier; } set { _bezier = value; } }

    private bool _isClosed;
    public bool IsClosed { get { return _isClosed; } set { _isClosed = value; Program.Instance.ChangeState(); } }

    private bool _endPointSelected;
    public bool EndPointSelected { get { return _endPointSelected; } set { _endPointSelected = value; } }

    private int _segmentCount;
    public int SegmentCount { get { return _segmentCount; } set { _segmentCount = value; Program.Instance.ChangeState(); } }

    private List<Vector2[]> _vertices = new List<Vector2[]>();
    public List<Vector2[]> FillVertices { get { return _vertices; } set { _vertices = value; } }

    private List<Vector2> _pointsSorted;
    private List<Vector2> _workingCopy;
    private List<Vector2> _points = new List<Vector2>();
    private bool showPointsHandles = true;

    public override void Init()
    {
        base.Init();
        _inLineAreaRenderer.Init(this);
        _selectionRenderer = _pathSelectionRenderer;
    }

    public void ShowPointsHandles(bool enable)
    {
        showPointsHandles = enable;
    }

    public override void Hover(bool isHovered, bool secondaryHover = false)
    {
        _isHovered = isHovered;
        _selectionRenderer.enabled = isHovered;
        if (secondaryHover)
            _pointsHandlesRenderer.enabled = true && showPointsHandles;
        else
            _pointsHandlesRenderer.enabled = (isHovered && showPointsHandles) || (selected && showPointsHandles);
    }

    public void EnablePointsHandlesRenderer(bool enable)
    {
        _pointsHandlesRenderer.enabled = enable;
    }


    public override void Clone(ScalableObject toClone, Vector2 positioner)
    {
        PathObject pathObjectToClone = toClone as PathObject;
        Init();
        BezierCurve.Clear();
        BezierCurve = pathObjectToClone.BezierCurve.ConvertAll(segment => new BezierSegment(this, segment));

        IsClosed = pathObjectToClone.IsClosed;
        OrganizeAllNeighbors();

        HandOverPointsToRenderer();
        _pathRenderer.UpdateRenderer();
        _pathSelectionRenderer.UpdateRenderer();
        Anchors(GetAllPoints());
        PositionAtCenter(positioner);
        SortPoints(GetAllPoints());


        UpdateInlineAreaRenderer();
        IColorable selectable = toClone as IColorable;
        UpdateFillRendererColor(selectable.GetInlineAreaColor());
        UpdateLineRendererColor(selectable.GetPathColor());
        UpdateLineRendererThickness(selectable.GetPathThickness());
        Hover(false);
        ShowBoundingRect(false);
        selected = false;
        SegmentCount = BezierCurve.Count;
        showPointsHandles = false;
    }

    public override void Unselect()
    {
        EndPointSelected = false;

        if (BezierCurve.Count > 0)
        {
            BezierCurve[0].PointSelected = false;
            BezierCurve[BezierCurve.Count - 1].PointSelected = false;
            UpdatePointsHandlesRenderer();

        }
    }

    bool RightHandCurve(List<Vector2> points)
    {
        List<Vector2> toSort = new List<Vector2>(points);

        var sorted = toSort
                     .Select((x, i) => new KeyValuePair<Vector2, int>(x, i))
                     .OrderBy(x => x.Key.y)
                     .ToArray();
        int[] idx = sorted.Select(x => x.Value).ToArray();
        int index = idx[0];

        float smallestY_X = _workingCopy[index].x;
        int oneSide;
        int overPassedCountBy = 0;
        for (int i = 0; i < _workingCopy.Count; i++)
        {
            if (index + 1 + i < _workingCopy.Count)
                oneSide = index + 1 + i;
            else
            {
                oneSide = 0 + overPassedCountBy;
                overPassedCountBy++;
            }

            if (_workingCopy[oneSide].x < smallestY_X)
                return true;

            if (_workingCopy[oneSide].x > smallestY_X)
                return false;
        }
        return false;
    }

    public void UpdateInlineArea()
    {
        SortPoints(GetAllPoints());

        UpdateInlineAreaRenderer();
    }

    public void UpdateInlineAreaRenderer()
    {
        List<Vector2> pointList = GetAllPoints();
        if (!RightHandCurve(pointList))
        {
            List<Vector2> helper = new List<Vector2>(pointList);
            pointList.Clear();
            pointList.Add(helper[0]);

            for (int i = helper.Count - 1; i > 1; i--)
                pointList.Add(helper[i]);
        }
        _inLineAreaRenderer.UpdateRenderer(pointList);
    }

    public void UpdateFillRendererColor(Color newColor)
    {
        _inLineAreaRenderer.color = newColor;
    }

    public void UpdateLineRendererColor(Color newColor)
    {
        _pathRenderer.color = newColor;
        _pathRenderer.UpdateRenderer();
    }

    public Color GetPathColor()
    {
        return _pathRenderer.color;
    }

    public Color GetInlineAreaColor()
    {
        return _inLineAreaRenderer.color;
    }

    public void UpdateLineRenderer(BezierSegment segment)
    {
        _pathRenderer.UpdateRenderer();
        _pathSelectionRenderer.UpdateRenderer();
    }

    public float GetPathThickness()
    {
        return _pathRenderer.LineThickness;
    }

    public void UpdateLineRendererThickness(float thickness)
    {
        _pathRenderer.LineThickness = thickness;
        _pathRenderer.UpdateRenderer();
        _pathSelectionRenderer.UpdateRenderer();
    }


    public void UpdatePointsHandlesRenderer()
    {
        _pointsHandlesRenderer.UpdateRenderer();
    }

    public void HandOverPointsToRenderer()
    {
        _pointsHandlesRenderer.BezierCurve = _bezier;
        _pathSelectionRenderer.bezierSegments = _bezier;
        _pathRenderer.bezierSegments = _bezier;
    }

    public void Anchors(List<Vector2> points)
    {
        _pointsSorted = new List<Vector2>(points);

        List<Vector2> sortedVectorsX = _pointsSorted.OrderBy(v => v.x).ToList();
        List<Vector2> sortedVectorsY = _pointsSorted.OrderBy(v => v.y).ToList();
        _downLeftAnchor = new Vector2(sortedVectorsX[0].x, sortedVectorsY[0].y);
        _upRightAnchor = new Vector2(sortedVectorsX[sortedVectorsX.Count - 1].x, sortedVectorsY[sortedVectorsY.Count - 1].y);
    }

    public void SortPoints(List<Vector2> points)
    {
        _workingCopy = new List<Vector2>(points);
        Anchors(points);

        var sorted = _pointsSorted
        .Select((x, i) => new KeyValuePair<Vector2, int>(x, i))
        .OrderBy(x => x.Key.y)
        .ToArray();
        int[] idx = sorted.Select(x => x.Value).ToArray();
        int index = idx[0];


        int indexFromZero = 0;

        for (int i = 0; i < _workingCopy.Count; i++)
        {
            if (index < _workingCopy.Count)
            {

                _pointsSorted[i] = _workingCopy[index];
                index++;
            }
            else
            {

                _pointsSorted[i] = _workingCopy[indexFromZero];
                indexFromZero++;
            }
        }

        SetPoints();
    }

    public bool MouseOverPath()
    {
        Vector2 mousePosition = Program.Instance.GetMouseCanvasPosition();
        for (int i = 0; i < BezierCurve.Count; i++)
        {
            if (BezierCurve[i].MouseOverSegmentPoints(mousePosition))
                return true;
        }
        return false;
    }

    public bool MouseOverPath(ref HoverData hoverData)
    {
        Vector2 mousePosition = Program.Instance.GetMouseCanvasPosition();
        for (int i = 0; i < BezierCurve.Count; i++)
        {
            if (BezierCurve[i].MouseOverSegmentPoints(mousePosition, ref hoverData))
            {
                hoverData.HoveredSegmentIndex = i;
                return true;
            }
        }
        return false;
    }

    public bool MouseOverPathPoints()
    {
        Vector2 mousePosition = Program.Instance.GetMouseCanvasPosition();

        for (int i = 0; i < BezierCurve.Count; i++)
        {
            if (BezierCurve[i].MouseOverPathPoint(mousePosition))
                return true;
        }
        return false;
    }

    public bool MouseOverPathPoints(ref HoverData hoverData)
    {
        Vector2 mousePosition = Program.Instance.GetMouseCanvasPosition();

        for (int i = 0; i < BezierCurve.Count; i++)
        {
            if (BezierCurve[i].MouseOverPathPoint(mousePosition))
            {
                BezierCurve[i].PointHovered = true;

                UpdateHoverData(ref hoverData, BezierCurve[i], i);
                return true;
            }
        }
        return false;
    }

    public bool MouseOverPathHandles()
    {
        Vector2 mousePosition = Program.Instance.GetMouseCanvasPosition();

        for (int i = 0; i < BezierCurve.Count; i++)
        {
            if (BezierCurve[i].MouseOverPathHandles(mousePosition))
                return true;
        }
        return false;
    }

    public bool MouseOverPathHandles(ref HoverData hoverData)
    {
        Vector2 mousePosition = Program.Instance.GetMouseCanvasPosition();

        for (int i = 0; i < BezierCurve.Count; i++)
        {
            if (BezierCurve[i].MouseOverPathHandles(mousePosition, ref hoverData))
                return true;
        }
        return false;
    }

    void UpdateHoverData(ref HoverData hoverData, BezierSegment segment, int index)
    {
        if (hoverData.HoveredBeziersegment == segment)
            return;

        hoverData.HoveredSegmentIndex = index;
        hoverData.UpdateBezierSegment(segment);
    }

    public bool CloseToOtherEndPoint(ref HoverData hoverData)
    {
        Vector2 mousePosition = Program.Instance.GetMouseCanvasPosition();

        BezierSegment first = BezierCurve[0];
        BezierSegment last = BezierCurve[BezierCurve.Count - 1];

        if (first.PointSelected)
            if (Vector2.Distance(mousePosition, last.Point) <= 5.5f)
            {
                last.PointHovered = true;

                UpdateHoverData(ref hoverData, last, BezierCurve.Count - 1);
                return true;
            }

        if (last.PointSelected)
            if (Vector2.Distance(mousePosition, first.Point) <= 5.5f)
            {
                first.PointHovered = true;
                UpdateHoverData(ref hoverData, first, 0);
                return true;
            }
        return false;
    }

    public bool CloseToEndPoints(ref HoverData hoverData)
    {
        Vector2 mousePosition = Program.Instance.GetMouseCanvasPosition();

        BezierSegment first = BezierCurve[0];
        BezierSegment last = BezierCurve[BezierCurve.Count - 1];

        //CloseToLast
        if (Vector2.Distance(mousePosition, last.Point) <= 5.5f)
        {
            last.PointHovered = true;

            UpdateHoverData(ref hoverData, last, BezierCurve.Count - 1);
            return true;
        }
        //CloseToFirst
        if (Vector2.Distance(mousePosition, first.Point) <= 5.5f)
        {
            first.PointHovered = true;

            UpdateHoverData(ref hoverData, first, 0);
            return true;
        }
        return false;
    }

    public override bool OverPathObject()
    {
        Vector2 mousePosition = Program.Instance.GetMouseCanvasPosition();
        // not over bounding box

        if (!MouseOverObject())
            return false;

        int crossings = 0;
        for (int i = 0; i < _pointsSorted.Count - 1; i++)
        {
            float biggerY = _pointsSorted[i].y > _pointsSorted[i + 1].y ? _pointsSorted[i].y : _pointsSorted[i + 1].y;

            bool isFound;

            if (mousePosition.y <= biggerY)
            {
                if ((mousePosition.x >= _pointsSorted[i].x && mousePosition.x <= _pointsSorted[i + 1].x) || (mousePosition.x <= _pointsSorted[i].x && mousePosition.x >= _pointsSorted[i + 1].x))
                {
                    if (mousePosition.y <= CurveMath.GetIntersectionPointCoordinates(_pointsSorted[i], _pointsSorted[i + 1], mousePosition, mousePosition + Vector2.up, out isFound).y)
                        if (isFound)
                            crossings++;
                }
            }
        }
        return crossings % 2 == 1;
    }

    public void OrganizeAllNeighbors()
    {
        if (BezierCurve.Count > 1)
        {
            BezierCurve[BezierCurve.Count - 1].Neighbor = BezierCurve[BezierCurve.Count - 2];
            BezierCurve[BezierCurve.Count - 1].Update();
        }

        for (int i = 0; i < BezierCurve.Count - 1; i++)
        {
            OrganizeNeighbors(BezierCurve[i], BezierCurve[i + 1]);
            BezierCurve[i].Update();
        }

        if (IsClosed)
        {
            OrganizeNeighbors(BezierCurve[BezierCurve.Count - 1], BezierCurve[0]);
            BezierCurve[0].Update();
        }
    }

    public void OrganizeNeighbors(BezierSegment neighbor, BezierSegment otherNeighbor)
    {
        neighbor.OtherNeighbor = otherNeighbor;
        otherNeighbor.Neighbor = neighbor;
    }

    public List<Vector2> GetAllPoints()
    {
        _points.Clear();
        for (int i = 0; i < BezierCurve.Count; i++)
        {
            List<Vector2> segmentPoints = BezierCurve[i].SegmentPoints;
            for (int index = 0; index < segmentPoints.Count; index++)
            {
                _points.Add(segmentPoints[index]);
            }
        }
        return _points;
    }

    public override void PositionAtCenter(Vector2 positioner)
    {
        Vector2 translate = DownLeftAnchor + positioner;
        for (int i = 0; i < BezierCurve.Count; i++)
            BezierCurve[i].MovePoints(-translate);
        
        UpRightAnchor -= translate;
        DownLeftAnchor -= translate;
        SetPoints();

        UpdatePointsHandlesRenderer();
    }

    public override void Move()
    {
        Vector2 newPosition = Program.Instance.GetMouseCanvasPosition() - _position;
        _position = Program.Instance.GetMouseCanvasPosition();
        for (int i = 0; i < BezierCurve.Count; i++)
            BezierCurve[i].MovePoints(newPosition);

        UpRightAnchor += newPosition;
        DownLeftAnchor += newPosition;
        SetPoints();

        ShowBoundingRect();
        UpdatePointsHandlesRenderer();
    }

    public override void StartScale()
    {
        base.StartScale();
        for (int i = 0; i < BezierCurve.Count; i++)
        {
            BezierCurve[i].StartScaleWidth(Width(), DownLeftAnchor.x);
            BezierCurve[i].StartScaleHeight(Height(), DownLeftAnchor.y);
        }
    }

    public override void Scale()
    {
        base.Scale();

        float width = DownLeftAnchor.x - UpRightAnchor.x;
        float height = UpRightAnchor.y - DownLeftAnchor.y;
        for (int i = 0; i < BezierCurve.Count; i++)
        {
            BezierCurve[i].Scale(width, DownLeftAnchor.x, height, DownLeftAnchor.y);
        }
        UpdatePointsHandlesRenderer();
    }
}

public class BoundingBoxPoint
{
    public Vector2 Position;
    public bool[] ManipulateXyAnchor = new bool[4];

    public BoundingBoxPoint(bool[] manipulateXyAnchor)
    {
        ManipulateXyAnchor = manipulateXyAnchor;
    }
}