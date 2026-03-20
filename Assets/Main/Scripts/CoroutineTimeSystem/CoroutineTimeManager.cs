using UnityEngine;

public static class CoroutineTimeManager
{
    private static float currentLimitedFrameRate;
    
    public static void SetLimitedFrameRate()
    {
        if (QualitySettings.vSyncCount != 0)
            currentLimitedFrameRate = (float)Screen.currentResolution.refreshRateRatio.numerator / Screen.currentResolution.refreshRateRatio.denominator / QualitySettings.vSyncCount;
        else if (Application.targetFrameRate >= 0) currentLimitedFrameRate = Application.targetFrameRate;
        else currentLimitedFrameRate = 60f;
    }

    public static bool GetSuspensionMoment()
    {
        return Time.deltaTime >= 1f / currentLimitedFrameRate;
    }
}
