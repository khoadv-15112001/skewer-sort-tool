using UnityEditor;
using UnityEngine;

namespace Sonat.Editor
{
    public static class SonatSDKEditorTool
    {
        [MenuItem("Sonat SDK/Documentation", priority = 0)]
        public static void OpenDocuments()
        {
            Application.OpenURL("https://docs.google.com/document/d/1UQqWw4ukS-HMvDrhYE7uSCuAwGaR3I1o1PK9RlP2t_k/edit?tab=t.0");
        }
    }
}
