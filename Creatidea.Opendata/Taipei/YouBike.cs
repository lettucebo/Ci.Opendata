using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Creatidea.Opendata.Taipei
{
    /// <summary>
    /// 微笑單車(YouBike)
    /// </summary>
    public class YouBike
    {
        /// <summary>
        /// 數量即時資訊
        /// </summary>
        public class Count : OpenData
        {
            private static readonly object StaticLockObj = new object();

            protected override JObject Data()
            {
                var jsonString = Tool.GetWebContent("http://data.taipei/youbike", Encoding.UTF8);

                var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                return jObject;
            }

            protected override void Save(JObject jObject)
            {
                Parallel.ForEach(jObject["retVal"], (items, loopState) =>
                {
                    foreach (var item in items)
                    {
                        var sno = Convert.ToString(item["sno"]);
                        lock (LockObj)
                        {
                            if (_ubikeList.ContainsKey(sno))
                            {
                                _ubikeList[sno] = item;
                            }
                            else
                            {
                                _ubikeList.Add(sno, item);
                            }
                        }
                    }
                });
            }
            
            /// <summary>
            /// 單車站資料
            /// </summary>
            private static Dictionary<string, JToken> _ubikeList = new Dictionary<string, JToken>();

            /// <summary>
            /// 剩餘車位
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <returns></returns>
            public static int GetSpace(string id)
            {
                var available = int.MinValue;

                if (_ubikeList.ContainsKey(id))
                {
                    available = int.Parse(Convert.ToString(_ubikeList[id]["bemp"]));
                }

                return available;
            }

            /// <summary>
            /// 剩餘車輛
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <returns></returns>
            public static int GetBike(string id)
            {
                var available = int.MinValue;
                lock (StaticLockObj)
                {
                    if (_ubikeList.ContainsKey(id))
                    {
                        available = int.Parse(Convert.ToString(_ubikeList[id]["sbi"]));
                    }
                }

                return available;
            }

        }

        /// <summary>
        /// 站點資訊
        /// </summary>
        public class Station : OpenDataDataBaseLocation
        {
            protected override string TableName()
            {
                return "TaipeiYouBike";
            }

            protected override string CreateTableSqlString()
            {
                return @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaipeiYouBike]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[TaipeiYouBike](
	[No] [int] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Area] [nvarchar](50) NULL,
	[Address] [nvarchar](200) NULL,
	[NameEn] [nvarchar](50) NULL,
	[AreaEn] [nvarchar](50) NULL,
	[AddressEn] [nvarchar](200) NULL,
	[TotalSpace] [int] NULL,
	[CurrentSpace] [int] NULL,
	[CurrentBike] [int] NULL,
	[ModifyTime] [nvarchar](50) NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
	[Action] [int] NULL,
 CONSTRAINT [PK_TaipeiYouBike] PRIMARY KEY CLUSTERED 
(
	[No] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END

";
            }
            protected override JObject Data()
            {
                var jsonString = Tool.GetWebContent("http://data.taipei/youbike", Encoding.UTF8);

                var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                return jObject;
            }

            protected override DataTable Resolve(JObject jObject)
            {
                var list = new List<StationEntity>();
                foreach (var items in jObject["retVal"])
                {
                    foreach (var item in items)
                    {
                        var entity = JsonConvert.DeserializeObject<StationEntity>(item.ToString());

                        list.Add(entity);
                    }
                }

                return list.ListToDataTable();
            }
            
            public class StationEntity
            {
                /// <summary>
                /// 
                /// </summary>
                //[JsonProperty("iid")]
                //public int Id { get; set; }
                /// <summary>
                /// 
                /// </summary>
                //[JsonProperty("sv")]
                //public int sv { get; set; }
                /// <summary>
                /// 
                /// </summary>
                //[JsonProperty("sd")]
                //public string sd { get; set; }
                /// <summary>
                /// 
                /// </summary>
                //[JsonProperty("vtyp")]
                //public string vtyp { get; set; }
                /// <summary>
                /// 站點代號
                /// </summary>
                [JsonProperty("sno")]
                public int No { get; set; }
                /// <summary>
                /// 場站名稱(中文)
                /// </summary>
                [JsonProperty("sna")]
                public string Name { get; set; }
                /// <summary>
                /// 站點IP
                /// </summary>
                //[JsonProperty("sip")]
                //public string Ip { get; set; }
                /// <summary>
                /// 場站總停車格
                /// </summary>
                [JsonProperty("tot")]
                public int TotalSpace { get; set; }
                /// <summary>
                /// 場站目前車輛數量
                /// </summary>
                [JsonProperty("sbi")]
                public int CurrentBike { get; set; }
                /// <summary>
                /// 場站區域(中文)
                /// </summary>
                [JsonProperty("sarea")]
                public string Area { get; set; }
                /// <summary>
                /// 資料更新時間
                /// </summary>
                [JsonProperty("mday")]
                public string ModifyTime { get; set; }
                /// <summary>
                /// 緯度
                /// </summary>
                [JsonProperty("lat")]
                public float Latitude { get; set; }
                /// <summary>
                /// 經度
                /// </summary>
                [JsonProperty("lng")]
                public float Longitude { get; set; }
                /// <summary>
                /// 地址(中文)
                /// </summary>
                [JsonProperty("ar")]
                public string Address { get; set; }
                /// <summary>
                /// 場站區域(英文)
                /// </summary>
                [JsonProperty("sareaen")]
                public string AreaEn { get; set; }
                /// <summary>
                /// 場站名稱(英文)
                /// </summary>
                [JsonProperty("snaen")]
                public string NameEn { get; set; }
                /// <summary>
                /// 地址(英文)
                /// </summary>
                [JsonProperty("aren")]
                public string AddressEn { get; set; }
                /// <summary>
                /// 
                /// </summary>
                //[JsonProperty("nbcnt")]
                //public string nbcnt { get; set; }
                /// <summary>
                /// 空位數量
                /// </summary>
                [JsonProperty("bemp")]
                public int CurrentSpace { get; set; }
                /// <summary>
                /// 全站禁用狀態
                /// </summary>
                [JsonProperty("act")]
                public int Action { get; set; }

            }

            /// <summary>
            /// 取得站點資料
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <returns></returns>
            public static StationEntity Get(int id)
            {
                StationEntity entity = null;

                using (var openData = new Station())
                {
                    var table = openData.GetById(id);

                    entity = table.ToList<StationEntity>().FirstOrDefault();
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
            public static IList<StationEntity> Get(float lat, float lng, int locationRadius)
            {
                IList<StationEntity> list = null;

                using (var openData = new Station())
                {
                    var table = openData.GetByLatLng(lat, lng, locationRadius);

                    list = table.ToList<StationEntity>();
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
    }
}
