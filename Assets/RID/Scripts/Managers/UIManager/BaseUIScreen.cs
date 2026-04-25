using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public abstract class BaseUIScreen : MonoBehaviour
{
    protected UIManager m_ScreenManager;
    protected AudioManager m_AudioManager;
    
    [BoxGroup("Base Settings"), SerializeField] SoundType m_OnOpenSoundEffect;
    
    public virtual void Init(UIManager manager)
    {
        m_ScreenManager = manager;
        m_AudioManager = CoreSystems.Instance.GetManager<AudioManager>();
    }

    public virtual void OnOpen(object data = null)
    {
        m_AudioManager.PlayOneShot(m_OnOpenSoundEffect);
        gameObject.SetActive(true);
    }

    public virtual void OnClose(Action OnComplete = null)
    {
        OnComplete?.Invoke();
    }
}
