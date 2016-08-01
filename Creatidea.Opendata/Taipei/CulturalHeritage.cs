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
    /// 文化資產
    /// </summary>
    public class CulturalHeritage : OpenDataDataBaseLocation
    {
        protected override string TableName()
        {
            return "TaipeiCulturalHeritage";
        }

        protected override string CreateTableSqlString()
        {
            return @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaipeiCulturalHeritage]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[TaipeiCulturalHeritage](
	[Id] [int] NULL,
	[CaseId] [nvarchar](50) NULL,
	[Type] [nvarchar](100) NULL,
	[Name] [nvarchar](100) NULL,
	[Area] [nvarchar](50) NULL,
	[Address] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[RegisterDate] [datetime] NULL,
	[BuildingActualState] [nvarchar](max) NULL,
	[OfficialDocumentNo] [nvarchar](100) NULL,
	[LandScope] [nvarchar](max) NULL,
	[RegisterReason] [nvarchar](max) NULL,
	[LawsReference] [nvarchar](100) NULL,
	[Longitude] [float] NULL,
	[Latitude] [float] NULL,
	[PageUrl] [nvarchar](max) NULL,
	[Photo96Url] [nvarchar](max) NULL
) ON [PRIMARY]
END
";
        }

        protected override JObject Data()
        {
            var jsonString = Tool.GetWebContent("http://data.taipei/opendata/datalist/apiAccess?scope=resourceAquire&rid=d40ee29c-a538-4a87-84f0-f43acfa19a20", Encoding.UTF8);

            var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

            return jObject;
        }

        protected override DataTable Resolve(JObject jObj)
        {
            var list = JsonConvert.DeserializeObject<List<CulturalHeritageResolveEntity>>(jObj["result"]["results"].ToString());

            return list.ListToDataTable();
        }

        private class CulturalHeritageResolveEntity: CulturalHeritageEntity
        {
            /// <summary>
            /// 代碼
            /// </summary>
            [JsonProperty("_id")]
            public int Id { get; set; }
            /// <summary>
            /// 個案編號
            /// </summary>
            [JsonProperty("case_id")]
            public string CaseId { get; set; }
            /// <summary>
            /// 名稱
            /// </summary>
            [JsonProperty("case_name")]
            public string Name { get; set; }
            /// <summary>
            /// 種類
            /// </summary>
            [JsonProperty("assets_type_code")]
            public string Type { get; set; }
            /// <summary>
            /// 簡介
            /// </summary>
            [JsonProperty("building_brief")]
            public string Description { get; set; }
            /// <summary>
            /// 現狀
            /// </summary>
            [JsonProperty("building_actual_state")]
            public string BuildingActualState { get; set; }
            /// <summary>
            /// 公告日期
            /// </summary>
            [JsonProperty("register_date")]
            public DateTime RegisterDate { get; set; }
            /// <summary>
            /// 公告文號
            /// </summary>
            [JsonProperty("official_doc_no")]
            public string OfficialDocumentNo { get; set; }
            /// <summary>
            /// 行政區域
            /// </summary>
            [JsonProperty("belong_city_name")]
            public string Area { get; set; }
            /// <summary>
            /// 地址或位置
            /// </summary>
            [JsonProperty("belong_address")]
            public string Address { get; set; }
            /// <summary>
            /// 定著土地之範圍
            /// </summary>
            [JsonProperty("land_scope")]
            public string LandScope { get; set; }
            /// <summary>
            /// 指定理由
            /// </summary>
            [JsonProperty("register_reason")]
            public string RegisterReason { get; set; }
            /// <summary>
            /// 法令依據
            /// </summary>
            [JsonProperty("laws_reference")]
            public string LawsReference { get; set; }
            /// <summary>
            /// 文化局對應頁面連結
            /// </summary>
            [JsonProperty("page_url")]
            public string PageUrl { get; set; }
            /// <summary>
            /// 經度
            /// </summary>
            [JsonProperty("longitude")]
            public float Longitude { get; set; }
            /// <summary>
            /// 緯度
            /// </summary>
            [JsonProperty("latitude")]
            public float Latitude { get; set; }
            /// <summary>
            /// 
            /// </summary>
            [JsonProperty("pic96_url")]
            public string Photo96Url { get; set; }
        }

        public class CulturalHeritageEntity
        {
            /// <summary>
            /// 代碼
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// 個案編號
            /// </summary>
            public string CaseId { get; set; }
            /// <summary>
            /// 名稱
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 種類
            /// </summary>
            public string Type { get; set; }
            /// <summary>
            /// 簡介
            /// </summary>
            public string Description { get; set; }
            /// <summary>
            /// 現狀
            /// </summary>
            public string BuildingActualState { get; set; }
            /// <summary>
            /// 公告日期
            /// </summary>
            public DateTime RegisterDate { get; set; }
            /// <summary>
            /// 公告文號
            /// </summary>
            public string OfficialDocumentNo { get; set; }
            /// <summary>
            /// 行政區域
            /// </summary>
            public string Area { get; set; }
            /// <summary>
            /// 地址或位置
            /// </summary>
            public string Address { get; set; }
            /// <summary>
            /// 定著土地之範圍
            /// </summary>
            public string LandScope { get; set; }
            /// <summary>
            /// 指定理由
            /// </summary>
            public string RegisterReason { get; set; }
            /// <summary>
            /// 法令依據
            /// </summary>
            public string LawsReference { get; set; }
            /// <summary>
            /// 文化局對應頁面連結
            /// </summary>
            public string PageUrl { get; set; }
            /// <summary>
            /// 經度
            /// </summary>
            public float Longitude { get; set; }
            /// <summary>
            /// 緯度
            /// </summary>
            public float Latitude { get; set; }
            /// <summary>
            /// 
            /// </summary>
            public string Photo96Url { get; set; }
        }

        /// <summary>
        /// 取得文化資產
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static CulturalHeritageEntity Get(string id)
        {
            CulturalHeritageEntity entity = null;

            using (var openData = new CulturalHeritage())
            {
                var table = openData.GetById(id);

                entity = table.ToList<CulturalHeritageEntity>().FirstOrDefault();
            }

            return entity;
        }

        /// <summary>
        /// 取得文化資產
        /// </summary>
        /// <param name="lat">緯度</param>
        /// <param name="lng">經度</param>
        /// <param name="locationRadius">半徑範圍</param>
        /// <returns></returns>
        public static IList<CulturalHeritageEntity> Get(float lat, float lng, int locationRadius = 1)
        {
            IList<CulturalHeritageEntity> list = null;

            using (var openData = new CulturalHeritage())
            {
                var table = openData.GetByLatLng(lat, lng, locationRadius);

                list = table.ToList<CulturalHeritageEntity>();
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
