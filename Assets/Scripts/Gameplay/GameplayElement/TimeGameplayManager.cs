using System;
using System.Collections;
using Gameplay.Entities;
using Sonat.Enums;
using SonatFramework.Systems.EventBus;
using UnityEngine;

namespace Gameplay.GameplayElement
{
    public class TimeGameplayManager
    {

        private GameplayController gameplayController;
        public bool isFreeze = false;

        public int timeFreeze;
        private Coroutine freezeCoroutine;
        private float timeRemaining;
        
        private Coroutine countTimeCoroutine;
        public bool timeCounting = false;
        
        public Action<float> OnTimeUpdate;
        public Action<bool> OnFreeze;
        public Action OnStartCountTime;
        
        private GameState gameState;
        private bool runTime = true;
        private float timeRunning = 0f;

        public void Initialize(GameplayController gameplayController)
        {
            this.gameplayController = gameplayController;
            GameplayController.OnLoadLevel += OnLoadLevel;
        }

        public void OnDestroy()
        {
            GameplayController.OnLoadLevel -= OnLoadLevel;
            GameplayController.OnSelectItem -= OnSelectItem;
        }

        private void OnLoadLevel(int level)
        {
            GameplayController.OnSelectItem -= OnSelectItem;
            GameplayController.OnSelectItem += OnSelectItem;
        }
        

        public void OnGameStateChanged(GameState newGameState)
        {
            gameState = newGameState;
            runTime = gameState is GameState.Playing or GameState.UsingBooster;
        }

        public void OnSelectItem(Item item)
        {
            if (!timeCounting)
            {
                timeCounting = true;
                StartCountTime(GameplayController.instance.GetLevelTime());
            }
        }

        public void ClearData()
        {
            isFreeze = false;
            timeFreeze = 0;
            timeRemaining = 0;
            timeRunning = 0f;
            if(countTimeCoroutine != null) gameplayController.StopCoroutine(countTimeCoroutine);
            countTimeCoroutine = null;
            if(freezeCoroutine != null) gameplayController.StopCoroutine(freezeCoroutine);
            freezeCoroutine = null;
            OnFreeze?.Invoke(isFreeze);
        }

        public void StartCountTime(int time)
        {
            GameplayController.OnSelectItem -= OnSelectItem;
            timeRemaining = time;
            timeRunning = 0;
            if (timeRemaining < 10) timeRemaining = 180;
            if (countTimeCoroutine != null)
            {
                gameplayController.StopCoroutine(countTimeCoroutine);
            }

            countTimeCoroutine = gameplayController.StartCoroutine(CountTime());
            OnStartCountTime?.Invoke();
        }

        public void CheckCountTime()
        {
            countTimeCoroutine ??= gameplayController.StartCoroutine(CountTime());
        }

        public void AddTime(int seconds)
        {
            timeRemaining += seconds;
            if (countTimeCoroutine == null)
            {
                countTimeCoroutine = gameplayController.StartCoroutine(CountTime());
            }
        }

        private IEnumerator CountTime()
        {
            yield return new WaitForSeconds(1);
            while (timeRemaining > 0)
            {
                if (runTime)
                {
                    if (!isFreeze)
                    {
                        timeRemaining -= Time.deltaTime;
                        OnTimeUpdate?.Invoke(timeRemaining);
                    }
                    timeRunning += Time.deltaTime;
                }
                else if (gameState == GameState.GameOver)
                {
                    countTimeCoroutine = null;
                    yield break;
                }

                yield return null;
            }

            yield return new WaitForSeconds(1);
            if (gameState == GameState.GameOver)
            {
                countTimeCoroutine = null;
                yield break;
            }

            OnTimeUpdate?.Invoke(timeRemaining);
            gameplayController.Stuck(StuckType.OutOfTime).Forget();
            gameplayController.StopCoroutine(countTimeCoroutine);
            countTimeCoroutine = null;
        }

        public void SetTimeRemaining(float time)
        {
            timeRemaining = time;
        }
        
        public float GetTimeRemaining()
        {
            return timeRemaining;
        }

        public float GetTimeRunning()
        {
            return timeRunning;
        }

        public void Freeze()
        {
            timeFreeze += 20;
            isFreeze = true;
            freezeCoroutine ??= gameplayController.StartCoroutine(IEFreeze());
        }
        
        private IEnumerator IEFreeze()
        {
            OnFreeze?.Invoke(isFreeze);
            while (timeFreeze > 0)
            {
                if (runTime)
                {
                    timeFreeze--;
                }
                else if (gameState == GameState.GameOver)
                {
                    freezeCoroutine = null;
                    yield break;
                }

                yield return new WaitForSeconds(1);
            }

            freezeCoroutine = null;
            timeFreeze = 0;
            isFreeze = false;
            OnFreeze?.Invoke(isFreeze);
        }

        internal void AddTime(object value)
        {
            throw new NotImplementedException();
        }
    }
}