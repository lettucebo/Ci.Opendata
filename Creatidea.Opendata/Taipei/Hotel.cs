﻿using System;
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
    /// 住宿資訊
    /// </summary>
    public class Hotel
    {
        /// <summary>
        /// 中文
        /// </summary>
        public class Chinese : OpenDataDataBaseLocation
        {
            protected override string TableName()
            {
                return "TaipeiHotelChinese";
            }

            protected override string CreateTableSqlString()
            {
                return @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaipeiHotelChinese]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[TaipeiHotelChinese](
	[Id] [int] NOT NULL,
	[SerialNo] [nvarchar](50) NULL,
	[TravelClass] [nvarchar](50) NULL,
	[TravelType] [nvarchar](50) NULL,
	[Name] [nvarchar](100) NULL,
	[Address] [nvarchar](500) NULL,
	[Description] [nvarchar](max) NULL,
	[Info] [nvarchar](max) NULL,
	[Remark] [nvarchar](max) NULL,
	[Mrt] [nvarchar](50) NULL,
	[Source] [nvarchar](100) NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
 CONSTRAINT [PK_TaipeiHotelChinese] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
";
            }

            protected override JObject Data()
            {
                var jsonString = Tool.GetWebContent("http://data.taipei/opendata/datalist/apiAccess?scope=resourceAquire&rid=6f4e0b9b-8cb1-4b1d-a5c4-febd90f62469", Encoding.UTF8);

                var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                return jObject;
            }

            protected override DataTable Resolve(JObject jObj)
            {
                var list = JsonConvert.DeserializeObject<List<HotelResolveEntity>>(jObj["result"]["results"].ToString());

                return list.ListToDataTable();
            }
            
            /// <summary>
            /// 取得住宿資訊
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <returns></returns>
            public static HotelEntity Get(string id)
            {
                HotelEntity entity = null;

                using (var openData = new Chinese())
                {
                    var table = openData.GetById(id);

                    entity = table.ToList<HotelEntity>().FirstOrDefault();
                }

                return entity;
            }

            /// <summary>
            /// 取得住宿資訊
            /// </summary>
            /// <param name="lat">緯度</param>
            /// <param name="lng">經度</param>
            /// <param name="locationRadius">半徑範圍</param>
            /// <returns></returns>
            public static IList<HotelEntity> Get(float lat, float lng, int locationRadius = 1)
            {
                IList<HotelEntity> list = null;

                using (var openData = new Chinese())
                {
                    var table = openData.GetByLatLng(lat, lng, locationRadius);

                    list = table.ToList<HotelEntity>();
                }

                return list;
            }

            private DataTable GetById(string id)
            {
                DataTable table = null;

                var sqlConnection = new SqlConnection(ConnectionString);

                sqlConnection.Open();

                var sqlCommand = sqlConnection.CreateCommand();

                sqlCommand.CommandTimeout = TimeOut;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = string.Format(" SELECT * FROM {0} WHERE Id = @Id ", TableName());
                sqlCommand.Parameters.Add("@Id", SqlDbType.NVarChar).Value = id;

                table = new DataTable();
                var adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(table);

                sqlCommand.ExecuteNonQuery();

                sqlConnection.Close();
                sqlConnection.Dispose();


                return table;
            }
        }

        private class HotelResolveEntity : HotelEntity
        {
            /// <summary>
            /// 代碼
            /// </summary>
            [JsonProperty("_id")]
            public int Id { get; set; }
            /// <summary>
            /// 列數
            /// </summary>
            //public int RowNumber { get; set; }

            /// <summary>
            /// REF_WP
            /// </summary>
            //[JsonProperty("REF_WP")]
            //public string REF_WP { get; set; }


            /// <summary>
            /// 分類
            /// </summary>
            [JsonProperty("CAT1")]
            public string TravelClass { get; set; }

            /// <summary>
            /// 類型
            /// </summary>
            [JsonProperty("CAT2")]
            public string TravelType { get; set; }

            /// <summary>
            /// 住宿代碼
            /// </summary>
            [JsonProperty("SERIAL_NO")]
            public string SerialNo { get; set; }

            /// <summary>
            /// 備註
            /// </summary>
            [JsonProperty("MEMO_TIME")]
            public string Remark { get; set; }

            /// <summary>
            /// 住宿名稱
            /// </summary>
            [JsonProperty("stitle")]
            public string Name { get; set; }
            /// <summary>
            /// 住宿說明
            /// </summary>
            [JsonProperty("xbody")]
            public string Description { get; set; }

            /// <summary>
            /// 
            /// </summary>
            //[JsonProperty("avBegin")]
            //[JsonConverter(typeof(BoolConverter))]
            //public bool avBegin { get; set; }

            /// <summary>
            /// 
            /// </summary>
            //[JsonProperty("avEnd")]
            //public string avEnd { get; set; }

            /// <summary>
            /// 發佈機關
            /// </summary>
            [JsonProperty("idpt")]
            public string Source { get; set; }

            /// <summary>
            /// 地址
            /// </summary>
            [JsonProperty("address")]
            public string Address { get; set; }

            /// <summary>
            /// 
            /// </summary>
            //[JsonProperty("xpostDate")]
            //public string xpostDate { get; set; }

            /// <summary>
            /// 景點圖片(未整理)
            /// </summary>
            //[JsonProperty("file")]
            //public string File { get; set; }

            /// <summary>
            /// 
            /// </summary>
            //[JsonProperty("langinfo")]
            //public string langinfo { get; set; }

            /// <summary>
            /// 
            /// </summary>
            //[JsonProperty("POI")]
            //public string POI { get; set; }

            /// <summary>
            /// 交通資訊
            /// </summary>
            [JsonProperty("info")]
            public string Info { get; set; }
            /// <summary>
            /// 鄰近的捷運站
            /// </summary>
            [JsonProperty("Mrt")]
            public string Mrt { get; set; }


            /// <summary>
            /// 經度
            /// </summary>
            [JsonConverter(typeof(GeocodeConverter))]
            public float Longitude { get; set; }
            /// <summary>
            /// 緯度
            /// </summary>
            [JsonConverter(typeof(GeocodeConverter))]
            public float Latitude { get; set; }
        }

        public class HotelEntity
        {
            /// <summary>
            /// 代碼
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// 列數
            /// </summary>
            //public int RowNumber { get; set; }

            /// <summary>
            /// REF_WP
            /// </summary>
            //public string REF_WP { get; set; }


            /// <summary>
            /// 分類
            /// </summary>
            public string TravelClass { get; set; }

            /// <summary>
            /// 類型
            /// </summary>
            public string TravelType { get; set; }

            /// <summary>
            /// 住宿代碼
            /// </summary>
            public string SerialNo { get; set; }

            /// <summary>
            /// 備註
            /// </summary>
            public string Remark { get; set; }

            /// <summary>
            /// 住宿名稱
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 住宿說明
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// 
            /// </summary>
            //public bool avBegin { get; set; }

            /// <summary>
            /// 
            /// </summary>
            //public string avEnd { get; set; }

            /// <summary>
            /// 發佈機關
            /// </summary>
            public string Source { get; set; }

            /// <summary>
            /// 地址
            /// </summary>
            public string Address { get; set; }

            /// <summary>
            /// 
            /// </summary>
            //public string xpostDate { get; set; }

            /// <summary>
            /// 景點圖片(未整理)
            /// </summary>
            //public string File { get; set; }

            /// <summary>
            /// 
            /// </summary>
            //public string langinfo { get; set; }

            /// <summary>
            /// 
            /// </summary>
            //public string POI { get; set; }

            /// <summary>
            /// 交通資訊
            /// </summary>
            public string Info { get; set; }
            /// <summary>
            /// 鄰近的捷運站
            /// </summary>
            public string Mrt { get; set; }


            /// <summary>
            /// 經度
            /// </summary>
            public float Longitude { get; set; }
            /// <summary>
            /// 緯度
            /// </summary>
            public float Latitude { get; set; }
        }
    }
}
