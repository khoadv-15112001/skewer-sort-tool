using System.Collections.Generic;
using System.Linq;
using Gameplay.Entities;
using Gameplay.Entities.Grills;
using Gameplay.LevelData;
using Manager;
using MyGame.SkewerJam.Gameplay;
using UnityEngine;

namespace MyGame.SkewerJam.Objects
{
    public class GrillManager : MonoBehaviour
    {
        private List<PrimaryGrill> listGrills = new List<PrimaryGrill>();
        public List<PrimaryGrill> ListGrills => listGrills;

        void Start()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        public void Init()
        {

        }

        public void Clear()
        {
            foreach (var grill in listGrills)
            {
                GameFactory.Instance.ReturnEntity(grill);
            }
            listGrills.Clear();
        }

        public void AddGrill(PrimaryGrill grill)
        {
            listGrills.Add(grill);
        }

        public PrimaryGrill GetGrill(int id)
        {
            return listGrills.Find(e => e.id == id);
        }

        public List<PrimaryGrill> FindObstacleGrills(List<int> grillIds)
        {
            return listGrills.FindAll(e => grillIds.Contains(e.id));
        }

        public List<Item> GetItemsWithLayer(int layer = 1)
        {
            List<Item> items = new List<Item>();
            foreach (var primaryGrill in listGrills)
            {
                foreach (var slot in primaryGrill.GetSlots())
                {
                    var item = slot.GetItem();
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
            }
            return items;
        }

        public bool CheckClearAllItems()
        {
            foreach (var grill in listGrills)
            {
                foreach (var slot in grill.GetSlots())
                {
                    if (slot.GetItem() != null) return false;
                }
            }
            return true;
        }

        public List<GrillData> GetGrillData()
        {
            return listGrills.Select(e => e.GetGrillData()).ToList();
        }

        public List<PrimaryGrillIceStateData> GetIceState()
        {
            var states = new List<PrimaryGrillIceStateData>();
            foreach (var grill in listGrills)
            {
                if (grill is PrimaryGrillIce primaryGrillIce)
                {
                    var state = new PrimaryGrillIceStateData() { id = primaryGrillIce.id, currentState = primaryGrillIce.CurrentState, numStep = primaryGrillIce.NumStep };
                    states.Add(state);
                }
            }
            return states;
        }

        public void SetIceState(List<PrimaryGrillIceStateData> primaryGrillIces)
        {
            foreach (var state in primaryGrillIces)
            {
                var grill = GetGrill(state.id);
                if (grill is PrimaryGrillIce primaryGrillIce)
                {
                    primaryGrillIce.SetIceState(state.currentState, state.numStep);
                }
            }
        }
    }
}