using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Creatidea.Opendata.TaipeiBus
{
    public class Synchronize
    {
        public static void EstimateTime()
        {
            var jsonString = Library.Web.GetWebContent("http://data.taipei/bus/EstimateTime", Encoding.UTF8, gZip: true, onlyGzip: true);

            var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);


            Console.Write(jObject);
            Console.ReadLine();
        }

    }
}
