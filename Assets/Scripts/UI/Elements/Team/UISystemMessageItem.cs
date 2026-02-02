using I2.Loc;
using TMPro;
using UnityEngine;

public class UISystemMessageItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI content;
    [SerializeField] private Localize contentLocalize;
    [SerializeField] private LocalizationParamsManager param;

    public void Bind(TeamMessageData data)
    {
        try
        {
            switch (data.type)
            {
                case TeamMessageType.join_success:
                    contentLocalize.SetTerm("message_join_team");
                    param.SetParameterValue("VALUE", data.sender.name);
                    break;

                case TeamMessageType.leave_success:
                    contentLocalize.SetTerm("message_left_team");
                    param.SetParameterValue("VALUE", data.sender.name);
                    break;

                case TeamMessageType.member_removed:
                    contentLocalize.SetTerm("message_kick_out");
                    param.SetParameterValue("VALUE", data.sender.name);
                    break;

                case TeamMessageType.co_leader_promote:
                    contentLocalize.SetTerm("message_promote");
                    param.SetParameterValue("VALUE", data.sender.name);
                    break;

                case TeamMessageType.co_leader_demote:
                    contentLocalize.SetTerm("message_demote");
                    param.SetParameterValue("VALUE", data.sender.name);
                    break;
            }
        }
        catch
        {
            switch (data.type)
            {
                case TeamMessageType.join_success:
                    content.text = $"{data.sender.name} has joined the team!";
                    break;

                case TeamMessageType.leave_success:
                    content.text = $"{data.sender.name} has left the team!";
                    break;

                case TeamMessageType.member_removed:
                    content.text = $"{data.sender.name} has been kicked out of the team!";
                    break;

                case TeamMessageType.co_leader_promote:
                    content.text = $"{data.sender.name} has been promoted to team Co-leader!";
                    break;

                case TeamMessageType.co_leader_demote:
                    content.text = $"{data.sender.name} has been demoted!";
                    break;
            }
        }
    }
}
