using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class CoreSystems : SingletonInstance<CoreSystems>
{
    [TitleGroup("Manager References", subtitle: "Manager Inits Happen In The Order They Are Specified Here", TitleAlignments.Centered), SerializeField] 
    bool m_EnableDontDestroyOnLoad;
    
    [ReadOnly]
    [FoldoutGroup("Manager References/Managers")]
    [ListDrawerSettings(ShowFoldout = true)]
    [LabelWidth(0)]
    [LabelText("List")]
    [SerializeField]
    List<Transform> m_ManagerTransforms = new List<Transform>();
    
    Dictionary<Type, ICoreSystemManager> m_ManagerDict;
    List<ICoreSystemManager> m_OrderedManagers;
    
    
    bool m_IsSDKInitialized = false;
    

    [FoldoutGroup("Manager References/Managers", Expanded = true)]
    [Button(ButtonSizes.Medium)]
    void RefreshManagersList()
    {
        m_ManagerTransforms.Clear();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (!child.TryGetComponent(out ICoreSystemManager systemManager))
            {
                continue;
            }

            if (child.gameObject.activeSelf)
                m_ManagerTransforms.Add(child);
        }
    }
    
    public T GetManager<T>() where T : ICoreSystemManager
    {
        Type type = typeof(T);

        if (!m_ManagerDict.ContainsKey(type)) return default(T);

        return (T)m_ManagerDict[type];
    }
    
        
    public T GetManagerEditMode<T>() where T : ICoreSystemManager
    {
        Type type = typeof(T);

        return GetComponentInChildren<T>();
    }

    protected override void Setup()
    {
        if (m_IsSDKInitialized) return;

        m_IsSDKInitialized = true;
        
        m_OrderedManagers = new List<ICoreSystemManager>();
        m_ManagerDict = new Dictionary<Type, ICoreSystemManager>();

        foreach (var manager in m_ManagerTransforms)
        {
            var managerComp = manager.GetComponent<ICoreSystemManager>();
            m_OrderedManagers.Add(managerComp);
            m_ManagerDict.Add(managerComp.GetType(), managerComp);
        }

        Init();
    }

    void Init()
    {
        foreach (var manager in m_OrderedManagers)
        {
            manager.Init();
        }
    }
}
