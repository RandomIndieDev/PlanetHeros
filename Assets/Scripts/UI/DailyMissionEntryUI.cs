using BeautifulTransitions.Scripts.Transitions.Components;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class DailyMissionEntryUI : MonoBehaviour
{
    [SerializeField] private string m_MissionId;
    [SerializeField] private GameObject m_TickImage;
    [SerializeField] private TextMeshProUGUI m_MissionStatusText;
    
    [SerializeField] public TransitionBase m_TransitionBase;
    
    
    [SerializeField] private string m_MissionCompletedText = "Mission Completed";
    [SerializeField] private string m_MissionUnCompletedText = "Start Mission";
    [SerializeField] private float m_CompletionRevealDelay = 0.2f;
    [SerializeField] private float m_CompletionPunchDuration = 0.25f;
    [SerializeField] private Vector3 m_CompletionPunchScale = new Vector3(0.2f, 0.2f, 0f);
    [SerializeField] private int m_CompletionPunchVibrato = 8;
    [SerializeField] private float m_CompletionPunchElasticity = 0.9f;

    public string MissionId => m_MissionId;
    
    private Tween m_CompletionRevealTween;
    private Tween m_TickPunchTween;
    private Vector3 m_TickInitialScale = Vector3.one;

    public void Initialize()
    {
        if (m_TickImage != null)
        {
            m_TickInitialScale = m_TickImage.transform.localScale;
        }

        Refresh();
    }

    public void SetTransitionDelay(float delay, bool applyToTransitionOut = false)
    {
        if (m_TransitionBase == null)
        {
            return;
        }

        m_TransitionBase.TransitionInConfig.Delay = delay;

        if (applyToTransitionOut)
        {
            m_TransitionBase.TransitionOutConfig.Delay = delay;
        }
    }

    private void OnEnable()
    {
        DailyMissionRuntime.MissionStatusChanged += HandleMissionStatusChanged;
        Refresh();
    }

    private void OnDisable()
    {
        DailyMissionRuntime.MissionStatusChanged -= HandleMissionStatusChanged;
    }

    public void Refresh()
    {
        bool isCompleted = DailyMissionRuntime.IsCompleted(m_MissionId);
        ApplyState(isCompleted);
    }

    public void RefreshForScreenOpen()
    {
        bool isCompleted = DailyMissionRuntime.IsCompleted(m_MissionId);

        if (!isCompleted)
        {
            ApplyState(false);
            return;
        }

        if (DailyMissionRuntime.HasPresentedCompletion(m_MissionId))
        {
            ApplyState(true);
            return;
        }

        PlayCompletionReveal();
    }

    public void MarkCompleted()
    {
        DailyMissionRuntime.CompleteMission(m_MissionId);
    }

    private void HandleMissionStatusChanged(string missionId, bool isCompleted)
    {
        if (missionId != m_MissionId)
        {
            return;
        }

        ApplyState(isCompleted);
    }

    private void ApplyState(bool isCompleted)
    {
        if (m_TickImage != null)
        {
            m_TickImage.SetActive(isCompleted);
            m_TickImage.transform.localScale = m_TickInitialScale;
        }

        if (m_MissionStatusText != null)
        {
            m_MissionStatusText.text = isCompleted ? m_MissionCompletedText : m_MissionUnCompletedText;
        }
    }

    private void PlayCompletionReveal()
    {
        if (m_CompletionRevealTween != null && m_CompletionRevealTween.IsActive())
        {
            m_CompletionRevealTween.Kill();
        }

        if (m_TickPunchTween != null && m_TickPunchTween.IsActive())
        {
            m_TickPunchTween.Kill();
        }

        if (m_TickImage != null)
        {
            m_TickImage.SetActive(false);
            m_TickImage.transform.localScale = m_TickInitialScale;
        }

        if (m_MissionStatusText != null)
        {
            m_MissionStatusText.text = m_MissionCompletedText;
        }

        m_CompletionRevealTween = DOVirtual.DelayedCall(m_CompletionRevealDelay, () =>
        {
            if (m_TickImage != null)
            {
                m_TickImage.SetActive(true);
                m_TickImage.transform.localScale = m_TickInitialScale;
                m_TickPunchTween = m_TickImage.transform.DOPunchScale(
                    m_CompletionPunchScale,
                    m_CompletionPunchDuration,
                    m_CompletionPunchVibrato,
                    m_CompletionPunchElasticity);
            }

            DailyMissionRuntime.MarkCompletionPresented(m_MissionId);
        });
    }

    private void OnDestroy()
    {
        if (m_CompletionRevealTween != null && m_CompletionRevealTween.IsActive())
        {
            m_CompletionRevealTween.Kill();
        }

        if (m_TickPunchTween != null && m_TickPunchTween.IsActive())
        {
            m_TickPunchTween.Kill();
        }
    }

}
