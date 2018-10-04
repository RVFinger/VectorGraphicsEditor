using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaObject : ScalableObject, IColorable
{
    [SerializeField] RectangleRenderer _drawAreaRenderer;
    [SerializeField] RectangleRenderer _borderRenderer;
    [SerializeField] RectangleRenderer _drawAreaSelectionRenderer;

    public override void Init()
    {
        base.Init();

        _selectionRenderer = _drawAreaSelectionRenderer;

        _downLeftAnchor = Program.Instance.DownLeft;
        _upRightAnchor = Program.Instance.UpRight;
        _downLeftAnchor += new Vector2(10, 10);
        _upRightAnchor -= new Vector2(10, 10);
        _drawAreaRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, true);
        _drawAreaSelectionRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, false);
        _borderRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, false);

        SetPoints();
        BoundingBoxRenderer.UpdateRectangle();
    }

    public override void Hover(bool isHovered, bool secondaryHover = false)
    {
        _isHovered = isHovered;
        _selectionRenderer.enabled = isHovered;
    }

    public override void Clone(ScalableObject toClone, Vector2 positioner)
    {
        base.Init();

        _selectionRenderer = _drawAreaSelectionRenderer;

        AreaObject areaObjectToClone = toClone as AreaObject;
        _downLeftAnchor = areaObjectToClone.DownLeftAnchor;
        _upRightAnchor = areaObjectToClone.UpRightAnchor;

        _drawAreaRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, true);
        _drawAreaSelectionRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, false);
        _borderRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, false);
        SetPoints();
        BoundingBoxRenderer.UpdateRectangle();
        PositionAtCenter(positioner);
        Hover(false);
        ShowBoundingRect(false);
        selected = false;
    }

    public override void PositionAtCenter(Vector2 positioner)
    {
        Vector2 translate = DownLeftAnchor + positioner;

        UpRightAnchor -= translate;
        DownLeftAnchor -= translate;
        SetPoints();

        _drawAreaRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, true);
        _drawAreaSelectionRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, false);
        _borderRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, false);

    }
    public void UpdateInlineArea()
    {
        _drawAreaRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, true);
    }

    public Color GetPathColor()
    {
        return _borderRenderer.color;
    }

    public void UpdateLineRendererColor(Color newColor)
    {
        _borderRenderer.color = newColor;
        _borderRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, false);
    }
    public Color GetInlineAreaColor()
    {
        return _drawAreaRenderer.color;
    }

    public void UpdateFillRendererColor(Color newColor)
    {
        _drawAreaRenderer.color = newColor;
        UpdateInlineArea();
    }

    public void UpdateLineRendererThickness(float thickness)
    {
        _borderRenderer.Thickness = thickness;
        _borderRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, false);
    }

    public float GetPathThickness()
    {
        return _borderRenderer.Thickness;
    }

    public override bool OverPathObject()
    {
        return MouseOverObject();
   }

    public override void Scale()
    {
        base.Scale();


        _drawAreaRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, true);
        _drawAreaSelectionRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, false);
        _borderRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, false);
    }

    public override void Move()
    {
        Vector2 newPosition = Program.Instance.GetMouseCanvasPosition() - _position;
        _position = Program.Instance.GetMouseCanvasPosition();

        UpRightAnchor += newPosition;
        DownLeftAnchor += newPosition;
        SetPoints();
        _drawAreaRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, true);
        _drawAreaSelectionRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, false);
        _borderRenderer.UpdateRenderer(DownLeftAnchor, UpRightAnchor, false);

        ShowBoundingRect();
    }
}