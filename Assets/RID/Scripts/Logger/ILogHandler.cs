using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILogHandler
{
    void Log(ILogUser user, string message, LogType type = LogType.Log);
    void Log(ILogUser user, string message, LogSettings settings, LogType type = LogType.Log);
    void Flush();
}

