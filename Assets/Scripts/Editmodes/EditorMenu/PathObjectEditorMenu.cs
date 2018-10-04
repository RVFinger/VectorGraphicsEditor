using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PathObjectEditorMenu : EditorMenuBase
{
    [Header("PathThickness")]
    [SerializeField] InputField _pathThickness;

    [Header("Layout Order Options")]
    [SerializeField] ToggleGroup _orderGRP;
    [SerializeField] Toggle _top;
    [SerializeField] Toggle _bottom;
    [SerializeField] Button _upwards;
    [SerializeField] Button _downwards;

    [Header("ColorSelector")]
    [SerializeField] ColorSelector _colorSelector;

    private PathObjectUpdater _updater;
    private bool _isToggleClicked = false;

    private void Start()
    {
        _pathThickness.onEndEdit.AddListener(delegate { UpdatePathThickness(_pathThickness); });

        _top.onValueChanged.AddListener(delegate { SetSiblingIndex(true); });
        _bottom.onValueChanged.AddListener(delegate { SetSiblingIndex(false); });
        _upwards.onClick.AddListener(delegate { ChangeSiblingIndex(true); });
        _downwards.onClick.AddListener(delegate { ChangeSiblingIndex(false); });
    }

    public override void Init(UpdaterBase updater)
    {
        base.Init(updater);
        _updater = updater as PathObjectUpdater;
        _updater.SetEditorMenu(this);
        _updater.SynchronizeColorablesList();
    }

    public void SynchronizeColorablesList(List<IColorable> colorables)
    {
        _colorSelector.SynchronizeColorablesList(colorables);
    }

    public override void UpdateMenu(IColorable colorables)
    {
        _colorSelector.UpdateColorable(colorables);
    }
    public override void UpdateMenu(UpdaterBase.SelectionMode selectionMode)
    {
        bool singleSelection = selectionMode == UpdaterBase.SelectionMode.SingleObject;
        bool noSelection = selectionMode == UpdaterBase.SelectionMode.NoObject;

        _pathThickness.interactable = !noSelection;

        _top.interactable = singleSelection;
        _bottom.interactable = singleSelection;
        _upwards.interactable = singleSelection;
        _downwards.interactable = singleSelection;

        _isToggleClicked = false;

        if (noSelection)
            _orderGRP.SetAllTogglesOff();
        else
        {
            _pathThickness.text = _updater.GetPathThickness();
            UpdateLayoutOrderTglGrp();
        }

        _isToggleClicked = true;

        _colorSelector.UpdateSelector(!noSelection);
    }

    void UpdatePathThickness(InputField input)
    {
        _updater.SetPathThickness(float.Parse(input.text));
    }

    void ChangeSiblingIndex(bool increase)
    {
        if (_isToggleClicked)
            _updater.ChangeSiblingIndex(increase);
    }

    void SetSiblingIndex(bool isFirst)
    {
        if (_isToggleClicked)
            _updater.SetSiblingIndex(isFirst);
    }

    void UpdateLayoutOrderTglGrp()
    {
        int siblingsCount;
        int index = _updater.GetSiblingIndex(out siblingsCount);
        if (index == siblingsCount - 1)
            _top.isOn = true;

        else if (index == 0)
            _bottom.isOn = true;

        else
        {
            _top.isOn = false;
            _bottom.isOn = false;
        }
    }
}
