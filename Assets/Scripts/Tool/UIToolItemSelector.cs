using Gameplay.LevelData;
using Manager;
using SonatFramework.Scripts.UIModule.UIElements;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Tool
{
    public class UIToolItemSelector: MonoBehaviour
    {
        [SerializeField] private FixedImageRatio icon;
        [SerializeField] private TMP_Text text;
        [SerializeField] private GameObject pickedObj;
        [SerializeField] private Image bgTop;
        [SerializeField] private Color[] bgColors;
        private int id;
        private bool selected;
        public int Id => this.id;

        public void Setup(int id)
        {
            this.id = id;
            icon.SetSpriteAsync(PathManager.ItemSprite(id));
            text.text = id.ToString();
            selected = false;
            pickedObj.SetActive(false);
            SetBgSelected(selected);
        }

        public void SetSelected(bool selected)
        {
            this.selected = selected;
            pickedObj.SetActive(selected);
            SetBgSelected(selected);
        }

        public void SelectThis()
        {
            selected = !selected;
            pickedObj.SetActive(selected);
            SetBgSelected(selected);
            ToolItemSelectorPanel.Instance.OnSelectItem(id);
        }

        private void SetBgSelected(bool selected)
        {
            if (PathManager.IsItemOfType5(id))
            {
                bgTop.color = Color.red;
            }
            else
            {
                bgTop.color = !selected ? bgColors[0] : bgColors[1];
            }
            
        }
    }
}