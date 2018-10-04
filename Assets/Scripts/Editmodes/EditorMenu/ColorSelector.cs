using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour
{
    [SerializeField] Image _fillAreaColor;
    [SerializeField] Image _borderColor;
    [SerializeField] Image _recentUsedColor;
    [SerializeField] Image _transparentFillArea;
    [SerializeField] Image _transparentBorder;
    [SerializeField] Toggle _borderColorTgl;
    [SerializeField] Toggle _fillAreaColorTgl;
    [SerializeField] Button _transparentBtn;
    [SerializeField] Button _recentUsedBtn;

    private bool _isAreaFill = true;
    private Color _transparent = new Color(0, 0, 0, 0);
    private Colorpicker _colorPicker;
    //shown in editor, button click
    public bool IsAreaFill { get { return _isAreaFill; } set { _isAreaFill = value; UpdateColorMode(value); } }

    List<IColorable> _colorables;

    public void SynchronizeColorablesList(List<IColorable> colorables)
    {
        _colorables = colorables;
    }

    public void Init(Colorpicker colorpicker)
    {
        _colorPicker = colorpicker;
        if (_colorPicker == null)
            _colorPicker = FindObjectOfType<Colorpicker>();
        _borderColorTgl.onValueChanged.AddListener(delegate
        {
            _colorPicker.ShowColorpicker(true);
            IsAreaFill = false;
        });
        _fillAreaColorTgl.onValueChanged.AddListener(delegate
        {
            _colorPicker.ShowColorpicker(true);
            IsAreaFill = true;
        });

        _transparentBtn.onClick.AddListener(delegate { SetTransparentColor(); });
        _recentUsedBtn.onClick.AddListener(delegate { SetRecentUsedColor(); });

        Colorpicker.OnColorChanged += UpdateColor;
    }

    void OnDestroy()
    {
        Colorpicker.OnColorChanged -= UpdateColor;
    }

    public void UpdateSelector(bool interactable)
    {
        _borderColorTgl.interactable = interactable;
        _fillAreaColorTgl.interactable = interactable;
        _transparentBtn.interactable = interactable;
        _recentUsedBtn.interactable = interactable;
    }

    public void UpdateColorable(IColorable colorable)
    {
        _colorPicker.UpdatePicker(colorable);
    }

    public void UpdateColorMode(bool fillArea)
    {
        foreach (IColorable colorable in _colorables)
            colorable.ColorFillArea = fillArea;
    }

    public void SetColor(Color color)
    {
        foreach (IColorable colorable in _colorables)
        {
            if (_isAreaFill)
            {
                colorable.UpdateFillRendererColor(color);
            }
            else
            {
                colorable.UpdateLineRendererColor(color);
            }
        }
    }

    public void SetTransparentColor()
    {
        SetColor(_transparent);

        if (_isAreaFill)
            _transparentFillArea.enabled = true;
        else
            _transparentBorder.enabled = true;
    }

    public void SetRecentUsedColor()
    {
        SetColor(_recentUsedColor.color);
        UpdateImages(_recentUsedColor.color);
    }

    void UpdateOnColorableChanged(Color fillColor, Color borderColor)
    {
        bool currentIsAreaFill = _isAreaFill;
        IsAreaFill = true;
        if (fillColor.a == 0)
            SetTransparentColor();
        else
            UpdateColorAndImage(fillColor);

        IsAreaFill = false;
        if (borderColor.a == 0)
            SetTransparentColor();
        else
            UpdateColorAndImage(borderColor);
        IsAreaFill = currentIsAreaFill;
    }

    public void UpdateColor(Color currentColor, Color? secondColor = null)
    {
        if (secondColor != null)
            UpdateOnColorableChanged(currentColor, (Color)secondColor);
        else
        {
            UpdateColorAndImage(currentColor);
            if (currentColor != _transparent)
                _recentUsedColor.color = currentColor;
        }
    }

    void UpdateColorAndImage(Color currentColor)
    {
        SetColor(currentColor);
        UpdateImages(currentColor);
    }

    void UpdateImages(Color color)
    {
        if (_isAreaFill)
        {
            if (_transparentFillArea.enabled != false)
                _transparentFillArea.enabled = false;
            _fillAreaColor.color = color;
        }
        else
        {
            if (_transparentBorder.enabled != false)
                _transparentBorder.enabled = false;
            _borderColor.color = color;
        }
    }
}
