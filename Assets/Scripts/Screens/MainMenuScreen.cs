using Sirenix.OdinInspector;
using UnityEngine;

public class MainMenuScreen : BaseUIScreen
{
    [SerializeField] public BaseButton m_MyHeroButton;
    [SerializeField] public BaseButton m_GamesButton;
    [SerializeField] public BaseButton m_MissionsButton;
    [SerializeField] public BaseButton m_ExitButton;

    public override void Init(UIManager manager)
    {
        base.Init(manager);
        
        m_MyHeroButton.Subscribe(() =>
        {
            OpenHeroScreen();
        });
        
        m_GamesButton.Subscribe(() =>
        {
            OpenGamesScreen();
        });
        
        m_MissionsButton.Subscribe(() =>
        {
            OpenMissionsScreen();
        });
        
        m_ExitButton.Subscribe(() =>
        {
            CloseProgram();
        });
    }

    void OpenHeroScreen()
    {
        m_ScreenManager.OpenScreen(ScreenName.HeroScreen);
    }
    
    void OpenGamesScreen()
    {
        m_ScreenManager.OpenScreen(ScreenName.GameTypeSelectionScreen);
    }
    
    void OpenMissionsScreen()
    {
        m_ScreenManager.OpenScreen(ScreenName.DailyMission);
    }
    
    private void CloseProgram()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
