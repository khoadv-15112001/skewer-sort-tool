using System;

namespace Sonat
{
    public interface ISonatService
    {
        public SonatServiceType ServiceType { get;}
        public bool Ready { get; set; }
        
        public void Initialize(Action<ISonatService> onInitialized);
        
        public void OnApplicationFocus(bool focus);
        public void OnApplicationPause(bool pause);
        public void OnApplicationQuit();
    }

    public enum SonatServiceType: byte
    {
        AdsService,
        IapService,
        FirebaseService,
        //RemoteConfig,
        TrackingService,
        AppsFlyerService,
        FacebookService
    }
}