using Sirenix.OdinInspector;
using Sonat;
using Sonat.Enums;
using SonatFramework.Systems.ConfigManagement;
using SonatFramework.Systems.InventoryManagement;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ProfileConfig", menuName = "My Configs/ProfileConfig")]
public class ProfileConfig : ConfigSo
{
    public List<Avatar> avatars = new();
    public List<Frame> frames = new();
    public List<Badge> badges = new();

    public Badge selfBadgeDefault;

    public int numAvatar => avatars.Count;
    public int numFrame => frames.Count;
    public int numBadge => badges.Count;

    public int nameLimitCharacter = 12;

    [Serializable]
    public class BaseData
    {
        [GUIColor(0, 1, 0, 1)]
        [ReadOnly]
        public int ID;

        public bool mustCondition;
        [ShowIf(@"mustCondition")]
        [ValueDropdown(nameof(GetAllTerms))]
        public string termSource;
        [ShowIf(@"mustCondition")]
        public List<GameResource> resources = new();

        public bool IsAvailable()
        {
            if (!mustCondition)
                return true;

            if (resources == null || resources.Count == 0)
                return true;

            var inventory = MySonatFramework.GetService<InventoryService>();

            foreach (var res in resources)
            {
                if (res == GameResource.MAX || res == GameResource.None)
                    continue;

                if (inventory.GetResource(res) > 0)
                    return true;
            }

            return false;
        }

        public IEnumerable<string> GetAllTerms()
        {
            return LocalizationUtils.GetAllTerms();
        }
    }


    [Serializable]
    public class Avatar : BaseData
    {

    }

    [Serializable]
    public class Frame : BaseData
    {

    }

    [Serializable]
    public class Badge : BaseData
    {
        public Sprite sprite;
        public Sprite icoMedal;
        public Material nameMaterial;
        public Material rankMaterial;
        public Color colorName;
        public Color colorLevel;
    }

#if UNITY_EDITOR
    public void OnValidate()
    {
        for (int i = 0; i < avatars.Count; i++)
        {
            avatars[i].ID = i;
        }

        for (int i = 0; i < frames.Count; i++)
        {
            frames[i].ID = i;
        }

        for (int i = 0; i < badges.Count; i++)
        {
            badges[i].ID = i;
        }
    }
#endif

}