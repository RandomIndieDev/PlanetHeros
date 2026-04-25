using System.Collections;
using System.Collections.Generic;
using Reflex.Core;
using UnityEditor;
using UnityEngine;

public class ProjectInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] LoggerManager m_LoggerManager;
    
    public void InstallBindings(ContainerBuilder containerBuilder)
    {
        containerBuilder.AddSingleton(m_LoggerManager);
        containerBuilder.AddSingleton("TEst");
    }
}
