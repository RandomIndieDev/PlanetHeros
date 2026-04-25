using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIWaterFlowRangeFill : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image m_WaterImage;

    [Header("Flow")]
    [SerializeField] private float m_OpenSpeed = 1.5f;
    [SerializeField] private float m_CloseSpeed = 1.5f;

    [Header("Bucket Fill Event")]
    [SerializeField] private bool m_InvokeEventWhileReachedEnd = true;
    [SerializeField, Range(0f, 1f)] private float m_ReachedEndThreshold = 0.98f;

    [Tooltip("How much fill value to send per second while the tap is open and the stream has reached the bottom.")]
    [SerializeField] private float m_FillAmountPerSecond = 1f;

    [SerializeField] private UnityEvent<float> m_OnReachedEndTick;

    public event Action<float> OnReachedEndTick;

    [Header("Debug")]
    [SerializeField, Range(0f, 1f)] private float m_HeadProgress;
    [SerializeField, Range(0f, 1f)] private float m_TailProgress;
    [SerializeField] private bool m_IsTapOpen;

    private Material m_RuntimeMaterial;

    private static readonly int HeadProgressId = Shader.PropertyToID("_HeadProgress");
    private static readonly int TailProgressId = Shader.PropertyToID("_TailProgress");

    public bool IsTapOpen => m_IsTapOpen;
    public bool HasReachedEnd => m_HeadProgress >= m_ReachedEndThreshold;
    public float HeadProgress => m_HeadProgress;
    public float TailProgress => m_TailProgress;

    private void Awake()
    {
        InitializeMaterial();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
            return;

        ApplyMaterialValues();
    }
#endif

    private void Update()
    {
        if (m_RuntimeMaterial == null)
            return;

        if (m_IsTapOpen)
        {
            m_HeadProgress += Time.deltaTime * m_OpenSpeed;
            m_HeadProgress = Mathf.Clamp01(m_HeadProgress);

            // While tap is open, keep the top edge at the top.
            m_TailProgress = 0f;
        }
        else
        {
            m_TailProgress += Time.deltaTime * m_CloseSpeed;
            m_TailProgress = Mathf.Clamp01(m_TailProgress);

            // If stream was stopped before reaching the bottom,
            // let the visible water continue travelling downward.
            if (m_HeadProgress < 1f)
            {
                m_HeadProgress += Time.deltaTime * m_CloseSpeed;
                m_HeadProgress = Mathf.Clamp01(m_HeadProgress);
            }

            TryInvokeReachedEndTick();

            if (m_TailProgress >= m_HeadProgress)
            {
                m_TailProgress = 0f;
                m_HeadProgress = 0f;
            }

            ApplyMaterialValues();
            return;
        }

        TryInvokeReachedEndTick();
        ApplyMaterialValues();
    }

    private void TryInvokeReachedEndTick()
    {
        if (!m_InvokeEventWhileReachedEnd)
            return;

        if (!HasReachedEnd)
            return;

        // Once the head has reached the bottom, keep reporting fill
        // until the tail has also drained past the visible stream.
        if (m_TailProgress >= m_HeadProgress)
            return;

        float fillDelta = m_FillAmountPerSecond * Time.deltaTime;

        m_OnReachedEndTick?.Invoke(fillDelta);
        OnReachedEndTick?.Invoke(fillDelta);
    }

    private void InitializeMaterial()
    {
        if (m_WaterImage == null)
            m_WaterImage = GetComponent<Image>();

        if (m_WaterImage == null)
            return;

        m_RuntimeMaterial = Instantiate(m_WaterImage.material);
        m_WaterImage.material = m_RuntimeMaterial;

        ApplyMaterialValues();
    }

    private void ApplyMaterialValues()
    {
        if (m_RuntimeMaterial == null)
            return;

        m_RuntimeMaterial.SetFloat(HeadProgressId, m_HeadProgress);
        m_RuntimeMaterial.SetFloat(TailProgressId, m_TailProgress);
    }

    [Button(ButtonSizes.Large)]
    public void OpenTap()
    {
        m_IsTapOpen = true;
    }

    [Button(ButtonSizes.Large)]
    public void CloseTap()
    {
        m_IsTapOpen = false;
    }

    [Button(ButtonSizes.Medium)]
    public void ToggleTap()
    {
        if (m_IsTapOpen)
            CloseTap();
        else
            OpenTap();
    }

    [Button(ButtonSizes.Medium)]
    public void ResetFlow()
    {
        m_IsTapOpen = false;
        m_HeadProgress = 0f;
        m_TailProgress = 0f;
        ApplyMaterialValues();
    }

    [Button(ButtonSizes.Medium)]
    private void FillFully()
    {
        m_IsTapOpen = true;
        m_HeadProgress = 1f;
        m_TailProgress = 0f;
        ApplyMaterialValues();
    }

    [Button(ButtonSizes.Medium)]
    private void CloseAndDrain()
    {
        m_IsTapOpen = false;
    }
}
