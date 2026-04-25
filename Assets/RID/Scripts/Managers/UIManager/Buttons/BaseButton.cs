using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public abstract class BaseButton : MonoBehaviour
{
    [BoxGroup("References"), SerializeField] protected Button m_Button;

    [BoxGroup("Settings"), SerializeField] protected float m_ClickScale = 0.9f;
    [BoxGroup("Settings"), SerializeField] protected float m_AnimationDuration = 0.15f;
    [BoxGroup("Settings"), SerializeField] protected bool m_InvokeAfterAnimation = true;
    [BoxGroup("Settings"), SerializeField, Tooltip("Extra delay before invoking the OnClick or callbacks.")]
    protected float m_ExtraInvokeDelay = 0f;
    public Button Button => m_Button;
    
    bool m_IsEnabled = true;

    Tween m_ClickTween;
    event Action OnClicked; 
    
    
    public virtual void Init() { }

    protected virtual void OnEnable()
    {
        if (m_Button != null)
            m_Button.onClick.AddListener(OnClickInternal);
    }

    protected virtual void OnDisable()
    {
        if (m_Button != null)
            m_Button.onClick.RemoveListener(OnClickInternal);
    }

    protected virtual void Reset()
    {
        if (m_Button == null)
            m_Button = GetComponent<Button>();
    }

    void OnClickInternal()
    {
        if (!m_IsEnabled) return;
        if (m_InvokeAfterAnimation)
            PlayClickAnimation(() => InvokeCallbacks());
        else
        {
            PlayClickAnimation();
            InvokeCallbacks();
        }
    }

    protected virtual void InvokeCallbacks()
    {
        DOVirtual.DelayedCall(m_ExtraInvokeDelay, () =>
        {
            OnClick();           // abstract method for subclasses
            OnClicked?.Invoke(); // invoke external subscribers
        });
    }

    protected abstract void OnClick();

    public void Subscribe(Action callback)
    {
        OnClicked += callback;
    }

    public void Unsubscribe(Action callback)
    {
        OnClicked -= callback;
    }

    public void EnableButton()
    {
        m_IsEnabled = true;
    }
    
    public void DisableButton()
    {
        m_IsEnabled = false;
    }

    protected void PlayClickAnimation(Action onComplete = null)
    {
        if (m_ClickTween != null && m_ClickTween.IsActive())
            m_ClickTween.Kill();

        Transform t = transform;
        m_ClickTween = t
            .DOScale(m_ClickScale, m_AnimationDuration * 0.5f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                t.DOScale(1f, m_AnimationDuration * 0.5f)
                    .SetEase(Ease.OutBack)
                    .OnComplete(() => onComplete?.Invoke());
            });
    }
}
