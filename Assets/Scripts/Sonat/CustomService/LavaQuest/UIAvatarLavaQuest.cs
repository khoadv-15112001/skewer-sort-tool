using Cysharp.Threading.Tasks;
using Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GrillSort.LavaQuest
{
    public class UIAvatarLavaQuest : MonoBehaviour
    {
        [SerializeField] private Image frameImg;
        [SerializeField] private Image avtImg;

        public void Init(LavaQuestConfig.Players.Player playerData)
        {
            frameImg.SetSpriteAsync(PathManager.FrameSprite(playerData.GetFrameID())).Forget();
            avtImg.SetSpriteAsync(PathManager.AvatarSprite(playerData.GetAvatarID())).Forget();
        }
    }
}