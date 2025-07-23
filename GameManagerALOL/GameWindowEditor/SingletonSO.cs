using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class SingletonSO<T> : SerializedScriptableObject where T : SerializedScriptableObject
{
    private static T s_Instance;
    public static T Instance
    {
        get
        {
            try
            {
                if (s_Instance == null)
                    s_Instance = Resources.LoadAll<T>("")[0];
            }
            catch (System.Exception exc)
            {
                Debug.LogException(exc);
            }
            return s_Instance;
        }
    }
}