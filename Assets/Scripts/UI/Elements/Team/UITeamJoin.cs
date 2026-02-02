using UnityEngine;

public class UITeamJoin : MonoBehaviour
{
    [SerializeField] private UITeamListScrollView scroll;

    [Header("Request config")]
    [SerializeField] private int totalRequest = 10;

    private void OnEnable()
    {
        scroll.LoadScroll(new GetListTeamsRequest
        {
            limit = totalRequest,
        });
    }
}
