using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GrillSort.LoseRwdService;
namespace GrillSort.UI
{
    public class LoseRwdUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button watchAdButton;
        [SerializeField] private GameObject[] stageNodes;
        [SerializeField] private TMP_Text[] stageTexts;
        [SerializeField] private GameObject activeHighlight;

        [Header("Colors")]
        [SerializeField] private Color activeColor = Color.red;
        [SerializeField] private Color inactiveColor = Color.gray;

        public void RefreshUI(LoseRwdConfig config, int currentStage)
        {
            int maxStage = config.maxStage;

            if (activeHighlight != null)
                activeHighlight.SetActive(false);

            for (int i = 0; i < stageNodes.Length; i++)
            {
                bool isVisible = i < maxStage;
                stageNodes[i].SetActive(isVisible);

                if (!isVisible) continue;

                stageTexts[i].color = inactiveColor;

                if (i < config.datas.Count)
                {
                    var data = config.datas[i];
                    stageTexts[i].text = $"+{data.addTimeSeconds}s";
                }

                bool isCurrent = (currentStage == i + 1);

                if (isCurrent)
                {
                    stageTexts[i].color = activeColor;
                    if (activeHighlight != null)
                    {
                        activeHighlight.transform.parent = stageNodes[i].transform;

                        activeHighlight.transform.localScale = Vector3.one;

                        activeHighlight.transform.localPosition = Vector3.zero;

                        activeHighlight.SetActive(true);
                    }
                }
            }

            watchAdButton.gameObject.SetActive(currentStage <= maxStage);
        }
    }
}
