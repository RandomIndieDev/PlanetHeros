using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayScreen : BaseUIScreen
{
    private const string WaterHeroMissionId = "water_hero";
    private const int WinCurrencyReward = 40;

    [SerializeField] private BasicButton m_BackButton;
    [SerializeField] private Button m_TapButton;
    [SerializeField] private UIWaterFlowRangeFill m_WaterFlowRangeFill;
    [SerializeField] private Image m_WaterFillImage;
    [SerializeField] private Image m_RemainingClosesPrompt;
    [SerializeField] private TextMeshProUGUI m_RemainingClosesText;
    [SerializeField] private WinPanelUI m_WinPanelUI;
    

    [Header("Gameplay Timing")]
    [SerializeField] private float m_MinTapStartDelay = 0.5f;
    [SerializeField] private float m_MaxTapStartDelay = 2f;

    [Header("Tap Animation")]
    [SerializeField] private RectTransform m_TapVisual;
    [SerializeField] private float m_TapPunchDuration = 0.2f;
    [SerializeField] private Vector3 m_TapPunchScale = new Vector3(0.2f, 0.2f, 0f);
    [SerializeField] private int m_TapPunchVibrato = 8;
    [SerializeField] private float m_TapPunchElasticity = 0.8f;

    [Header("Close Counter")]
    [SerializeField] private int m_RequiredTapCloses = 5;
    [SerializeField] private float m_GreatJobDisplayDuration = 0.75f;
    [SerializeField] private float m_PromptHideDuration = 0.25f;
    [SerializeField] private Vector3 m_PromptPunchScale = new Vector3(0.12f, 0.12f, 0f);
    [SerializeField] private float m_PromptPunchDuration = 0.2f;
    [SerializeField] private int m_PromptPunchVibrato = 8;
    [SerializeField] private float m_PromptPunchElasticity = 0.8f;

    [Header("Sink Fill")]
    [SerializeField, Range(0f, 1f)] private float m_FillStartThreshold = 0.98f;
    [SerializeField] private float m_SinkFillMultiplier = 1f;
    [SerializeField] private float m_SinkFillPerSecondMultiplier = 1f;
    [SerializeField] private float m_StartSinkFillAmount = 0f;

    private Coroutine m_TapStartRoutine;
    private bool m_IsGameRunning;
    private bool m_IsGameOver;
    private Tween m_TapPunchTween;
    private Tween m_PromptPunchTween;
    private Tween m_PromptHideTween;
    private Tween m_WinRevealTween;
    private int m_RemainingTapCloses;
    private Vector3 m_RemainingClosesPromptInitialScale = Vector3.one;
    private Vector3 m_RemainingClosesTextInitialScale = Vector3.one;
    private bool m_HasGrantedWinRewards;
    
    
    public override void Init(UIManager manager)
    {
        base.Init(manager);
        
        m_BackButton.Subscribe(() =>
        {
            m_ScreenManager.OpenScreen(ScreenName.GameSelectionScreen);
        });

        if (m_WinPanelUI != null)
        {
            m_WinPanelUI.Initialize(HandlePanelBackPressed, RetryGameplay);
        }

        if (m_TapButton != null)
        {
            m_TapButton.onClick.AddListener(HandleTapButtonClicked);
        }

        if (m_TapVisual == null && m_TapButton != null)
        {
            m_TapVisual = m_TapButton.transform as RectTransform;
        }

        if (m_WaterFlowRangeFill != null)
        {
            m_WaterFlowRangeFill.OnReachedEndTick += HandleReachedEndTick;
        }

        if (m_RemainingClosesPrompt != null)
        {
            m_RemainingClosesPromptInitialScale = m_RemainingClosesPrompt.rectTransform.localScale;
        }

        if (m_RemainingClosesText != null)
        {
            m_RemainingClosesTextInitialScale = m_RemainingClosesText.rectTransform.localScale;
        }
    }

    public override void OnOpen(object data = null)
    {
        base.OnOpen(data);
        StartGameplay();
    }

    public override void OnClose(System.Action OnComplete = null)
    {
        StopGameplayLoop();
        base.OnClose(OnComplete);
    }

    private void StartGameplay()
    {
        m_IsGameRunning = true;
        m_IsGameOver = false;
        m_HasGrantedWinRewards = false;
        m_RemainingTapCloses = Mathf.Max(0, m_RequiredTapCloses);

        if (m_WinRevealTween != null && m_WinRevealTween.IsActive())
        {
            m_WinRevealTween.Kill();
        }

        if (m_WinPanelUI != null)
        {
            m_WinPanelUI.HideAll();
        }

        ResetRemainingClosesPromptVisuals();

        if (m_WaterFillImage != null)
        {
            m_WaterFillImage.fillAmount = Mathf.Clamp01(m_StartSinkFillAmount);
        }

        if (m_WaterFlowRangeFill != null)
        {
            m_WaterFlowRangeFill.ResetFlow();
        }

        RefreshRemainingClosesText();
        ScheduleNextTapStart();
    }

    private void StopGameplayLoop()
    {
        m_IsGameRunning = false;

        if (m_TapStartRoutine != null)
        {
            StopCoroutine(m_TapStartRoutine);
            m_TapStartRoutine = null;
        }

        if (m_WaterFlowRangeFill != null)
        {
            m_WaterFlowRangeFill.CloseTap();
            m_WaterFlowRangeFill.ResetFlow();
        }

        if (m_WinRevealTween != null && m_WinRevealTween.IsActive())
        {
            m_WinRevealTween.Kill();
        }
    }

    private void HandleTapButtonClicked()
    {
        if (!m_IsGameRunning || m_IsGameOver || m_WaterFlowRangeFill == null)
        {
            return;
        }

        if (!m_WaterFlowRangeFill.IsTapOpen)
        {
            return;
        }
        
        m_AudioManager.PlayOneShot(SoundType.dino_Show_Happy);

        m_WaterFlowRangeFill.CloseTap();
        RegisterSuccessfulTapClose();
        PlayTapPunchAnimation();

        if (m_RemainingTapCloses > 0)
        {
            ScheduleNextTapStart();
        }
    }

    private void ScheduleNextTapStart()
    {
        if (!m_IsGameRunning || m_IsGameOver || m_WaterFlowRangeFill == null)
        {
            return;
        }
        
        m_AudioManager.PlayOneShot(SoundType.dino_Show_Happy);
        
        if (m_TapStartRoutine != null)
        {
            StopCoroutine(m_TapStartRoutine);
        }

        m_TapStartRoutine = StartCoroutine(StartTapAfterRandomDelay());
    }

    private System.Collections.IEnumerator StartTapAfterRandomDelay()
    {
        float delay = Random.Range(m_MinTapStartDelay, m_MaxTapStartDelay);
        yield return new WaitForSeconds(delay);

        m_TapStartRoutine = null;

        if (!m_IsGameRunning || m_IsGameOver || m_WaterFlowRangeFill == null)
        {
            yield break;
        }

        m_WaterFlowRangeFill.OpenTap();
        PlayTapPunchAnimation();
    }

    private void HandleReachedEndTick(float fillAmount)
    {
        if (!m_IsGameRunning || m_IsGameOver || m_WaterFillImage == null)
        {
            return;
        }

        if (m_WaterFlowRangeFill != null && m_WaterFlowRangeFill.HeadProgress < m_FillStartThreshold)
        {
            return;
        }
        
        float sinkFillDelta = fillAmount * m_SinkFillMultiplier * m_SinkFillPerSecondMultiplier;
        m_WaterFillImage.fillAmount = Mathf.Clamp01(m_WaterFillImage.fillAmount + sinkFillDelta);

        if (m_WaterFillImage.fillAmount >= 1f)
        {
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        if (m_IsGameOver)
        {
            return;
        }

        m_IsGameOver = true;
        m_IsGameRunning = false;

        if (m_TapStartRoutine != null)
        {
            StopCoroutine(m_TapStartRoutine);
            m_TapStartRoutine = null;
        }

        if (m_WaterFlowRangeFill != null)
        {
            m_WaterFlowRangeFill.CloseTap();
        }

        GameOver();
    }

    private void GameOver()
    {
        Debug.Log("Game Over");

        if (m_WinPanelUI != null)
        {
            m_WinPanelUI.ShowLosePanel();
        }
    }

    private void RegisterSuccessfulTapClose()
    {
        if (m_RemainingTapCloses <= 0)
        {
            return;
        }

        m_RemainingTapCloses--;
        RefreshRemainingClosesText();

        if (m_RemainingTapCloses <= 0)
        {
            CompleteCloseCounterGoal();
        }
    }

    private void RefreshRemainingClosesText()
    {
        if (m_RemainingClosesText == null)
        {
            return;
        }

        if (m_RemainingClosesPrompt != null)
        {
            m_RemainingClosesPrompt.gameObject.SetActive(true);
        }

        if (m_RemainingTapCloses <= 0)
        {
            m_RemainingClosesText.text = "Great job!";
            PlayRemainingClosesPunchAnimation();
            return;
        }

        string timeWord = m_RemainingTapCloses == 1 ? "time" : "times";
        m_RemainingClosesText.text = $"Close it {m_RemainingTapCloses} more {timeWord}!";
        PlayRemainingClosesPunchAnimation();
    }

    private void CompleteCloseCounterGoal()
    {
        m_IsGameRunning = false;
        GrantWinRewards();

        if (m_TapStartRoutine != null)
        {
            StopCoroutine(m_TapStartRoutine);
            m_TapStartRoutine = null;
        }

        if (m_WaterFlowRangeFill != null)
        {
            m_WaterFlowRangeFill.CloseTap();
        }

        HideRemainingClosesPrompt();
        RevealWinPanelAfterPrompt();
    }

    private void GrantWinRewards()
    {
        if (m_HasGrantedWinRewards)
        {
            return;
        }

        m_HasGrantedWinRewards = true;
        DailyMissionRuntime.CompleteMission(WaterHeroMissionId);
        DailyCurrencyRuntime.AddCurrency(WinCurrencyReward);
    }

    private void HideRemainingClosesPrompt()
    {
        if (m_RemainingClosesPrompt == null)
        {
            return;
        }

        if (m_PromptHideTween != null && m_PromptHideTween.IsActive())
        {
            m_PromptHideTween.Kill();
        }

        RectTransform promptTransform = m_RemainingClosesPrompt.rectTransform;
        promptTransform.localScale = m_RemainingClosesPromptInitialScale;

        Sequence hideSequence = DOTween.Sequence();
        hideSequence.AppendInterval(m_GreatJobDisplayDuration);
        hideSequence.Append(promptTransform
            .DOScale(Vector3.zero, m_PromptHideDuration)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                m_RemainingClosesPrompt.gameObject.SetActive(false);
            }));

        if (m_RemainingClosesText != null)
        {
            hideSequence.Join(
                m_RemainingClosesText.rectTransform
                    .DOScale(Vector3.zero, m_PromptHideDuration)
                    .SetEase(Ease.InBack)
                    .SetDelay(m_GreatJobDisplayDuration));
        }

        m_PromptHideTween = hideSequence;
    }

    private void RevealWinPanelAfterPrompt()
    {
        if (m_WinPanelUI == null)
        {
            return;
        }

        if (m_WinRevealTween != null && m_WinRevealTween.IsActive())
        {
            m_WinRevealTween.Kill();
        }

        float revealDelay = m_GreatJobDisplayDuration + m_PromptHideDuration;
        m_WinRevealTween = DOVirtual.DelayedCall(revealDelay, () =>
        {
            m_WinPanelUI.ShowWinPanel();
        });
    }

    private void RetryGameplay()
    {
        StartGameplay();
    }

    private void HandlePanelBackPressed()
    {
        m_ScreenManager.OpenScreen(ScreenName.DailyMission);
    }

    private void ResetRemainingClosesPromptVisuals()
    {
        if (m_PromptHideTween != null && m_PromptHideTween.IsActive())
        {
            m_PromptHideTween.Kill();
        }

        if (m_PromptPunchTween != null && m_PromptPunchTween.IsActive())
        {
            m_PromptPunchTween.Kill();
        }

        if (m_RemainingClosesPrompt != null)
        {
            m_RemainingClosesPrompt.gameObject.SetActive(true);
            m_RemainingClosesPrompt.rectTransform.localScale = m_RemainingClosesPromptInitialScale;
        }

        if (m_RemainingClosesText != null)
        {
            m_RemainingClosesText.gameObject.SetActive(true);
            m_RemainingClosesText.rectTransform.localScale = m_RemainingClosesTextInitialScale;
        }
    }

    private void PlayRemainingClosesPunchAnimation()
    {
        if (m_RemainingClosesText == null)
        {
            return;
        }

        if (m_PromptPunchTween != null && m_PromptPunchTween.IsActive())
        {
            m_PromptPunchTween.Kill();
        }

        Sequence punchSequence = DOTween.Sequence();

        if (m_RemainingClosesPrompt != null)
        {
            m_RemainingClosesPrompt.rectTransform.localScale = m_RemainingClosesPromptInitialScale;
            punchSequence.Join(
                m_RemainingClosesPrompt.rectTransform.DOPunchScale(
                    m_PromptPunchScale,
                    m_PromptPunchDuration,
                    m_PromptPunchVibrato,
                    m_PromptPunchElasticity));
        }

        m_RemainingClosesText.rectTransform.localScale = m_RemainingClosesTextInitialScale;
        punchSequence.Join(
            m_RemainingClosesText.rectTransform.DOPunchScale(
                m_PromptPunchScale,
                m_PromptPunchDuration,
                m_PromptPunchVibrato,
                m_PromptPunchElasticity));

        m_PromptPunchTween = punchSequence;
    }

    private void PlayTapPunchAnimation()
    {
        if (m_TapVisual == null)
        {
            return;
        }

        if (m_TapPunchTween != null && m_TapPunchTween.IsActive())
        {
            m_TapPunchTween.Kill();
        }

        m_TapVisual.localScale = Vector3.one;
        m_TapPunchTween = m_TapVisual.DOPunchScale(
            m_TapPunchScale,
            m_TapPunchDuration,
            m_TapPunchVibrato,
            m_TapPunchElasticity);
    }

    private void OnDestroy()
    {
        if (m_TapButton != null)
        {
            m_TapButton.onClick.RemoveListener(HandleTapButtonClicked);
        }

        if (m_WaterFlowRangeFill != null)
        {
            m_WaterFlowRangeFill.OnReachedEndTick -= HandleReachedEndTick;
        }

        if (m_TapPunchTween != null && m_TapPunchTween.IsActive())
        {
            m_TapPunchTween.Kill();
        }

        if (m_PromptPunchTween != null && m_PromptPunchTween.IsActive())
        {
            m_PromptPunchTween.Kill();
        }

        if (m_PromptHideTween != null && m_PromptHideTween.IsActive())
        {
            m_PromptHideTween.Kill();
        }

        if (m_WinRevealTween != null && m_WinRevealTween.IsActive())
        {
            m_WinRevealTween.Kill();
        }
    }
}
