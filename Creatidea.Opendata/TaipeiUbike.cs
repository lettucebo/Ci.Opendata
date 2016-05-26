using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Creatidea.Opendata
{
    public class TaipeiUbike
    {
        public JObject Ubike()
        {
            var jsonString = Tool.GetWebContent("http://data.taipei/youbike", Encoding.UTF8);

            var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

            return jObject;
        }

        private readonly object _objlock = new object();

        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<string, JToken> _ubikeList = new Dictionary<string, JToken>();

        /// <summary>
        /// 剩餘車位
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public int GetLeftUbikeSpace(string id)
        {
            var available = int.MinValue;
            lock (_objlock)
            {
                if (_ubikeList.ContainsKey(id))
                {
                    available = int.Parse(Convert.ToString(_ubikeList[id]["bemp"]));
                }
            }

            return available;
        }

        /// <summary>
        /// 剩餘車輛
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public int GetLeftUbike(string id)
        {
            var available = int.MinValue;
            lock (_objlock)
            {
                if (_ubikeList.ContainsKey(id))
                {
                    available = int.Parse(Convert.ToString(_ubikeList[id]["sbi"]));
                }
            }

            return available;
        }

        public void SaveUbike(JObject jObject)
        {
            Parallel.ForEach(jObject["retVal"], (items, loopState) =>
            {
                foreach (var item in items)
                {
                    var sno = Convert.ToString(item["sno"]);
                    lock (_objlock)
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
    }
}
