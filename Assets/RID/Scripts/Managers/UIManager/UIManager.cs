using System;
using System.Collections.Generic;
using Reflex.Attributes;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public enum ScreenName
{
    MainMenu,
    DailyMission,
    GameTypeSelectionScreen,
    GameSelectionScreen,
    GameplayScreen,
    HeroScreen
}

public enum ScreenStackMode
{
    Replace,
    Overlay
}

[Serializable]
public class ScreenEntryDict : UnitySerializedDictionary<ScreenName, BaseUIScreen> { }

public class UIManager : MonoBehaviour, ICoreSystemManager
{
    [BoxGroup("References"), SerializeField] ScreenEntryDict m_ScreenEntries;

    private Stack<BaseUIScreen> m_History = new Stack<BaseUIScreen>();
    BaseUIScreen m_CurrentUIScreen;

    [BoxGroup("Settings"), SerializeField] ScreenName m_InitalOpenedScreen;
    
    public void Init()
    {
        foreach (var screenEntry in m_ScreenEntries)
        {
            screenEntry.Value.Init(this);
        }
        
        m_CurrentUIScreen = null;
        m_History.Push(m_CurrentUIScreen);
        
        OpenScreen(m_InitalOpenedScreen);
    }
    
    public void OpenScreen(ScreenName name, ScreenStackMode mode = ScreenStackMode.Replace, object data = null)
    {
        if (!m_ScreenEntries.ContainsKey(name))
        {
            Debug.LogError($"No screen found with id {name}");
            return;
        }

        var nextScreen = m_ScreenEntries[name];
        
        if (m_CurrentUIScreen == nextScreen && mode == ScreenStackMode.Replace)
        {
            Debug.Log($"[UIManager] Screen {name} is already open. Ignoring request.");
            return;
        }

        switch (mode)
        {
            case ScreenStackMode.Replace:
                ReplaceScreen(nextScreen, data);
                break;

            case ScreenStackMode.Overlay:
                OverlayScreen(nextScreen, data);
                break;
        }
    }
    
    public void OpenScreenOnce(ScreenName name, object data = null)
    {
        if (!m_ScreenEntries.TryGetValue(name, out var screen))
        {
            Debug.LogError($"No screen found with id {name}");
            return;
        }

        screen.gameObject.SetActive(true);
        screen.OnOpen(data);
    }
    
    private void ReplaceScreen(BaseUIScreen nextScreen, object data)
    {
        if (m_CurrentUIScreen != null)
        {
            var closing = m_CurrentUIScreen;
            closing.OnClose(() =>
            {
                closing.gameObject.SetActive(false);
                m_History.Pop();
                ShowScreen(nextScreen, data);
                m_History.Push(nextScreen);

            });
        }
        else
        {
            ShowScreen(nextScreen, data);
        }
    }
    
    public void CloseScreen(BaseUIScreen screen)
    {
        if (screen == null)
            return;

        screen.OnClose(() =>
        {
            screen.gameObject.SetActive(false);

            if (m_History.Count <= 0)
            {
                return;
            }

            m_History.Pop();
            m_CurrentUIScreen = m_History.Count > 0 ? m_History.Peek() : null;
        });
    }

    
    private void OverlayScreen(BaseUIScreen nextScreen, object data)
    {
        if (m_CurrentUIScreen != null)
            m_CurrentUIScreen.gameObject.SetActive(true);

        nextScreen.gameObject.SetActive(true);
        m_History.Push(nextScreen);
        m_CurrentUIScreen = nextScreen;
        nextScreen.OnOpen(data);
    }
    
    private void ShowScreen(BaseUIScreen nextScreen, object data)
    {
        nextScreen.gameObject.SetActive(true);
        m_CurrentUIScreen = nextScreen;
        nextScreen.OnOpen(data);
    }
    
    public void UpdateScreen(ScreenName name, object data = null)
    {
        if (m_ScreenEntries.TryGetValue(name, out var screen))
        {
            if (screen.gameObject.activeSelf && screen is IUpdatableScreen updatable)
                updatable.OnScreenUpdate(data);
        }
    }

}