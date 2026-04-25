using UnityEngine;
using System.Threading.Tasks;

public class GameBootstrapper : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] ScriptableObject configAsset; // must implement IGameConfig

    GameContext _context;
    
    void Awake()
    {
        if (configAsset is not IGameConfig gameConfig)
        {
            Debug.LogError("Invalid config — must implement IGameConfig");
            return;
        }

        _context = new GameContext(gameConfig);

        var initializer = GameInitializerResolver.GetInitializerFor(gameConfig);

        if (initializer == null)
            return;

        initializer.Initialize(_context);
    }
}