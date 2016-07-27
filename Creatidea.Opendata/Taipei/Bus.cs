﻿using System;
using System.Collections.Generic;
using System.Data;
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
            private static readonly object StaticLockObj = new object();

            public override JObject Data()
            {
                var jsonString = Tool.GetWebContent("http://data.taipei/bus/EstimateTime", Encoding.UTF8, gZip: true, onlyGzip: true);

                var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                return jObject;
            }

            protected override void Save(JObject jObj)
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
        public class Stop : OpenDataDataBaseLocation
        {
            public override JObject Data()
            {
                var jsonString = Tool.GetWebContent("http://data.taipei/bus/Stop", Encoding.UTF8, gZip: true, onlyGzip: true);

                var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                return jObject;
            }

            protected override string TableName()
            {
                return "TaipeiBusStop";
            }

            protected override string CreateTableSqlString()
            {
                return @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaipeiBusStop]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[TaipeiBusStop](
	[Id] [int] NOT NULL,
	[RouteId] [int] NULL,
	[Name] [nvarchar](50) NULL,
	[NameEn] [nvarchar](100) NULL,
	[SeqNo] [int] NULL,
	[Pgp] [int] NULL,
	[GoBack] [int] NULL,
	[StopLocationId] [int] NULL,
	[Vector] [int] NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
 CONSTRAINT [PK_TaipeiBusStop] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END

";
            }

            protected override DataTable ImportTable()
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("Id", typeof(int));
                dataTable.Columns.Add("RouteId", typeof(int));
                dataTable.Columns.Add("Name", typeof(string));
                dataTable.Columns.Add("NameEn", typeof(string));
                dataTable.Columns.Add("SeqNo", typeof(int));
                dataTable.Columns.Add("Pgp", typeof(int));
                dataTable.Columns.Add("GoBack", typeof(int));
                dataTable.Columns.Add("StopLocationId", typeof(int));
                dataTable.Columns.Add("Vector", typeof(int));

                dataTable.Columns.Add("Latitude", typeof(float));
                dataTable.Columns.Add("Longitude", typeof(float));

                return dataTable;
            }

            protected override DataTable Resolve(JObject jObj)
            {
                var list = JsonConvert.DeserializeObject<List<BusStopEntity>>(jObj["BusInfo"].ToString());

                var dataTable = ImportTable();
                foreach (var item in list)
                {
                    var row = dataTable.NewRow();

                    row["Id"] = item.Id;
                    row["RouteId"] = item.RouteId;
                    row["Name"] = item.Name;
                    row["NameEn"] = item.NameEn;
                    row["SeqNo"] = item.SeqNo;
                    if (item.Pgp.HasValue)
                    {
                        row["Pgp"] = item.Pgp.Value;
                    }
                    else
                    {
                        row["Pgp"] = DBNull.Value;
                    }
                    row["GoBack"] = item.GoBack;
                    row["StopLocationId"] = item.StopLocationId;

                    if (item.Vector.HasValue)
                    {
                        row["Vector"] = item.Vector.Value;
                    }
                    else
                    {
                        row["Vector"] = DBNull.Value;
                    }

                    row["Latitude"] = item.Latitude;
                    row["Longitude"] = item.Longitude;

                    dataTable.Rows.Add(row);
                }

                return dataTable;
            }

            public class BusStopEntity
            {
                /// <summary>
                /// 站牌代碼
                /// </summary>
                public int Id { get; set; }
                /// <summary>
                /// 所屬路線代碼 (主路線 ID) 
                /// </summary>
                public int RouteId { get; set; }
                /// <summary>
                /// 中文名稱
                /// </summary>
                [JsonProperty("NameZh")]
                public string Name { get; set; }
                /// <summary>
                /// 英文名稱
                /// </summary>
                public string NameEn { get; set; }
                /// <summary>
                /// 於路線上的順序
                /// </summary>
                public int SeqNo { get; set; }
                /// <summary>
                /// 上下車站別
                /// </summary>
                public int? Pgp { get; set; }
                /// <summary>
                /// 去返程
                /// </summary>
                public int GoBack { get; set; }
                /// <summary>
                /// 站位 ID
                /// </summary>
                public int StopLocationId { get; set; }
                /// <summary>
                /// 向量角：0~359，預設為空白
                /// </summary>
                public int? Vector { get; set; }
                /// <summary>
                /// 緯度 
                /// </summary>
                [JsonProperty("showLat")]
                public float Latitude { get; set; }
                /// <summary>
                /// 經度
                /// </summary>
                [JsonProperty("showLon")]
                public float Longitude { get; set; }
            }
        }

        /// <summary>
        /// 路線
        /// </summary>
        public class Route : OpenDataDataBase
        {
            public override JObject Data()
            {
                var jsonString = Tool.GetWebContent("http://data.taipei/bus/ROUTE", Encoding.UTF8, gZip: true, onlyGzip: true);

                var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                return jObject;
            }

            protected override string CreateTableSqlString()
            {
                return @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaipeiBusRoute]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[TaipeiBusRoute](
	[Id] [int] NULL,
	[ProviderId] [int] NULL,
	[ProviderName] [nvarchar](50) NULL,
	[Name] [nvarchar](50) NULL,
	[NameEn] [nvarchar](100) NULL,
	[PathAttributeId] [int] NULL,
	[PathAttributeName] [nvarchar](50) NULL,
	[PathAttributeNameEn] [nvarchar](100) NULL,
	[BuildPeriod] [int] NULL,
	[Departure] [nvarchar](100) NULL,
	[DepartureEn] [nvarchar](500) NULL,
	[Destination] [nvarchar](100) NULL,
	[DestinationEn] [nvarchar](500) NULL,
	[RealSequence] [int] NULL,
	[Distance] [float] NULL,
	[GoFirstBusTime] [nvarchar](4) NULL,
	[BackFirstBusTime] [nvarchar](4) NULL,
	[GoLastBusTime] [nvarchar](4) NULL,
	[BackLastBusTime] [nvarchar](4) NULL,
	[BusTimeDesc] [nvarchar](max) NULL,
	[PeakHeadway] [nvarchar](4) NULL,
	[OffPeakHeadway] [nvarchar](4) NULL,
	[HeadwayDesc] [nvarchar](max) NULL,
	[HolidayGoFirstBusTime] [nvarchar](4) NULL,
	[HolidayBackFirstBusTime] [nvarchar](4) NULL,
	[HolidayGoLastBusTime] [nvarchar](4) NULL,
	[HolidayBackLastBusTime] [nvarchar](4) NULL,
	[HolidayBusTimeDesc] [nvarchar](max) NULL,
	[HolidayPeakHeadway] [nvarchar](4) NULL,
	[HolidayOffPeakHeadway] [nvarchar](4) NULL,
	[HolidayHeadwayDesc] [nvarchar](max) NULL,
	[SegmentBuffer] [nvarchar](max) NULL,
	[SegmentBufferEn] [nvarchar](max) NULL,
	[TicketPriceDescription] [nvarchar](50) NULL,
	[TicketPriceDescriptionEn] [nvarchar](50) NULL
) ON [PRIMARY]
END
";
            }

            protected override DataTable ImportTable()
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("Id", typeof(int));
                dataTable.Columns.Add("ProviderId", typeof(int));
                dataTable.Columns.Add("ProviderName", typeof(string));
                dataTable.Columns.Add("Name", typeof(string));
                dataTable.Columns.Add("NameEn", typeof(string));
                dataTable.Columns.Add("PathAttributeId", typeof(int));
                dataTable.Columns.Add("PathAttributeName", typeof(string));
                dataTable.Columns.Add("PathAttributeNameEn", typeof(string));
                dataTable.Columns.Add("BuildPeriod", typeof(int));
                dataTable.Columns.Add("Departure", typeof(string));
                dataTable.Columns.Add("DepartureEn", typeof(string));
                dataTable.Columns.Add("Destination", typeof(string));
                dataTable.Columns.Add("DestinationEn", typeof(string));
                dataTable.Columns.Add("RealSequence", typeof(int));
                dataTable.Columns.Add("Distance", typeof(float));
                dataTable.Columns.Add("GoFirstBusTime", typeof(string));
                dataTable.Columns.Add("BackFirstBusTime", typeof(string));
                dataTable.Columns.Add("GoLastBusTime", typeof(string));
                dataTable.Columns.Add("BackLastBusTime", typeof(string));
                dataTable.Columns.Add("BusTimeDesc", typeof(string));
                dataTable.Columns.Add("PeakHeadway", typeof(string));
                dataTable.Columns.Add("OffPeakHeadway", typeof(string));
                dataTable.Columns.Add("HeadwayDesc", typeof(string));
                dataTable.Columns.Add("HolidayGoFirstBusTime", typeof(string));
                dataTable.Columns.Add("HolidayBackFirstBusTime", typeof(string));
                dataTable.Columns.Add("HolidayGoLastBusTime", typeof(string));
                dataTable.Columns.Add("HolidayBackLastBusTime", typeof(string));
                dataTable.Columns.Add("HolidayBusTimeDesc", typeof(string));
                dataTable.Columns.Add("HolidayPeakHeadway", typeof(string));
                dataTable.Columns.Add("HolidayOffPeakHeadway", typeof(string));
                dataTable.Columns.Add("HolidayHeadwayDesc", typeof(string));
                dataTable.Columns.Add("SegmentBuffer", typeof(string));
                dataTable.Columns.Add("SegmentBufferEn", typeof(string));
                dataTable.Columns.Add("TicketPriceDescription", typeof(string));
                dataTable.Columns.Add("TicketPriceDescriptionEn", typeof(string));

                return dataTable;
            }

            protected override DataTable Resolve(JObject jObj)
            {
                var list = JsonConvert.DeserializeObject<List<BusRouteEntity>>(jObj["BusInfo"].ToString());

                var dataTable = ImportTable();
                foreach (var item in list)
                {
                    var row = dataTable.NewRow();

                    row["Id"] = item.Id;
                    row["ProviderId"] = item.ProviderId;
                    row["ProviderName"] = item.ProviderName;
                    row["Name"] = item.Name;
                    row["NameEn"] = item.NameEn;
                    row["PathAttributeId"] = item.PathAttributeId;
                    row["PathAttributeName"] = item.PathAttributeName;
                    row["PathAttributeNameEn"] = item.PathAttributeNameEn;
                    row["BuildPeriod"] = item.BuildPeriod;
                    row["Departure"] = item.Departure;
                    row["DepartureEn"] = item.DepartureEn;
                    row["Destination"] = item.Destination;
                    row["DestinationEn"] = item.DestinationEn;
                    row["RealSequence"] = item.RealSequence;
                    row["Distance"] = item.Distance;
                    row["GoFirstBusTime"] = item.GoFirstBusTime;
                    row["BackFirstBusTime"] = item.BackFirstBusTime;
                    row["GoLastBusTime"] = item.GoLastBusTime;
                    row["BackLastBusTime"] = item.BackLastBusTime;
                    row["BusTimeDesc"] = item.BusTimeDesc;
                    row["PeakHeadway"] = item.PeakHeadway;
                    row["OffPeakHeadway"] = item.OffPeakHeadway;
                    row["HeadwayDesc"] = item.HeadwayDesc;
                    row["HolidayGoFirstBusTime"] = item.HolidayGoFirstBusTime;
                    row["HolidayBackFirstBusTime"] = item.HolidayBackFirstBusTime;
                    row["HolidayGoLastBusTime"] = item.HolidayGoLastBusTime;
                    row["HolidayBackLastBusTime"] = item.HolidayBackLastBusTime;
                    row["HolidayBusTimeDesc"] = item.HolidayBusTimeDesc;
                    row["HolidayPeakHeadway"] = item.HolidayPeakHeadway;
                    row["HolidayOffPeakHeadway"] = item.HolidayOffPeakHeadway;
                    row["HolidayHeadwayDesc"] = item.HolidayHeadwayDesc;
                    row["SegmentBuffer"] = item.SegmentBuffer;
                    row["SegmentBufferEn"] = item.SegmentBufferEn;
                    row["TicketPriceDescription"] = item.TicketPriceDescription;
                    row["TicketPriceDescriptionEn"] = item.TicketPriceDescriptionEn;

                    dataTable.Rows.Add(row);
                }

                return dataTable;
            }

            protected override string TableName()
            {
                return "TaipeiBusRoute";
            }

            public class BusRouteEntity
            {
                /// <summary>
                /// 路線代碼
                /// </summary>
                public int Id { get; set; }
                /// <summary>
                /// 業者代碼
                /// </summary>
                public int ProviderId { get; set; }
                /// <summary>
                /// 業者中文名稱
                /// </summary>
                public string ProviderName { get; set; }
                /// <summary>
                /// 中文名稱
                /// </summary>
                [JsonProperty("NameZh")]
                public string Name { get; set; }
                /// <summary>
                /// 英文名稱
                /// </summary>
                public string NameEn { get; set; }
                /// <summary>
                /// 所屬附屬路線
                /// </summary>
                public int PathAttributeId { get; set; }
                /// <summary>
                /// 所屬附屬路線中文名稱
                /// </summary>
                public string PathAttributeName { get; set; }
                /// <summary>
                /// 所屬附屬路線英文名稱
                /// </summary>
                [JsonProperty("pathAttributeEname")]
                public string PathAttributeNameEn { get; set; }
                /// <summary>
                /// 建置時間，分為 1：1 期、2：2 期、3：3 期、9：非動態資料、10：北縣
                /// </summary>
                public int BuildPeriod { get; set; }
                /// <summary>
                /// '去程第 1 站' 起站中文名稱
                /// </summary>
                [JsonProperty("DepartureZh")]
                public string Departure { get; set; }
                /// <summary>
                /// '去程第 1 站' 起站英文名稱
                /// </summary>
                public string DepartureEn { get; set; }
                /// <summary>
                /// 回程第 1 站' 訖站中文名稱
                /// </summary>
                [JsonProperty("DestinationZh")]
                public string Destination { get; set; }
                /// <summary>
                /// '回程第 1 站' 訖站英文名稱
                /// </summary>
                public string DestinationEn { get; set; }
                /// <summary>
                /// 核定總班次
                /// </summary>
                public int RealSequence { get; set; }
                /// <summary>
                /// 總往返里程(公里/全程)
                /// </summary>
                public float Distance { get; set; }
                /// <summary>
                /// 站牌顯示時使用，去程第一班發車時間(hhmm)
                /// </summary>
                public string GoFirstBusTime { get; set; }
                /// <summary>
                /// 站牌顯示時使用，回程第一班發車時間(hhmm)
                /// </summary>
                public string BackFirstBusTime { get; set; }
                /// <summary>
                /// 站牌顯示時使用，去程最後一班發車時間(hhmm)
                /// </summary>
                public string GoLastBusTime { get; set; }
                /// <summary>
                /// 站牌顯示時使用，回程最後一班發車時間(hhmm)
                /// </summary>
                public string BackLastBusTime { get; set; }
                /// <summary>
                /// 平日頭末班描述
                /// </summary>
                public string BusTimeDesc { get; set; }
                /// <summary>
                /// 站牌顯示時使用，尖峰時段發車間隔(hhmm OR mm)
                /// </summary>
                public string PeakHeadway { get; set; }
                /// <summary>
                /// 站牌顯示時使用，離峰時段發車間隔(hhmm OR mm)
                /// </summary>
                public string OffPeakHeadway { get; set; }
                /// <summary>
                /// 平日發車間距描述 
                /// </summary>
                public string HeadwayDesc { get; set; }
                /// <summary>
                /// 假日站牌顯示時使用，去程第一班發車時間(HHmm)
                /// </summary>
                public string HolidayGoFirstBusTime { get; set; }
                /// <summary>
                /// 假日站牌顯示時使用，回程第一班發車時間(HHmm) 
                /// </summary>
                public string HolidayBackFirstBusTime { get; set; }
                /// <summary>
                /// 假日站牌顯示時使用，去程最後一班發車時間(HHmm)
                /// </summary>
                public string HolidayGoLastBusTime { get; set; }
                /// <summary>
                /// 假日站牌顯示時使用，回程最後一班發車時間(HHmm)
                /// </summary>
                public string HolidayBackLastBusTime { get; set; }
                /// <summary>
                /// 假日頭末班描述 
                /// </summary>
                public string HolidayBusTimeDesc { get; set; }
                /// <summary>
                /// 假日站牌顯示時使用，尖峰時段發車間隔(mmmm OR mm)
                /// </summary>
                public string HolidayPeakHeadway { get; set; }
                /// <summary>
                /// 假日站牌顯示時使用，離峰時段發車間隔(mmmm OR mm) 
                /// </summary>
                public string HolidayOffPeakHeadway { get; set; }
                /// <summary>
                /// 假日發車間距描述
                /// </summary>
                public string HolidayHeadwayDesc { get; set; }
                /// <summary>
                /// 分段緩衝區(中文)
                /// </summary>
                [JsonProperty("SegmentBufferZh")]
                public string SegmentBuffer { get; set; }
                /// <summary>
                /// 分段緩衝區(英文)
                /// </summary>
                public string SegmentBufferEn { get; set; }
                /// <summary>
                /// 票價描述(中文)
                /// </summary>
                [JsonProperty("TicketPriceDescriptionZh")]
                public string TicketPriceDescription { get; set; }
                /// <summary>
                /// 票價描述(英文)
                /// </summary>
                public string TicketPriceDescriptionEn { get; set; }
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
                _main.DataSave();
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
            private Bus.Stop _main = new Bus.Stop();

            public override void Run()
            {
                _main.DataSave();
            }

            public override void Dispose()
            {
                _main.Dispose();
                _main = null;
            }
        }
        
        /// <summary>
        /// 路線資訊排程
        /// </summary>
        public class Route : OpenDataSchedule
        {
            private Bus.Route _main = new Bus.Route();

            public override void Run()
            {
                _main.DataSave();
            }

            public override void Dispose()
            {
                _main.Dispose();
                _main = null;
            }
        }
    }
}
