using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            public override JObject Get()
            {
                var jsonString = Tool.GetWebContent("http://data.taipei/tcmsv/allavailable", Encoding.UTF8);

                var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

                return jObject;
            }

            public override void Save(JObject jObj)
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
            private Dictionary<string, Dictionary<string, int>> _leftParkingList = new Dictionary<string, Dictionary<string, int>>();

            /// <summary>
            /// 剩餘車位(汽車)
            /// </summary>
            /// <param name="id">The identifier.</param>
            /// <returns></returns>
            public int GetCar(string id)
            {
                var available = -1;
                lock (LockObj)
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
            public int GetMotor(string id)
            {
                var available = -1;
                lock (LockObj)
                {
                    if (_leftParkingList.ContainsKey(id))
                    {
                        available = int.Parse(Convert.ToString(_leftParkingList[id]["availablemotor"]));
                    }
                }

                return available;
            }

        }
    }
}
