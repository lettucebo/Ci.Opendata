using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Creatidea.Opendata.Taipei
{
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

        public override JObject Data()
        {
            var jsonString = Tool.GetWebContent("http://data.taipei/opendata/datalist/apiAccess?scope=resourceAquire&rid=d40ee29c-a538-4a87-84f0-f43acfa19a20", Encoding.UTF8);

            var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

            return jObject;
        }

        protected override DataTable Resolve(JObject jObj)
        {
            var list = JsonConvert.DeserializeObject<List<CulturalHeritageEntity>>(jObj["result"]["results"].ToString());

            return list.ListToDataTable();
        }

        public class CulturalHeritageEntity
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

    }
}
