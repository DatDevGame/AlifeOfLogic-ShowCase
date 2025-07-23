using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class WindowMenuItemAttribute : Attribute
{
    public enum Mode
    {
        Single,
        Multiple
    }

    public Mode mode { get; private set; }
    public string menuName { get; private set; }
    public string menuItemPath { get; private set; }
    public string assetFolderPath { get; private set; }
    public bool includeSubDirectories { get; private set; }
    public bool flattenSubDirectories { get; private set; }
    public bool sortByName { get; private set; }
    public int order { get; private set; }

    public WindowMenuItemAttribute(string menuItemPath, string menuName = "", string assetFolderPath = "Assets", Mode mode = Mode.Single, bool includeSubDirectories = true, bool flattenSubDirectories = false, bool sortByName = false, int order = 0)
    {
        this.mode = mode;
        this.menuName = menuName;
        this.menuItemPath = menuItemPath;
        this.assetFolderPath = assetFolderPath;
        this.includeSubDirectories = includeSubDirectories;
        this.flattenSubDirectories = flattenSubDirectories;
        this.sortByName = sortByName;
        this.order = order;
    }
}
