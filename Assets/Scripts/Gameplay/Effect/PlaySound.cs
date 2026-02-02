using Sonat.Enums;
using SonatFramework.Scripts.Utils;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    [SerializeField] private AudioId audioId;
    [SerializeField] private float delay;

    void OnEnable()
    {
        SonatUtils.DelayCall(delay, ()=>{
            MySonatFramework.audioService.PlaySound(audioId);
        }, this);
    }
}
