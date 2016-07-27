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
    /// <summary>
    /// 臺北捷運
    /// </summary>
    public class Metro
    {
        /// <summary>
        /// 出入口
        /// </summary>
        public class Entrance : OpenDataDataBaseLocation
        {
            protected override string TableName()
            {
                return "TaipeiMetroEntrance";
            }

            protected override string CreateTableSqlString()
            {
                return @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaipeiMetroEntrance]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[TaipeiMetroEntrance](
	[Id] [int] NOT NULL,
	[No] [int] NULL,
	[Name] [nvarchar](50) NULL,
	[EntranceNo] [nvarchar](50) NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
 CONSTRAINT [PK_TaipeiMetroEntrance] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
";
            }
            
            public override JObject Data()
            {
                var jsonString = Tool.GetWebContent("http://data.taipei/opendata/datalist/apiAccess?scope=resourceAquire&rid=9c2fa3e8-118e-4b49-8a7c-57edd29ca3ec", Encoding.UTF8);

                var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                return jObject;
            }
            protected override DataTable Resolve(JObject jObj)
            {
                var list = JsonConvert.DeserializeObject<List<EntranceEntity>>(jObj["result"]["results"].ToString());

                return list.ListToDataTable();
            }


            public class EntranceEntity
            {
                [JsonProperty("_id")]
                public int Id { get; set; }
                [JsonProperty("項次")]
                public int No { get; set; }
                [JsonProperty("出入口名稱")]
                public string Name { get; set; }
                [JsonProperty("出入口編號")]
                public string EntranceNo { get; set; }
                [JsonProperty("緯度")]
                public float Latitude { get; set; }
                [JsonProperty("經度")]
                public float Longitude { get; set; }
            } 

        }


    }
}
