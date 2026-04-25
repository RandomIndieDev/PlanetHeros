using System;
using UnityEngine;

public interface IGameConfig
{
    string GameId { get; }
}

public interface IGameInitializer
{
    void Initialize(GameContext ctx);
}

public static class GameInitializerResolver
{
    public static IGameInitializer GetInitializerFor(IGameConfig config)
    {
        switch (config)
        {

            // 👇 Add other games later
            // case MemoryMatchConfig: return new MemoryMatchInitializer();
            // case RunnerConfig: return new RunnerInitializer();

            default:
                Debug.LogError($"No initializer registered for config type: {config.GetType().Name}");
                return null;
        }
    }
}

