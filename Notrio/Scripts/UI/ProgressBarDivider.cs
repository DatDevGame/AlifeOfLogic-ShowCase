using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarDivider : MonoBehaviour {
    public GameObject divider;
    private List<GameObject> dividers = new List<GameObject>();
    bool resetLayout = false;
    int segments = 1;
    Color dividerColor;

    private void OnEnable()
    {
        CreateSegment(segments);
        resetLayout = false;
    }
    public void SetSegments(int segments)
    {
        this.segments = segments;
        if (gameObject.activeInHierarchy)
        {
            CreateSegment(segments);
        }
        else
        {
            resetLayout = true;
        }
    }

    private void CreateSegment(int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            GameObject d = Instantiate(divider, transform);
            d.GetComponentInChildren<Image>().color = dividerColor;
            dividers.Add(d);
        }
    }

    internal void SetColor(Color dividerColor)
    {
        this.dividerColor = dividerColor;
    }

    public void Clear()
    {
        foreach (var d in dividers)
        {
            DestroyImmediate(d);
        }
        dividers.Clear();
    }
}
