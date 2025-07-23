#if UNITY_EDITOR

using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameWindowEditor : OdinMenuEditorWindow
{
    public static GameWindowEditor Instance;
    private void OnEnable()
    {
        Instance = this;
    }

    [MenuItem("ALOL/Open Window Editor", priority = -100)]
    private static void OpenWindow()
    {
        var window = GetWindow<GameWindowEditor>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1000, 700);
    }


    protected override void OnBeginDrawEditors()
    {
        if (MenuTree == null)
            return;
        var selected = MenuTree.Selection.FirstOrDefault();
        var toolbarHeight = MenuTree.Config.SearchToolbarHeight;

        // Draws a toolbar with the name of the currently selected menu item.
        SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
        {
            if (selected != null)
            {
                GUILayout.Label(selected.Name);
            }

            if (SirenixEditorGUI.ToolbarButton(new GUIContent("Delete All Data")))
            {
                DeleteAllData();
            }
        }
        SirenixEditorGUI.EndHorizontalToolbar();
    }

    protected static void DeleteAllData()
    {
        if (EditorUtility.DisplayDialog("Delete All Data!!!", "Are you sure about that, bruh???", "Ok", "Cancel"))
        {
            // Clear ES3 data
            DirectoryInfo di = new DirectoryInfo(Application.persistentDataPath);

            foreach (FileInfo file in di.GetFiles())
                file.Delete();
            foreach (DirectoryInfo dir in di.GetDirectories())
                dir.Delete(true);

            // Clear PPref data
            PlayerPrefs.DeleteAll();
            Debug.Log("Delete data successfully!");
        }
    }

    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree(true);
        tree.DefaultMenuStyle.IconSize = 28.00f;
        tree.Config.DrawSearchToolbar = true;

        var typesWithMenuItemAttribute = TypeCache.GetTypesWithAttribute<WindowMenuItemAttribute>()
            .Where(item => item.InheritsFrom<ScriptableObject>())
            .ToList();

        typesWithMenuItemAttribute.Sort((x, y) =>
            x.GetAttribute<WindowMenuItemAttribute>().order.CompareTo(y.GetAttribute<WindowMenuItemAttribute>().order));

        foreach (var type in typesWithMenuItemAttribute)
        {
            var attribute = type.GetAttribute<WindowMenuItemAttribute>();

            if (attribute.mode == WindowMenuItemAttribute.Mode.Single)
            {
                var data = EditorUtils.FindAssetOfType(type, attribute.assetFolderPath);
                if (data == null) continue;

                var menuItem = new OdinMenuItem(tree, string.IsNullOrEmpty(attribute.menuName) ? data.name : attribute.menuName, data);
                tree.AddMenuItemAtPath(attribute.menuItemPath, menuItem);
            }
            else
            {
                var menuItemIterator = tree.AddAllAssetsAtPath(
                    attribute.menuItemPath,
                    attribute.assetFolderPath,
                    type,
                    attribute.includeSubDirectories,
                    attribute.flattenSubDirectories
                );

                if (attribute.sortByName)
                    menuItemIterator.SortMenuItemsByName();
            }
        }

        return tree;
    }

}
#endif





