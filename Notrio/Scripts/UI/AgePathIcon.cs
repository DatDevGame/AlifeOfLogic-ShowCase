using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgePathIcon : MonoBehaviour {
    public const string resourcesPath = "ageicon/";

    public static Sprite Get(int index, bool isActive)
    {
        Sprite icon = Resources.Load<Sprite>(resourcesPath + "ageicon-" + index.ToString() + (isActive ? "-on" : "-off"));
        if (icon == null)
            icon = Resources.Load<Sprite>(resourcesPath + "ageicon-" + "default" + (isActive ? "-on" : "-off"));
        return icon;
    }

}
