using System;
using System.Collections.Generic;

public static class DailyMissionRuntime
{
    private static readonly Dictionary<string, bool> MissionCompletionStates = new Dictionary<string, bool>();
    private static readonly HashSet<string> PresentedMissionCompletions = new HashSet<string>();

    public static event Action<string, bool> MissionStatusChanged;

    public static bool IsCompleted(string missionId)
    {
        if (string.IsNullOrWhiteSpace(missionId))
        {
            return false;
        }

        return MissionCompletionStates.TryGetValue(missionId, out bool isCompleted) && isCompleted;
    }

    public static void CompleteMission(string missionId)
    {
        if (string.IsNullOrWhiteSpace(missionId))
        {
            return;
        }

        if (IsCompleted(missionId))
        {
            return;
        }

        MissionCompletionStates[missionId] = true;
        MissionStatusChanged?.Invoke(missionId, true);
    }

    public static bool HasPresentedCompletion(string missionId)
    {
        if (string.IsNullOrWhiteSpace(missionId))
        {
            return false;
        }

        return PresentedMissionCompletions.Contains(missionId);
    }

    public static void MarkCompletionPresented(string missionId)
    {
        if (string.IsNullOrWhiteSpace(missionId))
        {
            return;
        }

        PresentedMissionCompletions.Add(missionId);
    }

    public static void SetMissionCompleted(string missionId, bool isCompleted)
    {
        if (string.IsNullOrWhiteSpace(missionId))
        {
            return;
        }

        MissionCompletionStates[missionId] = isCompleted;

        if (!isCompleted)
        {
            PresentedMissionCompletions.Remove(missionId);
        }

        MissionStatusChanged?.Invoke(missionId, isCompleted);
    }

    public static void ResetMission(string missionId)
    {
        if (string.IsNullOrWhiteSpace(missionId))
        {
            return;
        }

        MissionCompletionStates.Remove(missionId);
        PresentedMissionCompletions.Remove(missionId);
        MissionStatusChanged?.Invoke(missionId, false);
    }

    public static void ResetAll()
    {
        if (MissionCompletionStates.Count == 0)
        {
            return;
        }

        string[] missionIds = new string[MissionCompletionStates.Keys.Count];
        MissionCompletionStates.Keys.CopyTo(missionIds, 0);

        MissionCompletionStates.Clear();
        PresentedMissionCompletions.Clear();

        foreach (string missionId in missionIds)
        {
            MissionStatusChanged?.Invoke(missionId, false);
        }
    }
}
