using System.Threading.Tasks;

public static class ServiceLoader
{
    static bool initialized;

    public static async Task Initialize()
    {
        if (initialized) return;
        

        initialized = true;
    }
}