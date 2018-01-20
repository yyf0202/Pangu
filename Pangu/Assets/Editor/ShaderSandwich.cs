using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;

public class ShaderSandwich : EditorWindow
{
    [MenuItem("Tools/Test Window")]
    private static void Open()
    {
        EditorWindow.CreateInstance<ShaderSandwich>().Show();
    }

    Camera previewCam;

    void OnGUI()
    {
        previewCam = (Camera)EditorGUILayout.ObjectField(previewCam, typeof(Camera), true);
        if (previewCam)
        {
            Handles.DrawCamera(new Rect(10 , 20, 200, 200), previewCam, DrawCameraMode.Overdraw);
        }
    }
}