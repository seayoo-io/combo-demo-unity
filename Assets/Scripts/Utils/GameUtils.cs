using UnityEngine;
using UnityEngine.Diagnostics;

public class GameUtils
{
    // Start is called before the first frame update
    public static void ForceCrash()
    {
        // Forcing a crash
        Utils.ForceCrash(ForcedCrashCategory.FatalError);
    }

    public static string GetPlatformName()
    {
#if UNITY_STANDALONE
        return "Windows";
#elif UNITY_ANDROID
        return "Android";
#elif UNITY_IOS
        return "iOS";
#else
        return $"{Application.platform}";
#endif
    }
}
