using System;
using System.Collections;
using System.Collections.Generic;
using Manager;
using Sonat.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIItemGoal : MonoBehaviour
{
    public Image backGroundImg;
    public Image iconImg;
    public TMP_Text amountTmp;

    public Sprite bgNormal;
    public Sprite bgHard;
    public Sprite bgSuperHard;
    public Sprite bgNightmare;

    public GameObject tickIcon;

    public ParticleSystem effect;

    public bool isCompleted = false;

    public void SetData(int id, int amount)
    {
        _ = iconImg.SetSpriteAsync(PathManager.ItemSprite(id));
        amountTmp.text = amount.ToString();
        amountTmp.SetMaterial(GameplayController.instance.levelGenerator.LevelData.difficulty);
        amountTmp.gameObject.SetActive(true);
        tickIcon.SetActive(false);
        isCompleted = false;
        SetBg();
    }

    public void OnUpdateProgress(int amount)
    {
        amountTmp.text = amount.ToString();
        effect.Play();
    }

    public void OnComplete()
    {
        isCompleted = true;
        tickIcon.SetActive(true);
        amountTmp.gameObject.SetActive(false);
    }

    private void SetBg()
    {
        switch (GameplayController.instance.levelGenerator.LevelData.difficulty)
        {
            case LevelDifficulty.Easy:
                backGroundImg.sprite = bgNormal;
                break;
            case LevelDifficulty.Hard:
                backGroundImg.sprite = bgHard;
                break;
            case LevelDifficulty.SuperHard:
                backGroundImg.sprite = bgSuperHard;
                break;
            case LevelDifficulty.NightmarishlyHard:
                backGroundImg.sprite = bgNightmare;
                break;
            default:
                backGroundImg.sprite = bgNormal;
                break;
        }
    }
}
