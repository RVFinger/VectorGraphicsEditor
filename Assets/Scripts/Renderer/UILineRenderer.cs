/// Credit jack.sydorenko, firagon
/// Sourced from - http://forum.unity3d.com/threads/new-ui-and-line-drawing.253772/
/// Updated/Refactored from - http://forum.unity3d.com/threads/new-ui-and-line-drawing.253772/#post-2528050
using System.Collections.Generic;

namespace UnityEngine.UI.Extensions
{
    public class UILineRenderer : MaskableGraphic
    {
        public enum JoinType
        {
            Bevel,
            Miter
        }

        private const float MIN_MITER_JOIN = 15 * Mathf.Deg2Rad;
        private const float MIN_BEVEL_NICE_JOIN = 30 * Mathf.Deg2Rad;
        private static readonly Vector2 UV_TOP_CENTER = new Vector2(0.5f, 0);
        private static readonly Vector2 UV_BOTTOM_CENTER = new Vector2(0.5f, 1);
        private static readonly Vector2[] middleUvs = new[] { UV_TOP_CENTER, UV_BOTTOM_CENTER, UV_BOTTOM_CENTER, UV_TOP_CENTER };
        public float LineThickness = 2;
        public List<BezierSegment> bezierSegments = new List<BezierSegment>();
        public bool relativeSize;
        public JoinType LineJoins = JoinType.Bevel;
        public PathObject PathObject;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if (bezierSegments.Count == 0)
                return;

            var sizeX = !relativeSize ? 1 : rectTransform.rect.width;
            var sizeY = !relativeSize ? 1 : rectTransform.rect.height;

            var offsetX = -rectTransform.pivot.x * sizeX;
            var offsetY = -rectTransform.pivot.y * sizeY;

            vh.Clear();

            // Generate the quads that make up the wide line
            var segments = new List<UIVertex[]>();

            for (int index = 0; index < bezierSegments.Count; index++)
            {
                BezierSegment item = bezierSegments[index];


                if (index > 0)
                {
                    var start = bezierSegments[index-1].SegmentPoints[bezierSegments[index - 1].SegmentPoints.Count - 1];
                    var end = item.SegmentPoints[0];
                    start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
                    end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);

                    segments.Add(CreateLineSegment(start, end));
                }
                else
                {
                    if(PathObject.IsClosed)
                    {
                        var segmentPoints = bezierSegments[bezierSegments.Count - 1].SegmentPoints;
                        var start = segmentPoints[segmentPoints.Count - 1];
                        var end = item.SegmentPoints[0];
                        start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
                        end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);

                        segments.Add(CreateLineSegment(start, end));
                    }
                }
           

                for (var i = 1; i < item.SegmentPoints.Count; i++)
                {
                    var start = item.SegmentPoints[i - 1];
                    var end = item.SegmentPoints[i];
                    start = new Vector2(start.x * sizeX + offsetX, start.y * sizeY + offsetY);
                    end = new Vector2(end.x * sizeX + offsetX, end.y * sizeY + offsetY);

                    segments.Add(CreateLineSegment(start, end));
                }
                var last = item.SegmentPoints[item.SegmentPoints.Count - 1];
            }
      

            // Add the line segments to the vertex helper, creating any joins as needed
            for (var i = 0; i < segments.Count; i++)
            {
                if (i < segments.Count - 1)
                {
                    var vec1 = segments[i][1].position - segments[i][2].position;
                    var vec2 = segments[i + 1][2].position - segments[i + 1][1].position;
                    var angle = Vector2.Angle(vec1, vec2) * Mathf.Deg2Rad;

                    // Positive sign means the line is turning in a 'clockwise' direction
                    var sign = Mathf.Sign(Vector3.Cross(vec1.normalized, vec2.normalized).z);

                    // Calculate the miter point
                    var miterDistance = LineThickness / (2 * Mathf.Tan(angle / 2));
                    var miterPointA = segments[i][2].position - vec1.normalized * miterDistance * sign;
                    var miterPointB = segments[i][3].position + vec1.normalized * miterDistance * sign;

                    var joinType = LineJoins;
                    if (joinType == JoinType.Miter)
                    {
                        // Make sure we can make a miter join without too many artifacts.
                        if (miterDistance < vec1.magnitude / 2 && miterDistance < vec2.magnitude / 2 && angle > MIN_MITER_JOIN)
                        {
                            segments[i][2].position = miterPointA;
                            segments[i][3].position = miterPointB;
                            segments[i + 1][0].position = miterPointB;
                            segments[i + 1][1].position = miterPointA;
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
                                segments[i][2].position = miterPointA;
                                segments[i + 1][1].position = miterPointA;
                            }
                            else
                            {
                                segments[i][3].position = miterPointB;
                                segments[i + 1][0].position = miterPointB;
                            }
                        }

                        var join = new UIVertex[] { segments[i][2], segments[i][3], segments[i + 1][0], segments[i + 1][1] };
                        vh.AddUIVertexQuad(join);
                    }
                }
                vh.AddUIVertexQuad(segments[i]);


            }
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
}