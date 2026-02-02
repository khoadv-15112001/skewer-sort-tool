using System.Collections;
using System.Collections.Generic;
using SonatFramework.Systems;
using SonatFramework.Systems.ObjectPooling;
using UnityEngine;

public class PoolingServiceBase : SonatServiceSo, IServiceInitialize
{
    protected static readonly Dictionary<string, Queue<IPoolingObject>> Pool = new();
    protected static readonly Dictionary<string, GameObject> ObjPrefs = new();
    protected static GameObject pool;
    
    public virtual void Initialize()
    {
        if (pool == null)
        {
            pool = new GameObject("Pool");
            pool.AddComponent<DontDestroyOnLoadObject>();
            Pool.Clear();
            ObjPrefs.Clear();
        }
    }
}
