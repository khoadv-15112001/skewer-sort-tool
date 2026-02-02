using UnityEngine;
using SonatFramework.Scripts.Helper;
using System.Collections.Generic;
using System;
using SonatFramework.Systems;

namespace GrillSort.BattlePass
{
    public abstract class BattlePassBase : ScriptableObject
    {
        [SerializeField] public BattlePassConfig config;
        [SerializeField] protected virtual string DATA_KEY => "BATTLEPASS_DATA";

        private readonly Service<BattlePassService> _battlePassService = new();
        
        protected ListDataPref<int> _listReceivedReward;
        protected IntDataPref _isUnlocked;
        public virtual void Initialize()
        {
            _listReceivedReward = new ListDataPref<int>(DATA_KEY + "_listReceivedReward");
            _isUnlocked = new IntDataPref(DATA_KEY + "_isUnlocked", 0);
        }

        public void ReceiveReward(int idx)
        {
            _listReceivedReward.Add(idx);
            _battlePassService.Instance.OnUpdateUI?.Invoke(-1);
        }

        public bool CheckReceivedReward(int index)
        {
            return _listReceivedReward.Value.Contains(index);
        }

        public bool IsUnlocked()
        {
            return _isUnlocked.Value == 1;
        }

        public void Unlock()
        {
            _isUnlocked.Value = 1;
        }

        public virtual void Reset()
        {
            _listReceivedReward.Value = new List<int>();
        }

        public bool CanClaim(int currentMilestoneIdx)
        {
            if(IsUnlocked() == false) return false;
            return _listReceivedReward.Value.Count < currentMilestoneIdx + 1;
        }
    }
}