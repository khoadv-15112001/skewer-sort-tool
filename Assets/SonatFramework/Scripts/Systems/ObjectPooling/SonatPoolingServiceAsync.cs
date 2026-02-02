using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using SonatFramework.Systems.LoadObject;
using UnityEngine;

namespace SonatFramework.Systems.ObjectPooling
{
    [CreateAssetMenu(fileName = "SonatPoolingServiceAsync", menuName = "Sonat Services/Pooling/Pooling Service Async")]
    public class SonatPoolingServiceAsync : PoolingServiceAsync
    {
        [SerializeField] private Service<LoadObjectServiceAsync> loadObjectService = new();
        private readonly HashSet<string> loadingObjects = new();

        public override void ReturnObj(IPoolingObject obj, bool keep = true)
        {
            if (!keep)
            {
                obj.OnReturnObj();
                Object.Destroy(obj.transform.gameObject);
            }
            else
            {
                obj.OnReturnObj();
                obj.transform.DOKill();
                obj.transform.gameObject.SetActive(false);
                obj.transform.SetParent(pool.transform);
                if (Pool.TryGetValue(obj.transform.name, out var queue))
                {
                    queue.Enqueue(obj);
                    return;
                }

                queue = new Queue<IPoolingObject>();
                queue.Enqueue(obj);
                Pool.Add(obj.transform.name, queue);
            }
        }

        public override async UniTask<T> CreateAsync<T>(string objectName, Vector3 position, Transform parent = null, params object[] args)
        {
            var res = await CreateAsync<T>(objectName, parent, args);
            res.transform.position = position;
            return res;
        }


        public override async UniTask<T> CreateAsync<T>(string objectName, Transform parent = null, params object[] args)
        {
            if (Pool.TryGetValue(objectName, out var queue))
                if (queue.TryDequeue(out var obj) && !obj.transform.gameObject.activeSelf)
                {
                    obj.transform.gameObject.SetActive(true);
                    obj.transform.SetParent(parent);
                    obj.OnCreateObj(args);
                    return (T)obj;
                }

            if (!ObjPrefs.TryGetValue(objectName, out var objPref))
            {
                if (loadingObjects.Contains(objectName))
                {
                    await UniTask.WaitWhile(() => loadingObjects.Contains(objectName));
                    objPref = ObjPrefs[objectName];
                }
                else
                {
                    loadingObjects.Add(objectName);
                    objPref = await loadObjectService.Instance.LoadAsync<GameObject>(objectName);
                    ObjPrefs.Add(objectName, objPref);
                    loadingObjects.Remove(objectName);
                }
            }

            var gameObj = Object.Instantiate(objPref, parent);
            gameObj.name = objectName;
            var res2 = gameObj.GetComponent<T>();
            res2.Setup();
            res2.OnCreateObj(args);
            return res2;
        }
    }
}