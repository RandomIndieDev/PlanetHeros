using UnityEngine;

public class WinPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject m_WinPanel;
    [SerializeField] private GameObject m_LosePanel;
    
    [SerializeField] private BaseButton m_WinBackButton;
    [SerializeField] private BaseButton m_LoseBackButton;

    [SerializeField] private BaseButton m_LoseTryAgainButton;

    private System.Action m_OnBackPressed;
    private System.Action m_OnRetryPressed;
    
    public bool IsShowingWinPanel => m_WinPanel != null && m_WinPanel.activeSelf;
    
    AudioManager m_AudioManager;

    public void Initialize(System.Action onBackPressed, System.Action onRetryPressed)
    {
        m_OnBackPressed = onBackPressed;
        m_OnRetryPressed = onRetryPressed;

        m_AudioManager = CoreSystems.Instance.GetManager<AudioManager>();

        if (m_WinBackButton != null)
        {
            m_WinBackButton.Subscribe(HandleBackPressed);
        }

        if (m_LoseBackButton != null)
        {
            m_LoseBackButton.Subscribe(HandleBackPressed);
        }

        if (m_LoseTryAgainButton != null)
        {
            m_LoseTryAgainButton.Subscribe(HandleRetryPressed);
        }

        HideAll();
    }

    public void ShowWinPanel()
    {
        m_AudioManager.PlayOneShot(SoundType.Effect_Experience_Gained);
        gameObject.SetActive(true);
        
        if (m_WinPanel != null)
        {
            m_WinPanel.SetActive(true);
        }

        if (m_LosePanel != null)
        {
            m_LosePanel.SetActive(false);
        }
    }

    public void ShowLosePanel()
    {
        m_AudioManager.PlayOneShot(SoundType.grid_WrongWord);
        gameObject.SetActive(true);
        
        if (m_WinPanel != null)
        {
            m_WinPanel.SetActive(false);
        }

        if (m_LosePanel != null)
        {
            m_LosePanel.SetActive(true);
        }
    }

    public void HideAll()
    {
        if (m_WinPanel != null)
        {
            m_WinPanel.SetActive(false);
        }

        if (m_LosePanel != null)
        {
            m_LosePanel.SetActive(false);
        }
    }

    private void HandleBackPressed()
    {
        gameObject.SetActive(false);
        
        m_OnBackPressed?.Invoke();
    }

    private void HandleRetryPressed()
    {        
        gameObject.SetActive(false);
        
        m_OnRetryPressed?.Invoke();
    }

}
