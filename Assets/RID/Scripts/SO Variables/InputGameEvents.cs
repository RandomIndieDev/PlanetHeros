using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu]
public class InputGameEvents : ScriptableObject
{
    public Action<ITapReciever> OnObjectTapped;

    public void ObjectTapped(ITapReciever reciever)
    {
        OnObjectTapped?.Invoke(reciever);
    }
}

public interface ITapReciever
{
    bool IsTapEnabled();
    Type GetConcreteType();
}
