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
    /// 臺北市藝文館所
    /// </summary>
    public class ArtsMuseum : OpenDataDataBaseLocation
    {
        protected override string TableName()
        {
            return "TaipeiArtsMuseum";
        }

        protected override string CreateTableSqlString()
        {
            return @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaipeiArtsMuseum]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[TaipeiArtsMuseum](
	[Id] [int] NOT NULL,
	[VenuesId] [int] NULL,
	[VenuesName] [nvarchar](50) NULL,
	[VenuesAddress] [nvarchar](500) NULL,
	[Phone] [nvarchar](50) NULL,
	[Fax] [nvarchar](50) NULL,
	[VenuesIntro] [nvarchar](max) NULL,
	[MainPhotoUrl] [nvarchar](max) NULL,
	[Photo96Url] [nvarchar](max) NULL,
	[HasTicket] [bit] NULL,
	[TicketInfo] [nvarchar](max) NULL,
	[BarrierFreeService] [nvarchar](max) NULL,
	[BarrierFreeOtherDescription] [nvarchar](max) NULL,
	[PageUrl] [nvarchar](max) NULL,
	[CityName] [nvarchar](50) NULL,
	[ZipCode] [nvarchar](50) NULL,
	[Address] [nvarchar](500) NULL,
	[Longitude] [float] NULL,
	[Latitude] [float] NULL,
 CONSTRAINT [PK_TaipeiArtsMuseum] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
";
        }

        protected override JObject Data()
        {
            var jsonString = Tool.GetWebContent("http://data.taipei/opendata/datalist/apiAccess?scope=resourceAquire&rid=dfad2ec4-fa19-4b2f-9efb-f6fe3456f469", Encoding.UTF8);

            var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

            return jObject;
        }

        protected override DataTable Resolve(JObject jObj)
        {
            var list = JsonConvert.DeserializeObject<List<ArtsMuseumEntity>>(jObj["result"]["results"].ToString());

            return list.ListToDataTable();
        }

        public class ArtsMuseumEntity
        {
            /// <summary>
            /// 館所代碼
            /// </summary>
            [JsonProperty("_id")]
            public int Id { get; set; }
            /// <summary>
            /// 館所編號
            /// </summary>
            [JsonProperty("venues_id")]
            public int VenuesId { get; set; }
            /// <summary>
            /// 館所名稱
            /// </summary>
            [JsonProperty("venues_name")]
            public string VenuesName { get; set; }
            /// <summary>
            /// 館所地址
            /// </summary>
            [JsonProperty("venues_address")]
            public string VenuesAddress { get; set; }
            /// <summary>
            /// 館所電話
            /// </summary>
            public string Phone { get; set; }
            /// <summary>
            /// 館所傳真
            /// </summary>
            public string Fax { get; set; }
            /// <summary>
            /// 館所簡介
            /// </summary>
            [JsonProperty("venues_intro")]
            public string VenuesIntro { get; set; }
            /// <summary>
            /// 原始圖片
            /// </summary>
            [JsonProperty("main_photo_url")]
            public string MainPhotoUrl { get; set; }
            /// <summary>
            /// 縮圖
            /// </summary>
            [JsonProperty("photo96_url")]
            public string Photo96Url { get; set; }
            /// <summary>
            /// 是否售票
            /// </summary>
            [JsonProperty("has_ticket")]
            [JsonConverter(typeof(BoolConverter))]
            public bool HasTicket { get; set; }
            /// <summary>
            /// 售票資訊
            /// </summary>
            [JsonProperty("ticket_info")]
            public string TicketInfo { get; set; }
            /// <summary>
            /// 無障礙服務
            /// </summary>
            [JsonProperty("barrier_free_service")]
            public string BarrierFreeService { get; set; }
            /// <summary>
            /// 無障礙服務其他說明
            /// </summary>
            [JsonProperty("barrier_free_otherDescription")]
            public string BarrierFreeOtherDescription { get; set; }
            /// <summary>
            /// 文化局對應頁面連結
            /// </summary>
            [JsonProperty("page_url")]
            public string PageUrl { get; set; }
            /// <summary>
            /// 所在縣市
            /// </summary>
            [JsonProperty("city_name")]
            public string CityName { get; set; }
            /// <summary>
            /// 郵遞區號
            /// </summary>
            [JsonProperty("zip_code")]
            public string ZipCode { get; set; }
            /// <summary>
            /// 詳細地址
            /// </summary>
            public string Address { get; set; }
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

        /// <summary>
        /// 取得藝文館所
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static ArtsMuseumEntity Get(string id)
        {
            ArtsMuseumEntity entity = null;

            using (var openData = new ArtsMuseum())
            {
                var table = openData.GetById(id);

                entity = table.ToList<ArtsMuseumEntity>().FirstOrDefault();
            }

            return entity;
        }

        /// <summary>
        /// 取得藝文館所
        /// </summary>
        /// <param name="lat">緯度</param>
        /// <param name="lng">經度</param>
        /// <param name="locationRadius">半徑範圍</param>
        /// <returns></returns>
        public static IList<ArtsMuseumEntity> Get(float lat, float lng, int locationRadius = 1)
        {
            IList<ArtsMuseumEntity> list = null;

            using (var openData = new ArtsMuseum())
            {
                var table = openData.GetByLatLng(lat, lng, locationRadius);

                list = table.ToList<ArtsMuseumEntity>();
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
}
