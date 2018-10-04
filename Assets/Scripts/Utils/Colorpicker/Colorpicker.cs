using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Colorpicker : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    [SerializeField] GameObject _colorpicker;
    [SerializeField] Transform _colorSelector;
    [SerializeField] Transform _hueSelector;
    [SerializeField] RectTransform _rect;
    [SerializeField] RectTransform _rectSV;
    [SerializeField] Image _image;
    [SerializeField] Slider _alpha;

    float _currentSaturation;
    float _currentValue;
    float _currentHue;
    PickMode _currentPickMode;
    Color _currentColor;
    float _hueRadius;
    int _saturationValueRectWidth;
    int _svRectWidth;
    public Color CurrentColor { get { return _currentColor; } private set { } }
    public RectTransform Rect { get { return _rect; } private set { } }
    RectTransform _colorPicker;
    Vector3 _bottomLeftColorPickerAnchor;
    Vector3 _topRightColorPickerAnchor;

    public delegate void ColorChanged(Color current, Color? secondColor = null);
    public static event ColorChanged OnColorChanged;

    enum PickMode
    {
        Hue,
        SaturationValue,
        None,
    }

    void Awake()
    {
        _alpha.onValueChanged.AddListener(delegate { SetAlpha(); });
    }

    public void Init()
    {
        _hueRadius = _hueSelector.localPosition.magnitude;
        _saturationValueRectWidth = (int)_rectSV.rect.width / 2;
        _svRectWidth = (int)_rectSV.rect.width;

    }
    void Start()
    {
        SetColorPickerBorders();
        ShowColorpicker(false);
    }

    public void SetColorPickerBorders()
    {
        Vector3[] corners = new Vector3[4];

        _rect.GetWorldCorners(corners);
        _bottomLeftColorPickerAnchor = corners[0];
        _topRightColorPickerAnchor = corners[2];
    }

    public void SetAlpha()
    {
        _currentColor.a = _alpha.value;
        OnColorChanged?.Invoke(_currentColor);
    }

    public void ShowColorpicker(bool show)
    {
        _colorpicker.SetActive(show);
    }

    public void HideColorpicker()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 screenPoint = Vector3.zero;

        screenPoint = mousePosition;
        screenPoint.z = 100;

        if(screenPoint.x < _bottomLeftColorPickerAnchor.x || screenPoint.x > _topRightColorPickerAnchor.x
            || screenPoint.y > _topRightColorPickerAnchor.y || screenPoint.y < _bottomLeftColorPickerAnchor.y)
        ShowColorpicker(false);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_currentPickMode == PickMode.None)
            return;
        SetColor(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        SetPickMode(eventData);
        SetColor(eventData);
    }

    void SetPickMode(PointerEventData eventData)
    {
        Vector2 localCursor;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, eventData.pressEventCamera, out localCursor))
            return;
        Vector2 pickPosition = localCursor;

        if (Mathf.Abs(pickPosition.x) <= _saturationValueRectWidth && Mathf.Abs(pickPosition.y) <= _saturationValueRectWidth)
            _currentPickMode = PickMode.SaturationValue;
        else if (Mathf.Abs(pickPosition.x) > _saturationValueRectWidth + 10 || Mathf.Abs(pickPosition.y) > _saturationValueRectWidth + 10)
            _currentPickMode = PickMode.Hue;
        else
            _currentPickMode = PickMode.None;  // prevent dragging from Hue selection into saturationValue selection

    }

    void SetColor(PointerEventData eventData)
    {
        if (_currentPickMode == PickMode.None)
            return;

        Vector2 localCursor;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, eventData.position, eventData.pressEventCamera, out localCursor))
            return;
        Vector2 pickPosition = localCursor;

        if (_currentPickMode == PickMode.SaturationValue)
            SetSaturationAndValue(pickPosition);
        else
            SetHue(pickPosition);
    }

    /* Select Saturation : the intensity of the color and 
       Brightness (or Value) : the brightness of the color
       Rectangle in the center of Texture
     */
    void SetSaturationAndValue(Vector2 pickPosition)
    {
        // position SaturationAndValueSelector

        pickPosition.x = Mathf.Clamp(pickPosition.x, -_saturationValueRectWidth, _saturationValueRectWidth);
        pickPosition.y = Mathf.Clamp(pickPosition.y, -_saturationValueRectWidth, _saturationValueRectWidth);

        _colorSelector.localPosition = pickPosition;

        // Set ColorSelectorImage Color

        _currentSaturation = (pickPosition.x + _saturationValueRectWidth) / _svRectWidth;
        _currentValue = (pickPosition.y + _saturationValueRectWidth) / _svRectWidth;

        _currentColor = Color.HSVToRGB(_currentHue, _currentSaturation, _currentValue);
        OnColorChanged?.Invoke(_currentColor);
    }

    // Select Hue : the color type; outer circle in Texture
    void SetHue(Vector2 pickPosition)
    {
        // position HueSelector
        _hueSelector.localPosition = pickPosition.normalized * _hueRadius;

        pickPosition = _hueSelector.localPosition;
        float angle = Vector2.Angle(pickPosition, Vector2.right);

        // Set ColorSelectorImage Color
        if (pickPosition.y < 0)
            angle = 360 - angle;

        _currentHue = angle / 360;
        _image.color = Color.HSVToRGB(_currentHue, 1, 1);
        _currentColor = Color.HSVToRGB(_currentHue, _currentSaturation, _currentValue);

        OnColorChanged?.Invoke(_currentColor);
    }

    public void UpdatePicker(IColorable currentSelectable)
    {
        if (currentSelectable == null)
            return;

        if (currentSelectable.ColorFillArea)
            _currentColor = currentSelectable.GetInlineAreaColor();
        else
            _currentColor = currentSelectable.GetPathColor();

        _alpha.value = _currentColor.a;

        Color.RGBToHSV(_currentColor, out _currentHue, out _currentSaturation, out _currentValue);

        float hue = 360 * _currentHue;
        Vector2 direction = CurveMath.Rotate(Vector2.right, hue);
        _hueSelector.localPosition = direction.normalized * _hueRadius;
        _image.color = Color.HSVToRGB(_currentHue, 1, 1);

        Vector2 newPositon;
        newPositon.x = _currentSaturation * _svRectWidth - _saturationValueRectWidth;
        newPositon.y = _currentValue * _svRectWidth - _saturationValueRectWidth;
        _colorSelector.localPosition = newPositon;

        OnColorChanged?.Invoke(currentSelectable.GetInlineAreaColor(), currentSelectable.GetPathColor());
    }
}
