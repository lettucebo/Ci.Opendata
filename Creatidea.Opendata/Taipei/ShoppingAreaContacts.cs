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
    /// 臺北市商圈通訊錄
    /// </summary>
    public class ShoppingArea
    {
        /// <summary>
        /// 有地理座標
        /// </summary>
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

            protected override JObject Data()
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

            /// <summary>
            /// 取得臺北市商圈
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <returns></returns>
            public static ShoppingAreaLocationEntity Get(string id)
            {
                ShoppingAreaLocationEntity entity = null;

                using (var openData = new Location())
                {
                    var table = openData.GetById(id);

                    entity = table.ToList<ShoppingAreaLocationEntity>().FirstOrDefault();
                }

                return entity;
            }

            /// <summary>
            /// 取得臺北市商圈
            /// </summary>
            /// <param name="lat">緯度</param>
            /// <param name="lng">經度</param>
            /// <param name="locationRadius">半徑範圍</param>
            /// <returns></returns>
            public static IList<ShoppingAreaLocationEntity> Get(float lat, float lng, int locationRadius = 1)
            {
                IList<ShoppingAreaLocationEntity> list = null;

                using (var openData = new Location())
                {
                    var table = openData.GetByLatLng(lat, lng, locationRadius);

                    list = table.ToList<ShoppingAreaLocationEntity>();
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
