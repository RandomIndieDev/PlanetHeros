using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DailyMissionScreen : BaseUIScreen
{
    [SerializeField] public List<DailyMissionEntryUI> m_DailyMissionEntries = new List<DailyMissionEntryUI>();
    [SerializeField] public TextMeshProUGUI m_CurrentCurrencyCount;
    [SerializeField] private bool m_ResetMissionsOnAwake;
    [SerializeField] private float m_TransitionDelayStep = 0.1f;
    [SerializeField] private bool m_ApplyDelayToTransitionOut;
    
    [SerializeField] BaseButton m_BackButton;

    public override void Init(UIManager manager)
    {
        base.Init(manager);
        
        m_BackButton.Subscribe(() =>
        {
            m_ScreenManager.OpenScreen(ScreenName.MainMenu);
        });
        
        if (m_ResetMissionsOnAwake)
        {
            DailyMissionRuntime.ResetAll();
            DailyCurrencyRuntime.ResetCurrency();
        }

        for (int i = 0; i < m_DailyMissionEntries.Count; i++)
        {
            var missionEntry = m_DailyMissionEntries[i];

            if (missionEntry == null)
            {
                continue;
            }

            missionEntry.SetTransitionDelay(i * m_TransitionDelayStep, m_ApplyDelayToTransitionOut);
            missionEntry.Initialize();
        }

        RefreshCurrencyText(DailyCurrencyRuntime.CurrentCurrency);
    }

    public override void OnOpen(object data = null)
    {
        base.OnOpen(data);

        DailyCurrencyRuntime.CurrencyChanged += RefreshCurrencyText;
        RefreshCurrencyText(DailyCurrencyRuntime.CurrentCurrency);

        foreach (var missionEntry in m_DailyMissionEntries)
        {
            if (missionEntry == null)
            {
                continue;
            }

            missionEntry.RefreshForScreenOpen();
        }
    }

    public override void OnClose(System.Action OnComplete = null)
    {
        DailyCurrencyRuntime.CurrencyChanged -= RefreshCurrencyText;
        base.OnClose(OnComplete);
    }

    private void RefreshCurrencyText(int currentCurrency)
    {
        if (m_CurrentCurrencyCount == null)
        {
            return;
        }

        m_CurrentCurrencyCount.text = currentCurrency.ToString();
    }
}
