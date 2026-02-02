using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class LocalizationBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        //bool isAAB = report.summary.outputPath.EndsWith(".aab");

        //if (isAAB && LocalizationUtils.ForceJapan)
        //{
        //    Debug.LogError("[LocalizationBuildProcessor] ❌ Build AAB không được phép khi ForceJapan = true!");
        //    throw new BuildFailedException("ForceJapan = true. Vui lòng tắt trước khi build AAB.");
        //}
    }
}
