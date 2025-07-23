using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Takuzu.Generator;
using System.IO;

namespace Takuzu
{
    [CustomEditor(typeof(ExtrudedButton))]
    public class ExtrudedButtonEditor : Editor
    {
        //ExtrudedButton instance;

        //public void OnEnable()
        //{
        //    instance = (ExtrudedButton)target;
        //}

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}