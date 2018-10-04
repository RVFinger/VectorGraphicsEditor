using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RectangleRendererBase : MaskableGraphic
{
    protected Vector2 offsetLeft;
    protected Vector2 offsetRight;
    protected Vector2 outerV0;
    protected Vector2 outerV1;
    protected Vector2 outerV2;
    protected Vector2 outerV3;
    protected Vector2 innerV0;
    protected Vector2 innerV1;
    protected Vector2 innerV2;
    protected Vector2 innerv3;
    Vector2 offsetX;
    Vector2 offset;

    protected Vector2[] uvs = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };

    protected void DrawLineToHandle(VertexHelper vh, BezierSegment segment)
    {
        Vector2 pointToHandle = segment.FirstHandle - segment.Point;
        Vector2 orthogonal = new Vector2(-pointToHandle.y, pointToHandle.x);
        orthogonal.Normalize();
        orthogonal = orthogonal * 0.4f;
        vh.AddUIVertexQuad(SetVbo(new[] { segment.Point - orthogonal, segment.Point + orthogonal, segment.FirstHandle + orthogonal, segment.FirstHandle - orthogonal },
            uvs, color));
        pointToHandle = segment.SecondHandle - segment.Point;
        orthogonal = new Vector2(-pointToHandle.y, pointToHandle.x);
        orthogonal.Normalize();
        orthogonal = orthogonal * 0.4f;
        vh.AddUIVertexQuad(SetVbo(new[] { segment.Point - orthogonal, segment.Point + orthogonal, segment.SecondHandle + orthogonal, segment.SecondHandle - orthogonal },
        uvs, color));

    }

    protected virtual void SetOuterVertices(Vector2 point, float width = 2, float height = 2)
    {
        offsetX = new Vector2(width, height);
        offset = new Vector2(-width, height);

        outerV0 = point - offsetX;
        outerV1 = point + offset;
        outerV2 = point + offsetX;
        outerV3 = point - offset;
    }

    protected virtual void SetOuterVertices( Vector2 downLeft, Vector2 upRight)
    {
        outerV0 = downLeft;
        outerV1 = new Vector2(downLeft.x, upRight.y);
        outerV2 = upRight;
        outerV3 = new Vector2(upRight.x, downLeft.y);
    }

    protected void SetInnerVertices(VertexHelper vh, Vector2 point, Color newColor)
    {
        innerV0 = point - offsetLeft;
        innerV1 = point + offsetRight;
        innerV2 = point + offsetLeft;
        innerv3 = point - offsetRight;

        SetUIVertexQuads(vh, newColor);
    }

    protected void SetInnerVertices(VertexHelper vh, Color newColor, float thickness)
    {
        offsetLeft = new Vector2(-thickness, -thickness);
        offsetRight =  new Vector2(thickness, -thickness);

        innerV0 = outerV0 - offsetLeft;
        innerV1 = outerV1  + offsetRight;
        innerV2 = outerV2  + offsetLeft;
        innerv3 = outerV3  - offsetRight;

        SetUIVertexQuads(vh, newColor);
    }

    void SetUIVertexQuads(VertexHelper vh, Color color)
    {
        Vector2[] right = new[] { outerV0, outerV1, innerV1, innerV0 };
        Vector2[] top = new[] { outerV1, innerV1, innerV2, outerV2 };
        Vector2[] left = new[] { innerv3, innerV2, outerV2, outerV3 };
        Vector2[] bottom = new[] { outerV0, innerV0, innerv3, outerV3 };
        Vector2[][] handleVertices = new Vector2[4][] { right, top, left, bottom };

        foreach (var item in handleVertices)
        {
            vh.AddUIVertexQuad(SetVbo(item, uvs, color));
        }
    }
    protected void DrawFilledRectangle(VertexHelper vh, Vector2 point, Color newColor)
    {
        SetOuterVertices(point);

        vh.AddUIVertexQuad(SetVbo(new[] { outerV0, outerV1, outerV2, outerV3 }, uvs, newColor));
    }

    void SetOffsetLeftRight(float thickness)
    {
        offsetLeft = offsetX * thickness;
        offsetRight = offset * thickness;
    }

    void SetOffsetLeftRight(Vector2 thickness)
    {
        offsetLeft = offsetX - new Vector2(2f, 2f);
        offsetRight = offset - new Vector2(-2f, 2f);
    }

    protected void DrawOutlineRectangle(VertexHelper vh, Vector2 point, Color newColor, float width, float height)
    {
        SetOuterVertices(point, width, height);
        SetOffsetLeftRight(new Vector2(2f, 2f));
        SetInnerVertices(vh, point, newColor);
    }
    protected void DrawOutlineRectangle(VertexHelper vh, Vector2 downLeft, Vector2 upRight, Color newColor, float thickness)
    {
        SetOuterVertices(downLeft, upRight);
        //SetOffsetLeftRight(new Vector2(6f, 6f));
        SetInnerVertices(vh, newColor, thickness);
    }

    protected void DrawOutlineRectangle(VertexHelper vh, Vector2 point, Color newColor)
    {
        SetOuterVertices(point);
        SetOffsetLeftRight(0.35f);
        SetInnerVertices(vh, point, newColor);
    }

    protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs, Color newColor)
    {
        UIVertex[] vbo = new UIVertex[4];
        for (int i = 0; i < vertices.Length; i++)
        {
            var vert = UIVertex.simpleVert;
            vert.color = newColor;
            vert.position = vertices[i];
            vert.uv0 = uvs[i];
            vbo[i] = vert;
        }
        return vbo;
    }
}
