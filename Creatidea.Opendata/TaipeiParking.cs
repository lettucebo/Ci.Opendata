using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Creatidea.Opendata
{
    public class TaipeiParking
    {
        public JObject LeftParkingAvailable()
        {
            var jsonString = Tool.GetWebContent("http://data.taipei/tcmsv/allavailable", Encoding.UTF8);

            var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

            return jObject;
        }

        private readonly object _objlock = new object();

        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<string, Dictionary<string, int>> _leftParkingList = new Dictionary<string, Dictionary<string, int>>();

        /// <summary>
        /// 剩餘車位
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public int GetLeftParkingAvailableCar(string id)
        {
            var available = int.MinValue;
            lock (_objlock)
            {
                if (_leftParkingList.ContainsKey(id))
                {
                    available = int.Parse(Convert.ToString(_leftParkingList[id]["availablecar"]));
                }
            }

            return available;
        }

        /// <summary>
        /// 剩餘車位
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public int GetLeftParkingAvailableMotor(string id)
        {
            var available = int.MinValue;
            lock (_objlock)
            {
                if (_leftParkingList.ContainsKey(id))
                {
                    available = int.Parse(Convert.ToString(_leftParkingList[id]["availablemotor"]));
                }
            }

            return available;
        }
        
        public void SaveLeftParkingAvailable(JObject jObject)
        {
            Parallel.ForEach(jObject["data"]["park"], (item, loopState) =>
            {
                var id = item["id"].ToString();
                var availablecar = int.Parse(item["availablecar"].ToString());
                var availablemotor = int.Parse(item["availablemotor"].ToString());

                var parkAvailable = new Dictionary<string, int>
                {
                    {"availablecar", availablecar},
                    {"availablemotor", availablemotor}
                };
                lock (_objlock)
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
    }
}
