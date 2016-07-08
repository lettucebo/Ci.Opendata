using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Creatidea.Opendata.Taipei
{
    /// <summary>
    /// 公車資訊
    /// </summary>
    public class Bus
    {
        /// <summary>
        /// 到站時間
        /// </summary>
        /// <seealso cref="Creatidea.Opendata.OpenData" />
        public class EstimateTime : OpenData
        {
            public override JObject Data()
            {
                var jsonString = Tool.GetWebContent("http://data.taipei/bus/EstimateTime", Encoding.UTF8, gZip: true, onlyGzip: true);

                var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                return jObject;
            }

            protected override void ToMemory(JObject jObj)
            {
                object newLock = new object();
                var busStopEstimateTimeList = new Dictionary<string, JToken>();

                Parallel.ForEach(jObj["BusInfo"], (item, loopState) =>
                {
                    var routeId = int.Parse(item["RouteID"].ToString());
                    var stopId = int.Parse(item["StopID"].ToString());

                    var stringFormat = string.Format(BusStopEstimateTimeKeyFormat, routeId, stopId);

                    lock (newLock)
                    {
                        if (busStopEstimateTimeList.ContainsKey(stringFormat))
                        {
                            busStopEstimateTimeList[stringFormat] = item;
                        }
                        else
                        {
                            busStopEstimateTimeList.Add(stringFormat, item);
                        }
                    }
                });

                lock (LockObj)
                {
                    _busStopEstimateTimeList = busStopEstimateTimeList;
                }
            }

            public override void Dispose()
            {
                lock (LockObj)
                {
                    _busStopEstimateTimeList.Clear();
                    _busStopEstimateTimeList = null;
                }
            }

            /// <summary>
            /// 取得到站時間
            /// </summary>
            /// <param name="routeId">路線代碼</param>
            /// <param name="stopId">站牌代碼</param>
            /// <returns>巴士到站時間（單位：秒） -1：尚未發車 -2：交管不停靠 -3：末班車已過 -4：今日未營運 GoBack	去返程 （0：去程 1：返程 2：尚未發車 3：末班已駛離）</returns>
            public static int Get(int routeId, int stopId)
            {
                var stringFormat = string.Format(BusStopEstimateTimeKeyFormat, routeId, stopId);
                var inTime = int.MinValue;
                lock (StaticLockObj)
                {
                    if (_busStopEstimateTimeList.ContainsKey(stringFormat))
                    {
                        inTime = int.Parse(Convert.ToString(_busStopEstimateTimeList[stringFormat]["EstimateTime"]));
                    }
                }

                return inTime;
            }

            /// <summary>
            /// 到站時間Key格式
            /// </summary>
            private const string BusStopEstimateTimeKeyFormat = "r{0}s{1}";

            /// <summary>
            /// 到站時間Json資料
            /// </summary>
            private static Dictionary<string, JToken> _busStopEstimateTimeList = new Dictionary<string, JToken>();

        }

        /// <summary>
        /// 站牌
        /// </summary>
        /// <seealso cref="Creatidea.Opendata.OpenData" />
        public class Stop : OpenData
        {
            public override JObject Data()
            {
                var jsonString = Tool.GetWebContent("http://data.taipei/bus/Stop", Encoding.UTF8, gZip: true, onlyGzip: true);

                var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                return jObject;
            }

            protected override void ToMemory(JObject jObj)
            {
                throw new NotImplementedException();
            }

            public override void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }

    /// <summary>
    /// 公車資訊排程
    /// </summary>
    public class BusSchedule
    {
        /// <summary>
        /// 即時資訊排程
        /// </summary>
        /// <seealso cref="OpenDataSchedule" />
        public class EstimateTime : OpenDataSchedule
        {
            private Bus.EstimateTime _main = new Bus.EstimateTime();

            public override void Run()
            {
                _main.DataToMemory();
            }

            public override void Dispose()
            {
                _main.Dispose();
                _main = null;
            }
        }

        /// <summary>
        /// 站牌資訊排程
        /// </summary>
        /// <seealso cref="OpenDataSchedule" />
        public class Stop : OpenDataSchedule
        {
            public override void Run()
            {
            }

            public override void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}
