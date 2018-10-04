using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PointsHandlesRenderer : RectangleRendererBase
{
    List<BezierSegment> _bezier = new List<BezierSegment>();

    public List<BezierSegment> BezierCurve { get { return _bezier; } set { _bezier = value; } }

    public void UpdateRenderer()
    {
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();


        foreach (var item in BezierCurve)
        {
            Color firstHandleColor = item.FirstHandleHovered ? Color.yellow : color;
            Color secondHandleColor = item.SecondHandleHovered ? Color.yellow : color;
            DrawOutlineRectangle(vh, item.FirstHandle, firstHandleColor);
            DrawOutlineRectangle(vh, item.SecondHandle, secondHandleColor);
            DrawLineToHandle(vh, item);

            Color pointColor = item.PointSelected ? Color.cyan : item.PointHovered ? Color.yellow : color;

            DrawFilledRectangle(vh, item.Point, pointColor);

        }
    }
}
