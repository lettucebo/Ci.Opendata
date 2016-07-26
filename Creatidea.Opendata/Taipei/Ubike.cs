using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Creatidea.Opendata.Taipei
{
    /// <summary>
    /// 微笑單車(Ubike)
    /// </summary>
    /// <seealso cref="Creatidea.Opendata.OpenData" />
    public class Ubike : OpenData
    {
        private static object _staticLockObj = new object();

        public override JObject Data()
        {
            var jsonString = Tool.GetWebContent("http://data.taipei/youbike", Encoding.UTF8);

            var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

            return jObject;
        }

        protected override void Save(JObject jObject)
        {
            Parallel.ForEach(jObject["retVal"], (items, loopState) =>
            {
                foreach (var item in items)
                {
                    var sno = Convert.ToString(item["sno"]);
                    lock (LockObj)
                    {
                        if (_ubikeList.ContainsKey(sno))
                        {
                            _ubikeList[sno] = item;
                        }
                        else
                        {
                            _ubikeList.Add(sno, item);
                        }
                    }
                }
            });
        }

        public override void Dispose()
        {
            lock (LockObj)
            {
                _ubikeList.Clear();
                _ubikeList = null;
            }
        }

        /// <summary>
        /// 單車站資料
        /// </summary>
        private static Dictionary<string, JToken> _ubikeList = new Dictionary<string, JToken>();

        /// <summary>
        /// 剩餘車位
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static int GetSpace(string id)
        {
            var available = int.MinValue;

            if (_ubikeList.ContainsKey(id))
            {
                available = int.Parse(Convert.ToString(_ubikeList[id]["bemp"]));
            }

            return available;
        }

        /// <summary>
        /// 剩餘車輛
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static int GetBike(string id)
        {
            var available = int.MinValue;
            lock (_staticLockObj)
            {
                if (_ubikeList.ContainsKey(id))
                {
                    available = int.Parse(Convert.ToString(_ubikeList[id]["sbi"]));
                }
            }

            return available;
        }

    }
}
