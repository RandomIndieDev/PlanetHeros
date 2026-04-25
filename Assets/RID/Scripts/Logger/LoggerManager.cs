using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class LogSettings
{
    [Header("Text Style")]
    public bool bold;
    public bool italic;
    
    [Header("Color")]
    public Color color = Color.white;

    [Header("Prefix / Tagging")]
    public string prefix = "";
    public string suffix = "";

    public string Apply(string message)
    {
        string styled = message;
        
        if (!string.IsNullOrEmpty(prefix))
            styled = $"{prefix} {styled}";
        if (!string.IsNullOrEmpty(suffix))
            styled = $"{styled} {suffix}";
        
        if (bold) styled = $"<b>{styled}</b>";
        if (italic) styled = $"<i>{styled}</i>";

        styled = $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{styled}</color>";

        return styled;
    }
}

public class LoggerManager : MonoBehaviour, ICoreSystemManager
{
    [SerializeField] public ScriptableObject m_CurrentLogHandler;
    
    private ILogHandler m_Handler;
    
    public void Init()
    {
        if (m_CurrentLogHandler == null) return;
        
        m_Handler = (ILogHandler) m_CurrentLogHandler;
    }
    
    public ILogHandler GetCurrentLogger()
    {
        Init();
        return m_Handler;
    }
}
