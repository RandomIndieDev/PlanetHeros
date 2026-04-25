#if UNITY_EDITOR
using System;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


public class DynamicColorTypeDrawer : OdinValueDrawer<ColorType>
{
    protected override bool CanDrawValueProperty(InspectorProperty property)
    {
        return property.GetAttribute<DynamicColorDrawerAttribute>() != null;
    }
    
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var colorType = ValueEntry.SmartValue;

        // Try to find your MaterialColorManager in the scene
        var manager = Object.FindObjectOfType<MaterialColorManager>();
        Color displayColor = Color.white;

        if (manager != null && manager.TryGetColorDisplayName(colorType, out _, out var color))
            displayColor = color;

        // Add padding around the whole field block
        EditorGUILayout.BeginVertical(GUI.skin.box); // gives a nice framed background
        GUILayout.Space(2); // top padding

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(4); // left padding

        // Draw the color swatch (fixed 16x16)
        Rect rect = GUILayoutUtility.GetRect(16, 16, GUILayout.Width(16), GUILayout.Height(16));
        EditorGUI.DrawRect(rect, displayColor);
        GUILayout.Space(8); // space between color and dropdown

        // Draw the enum popup (no label)
        ValueEntry.SmartValue = (ColorType)EditorGUILayout.EnumPopup(GUIContent.none, colorType);

        GUILayout.Space(4); // right padding
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(2); // bottom padding
        EditorGUILayout.EndVertical();
    }
}
#endif