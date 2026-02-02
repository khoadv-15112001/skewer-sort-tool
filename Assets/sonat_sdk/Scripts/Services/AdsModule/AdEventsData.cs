namespace Sonat.AdsModule
{

    public class AdEventData
    {
        public AdUnit adUnit;
    }

    
    public class AdLoadedData : AdEventData
    {
        public AdLoadedData(AdUnit adUnit)
        {
            this.adUnit = adUnit;
        }
    }

    public class AdLoadFailedData: AdEventData
    {
        public string message;
        public int errorCode;
        
        public AdLoadFailedData(AdUnit adUnit, string message, int errorCode)
        {
            this.adUnit = adUnit;
            this.message = message;
            this.errorCode = errorCode;
        }
    }
    
    public class AdShowFailedData: AdEventData
    {
        public int errorCode;
        public AdShowFailedData(AdUnit adUnit, int errorCode)
        {
            this.adUnit = adUnit;
            this.errorCode = errorCode;
        }
    }

    public class AdOpenedData: AdEventData
    {
        public AdOpenedData(AdUnit adUnit)
        {
            this.adUnit = adUnit;
        }
    }

    public class AdClosedData: AdEventData
    {
        public AdClosedData(AdUnit adUnit)
        {
            this.adUnit = adUnit;
        }
    }
    
    public class AdClickedData: AdEventData
    {
        public AdClickedData(AdUnit adUnit)
        {
            this.adUnit = adUnit;
        }
    }

    public class EarnedRewardData: AdEventData
    {
        public EarnedRewardData(AdUnit adUnit)
        {
            this.adUnit = adUnit;
        }
    }
    
    public class AdPaidData: AdEventData
    {
        public double value;
        public string precision;
        public string currencyCode;
        
        public AdPaidData(AdUnit adUnit, double value, string precision, string currencyCode)
        {
            this.adUnit = adUnit;
            this.value = value;
            this.precision = precision;
            this.currencyCode = currencyCode;
        }
    }
}