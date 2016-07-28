using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Creatidea.Opendata.Taipei
{
    /// <summary>
    /// 公車資訊
    /// </summary>
    public class Bus : OpenData
    {
        /// <summary>
        /// 到站時間
        /// </summary>
        /// <seealso cref="Creatidea.Opendata.OpenData" />
        public class EstimateTime : OpenData
        {
            private static readonly object StaticLockObj = new object();

            protected override JObject Data()
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
            public static string ClassTableName
            {
                get
                {
                    using (var thisClass = new Stop())
                    {
                        return thisClass.TableName();
                    }
                }
            }

            protected override JObject Data()
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

            protected override DataTable Resolve(JObject jObj)
            {
                var list = JsonConvert.DeserializeObject<List<BusStopEntity>>(jObj["BusInfo"].ToString());

                return list.ListToDataTable();
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

            /// <summary>
            /// 取得站點資料
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <returns></returns>
            public static BusStopEntity Get(int id)
            {
                BusStopEntity entity = null;

                using (var openData = new Stop())
                {
                    var table = openData.GetById(id);

                    entity = table.ToList<BusStopEntity>().FirstOrDefault();
                }

                return entity;
            }

            /// <summary>
            /// 取得站點資料
            /// </summary>
            /// <param name="lat">緯度</param>
            /// <param name="lng">經度</param>
            /// <param name="locationRadius">半徑範圍</param>
            /// <returns></returns>
            public static IEnumerable<BusStopEntity> Get(float lat, float lng, int locationRadius)
            {
                IEnumerable<BusStopEntity> list;

                using (var openData = new Stop())
                {
                    var table = openData.GetByLatLng(lat, lng, locationRadius);

                    list = table.ToList<BusStopEntity>();
                }

                return list;
            }

            private DataTable GetById(int id)
            {
                DataTable table = null;

                var sqlConnection = new SqlConnection(ConnectionString);

                sqlConnection.Open();

                var sqlCommand = sqlConnection.CreateCommand();

                sqlCommand.CommandTimeout = TimeOut;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = string.Format(" SELECT * FROM {0} WHERE Id = @Id ", TableName());
                sqlCommand.Parameters.Add("@Id", SqlDbType.Int).Value = id;

                table = new DataTable();
                var adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(table);

                sqlCommand.ExecuteNonQuery();

                sqlConnection.Close();
                sqlConnection.Dispose();


                return table;
            }

        }

        /// <summary>
        /// 路線
        /// </summary>
        public class Route : OpenDataDataBase
        {
            public static string ClassTableName
            {
                get
                {
                    using (var thisClass = new Route())
                    {
                        return thisClass.TableName();
                    }
                }
            }

            protected override JObject Data()
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

            protected override DataTable Resolve(JObject jObj)
            {
                var list = JsonConvert.DeserializeObject<List<BusRouteEntity>>(jObj["BusInfo"].ToString());

                return list.ListToDataTable();
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

            public static IEnumerable<BusRouteEntity> Get(int[] routeIds)
            {
                IEnumerable<BusRouteEntity> list = null;

                using (var openData = new Route())
                {
                    var table = openData.GetByRouteIds(routeIds);

                    list = table.ToList<BusRouteEntity>();
                }

                return list;
            }

            protected DataTable GetByRouteIds(int[] routeIds)
            {
                DataTable table = null;

                if (routeIds == null || routeIds.Length < 1)
                {
                    return null;
                }

                var sqlConnection = new SqlConnection(ConnectionString);

                sqlConnection.Open();

                var sqlCommand = sqlConnection.CreateCommand();

                sqlCommand.CommandTimeout = TimeOut;
                sqlCommand.CommandType = CommandType.Text;

                var inString = string.Empty;
                for (var i = 0; i < routeIds.Length; i++)
                {
                    if (!string.IsNullOrEmpty(inString))
                    {
                        inString += ",";
                    }

                    var pString = string.Format("@Id{0}", i);

                    inString += pString;
                    sqlCommand.Parameters.Add(pString, SqlDbType.Int).Value = routeIds[i];
                }

                sqlCommand.CommandText = string.Format(" SELECT * FROM {0} WHERE Id IN ( {1} ) ", TableName(), inString);

                table = new DataTable();
                var adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(table);

                sqlCommand.ExecuteNonQuery();

                sqlConnection.Close();
                sqlConnection.Dispose();


                return table;
            }
        }

        private readonly List<OpenData> _list = new List<OpenData>
        {
            new Stop(),
            new Route(),
        };

        protected override JObject Data()
        {
            foreach (var openData in _list)
            {
                openData.DataSave();
            }

            return null;
        }

        protected override void Save(JObject jObj)
        {

        }

        /// <summary>
        /// Gets the map stop.
        /// </summary>
        /// <param name="lat">The lat.</param>
        /// <param name="lng">The LNG.</param>
        /// <param name="locationRadius">The location radius.</param>
        /// <param name="interval">The interval.</param>
        /// <returns></returns>
        public static IEnumerable<MapStopEntity> GetMapStop(float lat, float lng, int locationRadius, int interval = 30)
        {
            var list = new List<MapStopEntity>();

            using (var estimateTime = new EstimateTime())
            {
                try
                {
                    estimateTime.Load(interval);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            var stops = Stop.Get(lat, lng, locationRadius);
            var routes = Route.Get(stops.Select(x => x.RouteId).Distinct().ToArray());

            foreach (var stop in stops)
            {
                var isNew = false;
                var entity = list.FirstOrDefault(x => x.Id == stop.StopLocationId);
                if (entity == null)
                {
                    entity = new MapStopEntity
                    {
                        Id = stop.StopLocationId,
                        Name = stop.Name,
                        NameEn = stop.NameEn,
                        Latitude = stop.Latitude,
                        Longitude = stop.Longitude,
                        Routes = new List<MapStopRouteEntity>(),
                    };
                    isNew = true;
                }

                var route = routes.FirstOrDefault(x => x.Id == stop.RouteId);

                if (route != null)
                {
                    var routeEntity = new MapStopRouteEntity
                    {
                        Id = route.Id,
                        ProviderName = route.ProviderName,
                        Name = route.PathAttributeName,
                        NameEn = route.PathAttributeNameEn,
                        Estimate = EstimateTime.Get(route.Id, stop.Id)
                    };

                    entity.Routes.Add(routeEntity);
                }

                if (isNew)
                {
                    list.Add(entity);
                }
            }

            return list;
        }

        public class MapStopEntity
        {
            /// <summary>
            /// 位置代碼
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// 中文名稱
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 英文名稱
            /// </summary>
            public string NameEn { get; set; }
            /// <summary>
            /// 緯度 
            /// </summary>
            public float Latitude { get; set; }
            /// <summary>
            /// 經度
            /// </summary>
            public float Longitude { get; set; }

            /// <summary>
            /// 公車路線
            /// </summary>
            public List<MapStopRouteEntity> Routes { get; set; }
        }

        public class MapStopRouteEntity
        {
            /// <summary>
            /// 路線代碼
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// 業者中文名稱
            /// </summary>
            public string ProviderName { get; set; }
            /// <summary>
            /// 中文名稱
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 英文名稱
            /// </summary>
            public string NameEn { get; set; }
            /// <summary>
            /// 預估到站剩餘時間（單位：秒）
            /// -1：尚未發車
            /// -2：交管不停靠
            /// -3：末班車已過
            /// -4：今日未營運
            /// </summary>
            public int Estimate { get; set; }
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
