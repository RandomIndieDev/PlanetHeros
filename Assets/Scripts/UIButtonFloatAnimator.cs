using UnityEngine;
using DG.Tweening;

public class UIButtonFloatAnimator : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private RectTransform[] m_Buttons;

    [Header("Float Settings")]
    [SerializeField] private float m_FloatHeight = 20f;
    [SerializeField] private float m_FloatDuration = 1.5f;
    [SerializeField] private float m_OffsetDelay = 0.15f;
    [SerializeField] private Ease m_FloatEase = Ease.InOutSine;

    [Header("Tilt Settings")]
    [SerializeField] private bool m_EnableTilt = true;
    [SerializeField] private float m_TiltAmount = 4f;
    [SerializeField] private float m_TiltDuration = 1.5f;

    [Header("Scale Pulse")]
    [SerializeField] private bool m_EnableScalePulse = false;
    [SerializeField] private float m_ScaleAmount = 1.05f;
    [SerializeField] private float m_ScaleDuration = 1.5f;

    private Vector2[] m_StartPositions;
    private Vector3[] m_StartRotations;
    private Vector3[] m_StartScales;
    private Sequence[] m_Sequences;

    private void OnEnable()
    {
        CacheStartValues();
        Play();
    }

    private void OnDisable()
    {
        Stop();
    }

    private void CacheStartValues()
    {
        m_StartPositions = new Vector2[m_Buttons.Length];
        m_StartRotations = new Vector3[m_Buttons.Length];
        m_StartScales = new Vector3[m_Buttons.Length];
        m_Sequences = new Sequence[m_Buttons.Length];

        for (int i = 0; i < m_Buttons.Length; i++)
        {
            if (m_Buttons[i] == null)
                continue;

            m_StartPositions[i] = m_Buttons[i].anchoredPosition;
            m_StartRotations[i] = m_Buttons[i].localEulerAngles;
            m_StartScales[i] = m_Buttons[i].localScale;
        }
    }

    public void Play()
    {
        Stop();

        for (int i = 0; i < m_Buttons.Length; i++)
        {
            RectTransform button = m_Buttons[i];

            if (button == null)
                continue;

            button.anchoredPosition = m_StartPositions[i];
            button.localEulerAngles = m_StartRotations[i];
            button.localScale = m_StartScales[i];

            float phase = i / (float)m_Buttons.Length;
            float phaseTime = phase * m_FloatDuration;

            Tween floatTween = button
                .DOAnchorPosY(m_StartPositions[i].y + m_FloatHeight, m_FloatDuration)
                .SetEase(m_FloatEase)
                .SetLoops(-1, LoopType.Yoyo);

            floatTween.Goto(phaseTime, true);

            if (m_EnableTilt)
            {
                Tween tiltTween = button
                    .DOLocalRotate(new Vector3(0f, 0f, m_TiltAmount), m_TiltDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);

                tiltTween.Goto(phaseTime, true);
            }

            if (m_EnableScalePulse)
            {
                Tween scaleTween = button
                    .DOScale(m_StartScales[i] * m_ScaleAmount, m_ScaleDuration)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);

                scaleTween.Goto(phaseTime, true);
            }
        }
    }

    public void Stop()
    {
        if (m_Sequences != null)
        {
            for (int i = 0; i < m_Sequences.Length; i++)
            {
                m_Sequences[i]?.Kill();
            }
        }

        if (m_Buttons == null || m_StartPositions == null)
            return;

        for (int i = 0; i < m_Buttons.Length; i++)
        {
            if (m_Buttons[i] == null)
                continue;

            m_Buttons[i].DOKill();

            m_Buttons[i].anchoredPosition = m_StartPositions[i];
            m_Buttons[i].localEulerAngles = m_StartRotations[i];
            m_Buttons[i].localScale = m_StartScales[i];
        }
    }
}