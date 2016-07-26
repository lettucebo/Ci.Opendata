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
    /// 停車場資訊
    /// </summary>
    public class Parking
    {
        /// <summary>
        /// 剩餘車位
        /// </summary>
        public class Available : OpenData
        {
            private static object _staticLockObj = new object();

            public override JObject Data()
            {
                //var jsonString = Tool.GetWebContent("http://data.taipei/tcmsv/allavailable", Encoding.UTF8);
                var jsonString = Tool.GetGzContent("http://data.taipei/tcmsv/allavailable", Encoding.Default);

                var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                return jObject;
            }

            protected override void Save(JObject jObj)
            {
                Parallel.ForEach(jObj["data"]["park"], (item, loopState) =>
                {
                    var id = item["id"].ToString();
                    var availablecar = int.Parse(item["availablecar"].ToString());
                    var availablemotor = int.Parse(item["availablemotor"].ToString());

                    var parkAvailable = new Dictionary<string, int>
                    {
                        { "availablecar", availablecar},
                        { "availablemotor", availablemotor}
                    };
                    lock (LockObj)
                    {
                        if (_leftParkingList.ContainsKey(id))
                        {
                            _leftParkingList[id] = parkAvailable;
                        }
                        else
                        {
                            _leftParkingList.Add(id, parkAvailable);
                        }
                    }
                });
            }

            public override void Dispose()
            {
                lock (LockObj)
                {
                    _leftParkingList.Clear();
                    _leftParkingList = null;
                }
            }

            /// <summary>
            /// 剩餘車位資料
            /// </summary>
            private static Dictionary<string, Dictionary<string, int>> _leftParkingList = new Dictionary<string, Dictionary<string, int>>();

            /// <summary>
            /// 剩餘車位(汽車)
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <returns></returns>
            public static int GetCar(string id)
            {
                var available = -1;
                lock (_staticLockObj)
                {
                    if (_leftParkingList.ContainsKey(id))
                    {
                        available = int.Parse(Convert.ToString(_leftParkingList[id]["availablecar"]));
                    }
                }

                return available;
            }

            /// <summary>
            /// 剩餘車位(機車)
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <returns></returns>
            public static int GetMotor(string id)
            {
                var available = -1;
                lock (_staticLockObj)
                {
                    if (_leftParkingList.ContainsKey(id))
                    {
                        available = int.Parse(Convert.ToString(_leftParkingList[id]["availablemotor"]));
                    }
                }

                return available;
            }

        }

        public class Description : OpenDataDataBase
        {
            private static object _staticLockObj = new object();

            private static DataTable NewTable()
            {
                var dataTable = new DataTable();
                dataTable.Columns.Add("Id", typeof(string));
                dataTable.Columns.Add("Area", typeof(string));
                dataTable.Columns.Add("Name", typeof(string));
                dataTable.Columns.Add("Type", typeof(int));
                dataTable.Columns.Add("Type2", typeof(int));
                dataTable.Columns.Add("Summary", typeof(string));
                dataTable.Columns.Add("Address", typeof(string));
                dataTable.Columns.Add("Tel", typeof(string));
                dataTable.Columns.Add("PayEx", typeof(string));
                dataTable.Columns.Add("ServiceTime", typeof(string));
                dataTable.Columns.Add("TotalCar", typeof(int));
                dataTable.Columns.Add("TotalMotor", typeof(int));
                dataTable.Columns.Add("TotalBike", typeof(int));
                dataTable.Columns.Add("PregnancyFirst", typeof(int));
                dataTable.Columns.Add("HandicapFirst", typeof(int));

                dataTable.Columns.Add("Latitude", typeof(float));
                dataTable.Columns.Add("Longitude", typeof(float));

                return dataTable;
            }

            public override JObject Data()
            {
                var jsonString = Tool.GetGzContent("http://data.taipei/tcmsv/alldesc", Encoding.Default);

                var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                return jObject;
            }

            protected override void Save(JObject jObj)
            {
                var dataTable = NewTable();

                var jObject = JsonConvert.DeserializeObject<List<DescriptionEntity>>(jObj["data"]["park"].ToString());
                foreach (var item in jObject)
                {
                    var ct = new Tool.CoordinateTransform();
                    var lonlat = ct.TWD97_To_lonlat(item.Longitude, item.Latitude, 2);

                    var lnglatArray = lonlat.Split(new[] { "," }, StringSplitOptions.None);

                    var row = dataTable.NewRow();

                    row["Id"] = item.Id;
                    row["Area"] = item.Area;
                    row["Name"] = item.Name;
                    row["Type"] = item.Type;
                    row["Type2"] = item.Type2;
                    row["Summary"] = item.Summary;
                    row["Address"] = item.Address;
                    row["Tel"] = item.Tel;
                    row["PayEx"] = item.PayEx;
                    row["ServiceTime"] = item.ServiceTime;
                    if (item.TotalCar.HasValue)
                    {
                        row["TotalCar"] = item.TotalCar.Value;
                    }
                    else
                    {
                        row["TotalCar"] = DBNull.Value;
                    }

                    if (item.TotalMotor.HasValue)
                    {
                        row["TotalMotor"] = item.TotalMotor.Value;
                    }
                    else
                    {
                        row["TotalMotor"] = DBNull.Value;
                    }

                    if (item.TotalBike.HasValue)
                    {
                        row["TotalBike"] = item.TotalBike.Value;
                    }
                    else
                    {
                        row["TotalBike"] = DBNull.Value;
                    }

                    if (item.PregnancyFirst.HasValue)
                    {
                        row["PregnancyFirst"] = item.PregnancyFirst.Value;
                    }
                    else
                    {
                        row["PregnancyFirst"] = DBNull.Value;
                    }

                    if (item.HandicapFirst.HasValue)
                    {
                        row["HandicapFirst"] = item.HandicapFirst.Value;
                    }
                    else
                    {
                        row["HandicapFirst"] = DBNull.Value;
                    }

                    row["Latitude"] = Convert.ToDouble(lnglatArray[1]);
                    row["Longitude"] = Convert.ToDouble(lnglatArray[0]);

                    dataTable.Rows.Add(row);

                }

                lock (_staticLockObj)
                {
                    CreateDataTable();
                    SaveToDatabase(dataTable);
                }
            }

            public override void Dispose()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// 取得停車場
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <returns></returns>
            public static DescriptionEntity Get(string id)
            {
                DescriptionEntity entity = null;

                var description = new Description();

                var table = description.GetById(id);

                entity = table.ToList<DescriptionEntity>().FirstOrDefault();

                description.Dispose();

                return entity;
            }

            /// <summary>
            /// 取得停車場
            /// </summary>
            /// <param name="lat">緯度</param>
            /// <param name="lng">經度</param>
            /// <returns></returns>
            public static IList<DescriptionEntity> Get(float lat, float lng)
            {
                IList<DescriptionEntity> list = null;

                var description = new Description();
                var table = description.GetByLatLng(lat, lng);
                
                list = table.ToList<DescriptionEntity>();

                description.Dispose();
                
                return list;
            }

            public class DescriptionEntity
            {
                /// <summary>
                /// 停車場的編號
                /// </summary>
                public string Id { get; set; }
                /// <summary>
                /// 行政區
                /// </summary>
                public string Area { get; set; }
                /// <summary>
                /// 停車場名稱
                /// </summary>
                public string Name { get; set; }
                /// <summary>
                /// 1:動態停車場(可取得目前剩餘車位數) 2:靜態停車場
                /// </summary>
                public int Type { get; set; }
                /// <summary>
                /// 1:停管處經營 2:非停管處經營
                /// </summary>
                public int Type2 { get; set; }
                /// <summary>
                /// 停車場概況
                /// </summary>
                public string Summary { get; set; }
                /// <summary>
                /// 停車場地址
                /// </summary>
                public string Address { get; set; }
                /// <summary>
                /// 停車場電話
                /// </summary>
                public string Tel { get; set; }
                /// <summary>
                /// 停車場收費資訊 
                /// </summary>
                public string PayEx { get; set; }
                /// <summary>
                /// 開放時間
                /// </summary>
                public string ServiceTime { get; set; }
                /// <summary>
                /// TWD97 Y座標值
                /// </summary>
                [JsonProperty("Tw97Y")]
                public float Latitude { get; set; }
                /// <summary>
                /// TWD97 X座標值
                /// </summary>
                [JsonProperty("Tw97X")]
                public float Longitude { get; set; }
                /// <summary>
                /// 停車場（汽車）總車位數
                /// </summary>
                public int? TotalCar { get; set; }
                /// <summary>
                /// 停車場（機車）總格位數
                /// </summary>
                public int? TotalMotor { get; set; }
                /// <summary>
                /// 停車場（腳踏車）總車架數
                /// </summary>
                public int? TotalBike { get; set; }
                /// <summary>
                /// 孕婦優先車格位數
                /// </summary>
                [JsonProperty("Pregnancy_First")]
                public int? PregnancyFirst { get; set; }
                /// <summary>
                /// 身障車格位數
                /// </summary>
                [JsonProperty("Handicap_First")]
                public int? HandicapFirst { get; set; }
                /// <summary>
                /// 費率資訊
                /// </summary>
                //public FareInfoEntity[] FareInfo { get; set; }

                /// <summary>
                /// 入口座標資訊
                /// </summary>
                //public EntrancecoordInfo[] Entrancecoord { get; set; }

                /// <summary>
                /// 入口座標資訊
                /// </summary>
                public class EntrancecoordInfo
                {
                    /// <summary>
                    /// WGS X座標值（經度）
                    /// </summary>
                    public float Xcod { get; set; }
                    /// <summary>
                    /// WGS Y座標值（緯度）
                    /// </summary>
                    public float Ycod { get; set; }
                    /// <summary>
                    /// 地址
                    /// </summary>
                    public float Addresss { get; set; }
                }

                /// <summary>
                /// 費率資訊
                /// </summary>
                public class FareInfoEntity
                {
                    /// <summary>
                    /// 一般日
                    /// </summary>
                    public PeriodFare[] WorkingDay { get; set; }
                    /// <summary>
                    /// 假日
                    /// </summary>
                    public PeriodFare[] Holiday { get; set; }

                    public class PeriodFare
                    {
                        /// <summary>
                        /// 某時段（例如：00～09表示凌晨0時至上午9時）
                        /// </summary>
                        public string Period { get; set; }
                        /// <summary>
                        /// 費率
                        /// </summary>
                        public string Fare { get; set; }
                    }
                }

                /// <summary>
                /// 充電站資訊
                /// </summary>
                //public List<ChargeStationEntity> ChargeStation { get; set; }

                /// <summary>
                /// 充電站資訊
                /// </summary>
                public class ChargeStationEntity
                {
                    public string StationName { get; set; }
                    public string StationAddr { get; set; }
                    public string LocLongitude { get; set; }
                    public string LocLatitude { get; set; }
                    public string OpenFlag { get; set; }
                    public string IsCharge { get; set; }
                    public string ContactName { get; set; }
                    public string ContactMobilNo { get; set; }
                    public string ScoketCount { get; set; }
                    public string AvailableCount { get; set; }
                    public string Country { get; set; }
                    public string Town { get; set; }
                }


            }

            public void CreateDataTable()
            {
                var sqlConnection = new SqlConnection(ConnectionString);

                sqlConnection.Open();
                
                var sqlCommand = sqlConnection.CreateCommand();

                sqlCommand.CommandTimeout = TimeOut;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TaipeiParkingArea]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[TaipeiParkingArea](
	[Id] [nvarchar](50) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Area] [nvarchar](50) NULL,
	[Type] [int] NULL,
	[Type2] [int] NULL,
	[Summary] [nvarchar](max) NULL,
	[Address] [nvarchar](max) NULL,
	[Tel] [nvarchar](50) NULL,
	[PayEx] [nvarchar](max) NULL,
	[ServiceTime] [nvarchar](max) NULL,
	[TotalCar] [int] NULL,
	[TotalMotor] [int] NULL,
	[TotalBike] [int] NULL,
	[PregnancyFirst] [int] NULL,
	[HandicapFirst] [int] NULL,
	[Latitude] [float] NULL,
	[Longitude] [float] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END

";
                sqlCommand.ExecuteNonQuery();

                sqlConnection.Close();
                sqlConnection.Dispose();


            }

            public void SaveToDatabase(DataTable table)
            {
                table.TableName = "TaipeiParkingArea";
                var sqlConnection = new SqlConnection(ConnectionString);

                sqlConnection.Open();

                var transaction = sqlConnection.BeginTransaction();

                var sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.Transaction = transaction;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = @" TRUNCATE TABLE TaipeiParkingArea ";
                sqlCommand.CommandTimeout = TimeOut;

                sqlCommand.ExecuteNonQuery();

                // make sure to enable triggers
                // more on triggers in next post
                using (var sqlBulkCopy = new SqlBulkCopy(sqlConnection, SqlBulkCopyOptions.KeepIdentity
                    , transaction))
                {
                    sqlBulkCopy.BulkCopyTimeout = int.MaxValue;
                    sqlBulkCopy.DestinationTableName = "dbo.TaipeiParkingArea";

                    sqlBulkCopy.ColumnMappings.Add("Id", "Id");
                    sqlBulkCopy.ColumnMappings.Add("Area", "Area");
                    sqlBulkCopy.ColumnMappings.Add("Name", "Name");
                    sqlBulkCopy.ColumnMappings.Add("Type", "Type");
                    sqlBulkCopy.ColumnMappings.Add("Type2", "Type2");
                    sqlBulkCopy.ColumnMappings.Add("Summary", "Summary");
                    sqlBulkCopy.ColumnMappings.Add("Address", "Address");
                    sqlBulkCopy.ColumnMappings.Add("Tel", "Tel");
                    sqlBulkCopy.ColumnMappings.Add("PayEx", "PayEx");
                    sqlBulkCopy.ColumnMappings.Add("ServiceTime", "ServiceTime");
                    sqlBulkCopy.ColumnMappings.Add("TotalCar", "TotalCar");
                    sqlBulkCopy.ColumnMappings.Add("TotalMotor", "TotalMotor");
                    sqlBulkCopy.ColumnMappings.Add("TotalBike", "TotalBike");
                    sqlBulkCopy.ColumnMappings.Add("PregnancyFirst", "PregnancyFirst");
                    sqlBulkCopy.ColumnMappings.Add("HandicapFirst", "HandicapFirst");
                    sqlBulkCopy.ColumnMappings.Add("Latitude", "Latitude");
                    sqlBulkCopy.ColumnMappings.Add("Longitude", "Longitude");

                    sqlBulkCopy.WriteToServer(table);

                }

                transaction.Commit();

                sqlConnection.Close();
                sqlConnection.Dispose();
            }

            public DataTable GetById(string id)
            {
                DataTable table = null;

                var sqlConnection = new SqlConnection(ConnectionString);

                sqlConnection.Open();

                var sqlCommand = sqlConnection.CreateCommand();

                sqlCommand.CommandTimeout = TimeOut;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = @" SELECT * FROM TaipeiParkingArea WHERE Id = @Id ";
                sqlCommand.Parameters.Add("@Id", SqlDbType.NVarChar).Value = id;

                table = new DataTable();
                var adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(table);

                sqlCommand.ExecuteNonQuery();

                sqlConnection.Close();
                sqlConnection.Dispose();


                return table;
            }

            public DataTable GetByLatLng(float lat, float lng)
            {
                DataTable table = null;

                var sqlConnection = new SqlConnection(ConnectionString);

                sqlConnection.Open();

                var sqlCommand = sqlConnection.CreateCommand();

                sqlCommand.CommandTimeout = TimeOut;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = @" SELECT * FROM TaipeiParkingArea WHERE SQRT((((CONVERT(float,@Lng)-CONVERT(float,Longitude))*PI()*12656*cos(((CONVERT(float,@Lat)+CONVERT(float,Latitude))/2)*PI()/180)/180)*((CONVERT(float,@Lng)-CONVERT(float,Longitude))*PI()*12656*cos (((CONVERT(float,@Lat)+CONVERT(float,Latitude))/2)*PI()/180)/180))+(((CONVERT(float,@Lat)-CONVERT(float,Latitude))*PI()*12656/180)*((CONVERT(float,@Lat)-CONVERT(float,Latitude))*PI()*12656/180)))< CONVERT(float,@KM) ";

                sqlCommand.Parameters.Add("@Lat", SqlDbType.NVarChar).Value = lat;
                sqlCommand.Parameters.Add("@Lng", SqlDbType.NVarChar).Value = lng;

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
