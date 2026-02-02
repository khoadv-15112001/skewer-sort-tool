using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems;
using SonatFramework.Systems.AudioManagement;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace GrillSort.Pinata
{
    public class PopupPinata : Panel
    {
        [SerializeField] private PinataController pinataController;
        [SerializeField] private AudioClip musicPinata;

        private SonatAudioService audioService => SonatSystem.GetService<SonatAudioService>();

        private bool _isClosing;
        private AudioSource _pinataSource;

        public override void OnSetup()
        {
            base.OnSetup();

            _isClosing = false;
            SetupPinata();
            SetupMusic();
        }

        private void SetupPinata()
        {
            pinataController.Setup();
        }

        private void SetupMusic()
        {
            audioService.FadeVolume(0f, 0.4f);

            GameObject audioObj = new GameObject("PinataMusicTemp");
            _pinataSource = audioObj.AddComponent<AudioSource>();
            _pinataSource.clip = musicPinata;
            _pinataSource.loop = false;
            _pinataSource.playOnAwake = false;
            _pinataSource.volume = audioService.IsMuted(AudioTracks.Music) ? 0f : 1f;
            _pinataSource.Play();

            // Giữ sống qua async
            MonitorMusicEndAsync(_pinataSource).Forget();
        }

        private async UniTaskVoid MonitorMusicEndAsync(AudioSource source)
        {
            await UniTask.WaitUntil(() => source == null || !source.isPlaying);

            if (!_isClosing)
            {
                audioService.FadeVolume(1f, 0.4f);
            }

            if (source != null)
            {
                Object.Destroy(source.gameObject);
            }
        }

        public override void Close()
        {
            _isClosing = true;

            if (_pinataSource != null)
            {
                if (_pinataSource.isPlaying)
                    _pinataSource.Stop();

                Object.Destroy(_pinataSource.gameObject);
                _pinataSource = null;
            }

            audioService.FadeVolume(1f, 0.4f);

            base.Close();
        }
    }
}
