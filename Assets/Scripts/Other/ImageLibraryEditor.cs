#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ImageLibrary))]
public class ImageLibraryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        ImageLibrary ImageLibrarySc = (ImageLibrary)target;

        if (GUILayout.Button("SetResIcons"))
        {
            ImageLibrarySc.SetResIcons();
        }
    }
}
#endif