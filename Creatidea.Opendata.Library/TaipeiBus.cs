﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Creatidea.Opendata
{
    public class TaipeiBus
    {
        public JObject EstimateTime()
        {
            var jsonString = Tool.GetWebContent("http://data.taipei/bus/EstimateTime", Encoding.UTF8, gZip: true, onlyGzip: true);

            var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

            return jObject;
        }

        public JObject StopSign()
        {
            var jsonString = Tool.GetWebContent("http://data.taipei/bus/EstimateTime", Encoding.UTF8, gZip: true, onlyGzip: true);

            var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

            return jObject;
        }

        private static readonly object Objlock = new object();

        /// <summary>
        /// 
        /// </summary>
        private static Dictionary<string, JToken> _busStopEstimateTimeList = new Dictionary<string, JToken>();

        private static string _busStopEstimateTimeKeyFormat = "r{0}s{1}";
        /// <summary>
        /// 巴士到站時間（單位：秒） -1：尚未發車 -2：交管不停靠 -3：末班車已過 -4：今日未營運 GoBack	去返程 （0：去程 1：返程 2：尚未發車 3：末班已駛離）
        /// </summary>
        /// <param name="routeId">路線代碼</param>
        /// <param name="stopId">站牌代碼</param>
        /// <returns></returns>
        public static int GetEstimateTime(int routeId, int stopId)
        {
            var stringFormat = string.Format(_busStopEstimateTimeKeyFormat, routeId, stopId);
            var inTime = int.MinValue;
            lock (Objlock)
            {
                if (_busStopEstimateTimeList.ContainsKey(stringFormat))
                {
                    inTime = int.Parse(Convert.ToString(_busStopEstimateTimeList[stringFormat]["EstimateTime"]));
                }
            }

            return inTime;
        }

        //public static JToken 

        public class BusStopEstimateTime
        {
            public int RouteId { get; set; }
            public int StopId { get; set; }
            public int EstimateTime { get; set; }
        }

        public void SaveEstimateTime(JObject jObject)
        {
            Parallel.ForEach(jObject["BusInfo"], (item, loopState) =>
            {
                var routeId = int.Parse(item["RouteID"].ToString());
                var stopId = int.Parse(item["StopID"].ToString());

                var stringFormat = string.Format(_busStopEstimateTimeKeyFormat, routeId, stopId);

                lock (Objlock)
                {
                    if (_busStopEstimateTimeList.ContainsKey(stringFormat))
                    {
                        _busStopEstimateTimeList[stringFormat] = item;
                    }
                    else
                    {
                        _busStopEstimateTimeList.Add(stringFormat, item);
                    }
                }
            });
        }
    }
}
