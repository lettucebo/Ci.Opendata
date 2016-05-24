using System;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Creatidea.Opendata.Library
{
    public class TaipeiBus 
    {
        public JObject EstimateTime()
        {
            var jsonString = Tool.GetWebContent("http://data.taipei/bus/EstimateTime", Encoding.UTF8, gZip: true, onlyGzip: true);

            var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

            return jObject;
        }

        public void SaveEstimateTime(JObject jObject, string path)
        {
            path += "EstimateTime\\";

            foreach (var item in jObject)
            {
                
            }


        }
    }
}
