using System;
using System.Collections.Generic;
using System.Linq;

namespace Sonat.AdsModule
{
    [Serializable]
    public class AdsConfig
    {
        public string appId;
        public List<AdUnitId> adUnitIds = new();

        public AdUnitId GetAdUnitId(AdType adType)
        {
            AdUnitId adUnitId = adUnitIds.FirstOrDefault(ad => ad.adType == adType);
            return adUnitId;
        }
    }

    [Serializable]
    public class AdUnitId
    {
        public AdPlacement placement;
        public string id;
        public AdType adType;
    }
}