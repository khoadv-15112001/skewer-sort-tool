using SonatFramework.Scripts.UIModule;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Cysharp.Threading.Tasks;
using System;
using SonatFramework.Systems.AudioManagement;
using Sonat;
using SonatFramework.Systems;
using System.Threading;

namespace GrillSort.Story
{
    public class PopupVideoStory : Panel
    {
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage rawImage;
        [SerializeField] private Button skipBtn;
        [SerializeField] private float timeNearEnd = 1f;

        private string localPath;

        public const string UI_DATA_LOCAL_PATH = "localPath";
        public const string UI_DATA_CAN_SKIP = "canSkip";

        public static Action OnFinish;

        private CancellationTokenSource cts;

        private SonatAudioService audioService => SonatSystem.GetService<SonatAudioService>();

        public static void PlayVideo(string localPath)
        {
            UIData uiData = new UIData();
            uiData.Add(UI_DATA_LOCAL_PATH, localPath);

            PanelManager.Instance.OpenPanel<PopupVideoStory>(uiData);
        }

        public override void OnSetup()
        {
            base.OnSetup();

            skipBtn.onClick.AddListener(OnClickSkip);
        }

        public override void Open(UIData uiData)
        {
            base.Open(uiData);

            localPath = uiData.Get<string>(UI_DATA_LOCAL_PATH);

            bool canSkip = true;

            //if (uiData.TryGet(UI_DATA_CAN_SKIP, out canSkip))
            //{
            //}

            skipBtn.gameObject.SetActive(canSkip);

            audioService.FadeVolume(0f, 0f);

            if (audioService.IsMuted(AudioTracks.Sound))
                videoPlayer.SetDirectAudioMute(0, true);

            PlayVideoAsync().Forget();
        }

        public async UniTaskVoid PlayVideoAsync()
        {
            if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
                return;

            await UniTask.SwitchToMainThread();

            // --- setup target render surface ---
            if (videoPlayer.targetTexture == null)
            {
                RenderTexture rt = new RenderTexture(1920, 1080, 0);
                videoPlayer.targetTexture = rt;
                rawImage.texture = rt;
            }

            // --- clear old frame ---
            RenderTexture.active = videoPlayer.targetTexture;
            GL.Clear(true, true, Color.black);
            RenderTexture.active = null;

            videoPlayer.Stop();
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = localPath.StartsWith("file://") ? localPath : "file://" + localPath;
            videoPlayer.isLooping = false;
            videoPlayer.playOnAwake = false;

            Debug.Log($"[StoryService] Prepare video at: {videoPlayer.url}");

            // --- prepare and wait ---
            videoPlayer.Prepare();
            await UniTask.WaitUntil(() => videoPlayer.isPrepared);

            Debug.Log("[StoryService] Video prepared, starting playback");
            videoPlayer.Play();

            // --- wait for first valid frame ---
            var waitFrameTask = UniTask.WaitUntil(() => videoPlayer.frame >= 0 && videoPlayer.texture != null);
            var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(2));
            int index = await UniTask.WhenAny(waitFrameTask, timeoutTask);

            if (index == 1)
            {
                Debug.LogWarning("[StoryService] Fallback: no frame after 2s");
                OnVideoEnd(videoPlayer);
                return;
            }

            Debug.Log($"[StoryService] Video started successfully (frame={videoPlayer.frame})");

            // --- start monitoring near end ---
            cts = new CancellationTokenSource();
            CheckNearEnd(cts.Token).Forget();
        }

        private async UniTaskVoid CheckNearEnd(CancellationToken token)
        {
            try
            {
                double duration = videoPlayer.length;
                if (duration <= 0)
                    await UniTask.WaitUntil(() => videoPlayer.length > 0, cancellationToken: token);

                duration = videoPlayer.length;
                double triggerTime = Math.Max(0, duration - timeNearEnd);

                await UniTask.WaitUntil(() =>
                    videoPlayer.isPlaying && videoPlayer.time >= triggerTime, cancellationToken: token);

                if (this == null || token.IsCancellationRequested) return;

                OnVideoEnd(videoPlayer);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void OnVideoEnd(VideoPlayer vp)
        {
            //StopVideo();

            audioService.FadeVolume(1f, 0.4f);

            OnFinish?.Invoke();
            OnFinish = null;

            Close();
        }

        public void StopVideo()
        {
            if (videoPlayer.isPlaying)
                videoPlayer.Stop();

            cts?.Cancel();
            cts?.Dispose();
        }

        public void OnClickSkip()
        {
            StopVideo();

            audioService.FadeVolume(1f, 0.4f);

            OnFinish?.Invoke();
            OnFinish = null;

            CloseImmediately();
        }
    }
}
