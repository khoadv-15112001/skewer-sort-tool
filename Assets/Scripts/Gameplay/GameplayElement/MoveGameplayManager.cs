using System;
using System.Collections;
using Gameplay.Entities;
using Sonat.Enums;
using SonatFramework.Systems.EventBus;
using UnityEngine;

namespace Gameplay.GameplayElement
{
    public class MoveGameplayManager
    {
        private GameplayController gameplayController;
        
        private int moveRemaining;
        private int maxMove;
        
        public bool moveCounting = false;
        
        public Action<int> OnMoveUpdate;
        public Action OnStartCountMove;
        
        private GameState gameState;

        public void Initialize(GameplayController gameplayController)
        {
            this.gameplayController = gameplayController;
            GameplayController.OnLoadLevel += OnLoadLevel;
        }

        public void OnDestroy()
        {
            GameplayController.OnLoadLevel -= OnLoadLevel;
            GameplayController.OnSelectItem -= OnSelectItem;
            GameplayController.OnDropItem -= OnDropItem;
        }

        private void OnLoadLevel(int level)
        {
            GameplayController.OnSelectItem -= OnSelectItem;
            GameplayController.OnSelectItem += OnSelectItem;
            GameplayController.OnDropItem -= OnDropItem;
            GameplayController.OnDropItem += OnDropItem;
        }

        public void OnGameStateChanged(GameState newGameState)
        {
            gameState = newGameState;
        }

        public void OnSelectItem(Item item)
        {
            // Chỉ đếm move khi ở mode Target
            if (!moveCounting && item != null && GameplayController.instance.levelGenerator.LevelMode == LevelMode.Target)
            {
                moveCounting = true;
                StartCountMove(GameplayController.instance.GetLevelMove());
            }
        }

        public void OnDropItem(Item item, bool changed)
        {
            // Chỉ giảm move khi ở mode Target
            if (!moveCounting || !changed || true) return;
            
            // Giảm số lượt di chuyển khi thả item
            moveRemaining--;
            OnMoveUpdate?.Invoke(moveRemaining);
            
            // Check nếu hết lượt
            if (moveRemaining <= 0)
            {
                gameplayController.Stuck(StuckType.OutOfMove).Forget();
            }
        }

        public void ClearData()
        {
            moveRemaining = 0;
            maxMove = 0;
            moveCounting = false;
        }

        public void StartCountMove(int moves)
        {
            GameplayController.OnSelectItem -= OnSelectItem;
            moveRemaining = moves;
            maxMove = moves;
            if (moveRemaining <= 0) moveRemaining = 30; // Default 30 moves
            
            OnStartCountMove?.Invoke();
            OnMoveUpdate?.Invoke(moveRemaining);
        }

        public void AddMove(int moves)
        {
            moveRemaining += moves;
            OnMoveUpdate?.Invoke(moveRemaining);
        }

        public int GetMoveRemaining()
        {
            return moveRemaining;
        }

        public int GetMaxMove()
        {
            return maxMove;
        }
    }
}

