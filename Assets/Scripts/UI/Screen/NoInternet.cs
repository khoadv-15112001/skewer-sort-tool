using System.Collections;
using Cysharp.Threading.Tasks;
using Sonat;
using Sonat.Enums;
using Sonat.FirebaseModule;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.EventBus;
using UnityEngine;

public class NoInternet : SingletonSimple<NoInternet>
{
    [Header("References")]
    [SerializeField] private Panel panel;

    [Header("Settings")]
    [SerializeField] private string homeName = "H";
    [SerializeField] private string currentScreen = "H";
    [SerializeField] private bool forceInternetOnlyHome = true;

    private Coroutine waitCheckInternet;
    private GameState preGameState;

    private async void Awake()
    {
        new EventBinding<LevelStartedEvent>(OnLevelStart);
        new EventBinding<UpdateScreenEvent>(OnUpdateScreen);

        await UniTask.WaitUntil(() => PanelManager.Instance != null);
        Debug.Log("[NoInternet] PanelManager ready.");

        CheckInternetHome(currentScreen);
    }


    private void OnUpdateScreen(UpdateScreenEvent e)
    {
        CheckInternetHome(e.screen);
    }
    public void CheckInternetHome(string screen)
    {
        currentScreen = screen;
        if (!SonatFirebase.remote.GetRemoteBool("internet_connection", true))
            return;
        if (forceInternetOnlyHome && currentScreen != homeName)
        {
            StopWaitCheckInternet();
            return;
        }
        CheckConnectInternet();
    }
    private void OnLevelStart()
    {
        if (!forceInternetOnlyHome && SonatFirebase.remote.GetRemoteBool("internet_connection", true))
        {
            CheckConnectInternet();
        }
    }

    private bool CheckAdditionalConditions()
    {
        return !forceInternetOnlyHome || currentScreen == homeName;
    }

    private void CheckConnectInternet()
    {
        if (!CheckAdditionalConditions())
            return;

        if (!MySonatFramework.IsNetworkAvailable())
        {
            StopWaitCheckInternet();

            PanelManager.Instance.CloseAllPanel();

            if (GameplayController.instance != null)
                preGameState = GameplayController.instance.gameState;

            EventBus<GameStateChangeEvent>.Raise(new GameStateChangeEvent { gameState = GameState.Paused });

            panel.Open(null);
        }
        else
        {
            WaitCheckInternet();
        }
    }


    private void StopWaitCheckInternet()
    {
        if (waitCheckInternet != null)
        {
            StopCoroutine(waitCheckInternet);
            waitCheckInternet = null;
        }
    }

    public void WaitCheckInternet()
    {
        if (waitCheckInternet == null)
        {
            int timeGap = SonatFirebase.remote.GetRemoteInt("check_internet_time_gap", 1);
            if (timeGap > 0)
                waitCheckInternet = StartCoroutine(WaitCheckConnectInternet(timeGap));
        }
    }

    private IEnumerator WaitCheckConnectInternet(int timeGap)
    {
        yield return new WaitForSeconds(timeGap);
        waitCheckInternet = null;
        CheckConnectInternet();
    }

    public void ClosePanel()
    {
        panel.Close();
        EventBus<GameStateChangeEvent>.Raise(new GameStateChangeEvent { gameState = GameState.Playing });

        if (CheckAdditionalConditions())
            WaitCheckInternet();
    }

    public void ForceShowPopup() => panel.Open(null);
}
