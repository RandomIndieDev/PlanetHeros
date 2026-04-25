using UnityEngine;

public class GameTypeSelectionScreen : BaseUIScreen
{
    [SerializeField] public BaseButton m_WaterGamesButton;
    [SerializeField] public BaseButton m_BackButton;

    public override void Init(UIManager manager)
    {
        base.Init(manager);
        
        m_BackButton.Subscribe(() =>
        {
            m_ScreenManager.OpenScreen(ScreenName.MainMenu);
        });
        
        m_WaterGamesButton.Subscribe(() =>
        {
            m_ScreenManager.OpenScreen(ScreenName.GameSelectionScreen);
        });
    }
}
