using UnityEngine;

public class GameSelectionScreen : BaseUIScreen
{
    [SerializeField] BasicButton m_TapGameButton;
    [SerializeField] BasicButton m_BackButton;

    public override void Init(UIManager manager)
    {
        base.Init(manager);
        
        m_BackButton.Subscribe(() =>
        {
            m_ScreenManager.OpenScreen(ScreenName.GameTypeSelectionScreen);
        });
        
        m_TapGameButton.Subscribe(() =>
        {
            m_ScreenManager.OpenScreen(ScreenName.GameplayScreen);
        });
    }
}
