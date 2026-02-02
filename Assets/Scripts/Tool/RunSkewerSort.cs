using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using Tool;
using UnityEngine;

public class RunSkewerSort : MonoBehaviour
{
    [SerializeField] private UIToolPanel uiToolPanel;
    [SerializeField] private PipeClient pipeClient;
    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnClickRunSkewerSort()
    {
        // Kiểm tra tiến trình "Skewer Sort.exe" đã chạy chưa
        var level = uiToolPanel.GetLevel();
        if (level == 0)
        {
            PopupToast.Cretate("Level is required!");
            return;
        }
#if !UNITY_EDITOR
        bool isRunning = false;
        foreach (var process in Process.GetProcessesByName("Skewer Sort"))
        {
            isRunning = true;
            break;
        }

        if (!isRunning)
        {
            RunExternalExe(level);
        }
        else
        {
            PopupToast.Cretate("Skewer Sort.exe đã đang chạy!");
            pipeClient.SendCommandToB($"LOAD_LEVEL {level}");
        }
#else
        PopupToast.Cretate("Skewer Sort.exe đã đang chạy!");
        pipeClient.SendCommandToB($"LOAD_LEVEL {level}");
#endif
    }

    private void RunExternalExe(int level)
    {
        string buildRoot = Directory.GetParent(Application.dataPath).FullName;
        // buildRoot = thư mục chứa B.exe

        string exePath = Path.Combine(
            buildRoot,
            "Assets/Tool-SkewerSort-20251208/Skewer Sort.exe"
        );
        var startInfo = new ProcessStartInfo();
        startInfo.FileName = exePath;
        startInfo.WorkingDirectory = Path.GetDirectoryName(exePath); // như cd vào thư mục của A
        startInfo.Arguments = $"--loadLevel {level}";
        startInfo.UseShellExecute = false;

        PopupToast.Cretate(startInfo.WorkingDirectory);
        Process.Start(startInfo);
    }



}
