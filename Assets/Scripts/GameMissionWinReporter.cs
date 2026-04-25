using UnityEngine;

public class GameMissionWinReporter : MonoBehaviour
{
    [SerializeField] private string m_MissionId;

    public void ReportWin()
    {
        DailyMissionRuntime.CompleteMission(m_MissionId);
    }

    public void ReportWin(string missionId)
    {
        DailyMissionRuntime.CompleteMission(missionId);
    }
}
