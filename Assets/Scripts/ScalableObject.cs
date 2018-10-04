using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class ScalableObject : MonoBehaviour
{
    [SerializeField] BoundingBoxRenderer _boundingBoxRenderer;

    protected BoundingBoxRenderer BoundingBoxRenderer => _boundingBoxRenderer;
    protected BoundingBoxPoint[] _boundingBoxPoints = new BoundingBoxPoint[8];
    protected Vector2 _downLeftAnchor;
    protected Vector2 _upRightAnchor;
    public Vector2 DownLeftAnchor { get { return _downLeftAnchor; } set { _downLeftAnchor = value; } }
    public Vector2 UpRightAnchor { get { return _upRightAnchor; } set { _upRightAnchor = value; } }
    protected BoundingBoxPoint _currentBoundingBox;
    public BoundingBoxPoint CurrentBoundingBox { get { return _currentBoundingBox; } set { _currentBoundingBox = value; } }
    protected bool selected;
    public bool Selected { get { return selected; } set { selected = value; Program.Instance.ChangeState(); } }
    protected Vector2 _position;
    protected Vector2 _scalePosition;
    public bool ColorFillArea { get { return _colorFillArea; } set { _colorFillArea = value; } }
    public Vector2 Position { get { return _position; } set { _position = value; } }
    public bool _colorFillArea = true;
    public bool Hovered => _isHovered; 
    protected MaskableGraphic _selectionRenderer;
    protected bool _isHovered;
    protected Vector2 _boundbox;

    public virtual void Init()
    {
        _boundingBoxPoints[0] = new BoundingBoxPoint(new bool[] { true, true, false, false });
        _boundingBoxPoints[1] = new BoundingBoxPoint(new bool[] { false, false, true, true });
        _boundingBoxPoints[2] = new BoundingBoxPoint(new bool[] { true, false, false, true });
        _boundingBoxPoints[3] = new BoundingBoxPoint(new bool[] { false, true, true, false });
        _boundingBoxPoints[4] = new BoundingBoxPoint(new bool[] { true, false, false, false });
        _boundingBoxPoints[5] = new BoundingBoxPoint(new bool[] { false, true, false, false });
        _boundingBoxPoints[6] = new BoundingBoxPoint(new bool[] { false, false, true, false });
        _boundingBoxPoints[7] = new BoundingBoxPoint(new bool[] { false, false, false, true });
        _boundingBoxRenderer.BoundingBoxPoints = _boundingBoxPoints;

        SetPoints();
    }

    public virtual void Clone(ScalableObject toClone, Vector2 positioner)
    {

    }

    public virtual void Hover (bool isHovered, bool secondaryHover = false)
    {

    }

    public virtual void Unselect()
    {

    }

    public void BoundingBoundIndex(int index)
    {
        CurrentBoundingBox = _boundingBoxPoints[index];
    }

    public float Width()
    {
        return Mathf.Abs(_upRightAnchor.x - _downLeftAnchor.x);
    }

    public float Height()
    {
        return Mathf.Abs(_upRightAnchor.y - _downLeftAnchor.y);

    }
    public void SetPoints()
    {
        Vector2 widthHeight = _upRightAnchor - _downLeftAnchor;
        float height = widthHeight.y / 2;
        float width = widthHeight.x / 2;

        _boundingBoxPoints[0].Position = _downLeftAnchor;
        _boundingBoxPoints[1].Position = _upRightAnchor;
        _boundingBoxPoints[2].Position = new Vector2(_downLeftAnchor.x, _upRightAnchor.y);
        _boundingBoxPoints[3].Position = new Vector2(_upRightAnchor.x, _downLeftAnchor.y);
        _boundingBoxPoints[4].Position = new Vector2(_downLeftAnchor.x, _downLeftAnchor.y + height);
        _boundingBoxPoints[5].Position = new Vector2(_downLeftAnchor.x + width, _downLeftAnchor.y);
        _boundingBoxPoints[6].Position = new Vector2(_upRightAnchor.x, _upRightAnchor.y - height);
        _boundingBoxPoints[7].Position = new Vector2(_upRightAnchor.x - width, _upRightAnchor.y);
    }

    public bool MouseOverObject()
    {
        Vector2 mousePosition = Program.Instance.GetMouseCanvasPosition();
        float buffer = 5;
        return (mousePosition.x >= _downLeftAnchor.x - buffer && mousePosition.x <= _upRightAnchor.x + buffer && mousePosition.y <= _upRightAnchor.y + buffer && mousePosition.y >= _downLeftAnchor.y - buffer);
    }

    public bool CloseToBoundingBoxPoints(int one, int other)
    {
        Vector2 mousePosition = Program.Instance.GetMouseCanvasPosition();
        return (CheckDistance(mousePosition, _boundingBoxPoints[one], one) ||
            CheckDistance(mousePosition, _boundingBoxPoints[other], other));
    }

    bool CheckDistance(Vector2 another, BoundingBoxPoint boxPoint, int index)
    {
        if (Vector2.Distance(boxPoint.Position, another) <= 5f)
        {
            CurrentBoundingBox = boxPoint;
            Program.Instance.SetBoundingBoxPoint(index);
            return true;
        }
        return false;
    }

    public void ShowBoundingRect(bool show = true)
    {
        if (show)
            BoundingBoxRenderer.UpdateRectangle();

        BoundingBoxRenderer.enabled = show;
    }

    public void SetStartPosition()
    {
        _position = Program.Instance.GetMouseCanvasPosition();
    }
    public virtual bool OverPathObject()
    {
        return false;
    }


    public virtual void PositionAtCenter(Vector2 translate)
    {

    }
    public virtual void Move()
    {

    }

    public virtual void StartScale()
    {
        _scalePosition = Program.Instance.GetMouseCanvasPosition();
        _boundbox = CurrentBoundingBox.Position;
    }

    public virtual void Scale()
    {
        Vector2 newPosition = _boundbox += Program.Instance.GetMouseCanvasPosition() - _scalePosition;
        _scalePosition = Program.Instance.GetMouseCanvasPosition();

        BoundingBoxPoint current = CurrentBoundingBox;
        bool[] manipulateAnchors = current.ManipulateXyAnchor;

        if (manipulateAnchors[0])
            DownLeftAnchor = new Vector2(newPosition.x, DownLeftAnchor.y);
        if (manipulateAnchors[1])
            DownLeftAnchor = new Vector2(DownLeftAnchor.x, newPosition.y);
        if (manipulateAnchors[2])
            UpRightAnchor = new Vector2(newPosition.x, UpRightAnchor.y);
        if (manipulateAnchors[3])
            UpRightAnchor = new Vector2(UpRightAnchor.x, newPosition.y);
        SetPoints();
        _boundbox = CurrentBoundingBox.Position;

        ShowBoundingRect();
    }

}

public interface IColorable
{
    bool ColorFillArea { get; set; }
    void UpdateInlineArea();
    float GetPathThickness();
    void UpdateLineRendererThickness(float thickness);
    void UpdateFillRendererColor(Color newColor);
    void UpdateLineRendererColor(Color newColor);
    Color GetPathColor();
    Color GetInlineAreaColor();
}