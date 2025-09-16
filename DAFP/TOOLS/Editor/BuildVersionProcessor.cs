using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
public class BuildVersionProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;
    public string initverison = "0.0";
    
    public void OnPreprocessBuild(BuildReport report)
    {
        
        Debug.Log($"Current patch:[{Currentversion()}]");
        Updateverison(Currentversion());
    }
    private string Currentversion()
    {
        string[] version = PlayerSettings.bundleVersion.Split("-");
        return version[1];
    }
    private void Updateverison(string version)
    {
        
        if(float.TryParse(version,out float paresedversion))
        {
            Debug.Log($"Parsed Patch: {paresedversion}");
            float newversion = paresedversion + 0.01f;
            Debug.Log($"Increased Version To: {newversion}");
            PlayerSettings.bundleVersion = $"InDevPatch-{newversion}";
            //string name = PlayerSettings.productName;
            //PlayerSettings.productName = name.Split("-")[0]+"-" + newversion.ToString();
            Debug.Log($"Patch Updated : {PlayerSettings.bundleVersion}");
        }
        else
        {
            Debug.Log($"Could not parse version:{Currentversion()}");
        }
  
    }
}
