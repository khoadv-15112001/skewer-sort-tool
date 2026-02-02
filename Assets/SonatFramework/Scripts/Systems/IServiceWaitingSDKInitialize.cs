using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IServiceWaitingSDKInitialize
{
    public void OnSonatSDKInitialize();
}

public interface IServiceWaitingRemoteConfig
{
    public void OnRemoteConfigReady();
}


public interface IServiceApplicationFocus
{
    public void OnApplicationFocus(bool focus);
}