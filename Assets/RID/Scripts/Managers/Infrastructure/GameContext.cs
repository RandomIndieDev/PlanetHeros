public class GameContext
{
    public IGameConfig Config;

    // later we can add:
    // public PlayerSessionData Player;
    // public object RuntimeData; // e.g., reference to CarController, etc.

    public GameContext(IGameConfig config)
    {
        Config = config;
    }
}