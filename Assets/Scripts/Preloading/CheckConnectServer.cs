using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using GrillSort.OnlineService;
using Sonat;
using Sonat.Enums;
using SonatFramework.Systems.EventBus;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckConnectServer : MonoBehaviour
{
    [SerializeField] private OnlineService onlineService;
    [SerializeField] private HybridDataService hybridDataService;
    [SerializeField] private BIGameConfigService bIGameConfigService;

    [SerializeField] private float timeout = 5f;

    public const string DATA_KEY = "CHECK_CONNECT_SERVER";
    private const string LOG_TAG = "[CheckConnectServer]";

    EventBinding<OnlineService.BIGetOrSignUpEvent> getOrSignUpEvent;

    [SerializeField] private TMP_Text txtUserID;

    public string currentUserId
    {
        get => PlayerPrefs.GetString(DATA_KEY + "_USER_ID");
        set => PlayerPrefs.SetString(DATA_KEY + "_USER_ID", value);
    }


    private void Start()
    {
        Debug.Log($"{LOG_TAG} OnEnable");
        SonatSdkManager.Initialize(OnSonatSdkInited);
        bIGameConfigService.Initialize(onlineService);

        UpdateUserText();
    }

    private void OnSonatSdkInited()
    {
        onlineService.Initialize();

        if (string.IsNullOrEmpty(currentUserId))
        {
            Debug.Log($"{LOG_TAG} OnSonatSdkInited");
            getOrSignUpEvent = new EventBinding<OnlineService.BIGetOrSignUpEvent>(OnBIGetOrSignUpEvent);
            //onlineService.Initialize();
            StartCoroutine(CheckTimeout());
        }
        else
        {
            HybridDataService.Ready = true;
            NextMainScene();
        }
        UpdateUserText();
    }
    private void UpdateUserText()
    {
        if (!txtUserID) 
        {
            txtUserID.text = "";
            return; 
        }

        if (string.IsNullOrEmpty(currentUserId))
        {
            txtUserID.text = "";
        }
        else
        {
            txtUserID.text = $"ID: {currentUserId} - Version: {Application.version}";
        }
    }
    private void OnDestroy()
    {
        EventBus<OnlineService.BIGetOrSignUpEvent>.Deregister(getOrSignUpEvent);
    }

    private void OnBIGetOrSignUpEvent(OnlineService.BIGetOrSignUpEvent @event)
    {
        if (SceneManager.GetActiveScene().name == GamePlacement.PreLoading.ToString())
        {
            Debug.Log($"{LOG_TAG} OnBIGetOrSignUpEvent: {@event.isNewUser}");
            if (@event.isNewUser)
            {
                // người chơi cũ trước bản này thì upload data
                if (hybridDataService.HasKey(DataSyncKey.UserLevel_Classic.ToString()))
                {
                    Debug.Log($"{LOG_TAG} UploadDataToServer");
                    UploadDataToServer().Forget();
                    return;
                }
                else
                {
                    Debug.Log($"{LOG_TAG} new user");
                    currentUserId = onlineService.UserID;
                    HybridDataService.Ready = true;
                    // mới hoàn toàn
                }
            }
            else
            {
                if (currentUserId != onlineService.UserID)
                {
                    Debug.Log($"{LOG_TAG} FetchDataFromServer");
                    FetchDataFromServer().Forget();
                    return;
                }
                else
                {
                    HybridDataService.Ready = true;
                    Debug.Log($"{LOG_TAG} Nothing");
                }
            }

            Debug.Log($"{LOG_TAG} NextScene From BIGetOrSignUpEvent: {currentUserId}");
            NextMainScene();
        }

    }

    private async UniTask UploadDataToServer()
    {
        await hybridDataService.UploadDataToServer();
        currentUserId = onlineService.UserID;
        Debug.Log($"{LOG_TAG} NextScene From Updating Data: {currentUserId}");
        NextMainScene();
    }

    private async UniTask FetchDataFromServer()
    {
        await hybridDataService.FetchDataFromServer();
        currentUserId = onlineService.UserID;
        Debug.Log($"{LOG_TAG} NextScene From Fetching Data: {currentUserId}");
        NextMainScene();
    }

    private IEnumerator CheckTimeout()
    {
        yield return new WaitForSeconds(timeout);
        Debug.Log($"{LOG_TAG} NextScene From Timeout connect to server");
        NextMainScene();
    }

    private async UniTaskVoid NextMainScene()
    {
        await UniTask.WaitUntil(() => bIGameConfigService.inited);
        SceneManager.LoadSceneAsync(GamePlacement.Loading.ToString());
    }

}
