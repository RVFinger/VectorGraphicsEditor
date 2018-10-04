using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HandlesPointsEditorMenu : EditorMenuBase
{
    [Header("Conjunct Options")]
    [SerializeField] ToggleGroup _conjunctGRP;
    [SerializeField] Toggle _conjunct;
    [SerializeField] Toggle _notConjunct;

    [Header("Handle Options")]
    [SerializeField] Button _handles;
    [SerializeField] Button _noHandles;
    [SerializeField] Button _leftHandle;
    [SerializeField] Button _rightHandle;

    private HandlesPointsUpdater _updater;

    private bool _isToggleClicked = true;

    private void Start()
    {
        _conjunct.onValueChanged.AddListener((value) => {   
            SetConjunct(true);
        });
        _notConjunct.onValueChanged.AddListener((value) => {   
            SetConjunct(false);
        });
        _handles.onClick.AddListener(delegate { Handles(); });
        _noHandles.onClick.AddListener(delegate { NoHandle(); });
        _leftHandle.onClick.AddListener(delegate { OneHandle(true); });
        _rightHandle.onClick.AddListener(delegate { OneHandle(false); });
    }

    public override void Init(UpdaterBase updater)
    {
        base.Init(updater);
        _updater = updater as HandlesPointsUpdater;
        _updater.SetEditorMenu(this);
    }

    public void SetInteractive(bool isInteractable = true)
    {
        _conjunct.interactable = isInteractable;
        _notConjunct.interactable = isInteractable;

        _handles.interactable = isInteractable;
        _noHandles.interactable = isInteractable;
        _leftHandle.interactable = isInteractable;
        _rightHandle.interactable = isInteractable;
    }

    public override void UpdateMenu(UpdaterBase.SelectionMode mode = UpdaterBase.SelectionMode.NoSelection)
    {
        SetInteractive(mode != UpdaterBase.SelectionMode.NoSelection);

        bool isOn = mode == UpdaterBase.SelectionMode.NoSelection || mode == UpdaterBase.SelectionMode.MultiplePoints;
        _isToggleClicked = false;

        if (isOn)
        {
            _conjunctGRP.SetAllTogglesOff();
        }
        else
        {
            _conjunct.isOn = _updater.Conjunct();
            _notConjunct.isOn = !_updater.Conjunct();
        }
        _isToggleClicked = true;
    }

    public void SetConjunct(bool isConjunct)
    {
        if(_isToggleClicked)
        {
            _updater.SetConjunct(isConjunct);
        }

    }

    public void OneHandle(bool isSecondHandle)
    {
        _updater.OneHandle(isSecondHandle);
    }

    public void NoHandle()
    {
        _updater.NoHandle();
    }

    public void Handles()
    {
        _updater.Handles();
    }
}
