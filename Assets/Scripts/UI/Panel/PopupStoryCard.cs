using I2.Loc;
using SonatFramework.Scripts.UIModule;
using SonatFramework.Systems.AudioManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.Story
{
    public class PopupStoryCard : Panel
    {
        [SerializeField] private Image backgroundImg;
        [SerializeField] private UISlideToWatch slideToWatch;

        [SerializeField] private AudioClip audioClip;
        [SerializeField] private ParticleSystem eff;

        [SerializeField] private Localize[] titleLocalizes;

        [HideInInspector]
        public StoryBase Story;

        public override void OnSetup()
        {
            base.OnSetup();
        }

        public override void Open(UIData uiData)
        {
            base.Open(uiData);

            MySonatFramework.GetService<SonatAudioService>().FadeVolume(0f, 0f);

            MySonatFramework.audioService.PlayAudio("", audioClip);

            Story = uiData.Get<StoryBase>("story");

            backgroundImg.sprite = Story.GetData().ImageConfig.BackgroundHomeSprite;

            foreach (var localize in titleLocalizes)
            {
                localize.SetTerm(Story.GetData().Name);
            }

            eff.Play();
        }
    }
}