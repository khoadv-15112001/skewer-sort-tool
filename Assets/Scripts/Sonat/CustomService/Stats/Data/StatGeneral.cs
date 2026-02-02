using Cysharp.Threading.Tasks;
using UnityEngine;


[CreateAssetMenu(fileName = "StatGeneral", menuName = "My Services/StatGeneral")]
public class StatGeneral : StatBase
{
    public override void Initialize()
    {

    }

    public override void AddValue(int amount)
    {
        UploadToServer();

        Debug.Log($"[StatGeneral] +{amount} to {StatType} (Total: {GetValue()})");
    }

    public override void SetValue(int amount)
    {
        UploadToServer();

        Debug.Log($"[StatGeneral] Set {amount} to {StatType} (Total: {GetValue()})");
    }

    public override int GetValue()
    {
        return 0;
    }

    public override void UploadToServer()
    {
        statsService.SetGeneralStats(GetGeneralStats());
        statsService.SaveLocal();
        statsService.UploadToServer().Forget();
    }

    public override void CheckSyncLocal()
    {
    }
}
