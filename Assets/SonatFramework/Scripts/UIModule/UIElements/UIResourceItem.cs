using MyGame.SkewerJam.Scripts.Service;
using Sonat.Enums;
using SonatFramework.Scripts.UIModule.SpriteService;
using SonatFramework.Scripts.UIModule.UIElements;
using SonatFramework.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class UIResourceItem : MonoBehaviour
{
    [SerializeField] private FixedImageRatio icon;
    [SerializeField] private GameObject coin3D;
    [SerializeField] private TMP_Text txtQuantity;

    [SerializeField] private Service<SpriteAtlasService> spriteAtlasService = new();
    private GameResource resource;
    public Transform Transform => transform;

    public void SetData(GameResource resource, int quantity = 0)
    {
        this.resource = resource;
        if (txtQuantity)
            txtQuantity.text = quantity.ToString();
        
        // Toggle between 3D coin and 2D icon based on resource type
        bool isCoin = this.resource == GameResource.Coin;
        
        if (coin3D)
            coin3D.SetActive(isCoin);
        
        if (icon)
        {
            icon.gameObject.SetActive(!isCoin);
            if (!isCoin)
            {
                // if(resource == GameResource.ItemEvent)
                //     icon.sprite = spriteAtlasService.Instance.GetSprite($"ico_{MySonatFramework.GetService<HLWEventService>().GetCurrentThemeByPopup()}");
                // else
                    icon.sprite = spriteAtlasService.Instance.GetSprite($"ico_{this.resource}");
            }
        }
    }
    
    public FixedImageRatio Icon => icon;
    
    public GameObject CurrentVisualObject
    {
        get
        {
            if (resource == GameResource.Coin && coin3D != null)
                return coin3D;
            return icon != null ? icon.gameObject : null;
        }
    }
}