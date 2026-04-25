using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(RectTransform))]
public class UIFloatEffect : MonoBehaviour
{
    [BoxGroup("Vertical Float Settings"), SerializeField] private float moveYAmplitude = 10f;
    [BoxGroup("Vertical Float Settings"), SerializeField] private float moveYSpeed = 1f;

    [BoxGroup("Horizontal Float Settings"), SerializeField] private float moveXAmplitude = 5f;
    [BoxGroup("Horizontal Float Settings"), SerializeField] private float moveXSpeed = 0.7f;

    [BoxGroup("Rotation Settings"), SerializeField] private float rotateAmplitude = 5f;
    [BoxGroup("Rotation Settings"), SerializeField] private float rotateSpeed = 1f;

    [BoxGroup("Extra Settings"), SerializeField] private bool randomStartOffset = true;
    [BoxGroup("Extra Settings"), SerializeField] private bool useIndependentOffsets = true;

    private RectTransform rectTransform;
    private Vector2 startPos;
    private float offsetY;
    private float offsetX;
    private float offsetRot;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;

        if (randomStartOffset)
        {
            offsetY = Random.Range(0f, 100f);
            offsetX = useIndependentOffsets ? Random.Range(0f, 100f) : offsetY;
            offsetRot = useIndependentOffsets ? Random.Range(0f, 100f) : offsetY;
        }
    }

    private void Update()
    {
        float t = Time.time;

        // Horizontal float (left-right)
        float newX = startPos.x + Mathf.Sin((t + offsetX) * moveXSpeed) * moveXAmplitude;

        // Vertical float (up-down)
        float newY = startPos.y + Mathf.Sin((t + offsetY) * moveYSpeed) * moveYAmplitude;

        // Gentle z rotation sway
        float newRot = Mathf.Sin((t + offsetRot) * rotateSpeed) * rotateAmplitude;

        // Apply movement and rotation
        rectTransform.anchoredPosition = new Vector2(newX, newY);
        rectTransform.localRotation = Quaternion.Euler(0, 0, newRot);
    }
}
