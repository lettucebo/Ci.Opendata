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
    /// 臺北市商圈通訊錄
    /// </summary>
    public class ShoppingArea
    {
        public class Location : OpenDataDataBaseLocation
        {
            protected override string TableName()
            {
                return "TaipeiShoppingAreaLocation";
            }

            protected override string CreateTableSqlString()
            {
                return @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaipeiShoppingAreaLocation]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[TaipeiShoppingAreaLocation](
	[Id] [int] NOT NULL,
	[Area] [nvarchar](50) NULL,
	[Name] [nvarchar](50) NULL,
	[LegalName] [nvarchar](50) NULL,
	[Telephone] [nvarchar](50) NULL,
	[Address] [nvarchar](500) NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL,
 CONSTRAINT [PK_TaipeiShoppingAreaLocation] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
END
";
            }

            public override JObject Data()
            {
                var jsonString = Tool.GetWebContent("http://data.taipei/opendata/datalist/apiAccess?scope=resourceAquire&rid=666d17d8-f759-42bf-9186-b15260445a9b", Encoding.UTF8);

                var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                return jObject;
            }

            protected override DataTable Resolve(JObject jObj)
            {
                var list = JsonConvert.DeserializeObject<List<ShoppingAreaLocationEntity>>(jObj["result"]["results"].ToString());

                foreach (var item in list)
                {
                    var addressJson = Tool.GetAddressLatLng(item.Address, item);

                    if (addressJson == null)
                    {
                        item.Latitude = 0;
                        item.Longitude = 0;
                        continue;
                    }

                    item.Latitude = addressJson.Latitude;
                    item.Longitude = addressJson.Longitude;
                }
                
                return list.ListToDataTable();
            }


            public class ShoppingAreaLocationEntity: ShoppingAreaEntity, ILocation
            {
                /// <summary>
                /// 緯度
                /// </summary>
                public float Latitude { get; set; }
                /// <summary>
                /// 經度
                /// </summary>
                public float Longitude { get; set; }
            }
        }

        public class ShoppingAreaEntity
        {
            /// <summary>
            /// 代碼
            /// </summary>
            [JsonProperty("_id")]
            public string Id { get; set; }
            /// <summary>
            /// 行政區
            /// </summary>
            [JsonProperty("district")]
            public string Area { get; set; }
            /// <summary>
            /// 商圈名稱
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 商圈組織
            /// </summary>
            public string LegalName { get; set; }
            /// <summary>
            /// 商圈聯絡方式
            /// </summary>
            public string Telephone { get; set; }
            /// <summary>
            /// 商圈通訊地址
            /// </summary>
            public string Address { get; set; }

        }

    }

}
