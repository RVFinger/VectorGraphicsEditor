using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InlineAreaRenderer : MaskableGraphic
{
    private PathObject _pathObject;
    private List<Vector2[]> _vertices = new List<Vector2[]>();
    private readonly Vector2[] uvs = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) };

    public void Init(PathObject pathObject)
    {
        _pathObject = pathObject;
        _vertices = _pathObject.FillVertices;
    }

    public void UpdateRenderer(List<Vector2> points)
    {
        _vertices.Clear();

        InlineAreaRendererHelper helper = new InlineAreaRendererHelper(points, _vertices);
        helper.Execute();

        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        foreach (var item in _vertices)
        {
            Draw(item, vh);
        }
    }

    public void Draw(Vector2[] vertices, VertexHelper vh)
    {
        vh.AddUIVertexQuad(SetVbo(vertices, uvs));
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
