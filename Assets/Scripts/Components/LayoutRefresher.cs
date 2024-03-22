using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayoutRefresher : MonoBehaviour
{
    void Start()
    {
        Refresh();
    }

    void OnEnable() {
        Refresh();
    }

    public void Refresh() {
        var transform = GetComponent<RectTransform>();
        if (transform != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
