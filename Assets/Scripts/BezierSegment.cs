using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BezierSegment
{
    public Vector2 Point;
    public Vector2 FirstHandle;
    public Vector2 SecondHandle;
    public bool PointSelected;

    private BezierSegment _neighbor;
    public BezierSegment Neighbor { get { return _neighbor; } set { _neighbor = value; if (value != null) _hasNeighbor = true; } }
    private bool _hasNeighbor;

    private BezierSegment _otherNeighbor;
    public BezierSegment OtherNeighbor { get { return _otherNeighbor; } set { _otherNeighbor = value; if (value != null) _hasOtherNeighbor = true; } }
    private bool _hasOtherNeighbor;

    private PathObject _pathObject;

    bool _conjunct = true;
    List<Vector2> segmentPoints = new List<Vector2>();
    int _segmentCount = 4;

    public List<Vector2> SegmentPoints { get { return segmentPoints; } set { segmentPoints = value; } }
    public int SegmentCount { get { return _segmentCount; } set { _segmentCount = value; } }
    public bool Conjunct { get { return _conjunct; } set { _conjunct = value; } }
    public bool PointHovered { get { return _pointHovered; } set { _pointHovered = value; _pathObject.UpdatePointsHandlesRenderer(); } }
    public bool FirstHandleHovered { get { return _firstHandleHovered; } set { _firstHandleHovered = value; _pathObject.UpdatePointsHandlesRenderer(); } }
    public bool SecondHandleHovered { get { return _secondHandleHovered; } set { _secondHandleHovered = value; _pathObject.UpdatePointsHandlesRenderer(); } }
    public bool ShowPointsHandles { get { return _showPointsHandles; } set { _showPointsHandles = value; } }


    bool _isSecondMoved;
    bool _pointHovered;
    bool _firstHandleHovered;
    bool _secondHandleHovered;
    bool _showPointsHandles = true;
    public bool isFirst = false;

    float scaleQuotient;
    float firstHandleQuotient;
    float secondHandleQuotient;


    float heightScaleQuotient;
    float heightFirstHandleQuotient;
    float heightSecondHandleQuotient;

    public BezierSegment(PathObject pathObject)
    {
        _pathObject = pathObject;
    }

    public BezierSegment(PathObject pathObject, BezierSegment toClone)
    {
        _pathObject = pathObject;

        Point = toClone.Point;
        FirstHandle = toClone.FirstHandle;
        SecondHandle = toClone.SecondHandle;
        SegmentCount = toClone.SegmentCount;
        SegmentPoints = new List<Vector2>(toClone.SegmentPoints);
        isFirst = toClone.isFirst;
    }

    public void AddCurvePoints(bool onlyOne = false)
    {
        if (!onlyOne)
        {
            for (int j = 0; j < SegmentCount; j++)
            {
                segmentPoints.Add(Vector2.zero);
            }
        }
        else
        {
            segmentPoints.Add(Point);
        }
    }

    public Vector2 MoveHandleCloserToPoint(float quotient, bool isSecondHandle)
    {
        if (isSecondHandle)
        {
            Vector2 handleToPoint = SecondHandle - Point;
            SecondHandle -= quotient * handleToPoint;
            return SecondHandle;
        }
        else
        {
            Vector2 handleToPoint = FirstHandle - Point;
            FirstHandle -= quotient * handleToPoint;
            return FirstHandle;
        }
    }

    /// <summary>
    /// Copies Elements from one list to another until it reaches lastIndex
    /// </summary>
    /// <param name="SegmentPoints">List to be copied</param>
    /// <param name="lastIndex">Last List index to be copied</param>
    public void CopySegmentsPoint(List<Vector2> SegmentPoints, int lastIndex)
    {
        _segmentCount = lastIndex;

        segmentPoints.Clear();

        for (int i = 0; i < SegmentCount; i++)
        {
            segmentPoints.Add(SegmentPoints[i]);
        }
    }

    /// <summary>
    /// Remove Range from SegmentPoint List
    /// </summary>
    /// <param name="index">Amount of items to be removed</param>
    public void RemoveSegmentPoints(int index)
    {
        _segmentCount -= index;
        segmentPoints.RemoveRange(0, index);
    }

    public void Update()
    {
        if (_hasNeighbor)
            BezierCurve(_neighbor);

        if (_hasOtherNeighbor)
            _otherNeighbor.BezierCurve(this);
    }

    void BezierCurve(BezierSegment otherSegment)
    {
        int segments = SegmentCount;

        for (int i = 1; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector2 pointToAddWithZIsIndex = CurveMath.CalculateBezierPoint(t, otherSegment.Point, otherSegment.SecondHandle, FirstHandle, Point);
            segmentPoints[i - 1] = pointToAddWithZIsIndex;
        }
        _pathObject.UpdateLineRenderer(this);
    }

    public bool MouseOverPathPoint(Vector2 mousePosition)
    {
        return Vector2.Distance(mousePosition, Point) <= 5;
    }

    public bool MouseOverPathHandles(Vector2 mousePosition)
    {
        return Vector2.Distance(mousePosition, SecondHandle) <= 6.5f ||
             Vector2.Distance(mousePosition, FirstHandle) <= 6.5f;
    }

    public bool MouseOverPathHandles(Vector2 mousePosition, ref HoverData hoverData)
    {
        bool isSecondHandle = Vector2.Distance(mousePosition, SecondHandle) <= 6.5f;
        if (isSecondHandle ||
             Vector2.Distance(mousePosition, FirstHandle) <= 6.5f)
        {
            hoverData.IsSecondHandleHovered = isSecondHandle;
            hoverData.UpdateBezierSegment(this);

            FirstHandleHovered = !isSecondHandle;
            SecondHandleHovered = isSecondHandle;
            return true;
        }
        return false;
    }

    public bool MouseOverSegmentPoints(Vector2 mousePosition)
    {
        for (int index = 0; index < SegmentPoints.Count; index++)
        {
            if (Vector2.Distance(mousePosition, SegmentPoints[index]) <= 3f)
            {
                return true;
            }
        }
        return false;
    }

    public Vector2 Tangent(int atIndex)
    {
        int segmentPointsCount = SegmentPoints.Count;

        Vector2 currentLeftPosition;
        Vector2 currentRightPosition;
        if (atIndex - 1 >= 0)
        {
            currentLeftPosition = SegmentPoints[atIndex - 1];
        }
        else
        {
            if (_neighbor != null && _neighbor.segmentPoints.Count > 0)
                currentLeftPosition = _neighbor.SegmentPoints[_neighbor.segmentPoints.Count - 1];
            else
                currentLeftPosition = SegmentPoints[atIndex];
        }

        if (atIndex + 1 < segmentPointsCount)
        {
            currentRightPosition = SegmentPoints[atIndex + 1];
        }
        else
        {
            if (_otherNeighbor != null && _otherNeighbor.segmentPoints.Count > 0)
                currentRightPosition = _otherNeighbor.SegmentPoints[0];
            else
                currentRightPosition = SegmentPoints[atIndex];
        }

        return currentLeftPosition - currentRightPosition;
    }

    public bool MouseOverSegmentPoints(Vector2 mousePosition, ref HoverData hoverData)
    {
        for (int i = 0; i < SegmentPoints.Count; i++)
        {
            if (Vector2.Distance(mousePosition, SegmentPoints[i]) <= 3f)
            {
                if (hoverData.HoveredBeziersegment != null &&
                    hoverData.HoveredBeziersegment.PointHovered)
                    hoverData.HoveredBeziersegment.PointHovered = false;

                hoverData.HoveredSegmentPointIndex = i + 1;
                hoverData.UpdateBezierSegment(this);
                return true;
            }
        }
        return false;
    }

    public void Delete()
    {
        segmentPoints.Clear();
    }

    public void SetConjunct(bool isConjunct)
    {
        Conjunct = isConjunct;
        if (isConjunct)
        {
            Vector2 newPosition = _isSecondMoved ? SecondHandle : FirstHandle;
            MoveHandle(newPosition, _isSecondMoved);
        }
    }

    public void OneHandle(bool isSecondHandle)
    {
        Vector2 tangent = Tangent(SegmentPoints.Count - 1);

        if (isSecondHandle)
        {
            FirstHandle = Point;
            SecondHandle -= tangent.normalized * 30;
        }
        else
        {
            SecondHandle = Point;
            FirstHandle += tangent.normalized * 30;
        }

        Update();
    }

    public void NoHandles()
    {
        FirstHandle = SecondHandle = Point;

        Update();
    }

    public void Handles()
    {
        Vector2 tangent = Tangent(SegmentPoints.Count - 1);

        FirstHandle += tangent.normalized * 30;
        SecondHandle -= tangent.normalized * 30;

        Update();
    }



    public void MoveHandle(Vector2 position, bool isSecondHandle, bool init = false)
    {
        Vector2 pointPosition = Point;
        _isSecondMoved = isSecondHandle;
        if (isSecondHandle)
        {
            SecondHandle = position;
            Vector2 handleToPoint = pointPosition - position;

            if (init)
            {
                FirstHandle = pointPosition + handleToPoint;
            }
            else
            {
                if (_conjunct)
                {
                    float length = (FirstHandle - Point).magnitude;
                    FirstHandle = pointPosition + length * handleToPoint.normalized;
                }
            }
        }
        else
        {
            FirstHandle = position;
            Vector2 handleToPoint = pointPosition - position;
            if (init)
                SecondHandle = pointPosition + handleToPoint;
            else
            {
                if (_conjunct)
                {
                    float length = (SecondHandle - Point).magnitude;
                    SecondHandle = pointPosition + length * handleToPoint.normalized;
                }
            }


        }

        if (_pathObject.SegmentCount > 0)
        {
            Update();
        }
    }

    public void MovePoints(Vector2 newPosition)
    {
        Point += newPosition;
        FirstHandle += newPosition;
        SecondHandle += newPosition;
        Update();

        if (isFirst && !_pathObject.IsClosed)
            segmentPoints[0] = Point;
    }

    public void StartScaleWidth(float width, float borderX)
    {
        scaleQuotient = (Point.x - borderX) / width;
        firstHandleQuotient = (FirstHandle.x - borderX) / width;
        secondHandleQuotient = (SecondHandle.x - borderX) / width;
    }

    public void StartScaleHeight(float heigth, float borderY)
    {
        heightScaleQuotient = (Point.y - borderY) / heigth;
        heightFirstHandleQuotient = (FirstHandle.y - borderY) / heigth;
        heightSecondHandleQuotient = (SecondHandle.y - borderY) / heigth;
    }

    public void Scale(float width, float borderX, float height, float borderY)
    {
        Point.x = borderX - scaleQuotient * width;
        FirstHandle.x = borderX - firstHandleQuotient * width;
        SecondHandle.x = borderX - secondHandleQuotient * width;

        Point.y = borderY + heightScaleQuotient * height;
        FirstHandle.y = borderY + heightFirstHandleQuotient * height;
        SecondHandle.y = borderY + heightSecondHandleQuotient * height;


        if (isFirst && !_pathObject.IsClosed)
            segmentPoints[0] = Point;
        Update();

    }
}

