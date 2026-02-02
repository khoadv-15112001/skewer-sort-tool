using Sonat.FirebaseModule.Analytic;
using UnityEditor;

namespace Sonat.Editor.PackageManager.Elements
{
    public class RemoteConfigWindowDraw
    {
        private SonatFirebaseWindow sonatFirebaseWindow;

        private SonatFirebaseConfig sonatFirebaseConfig;
        private ListRemoteConfigDraw listRemoteConfigDraw;


        #region Properties

        private SerializedObject remoteConfigSerializedObject;

        #endregion

        public void Init(SonatFirebaseWindow sonatFirebaseWindow)
        {
            this.sonatFirebaseWindow = sonatFirebaseWindow;
            sonatFirebaseConfig = SonatEditorHelper.LoadConfigSo<SonatFirebaseConfig>("SonatFirebaseConfig");
            if (sonatFirebaseConfig != null)
            {
                listRemoteConfigDraw = new ListRemoteConfigDraw();
                listRemoteConfigDraw.Init(sonatFirebaseConfig.defaultConfigs);
            }
        }


        public void Draw()
        {
            listRemoteConfigDraw.Draw();
        }
    }
}