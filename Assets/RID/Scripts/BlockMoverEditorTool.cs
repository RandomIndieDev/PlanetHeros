using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways] // Runs in edit mode
public class BlockMoverEditorTool : MonoBehaviour
{
    [Header("Grid Settings")]
    [Tooltip("The grid step for block placement.")]
    public Vector3 m_GridSizes = Vector3.one;
    
    [Header("Rotation Settings")]
    [Tooltip("Snap rotation step in degrees (e.g., 90 = right angles).")]
    public float m_RotationStep = 90f;

    [Tooltip("Offset applied to all children.")]
    public Vector3 gridOffset = Vector3.zero;

    [Header("Move Settings")]
    [Tooltip("Automatically snap all children every frame in edit mode.")]
    public bool autoSnap = true;

#if UNITY_EDITOR
    void Update()
    {
        if (!Application.isPlaying && autoSnap)
        {
            SnapChildrenToGrid();
        }
    }

    [ContextMenu("Snap Children Now")]
    void SnapChildrenToGrid()
    {
        Undo.RegisterFullObjectHierarchyUndo(gameObject, "Snap Children to Grid");

        foreach (Transform child in transform)
        {
            Vector3 pos = child.localPosition - gridOffset;
            
            pos.x = Mathf.Round(pos.x / m_GridSizes.x) * m_GridSizes.x;
            pos.y = Mathf.Round(pos.y / m_GridSizes.y) * m_GridSizes.y;
            pos.z = Mathf.Round(pos.z / m_GridSizes.z) * m_GridSizes.z;
            
            /*
            Vector3 euler = child.localEulerAngles;
            euler.x = Mathf.Round(euler.x / m_RotationStep) * m_RotationStep;
            euler.y = Mathf.Round(euler.y / m_RotationStep) * m_RotationStep;
            euler.z = Mathf.Round(euler.z / m_RotationStep) * m_RotationStep;
            child.localRotation = Quaternion.Euler(euler);*/

            child.localPosition = pos + gridOffset;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw visual grid markers around children
        Gizmos.color = Color.cyan;
        foreach (Transform child in transform)
        {
            Gizmos.DrawWireCube(child.position, m_GridSizes * 0.9f);
        }
    }
#endif
}