using System;
using System.Globalization;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildVersionProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;
    public const string INIT_VERSION = "0.0";

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log($"Current patch:[{current_version()}]");
        update_version(current_version());
    }

    private string current_version()
    {
        try
        {
            var _version = PlayerSettings.bundleVersion.Split("-");
            return _version[1];
        }
        catch (Exception _ex)
        {
            return INIT_VERSION;
        }
    }

    private void update_version(string version)
    {
        if (float.TryParse(version, NumberStyles.Float, CultureInfo.InvariantCulture, out var _parsedVersion))
        {
            var _newVersion = (_parsedVersion + 0.01f).ToString("F2", CultureInfo.InvariantCulture);
            Debug.Log($"Parsed Patch: {_parsedVersion}");
            Debug.Log($"Increased Version To: {_newVersion}");
            PlayerSettings.bundleVersion = $"InDevPatch-{_newVersion}";
            Debug.Log($"Patch Updated : {PlayerSettings.bundleVersion}");
        }
        else
        {
            Debug.Log($"Could not parse version: {version}");
        }
    }
}