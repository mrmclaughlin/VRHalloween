using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class AutoIncrementVersion : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        IncrementVersion();
    }

    private static void IncrementVersion()
    {
        // Get and increment the build number
        int buildNumber = int.Parse(PlayerSettings.bundleVersion.Split('.')[2]);
        buildNumber++;
        string newVersion = $"1.0.{buildNumber}";
        
        // Set the incremented version back to PlayerSettings
        PlayerSettings.bundleVersion = newVersion;
       // Increment the bundleVersionCode
        PlayerSettings.Android.bundleVersionCode++;

        Debug.Log("Updated build version to: " + newVersion);
        Debug.Log("Updated bundleVersionCode to: " + PlayerSettings.Android.bundleVersionCode);
    }
}
