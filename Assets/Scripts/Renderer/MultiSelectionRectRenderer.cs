using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MultiSelectionRectRenderer : RectangleRendererBase
{
    private Vector2 _point;
    private Vector2 _otherPoint;
    private readonly Vector2[] _vertices =  new Vector2[4];

    public void UpdateRectangle(Vector2 point, Vector2 otherPoint)
    {
        _point = point;
        _otherPoint = otherPoint;
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        DrawRect(vh);
    }

    public void DrawRect(VertexHelper vh)
    {
        _vertices[0] = _point;
        _vertices[1] = new Vector2(_point.x, _otherPoint.y);
        _vertices[2] = _otherPoint;
        _vertices[3] = new Vector2(_otherPoint.x, _point.y);

        vh.AddUIVertexQuad(SetVbo(_vertices, uvs, color));
    }
}
