using UnityEngine;

public static class TransformExtensions
{
    public static Vector3 GetChildrenCenter(this Transform parent)
    {
        if (parent.childCount == 0)
            return parent.position;

        Vector3 sum = Vector3.zero;
        foreach (Transform child in parent)
        {
            sum += child.position;
        }

        return sum / parent.childCount;
    }

    public static Bounds GetBoundsOfChildren(this Transform parent)
    {
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(parent.position, Vector3.zero);
        
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds;
    }
    
    public static void RecenterToChildren(this Transform parent)
    {
        Renderer[] renderers = parent.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
            return;

        // 1. Compute world-space bounds center of children
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        Vector3 worldCenter = bounds.center;

        // 2. Compute how much to move parent in local space
        Vector3 offset = parent.InverseTransformPoint(worldCenter);

        // 3. Offset all children in opposite direction (local space)
        foreach (Transform child in parent)
        {
            child.localPosition -= offset;
        }

        // 4. Move parent pivot to that center point (world space)
        parent.position = worldCenter;
    }
}