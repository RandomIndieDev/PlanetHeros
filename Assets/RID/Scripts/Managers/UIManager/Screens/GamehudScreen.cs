using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class GamehubScreenData
{
    public int CurrentLevel;
}

public class GamehudScreen : BaseUIScreen
{
    [BoxGroup("References"), SerializeField] public TMP_Text m_LevelText;

    void SetLevelText(int level)
    {
        m_LevelText.text = $"Level {level}";
    }

    public override void Init(UIManager manager)
    {
        throw new System.NotImplementedException();
    }
}
