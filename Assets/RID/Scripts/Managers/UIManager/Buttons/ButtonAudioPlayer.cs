using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(BaseButton))]
public class ButtonAudioPlayer : MonoBehaviour
{
    [BoxGroup("Settings"), SerializeField] SoundType m_SoundType;

    [BoxGroup("References"), SerializeField, ReadOnly] BaseButton m_Button;
    [BoxGroup("References"), SerializeField, ReadOnly] Button m_UnityButton;

    AudioManager m_AudioManager;
    bool m_IsSubscribed;
    
    protected virtual void Reset()
    {
        if (m_Button == null)
            m_Button = GetComponent<BaseButton>();

        if (m_UnityButton == null)
            m_UnityButton = GetComponent<Button>();
    }

    void Awake()
    {
        if (m_Button == null)
            m_Button = GetComponent<BaseButton>();

        if (m_UnityButton == null)
            m_UnityButton = GetComponent<Button>();
    }
    
    void OnEnable()
    {
        if (m_Button == null)
            m_Button = GetComponent<BaseButton>();

        if (m_UnityButton == null)
            m_UnityButton = GetComponent<Button>();

        m_AudioManager = CoreSystems.Instance.GetManager<AudioManager>();

        if (m_UnityButton == null || m_AudioManager == null || m_IsSubscribed)
            return;

        m_UnityButton.onClick.AddListener(HandleButtonClicked);
        m_IsSubscribed = true;
    }

    void OnDisable()
    {
        if (m_UnityButton != null && m_IsSubscribed)
        {
            m_UnityButton.onClick.RemoveListener(HandleButtonClicked);
            m_IsSubscribed = false;
        }
    }

    void HandleButtonClicked()
    {
        
        Debug.LogError("called her");
        if (m_AudioManager == null || m_SoundType == SoundType.NONE)
            return;

        m_AudioManager.PlayOneShot(m_SoundType);
    }
}
