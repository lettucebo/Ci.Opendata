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

            protected override JObject Data()
            {
                var jsonString = Tool.GetWebContent("http://data.taipei/opendata/datalist/apiAccess?scope=resourceAquire&rid=9c2fa3e8-118e-4b49-8a7c-57edd29ca3ec", Encoding.UTF8);

                var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                return jObject;
            }
            protected override DataTable Resolve(JObject jObj)
            {
                var list = JsonConvert.DeserializeObject<List<EntranceResolveEntity>>(jObj["result"]["results"].ToString());

                return list.ListToDataTable();
            }

            private class EntranceResolveEntity : EntranceEntity
            {
                [JsonProperty("_id")]
                public new int Id { get; set; }
                [JsonProperty("項次")]
                public new int No { get; set; }
                [JsonProperty("出入口名稱")]
                public new string Name { get; set; }
                [JsonProperty("出入口編號")]
                public new string EntranceNo { get; set; }
                [JsonProperty("緯度")]
                public new float Latitude { get; set; }
                [JsonProperty("經度")]
                public new float Longitude { get; set; }
            }

            /// <summary>
            /// 捷運出入口
            /// </summary>
            public class EntranceEntity
            {
                /// <summary>
                /// 代碼
                /// </summary>
                public int Id { get; set; }
                /// <summary>
                /// 項次
                /// </summary>
                public int No { get; set; }
                /// <summary>
                /// 出入口名稱
                /// </summary>
                public string Name { get; set; }
                /// <summary>
                /// 出入口編號
                /// </summary>
                public string EntranceNo { get; set; }
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
            /// 取得捷運出口
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <returns></returns>
            public static EntranceEntity Get(string id)
            {
                EntranceEntity entity = null;

                using (var openData = new Entrance())
                {
                    var table = openData.GetById(id);

                    entity = table.ToList<EntranceEntity>().FirstOrDefault();
                }

                return entity;
            }

            /// <summary>
            /// 取得捷運出口
            /// </summary>
            /// <param name="lat">緯度</param>
            /// <param name="lng">經度</param>
            /// <param name="locationRadius">半徑範圍</param>
            /// <returns></returns>
            public static IList<EntranceEntity> Get(float lat, float lng, int locationRadius = 1)
            {
                IList<EntranceEntity> list = null;

                using (var openData = new Entrance())
                {
                    var table = openData.GetByLatLng(lat, lng, locationRadius);

                    list = table.ToList<EntranceEntity>();
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
}
