/// Credit jack.sydorenko, firagon
/// Sourced from - http://forum.unity3d.com/threads/new-ui-and-line-drawing.253772/
/// Updated/Refactored from - http://forum.unity3d.com/threads/new-ui-and-line-drawing.253772/#post-2528050

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BezierSegmentRenderer : MaskableGraphic
{
    public bool relativeSize;

    private static readonly Vector2[] middleUvs = new[] { UV_TOP_CENTER, UV_BOTTOM_CENTER, UV_BOTTOM_CENTER, UV_TOP_CENTER };
    private static readonly Vector2 UV_TOP_CENTER = new Vector2(0.5f, 0);
    private static readonly Vector2 UV_BOTTOM_CENTER = new Vector2(0.5f, 1);

    public float LineThickness = 2;
    private const float MIN_MITER_JOIN = 15 * Mathf.Deg2Rad;
    private const float MIN_BEVEL_NICE_JOIN = 30 * Mathf.Deg2Rad;
    public List<BezierSegment> bezierSegments = new List<BezierSegment>();
    List<UIVertex[]> _segments = new List<UIVertex[]>();
    // A bevel 'nice' join displaces the vertices of the line segment instead of simply rendering a
    // quad to connect the endpoints. This improves the look of textured and transparent lines, since
    // there is no overlapping.
    public enum JoinType
    {
        Bevel,
        Miter
    }
    public JoinType LineJoins = JoinType.Bevel;




    public void UpdateRenderer()
    {
        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        foreach (BezierSegment segment in bezierSegments)
        {
            SetUIVertex(segment, vh);
        }

        AddLineSegmentsToVertexHelper(vh);

    }


    public void SetUIVertex(BezierSegment bezierSegment, VertexHelper vh)
    {
        var sizeX = !relativeSize ? 1 : rectTransform.rect.width;
        var sizeY = !relativeSize ? 1 : rectTransform.rect.height;

        var offsetX = -rectTransform.pivot.x * sizeX;
        var offsetY = -rectTransform.pivot.y * sizeY;

        List<Vector2> segmentPoints = bezierSegment.SegmentPoints;

        if (bezierSegment.Neighbor != null)
        {
            var start = bezierSegment.Neighbor.SegmentPoints[bezierSegment.Neighbor.SegmentPoints.Count - 1];
            var end = segmentPoints[0];
            start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
            end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);

            _segments.Add(CreateLineSegment(start, end));
            vh.AddUIVertexQuad(CreateLineSegment(start, end));
        }

        for (var i = 1; i < segmentPoints.Count; i++)
        {
            var start = segmentPoints[i - 1];
            var end = segmentPoints[i];
            start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
            end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);

            _segments.Add(CreateLineSegment(start, end));
            vh.AddUIVertexQuad(CreateLineSegment(start, end));
        }
    }

    public void AddLineSegmentsToVertexHelper(VertexHelper vh)
    {
        for (var i = 0; i < _segments.Count; i++)
        {
            if (i < _segments.Count - 1)
            {
                UIVertex[] segment = _segments[i];
                UIVertex[] followingSegment = _segments[i + 1];

                var vec1 = segment[1].position - segment[2].position;
                var vec2 = followingSegment[2].position - followingSegment[1].position;
                var angle = Vector2.Angle(vec1, vec2) * Mathf.Deg2Rad;

                // Positive sign means the line is turning in a 'clockwise' direction
                var sign = Mathf.Sign(Vector3.Cross(vec1.normalized, vec2.normalized).z);

                // Calculate the miter point
                var miterDistance = LineThickness / (2 * Mathf.Tan(angle / 2));
                var miterPointA = segment[2].position - vec1.normalized * miterDistance * sign;
                var miterPointB = segment[3].position + vec1.normalized * miterDistance * sign;

                var joinType = LineJoins;
                if (joinType == JoinType.Miter)
                {
                    // Make sure we can make a miter join without too many artifacts.
                    if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_MITER_JOIN)
                    {
                        segment[2].position = miterPointA;
                        segment[3].position = miterPointB;
                        followingSegment[0].position = miterPointB;
                        followingSegment[1].position = miterPointA;
                    }
                    else
                    {
                        joinType = JoinType.Bevel;
                    }
                }

                if (joinType == JoinType.Bevel)
                {
                    if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_BEVEL_NICE_JOIN)
                    {
                        if (sign < 0)
                        {
                            segment[2].position = miterPointA;
                            followingSegment[1].position = miterPointA;
                        }
                        else
                        {
                            segment[3].position = miterPointB;
                            followingSegment[0].position = miterPointB;
                        }
                    }

                    var join = new UIVertex[] { segment[2], segment[3], followingSegment[0], followingSegment[1] };

                    vh.AddUIVertexQuad(join);
                }
            }
        }
        _segments.Clear();
    }

    private UIVertex[] CreateLineSegment(Vector2 start, Vector2 end)
    {
        var uvs = middleUvs;

        Vector2 offset = new Vector2(start.y - end.y, end.x - start.x).normalized * LineThickness / 2;
        var v1 = start - offset;
        var v2 = start + offset;
        var v3 = end + offset;
        var v4 = end - offset;
        return SetVbo(new[] { v1, v2, v3, v4 }, uvs);
    }

    protected UIVertex[] SetVbo(Vector2[] vertices, Vector2[] uvs)
    {
        UIVertex[] vbo = new UIVertex[4];
        for (int i = 0; i < vertices.Length; i++)
        {
            var vert = UIVertex.simpleVert;
            vert.color = color;
            vert.position = vertices[i];
            vert.uv0 = uvs[i];
            vbo[i] = vert;
        }
        return vbo;
    }

}

