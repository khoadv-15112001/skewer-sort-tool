using TMPro;
using UnityEngine;

public class UITeamSearch : MonoBehaviour
{
    [SerializeField] private TMP_InputField searchField;

    [SerializeField] private UITeamListScrollView scroll;

    [Header("Request config")]
    [SerializeField] private int totalRequest = 20;

    private void OnEnable()
    {
        scroll.gameObject.SetActive(false);
    }

    public void ClickClearSearch()
    {
        searchField.text = "";
    }

    public void ClickSearch()
    {
        scroll.gameObject.SetActive(true);
        scroll.LoadScroll(new GetListTeamsRequest()
        {
            limit = totalRequest,
            search = searchField.text,
        });
    }
}
