using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[AddComponentMenu("UI/Editor Horizontal Spacer")]
public class HorizontalSpacer : MonoBehaviour
{
    [Header("Layout Settings")]
    [Min(0f)] public float spacing = 1f;
    public bool centerAlign = false;
    public bool autoUpdate = true;
    public bool useLocalSpace = true;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (autoUpdate)
            UpdateSpacing();
    }

    private void Update()
    {
        if (!Application.isPlaying && autoUpdate)
            UpdateSpacing();
    }
#endif

    [ContextMenu("Update Spacing")]
    public void UpdateSpacing()
    {
        if (transform.childCount == 0)
            return;

        // Collect active children
        var children = new System.Collections.Generic.List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
                children.Add(child);
        }

        if (children.Count == 0)
            return;

        // Compute total width
        float totalWidth = spacing * (children.Count - 1);

        // Starting offset for centering
        float startX = 0f;
        if (centerAlign)
            startX = -totalWidth / 2f;

        // Position children
        float x = startX;
        foreach (var child in children)
        {
            if (useLocalSpace)
                child.localPosition = new Vector3(x, child.localPosition.y, child.localPosition.z);
            else
                child.position = new Vector3(x, child.position.y, child.position.z);

            x += spacing;
        }

#if UNITY_EDITOR
        // Mark scene dirty so edits persist
        if (!Application.isPlaying)
            EditorUtility.SetDirty(this);
#endif
    }
}