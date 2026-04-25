using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class UIRaycastHelper
{
    static PointerEventData eventData = new PointerEventData(EventSystem.current);
    static List<RaycastResult> results = new List<RaycastResult>();

    public static GameObject RaycastUI(Vector2 screenPos)
    {
        eventData.position = screenPos;
        results.Clear();

        EventSystem.current.RaycastAll(eventData, results);

        if (results.Count > 0)
            return results[0].gameObject;

        return null;
    }
}