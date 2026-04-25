using System;

public static class DailyCurrencyRuntime
{
    private static int s_CurrentCurrency;

    public static event Action<int> CurrencyChanged;

    public static int CurrentCurrency => s_CurrentCurrency;

    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeOnGameStart()
    {
        ResetCurrency();
    }

    public static void ResetCurrency()
    {
        s_CurrentCurrency = 0;
        CurrencyChanged?.Invoke(s_CurrentCurrency);
    }

    public static void AddCurrency(int amount)
    {
        if (amount == 0)
        {
            return;
        }

        s_CurrentCurrency += amount;
        CurrencyChanged?.Invoke(s_CurrentCurrency);
    }
}
