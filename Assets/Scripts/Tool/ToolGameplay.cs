using System.Collections;
using System.Collections.Generic;
using SonatFramework.Systems.LevelManagement;
using UnityEngine;

public class ToolGameplay : MonoBehaviour
{
#if UNITY_STANDALONE
    [SerializeField] private LevelService levelService;

    // Start is called before the first frame update
    void Start()
    {
        LevelGenerator.levelService = levelService;
    }
#endif
}