// using System.Collections.Generic;
// using System.Linq;
// using sonat_sdk_v2.Attributes;
// using sonat_sdk_v2.Enums;
// using UnityEngine;
//
// namespace sonat_sdk_v2.FirebaseModule.RemoteConfig
// {
//     [CreateAssetMenu(menuName = "SonatSDK/Database/RemoteDatabase", fileName = "DefaultRemoteDatabase")]
//     public class DefaultRemoteDatabase : ScriptableObject
//     {
//         public FirebaseDefaultSettings setting;
//
//         [ArrayElementTitle(nameof(RemoteConfigDefaultByKey.key))]
//         public List<RemoteConfigDefaultByKey> defaultConfigs = new List<RemoteConfigDefaultByKey>();
//
//         public RemoteConfigKey[] listIntCollectionKeys;
//         public RemoteConfigKey[] listStringCollectionKeys;
//
//         [SerializeField] private DefaultRemoteDatabase other;
//
//          [ContextMenu("copy")]
//          private void Copy()
//          {
//              for (var i = 0; i < other.defaultConfigs.Count; i++)
//              {
//                  if (defaultConfigs.All(x => x.key != other.defaultConfigs[i].key))
//                      defaultConfigs.Add(other.defaultConfigs[i]);
//              }
//          }
//
//          [ContextMenu("buyLogData")]
//          private void Log()
//          {
//              Debug.Log(setting.LogPaidAd.Value);
//          }
//     }
// }