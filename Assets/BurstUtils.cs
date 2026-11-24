using Unity.Burst;

public static class BurstUtils
{
    public static bool IsBurstCompiled()
    {
        bool burst = true;
        Managed(ref burst);
        return burst;
    }
    
    [BurstDiscard]
    static void Managed(ref bool burst) => burst = false;
}