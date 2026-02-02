#if UNITY_EDITOR && UNITY_IOS
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

public class BuildProcessor : IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.iOS)
        {
#if use_webview
            // Initialize PBXProject instance
            var pbxprojPath = Path.Combine(report.summary.outputPath, "Unity-iPhone.xcodeproj/project.pbxproj");
            var pbxProject = new PBXProject();
            pbxProject.ReadFromFile(pbxprojPath);

            // Get GUID of target
            var targetGuid = pbxProject.GetUnityMainTargetGuid();

            // Setting Other Linker Flags (adding -ObjC to Other Linker Flags in Build Settings)
            pbxProject.AddBuildProperty(targetGuid, "OTHER_LDFLAGS", "-ObjC");

            // GPMWebView.bundle (adding GPMWebView.bundle to Copy Bundle Resources in Build Phases)
            var webViewBundleGuid = pbxProject.AddFile("Frameworks/GPM/WebView/Plugins/IOS/GPMWebView.bundle", "GPMWebView.bundle", PBXSourceTree.Build);
            pbxProject.AddFileToBuild(targetGuid, webViewBundleGuid);

            pbxProject.WriteToFile(pbxprojPath);
#endif
        }
    }
}
#endif