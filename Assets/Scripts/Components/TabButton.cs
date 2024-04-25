﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

[RequireComponent(typeof(Image))]
public class TabButton : MonoBehaviour, IPointerClickHandler
{
    public TabGroup tabGroup;
    public UnityEvent onTabSelected;
    public UnityEvent onTabDeselected;

    [HideInInspector]
    public Image background;

    void Start()
    {
        background = GetComponent<Image>();
        if (tabGroup != null)
        {
            tabGroup.Subscribe(this);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }
    
    public void Select()
    {
        if (onTabSelected != null)
        {
            onTabSelected.Invoke();
        }
    }

    public void Deselect()
    {
        if (onTabDeselected != null)
        {
            onTabSelected.Invoke();
        }

    }


}