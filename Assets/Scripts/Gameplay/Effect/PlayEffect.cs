using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using SonatFramework.Scripts.Utils;
using UnityEngine;

public class PlayEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;
    [SerializeField] private float delay;
    [SerializeField] private bool off = false;
    [SerializeField, ShowIf("off")] private float duration = 1;

    private void OnEnable()
    {
        StartEffect().Forget();
    }

    private async UniTask StartEffect()
    {
        await UniTask.Delay((int)(delay * 1000));
        ps.gameObject.SetActive(true);
        ps.Play();
        if (off)
        {
            await UniTask.Delay((int)(duration * 1000));
            ps.Stop();
            ps.gameObject.SetActive(false);
        }

    }
}
