using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RectangleRenderer : RectangleRendererBase
{
    protected Vector2 _p0;
    protected Vector2 _p1;
    protected Vector2 _p2;
    protected Vector2 _p3;

    protected Vector2[] _corners = new Vector2[4];
    bool _isFilled;
    public float Thickness = 2;

    //public void UpdateRenderer(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    public void UpdateRenderer(Vector2 downLeft, Vector2 upRight, bool isFilled)
    {
        _corners[0] = downLeft;
        _corners[1] = new Vector2(downLeft.x, upRight.y);
        _corners[2] = upRight;
        _corners[3] = new Vector2(upRight.x, downLeft.y);
        _isFilled = isFilled;
        SetVerticesDirty();

    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (_corners == null || _corners.Length < 4)
            return;
        if (_isFilled)
            vh.AddUIVertexQuad(SetVbo(_corners, uvs, color));
        else
            DrawOutlineRectangle(vh, _corners[0], _corners[2], color, Thickness);

    }

}