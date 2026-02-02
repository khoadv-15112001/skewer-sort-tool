using UnityEngine;

namespace SonatFramework.Systems.ObjectPooling
{
    [CreateAssetMenu(fileName = "SonatPoolingContainer", menuName = "Sonat Services/Pooling/Pooling Container")]
    public class SonatPoolingContainer : PoolingContainerService
    {
        public override T CreateObject<T>(Transform container)
        {
            for (var i = 1; i < container.childCount; i++)
            {
                var child = container.GetChild(i);
                if (!child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true);
                    child.transform.SetAsLastSibling();
                    return child.GetComponent<T>();
                }
            }


            var obj = Object.Instantiate(container.GetChild(0).gameObject, container);
            obj.SetActive(true);
            return obj.GetComponent<T>();
        }

        public override void CleanContainer(Transform container)
        {
            for (var i = 0; i < container.childCount; i++) container.GetChild(i).gameObject.SetActive(false);
        }
    }
}