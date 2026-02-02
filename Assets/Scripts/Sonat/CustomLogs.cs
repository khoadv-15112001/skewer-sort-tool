using System;
using System.Collections.Generic;
using Sonat;
using Sonat.Debugger;
using Sonat.TrackingModule;
using UnityEngine.Serialization;

namespace MyFramework.Sonat
{
    [Serializable]
    public class SonatLogOrderEnd : SonatLogBasicMode
    {
        public override string EventName => "order_end";

        public int remainTime;
        public int completeTime;
        public float completion;
        public int orderNumber;
        public string success;
        public string placement;
        public float orderCompletion;
        public int defaultTime;
        public int useBoosterCount;
        public int startCount;
        public int playTime;

        protected override List<LogParameter> GetParameters()
        {
            List<LogParameter> parameters = BaseLogs();

            parameters.Add(new LogParameter("remain_time", remainTime));
            parameters.Add(new LogParameter("complete_time", completeTime));
            parameters.Add(new LogParameter("completion", completion));
            parameters.Add(new LogParameter("order_number", orderNumber));
            parameters.Add(new LogParameter("success", success));
            parameters.Add(new LogParameter("placement", placement));
            parameters.Add(new LogParameter("order_completion", orderCompletion));
            parameters.Add(new LogParameter("default_time", defaultTime));
            parameters.Add(new LogParameter("use_booster_count", useBoosterCount));
            parameters.Add(new LogParameter("start_count", startCount));
            parameters.Add(new LogParameter("playtime", playTime));
            return parameters;
        }
    }
}