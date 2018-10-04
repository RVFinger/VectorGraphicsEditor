using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorMenuBase : MonoBehaviour
{
    GameObject _menu;

    public virtual void Init(UpdaterBase updater)
    {
        _menu = gameObject;
        ShowMenu(false);
    }

    public void ShowMenu(bool show)
    {
        if (_menu)
            _menu.SetActive(show);
    }

    public virtual void UpdateMenu(bool isInteractable = true)
    {
    }
    public virtual void UpdateMenu(UpdaterBase.SelectionMode mode)
    {
    }
    public virtual void UpdateMenu(IColorable colorables)
    {
    }
}