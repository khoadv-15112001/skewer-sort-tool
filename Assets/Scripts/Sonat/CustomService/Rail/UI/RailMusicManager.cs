using Cysharp.Threading.Tasks;
using SonatFramework.Systems;
using SonatFramework.Systems.AudioManagement;
using UnityEngine;
using Sonat;

namespace GrillSort.Rail
{
    public class RailMusicManager : MonoBehaviour
    {
        [Header("Music Clips")]
        [SerializeField] private AudioClip openLoopClip;
        [SerializeField] private AudioClip startConnectClip;
        [SerializeField] private AudioClip startLoopClip;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip openSceneSound;
        [SerializeField] private AudioClip rewardOpenSound;
        [SerializeField] private AudioClip[] rewardOpenDelaySounds; // Phát sau 1s khi rewardOpen chạy
        [SerializeField] private AudioClip startSceneSound;

        [Header("Settings")]
        [SerializeField] private float fadeBackgroundDuration = 0.4f;
        [SerializeField] private float fadeBackgroundVolume = 0f;

        private SonatAudioService audioService => SonatSystem.GetService<SonatAudioService>();
        
        private AudioSource railMusicSource;
        private bool isClosing;
        private MusicState currentState;

        private enum MusicState
        {
            None,
            OpenLoop,
            StartConnect,
            StartLoop
        }

        public void Setup(bool isJoined)
        {
            isClosing = false;
            
            // Fade background music
            audioService.FadeVolume(fadeBackgroundVolume, fadeBackgroundDuration);

            // Create audio source
            GameObject audioObj = new GameObject("RailMusicTemp");
            railMusicSource = audioObj.AddComponent<AudioSource>();
            railMusicSource.playOnAwake = false;
            railMusicSource.volume = audioService.IsMuted(AudioTracks.Music) ? 0f : 1f;

            // Play appropriate music based on join status
            if (isJoined)
            {
                PlayStartLoop();
            }
            else
            {
                PlayOpenLoop();
            }
        }

        /// <summary>
        /// Phát openLoop khi chưa join event
        /// </summary>
        public void PlayOpenLoop()
        {
            if (railMusicSource == null || openLoopClip == null) return;
            if (currentState == MusicState.OpenLoop) return;

            currentState = MusicState.OpenLoop;
            railMusicSource.clip = openLoopClip;
            railMusicSource.loop = true;
            railMusicSource.Play();

            Debug.Log("[RailMusicManager] Playing OpenLoop");
        }

        /// <summary>
        /// Phát startConnect khi ấn nút start event (transition)
        /// Sau khi kết thúc sẽ tự động chuyển sang startLoop
        /// </summary>
        public void PlayStartConnect()
        {
            if (railMusicSource == null || startConnectClip == null) return;
            if (currentState == MusicState.StartConnect) return;

            currentState = MusicState.StartConnect;
            railMusicSource.loop = false;
            railMusicSource.clip = startConnectClip;
            railMusicSource.Play();

            Debug.Log("[RailMusicManager] Playing StartConnect");

            // Monitor khi nào startConnect kết thúc thì chuyển sang startLoop
            MonitorTransitionToLoop().Forget();
        }

        /// <summary>
        /// Phát startLoop khi đã join event hoặc sau khi startConnect kết thúc
        /// </summary>
        public void PlayStartLoop()
        {
            if (railMusicSource == null || startLoopClip == null) return;
            if (currentState == MusicState.StartLoop) return;

            currentState = MusicState.StartLoop;
            railMusicSource.clip = startLoopClip;
            railMusicSource.loop = true;
            railMusicSource.Play();

            Debug.Log("[RailMusicManager] Playing StartLoop");
        }

        /// <summary>
        /// Monitor khi startConnect kết thúc để chuyển sang startLoop
        /// </summary>
        private async UniTaskVoid MonitorTransitionToLoop()
        {
            // Đợi cho đến khi startConnect kết thúc
            await UniTask.WaitUntil(() => 
                railMusicSource == null || 
                !railMusicSource.isPlaying || 
                currentState != MusicState.StartConnect
            );

            // Nếu vẫn đang trong state StartConnect và chưa đóng popup thì chuyển sang StartLoop
            if (!isClosing && currentState == MusicState.StartConnect)
            {
                PlayStartLoop();
            }
        }

        /// <summary>
        /// Stop tất cả music và restore background music
        /// </summary>
        public void Stop()
        {
            isClosing = true;

            if (railMusicSource != null)
            {
                if (railMusicSource.isPlaying)
                    railMusicSource.Stop();

                Destroy(railMusicSource.gameObject);
                railMusicSource = null;
            }

            // Restore background music
            audioService.FadeVolume(1f, fadeBackgroundDuration);

            currentState = MusicState.None;
            Debug.Log("[RailMusicManager] Stopped all music");
        }

        // ============ SOUND EFFECTS ============

        /// <summary>
        /// Phát sound khi băng chuyền di chuyển
        /// </summary>
        public void PlayOpenSceneSound()
        {
            if (openSceneSound != null)
            {
                MySonatFramework.audioService.PlayAudio("rail_open_scene", openSceneSound);
                Debug.Log("[RailMusicManager] Playing OpenScene sound");
            }
        }

        /// <summary>
        /// Phát sound khi mở reward (spawn rewards)
        /// </summary>
        public void PlayRewardOpenSound()
        {
            if (rewardOpenSound != null)
            {
                MySonatFramework.audioService.PlayAudio("rail_reward_open", rewardOpenSound);
                Debug.Log("[RailMusicManager] Playing RewardOpen sound");
                
                // Phát các sound delay sau 1 giây
                PlayRewardOpenDelaySounds().Forget();
            }
        }
        
        /// <summary>
        /// Phát 1 sound ngẫu nhiên delay sau khi rewardOpen được 1 giây
        /// </summary>
        private async UniTaskVoid PlayRewardOpenDelaySounds()
        {
            if (rewardOpenDelaySounds == null || rewardOpenDelaySounds.Length == 0) return;
            
            // Đợi 1 giây
            await UniTask.Delay(1000);
            
            // Chọn ngẫu nhiên 1 sound từ list
            int randomIndex = Random.Range(0, rewardOpenDelaySounds.Length);
            AudioClip randomSound = rewardOpenDelaySounds[randomIndex];
            
            if (randomSound != null)
            {
                MySonatFramework.audioService.PlayAudio("", randomSound);
            }
        }

        /// <summary>
        /// Phát sound khi nhân vật walk vào (sau khi ấn start)
        /// </summary>
        public void PlayStartSceneSound()
        {
            if (startSceneSound != null)
            {
                MySonatFramework.audioService.PlayAudio("rail_start_scene", startSceneSound);
                Debug.Log("[RailMusicManager] Playing StartScene sound");
            }
        }

        private void OnDestroy()
        {
            Stop();
        }
    }
}

