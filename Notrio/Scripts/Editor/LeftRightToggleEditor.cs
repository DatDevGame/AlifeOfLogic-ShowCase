using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Takuzu.Generator;
using System.IO;

namespace Takuzu
{
    [CustomEditor(typeof(LeftRightToggle))]
    public class LeftRightToggleEditor : Editor
    {
        //LeftRightToggle instance;

        //public void OnEnable()
        //{
        //    instance = (LeftRightToggle)target;
        //}

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}