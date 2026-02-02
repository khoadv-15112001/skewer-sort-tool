#if UNITY_EDITOR
using UnityEditor;

namespace SonatFramework.Scripts.Editor
{
    public static class SonatFrameworkEditorManager
    {
        [MenuItem("Sonat Framework/Resolve")]
        public static void ResolveSonatSystem()
        {
            CreateCustomEnum.CheckCreateEnums();
        }
    }
}
#endif