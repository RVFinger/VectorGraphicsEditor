using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoundingBoxRenderer : RectangleRendererBase
{
    public Vector2 _point;
    public Vector2 _otherPoint;
    BoundingBoxPoint[] boundingBoxPoints;
    public BoundingBoxPoint[] BoundingBoxPoints { get { return boundingBoxPoints; } set { boundingBoxPoints = value; } }

    public void UpdateRectangle()
    {
        if (BoundingBoxPoints == null)
            return;

        _point = boundingBoxPoints[0].Position;
        _otherPoint = boundingBoxPoints[1].Position;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (BoundingBoxPoints == null)
            return;

        Vector2 centerPoint = new Vector2(_point.x + (_otherPoint.x - _point.x) / 2,
            _point.y + (_otherPoint.y - _point.y) / 2);
        DrawOutlineRectangle(vh, centerPoint, color, centerPoint.x - _point.x, centerPoint.y - _point.y);

        foreach (var item in boundingBoxPoints)
        {
            DrawOutlineRectangle(vh, item.Position, color);
        }
    }
}
