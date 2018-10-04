using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InlineAreaRendererHelper
{
    public List<Vector2> _points;
    public List<Vector2> WorkingCopy;

    Vector2 newLeftPoint;
    Vector2 newRightPoint;

    Vector2 leftPoint;
    Vector2 rightPoint;

    public List<Vector2[]> Vertices;
    int[] idx;
    int beneathIndex;
    //InlineAreaRenderer m_debugFiller;

    public InlineAreaRendererHelper(List<Vector2> usePoints, List<Vector2[]> vertices)
    {
        _points = usePoints;
        Vertices = vertices;
    }

    public void Execute()
    {
        if (_points.Count < 4)
            return;


        Order(true);

        AddInitialQuart();
    }

    void Restart()
    {
        Order();
        AddInitialQuart();
    }

    void Order(bool init = false)
    {
        WorkingCopy = new List<Vector2>(_points);
        List<Vector2> toSort = new List<Vector2>(_points);

        var sorted = toSort
        .Select((x, i) => new KeyValuePair<Vector2, int>(x, i))
        .OrderBy(x => x.Key.y)
        .ToArray();

        idx = sorted.Select(x => x.Value).ToArray();
        int index = idx[0];

        int indexFromZero = 0;

        for (int i = 0; i < WorkingCopy.Count; i++)
        {
            if (index < WorkingCopy.Count)
            {
                _points[i] = WorkingCopy[index];
                index++;
            }
            else
            {
                _points[i] = WorkingCopy[indexFromZero];
                indexFromZero++;

            }
        }
        WorkingCopy.Clear();
    }

    // Draw quart at current lowest Point
    void AddInitialQuart()
    {
        if (_points.Count < 4)
            return;

        int left = 0;
        int right = _points.Count - 1;

        if (_points[left + 1].y < _points[right].y)
        {
            if (_points[left + 2].y > _points[right].y)
            {
                leftPoint = _points[left + 2];
                newLeftPoint.x = NewLeftPointX(left + 1, right);
                newLeftPoint.y = _points[right].y;
                rightPoint = _points[right];

                Vertices.Add(new Vector2[] { _points[left + 1], newLeftPoint, rightPoint, _points[left] });
                //m_debugFiller.Draw(new Vector2[] { _points[left + 1], newLeftPoint, rightPoint, _points[left] }, vh, Color.red);

                leftPoint = newLeftPoint;

                _points.RemoveAt(right);
                _points.RemoveAt(left + 1);
                _points.RemoveAt(left);
            }
            else
            {
                rightPoint = _points[right];
                newRightPoint.x = NewRightPointX(left, left + 2);
                newRightPoint.y = _points[left + 2].y;
                leftPoint = _points[left + 2];

                Vertices.Add(new Vector2[] { _points[left + 1], leftPoint, newRightPoint, _points[left] });
                //m_debugFiller.Draw(new Vector2[] { _points[left + 1], leftPoint, newRightPoint, _points[left] }, vh, Color.green);

                rightPoint = newRightPoint;

                _points.RemoveAt(left + 2);
                _points.RemoveAt(left + 1);
                _points.RemoveAt(left);
            }
        }
        else
        {
            if (_points[right - 1].y < _points[left + 1].y)
            {
                leftPoint = _points[left + 1];
                newLeftPoint.x = NewLeftPointX(left, right - 1);
                newLeftPoint.y = _points[right - 1].y;
                rightPoint = _points[right - 1];

                Vertices.Add(new Vector2[] { _points[left], newLeftPoint, rightPoint, _points[right] });
                //m_debugFiller.Draw(new Vector2[] { _points[left], newLeftPoint, rightPoint, _points[right] }, vh, Color.grey);

                leftPoint = newLeftPoint;

                _points.RemoveAt(right);
                _points.RemoveAt(right - 1);
                _points.RemoveAt(left);
            }
            else
            {
                rightPoint = _points[right - 1];
                newRightPoint.x = NewRightPointX(right, left + 1);
                newRightPoint.y = _points[left + 1].y;
                leftPoint = _points[left + 1];

                Vertices.Add(new Vector2[] { _points[left], leftPoint, newRightPoint, _points[right] });
                //m_debugFiller.Draw(new Vector2[] { _points[left], leftPoint, newRightPoint, _points[right] }, vh, Color.white);

                rightPoint = newRightPoint;

                _points.RemoveAt(right);
                _points.RemoveAt(left + 1);
                _points.RemoveAt(left);
            }
        }
        AddQuart();
    }

    // Fill up 
    void AddQuart()
    {
        while (_points.Count > 0)
        {
            int nextLeft = 0;
            int nextRight = _points.Count - 1;

            if (_points[nextLeft].y < leftPoint.y || _points[nextRight].y < rightPoint.y)
            {
                if (_points.Count > 4)
                {
                    _points.Insert(0, leftPoint);
                    _points.Add(rightPoint);
                }

                Restart();
                break;
            }

            if (_points[nextLeft].y > _points[nextRight].y)
            {
                newLeftPoint.x = NewLeftPointX(nextLeft, nextRight);
                newLeftPoint.y = _points[nextRight].y;
                if (CheckForBreak(newLeftPoint, _points[nextRight]))
                    break;
                Vertices.Add(new Vector2[] { leftPoint, newLeftPoint, _points[nextRight], rightPoint });
                //m_debugFiller.Draw(new Vector2[] { leftPoint, newLeftPoint, _points[nextRight], rightPoint }, vh, Color.yellow);

                leftPoint = newLeftPoint;
                rightPoint = _points[nextRight];

                _points.RemoveAt(nextRight);
            }
            else
            {
                newRightPoint.x = NewRightPointX(nextRight, nextLeft);
                newRightPoint.y = _points[nextLeft].y;
                if (CheckForBreak(_points[nextLeft], newRightPoint))
                    break;
                Vertices.Add(new Vector2[] { leftPoint, _points[nextLeft], newRightPoint, rightPoint });
                //m_debugFiller.Draw(new Vector2[] { leftPoint, _points[nextLeft], newRightPoint, rightPoint }, vh, Color.black);

                leftPoint = _points[nextLeft];
                rightPoint = newRightPoint;

                _points.RemoveAt(nextLeft);
            }
        }
    }

    bool CheckForBreak(Vector2 left, Vector2 right)
    {
        if (_points.Count > 3)
        {
            bool isAnysmaller;

            IsAnyOtherPointBelow(out isAnysmaller, left, right);
            if (isAnysmaller)
            {
                AddQuartBeforeSplitting(beneathIndex);
                SplitCurve();
                return true;
            }
        }
        return false;
    }

    void IsAnyOtherPointBelow(out bool isAnysmaller, Vector2 left, Vector2 right)
    {
        float leftY = left.y;
        float leftX = left.x;
        float rightY = right.y;
        float rightX = right.x;
        float _currentSmallest = float.MaxValue;
        isAnysmaller = false;

        for (int i = 2; i < _points.Count - 2; i++)
        {
            Vector2 currentPoint = _points[i];

            if (currentPoint.x > leftX && currentPoint.x < rightX)
            {

                float currentY = currentPoint.y;
                if (currentY < leftY && currentY < rightY)
                {
                    Vector2 followingPoint = _points[i + 1];
                    Vector2 previousPoint = _points[i - 1];
                    if ((previousPoint.y > leftY && previousPoint.x > leftX && previousPoint.x < rightX) ||
                        (followingPoint.y > leftY && followingPoint.x > leftX && followingPoint.x < rightX))
                    {
                        bool found;
                        Vector2 intersection = CurveMath.GetIntersectionPointCoordinates(currentPoint, previousPoint, left, right, out found);
                        if (found && CurveMath.IsPointOnLine(left, right, intersection))
                        {
                            if (currentY < _currentSmallest)
                            {
                                _currentSmallest = currentY;
                                beneathIndex = i;
                                isAnysmaller = true;
                            }
                        }
                    }
                }
            }
        }
    }

    void AddQuartBeforeSplitting(int beneathIndex)
    {
        float leftX = NewLeftPointX(0, beneathIndex);
        float rightX = NewRightPointX(_points.Count - 1, beneathIndex);

        newLeftPoint = new Vector2(leftX, _points[beneathIndex].y);
        newRightPoint = new Vector2(rightX, _points[beneathIndex].y);

        Vertices.Add(new[] { leftPoint, newLeftPoint, newRightPoint, rightPoint });
        //m_debugFiller.Draw(new Vector2[] { leftPoint, newLeftPoint, newRightPoint, rightPoint }, vh, Color.blue);
    }

    void SplitCurve()
    {
        int count = _points.Count - beneathIndex - 1;
        List<Vector2> newLeftList = new List<Vector2>(_points);
        newLeftList.RemoveRange(beneathIndex + 1, count);
        newLeftList.Insert(0, newLeftPoint);

        InlineAreaRendererHelper helperLeft = new InlineAreaRendererHelper(newLeftList, Vertices);
        helperLeft.Execute();

        _points.RemoveRange(0, beneathIndex);
        _points.Add(newRightPoint);
        List<Vector2> newRightList = new List<Vector2>(_points);
        InlineAreaRendererHelper calRight = new InlineAreaRendererHelper(newRightList, Vertices);

        calRight.Execute();
    }

    float NewLeftPointX(int indexLeft, int indexRight)
    {
        float leftX = leftPoint.x - _points[indexLeft].x;
        float leftY = leftPoint.y - _points[indexLeft].y;
        float m = leftY / leftX;
        float t = _points[indexLeft].y - _points[indexLeft].x * m;
        return (_points[indexRight].y - t) / m;
    }

    float NewRightPointX(int indexRight, int indexLeft)
    {
        float leftX = rightPoint.x - _points[indexRight].x;
        float leftY = rightPoint.y - _points[indexRight].y;
        float m = leftY / leftX;
        float t = _points[indexRight].y - _points[indexRight].x * m;
        return (_points[indexLeft].y - t) / m;
    }
}