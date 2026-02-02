using UnityEditor;

[InitializeOnLoad]

public class PreloadSigningAlias
{
    static PreloadSigningAlias()
    {
        PlayerSettings.Android.keystorePass = "Sonat@123";
        PlayerSettings.Android.keyaliasName = "grill_sort";
        PlayerSettings.Android.keyaliasPass = "Sonat@123";
    }
}