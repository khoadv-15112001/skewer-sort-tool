using SonatFramework.Systems.InventoryManagement.GameResources;
using UnityEngine;

namespace MyGame.SkewerJam.Scripts.SO.SkewerJam.Gameplay
{
    [CreateAssetMenu(menuName = "MyGame/SkewerJam/Game Config HLW", fileName = "GameConfigHLW")]
    public class GameConfigHLW : ScriptableObject
    {
        public ResourceData unlockTrayPrice;
        public ResourceData unlockPlatePrice;
        public ResourceData unlockOctoChefPrice;
    }
}