using Linn;

public static class TrackerConfiguration
{
    private static string kTrackerAccountDev = "UA-35633282-1";
    private static string kTrackerAccountBeta = "UA-35620184-1";
    private static string kTrackerAccountRelease = "UA-35652105-1";
    
    public static string TrackerAccount(Helper aHelper)
    {
        string result = kTrackerAccountDev;
        if (aHelper.BuildType == EBuildType.Beta)
        {
            result = kTrackerAccountBeta;
        }
        else if (aHelper.BuildType == EBuildType.Release)
        {
            result = kTrackerAccountRelease;
        }
        return result;
    }
}