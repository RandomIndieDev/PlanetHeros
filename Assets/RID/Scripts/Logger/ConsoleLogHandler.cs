using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Logging/ConsoleLogger")]
public class ConsoleLogHandler : ScriptableObject, ILogHandler
{
    public void Log(ILogUser user, string message, LogType type = LogType.Log)
    {
        if (!user.LoggingEnabled()) return;

        var formatedMsg = $"[{user.LoggerTag()}]: {message}";
        
        switch (type)
        {
            case LogType.Error:
                Debug.LogError(formatedMsg);
                break;
            case LogType.Assert:
                Debug.LogAssertion(formatedMsg);
                break;
            case LogType.Warning:
                Debug.LogWarning(formatedMsg);
                break;
            case LogType.Log:
            default:
                Debug.Log(formatedMsg);
                break;
        }
    }

    public void Log(ILogUser user, string message, LogSettings settings, LogType type = LogType.Log)
    {
        message = settings.Apply(message);
        Log(user, message, type);
    }

    public void Flush()
    {
        throw new System.NotImplementedException();
    }
}
