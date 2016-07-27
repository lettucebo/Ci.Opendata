using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace Creatidea.Opendata.Weather
{
    /// <summary>
    /// 行政院環境保護署-紫外線即時監測資料
    /// </summary>
    public class EpaUv : OpenData
    {
        private static readonly object _staticLockObj = new object();

        public class UvModel
        {
            public string SiteName { get; set; }
            public string Uvi { get; set; }
            public string PublishAgency { get; set; }
            public string County { get; set; }
            public string Wgs84Lon { get; set; }
            public string Wgs84Lat { get; set; }
            public string PublishTime { get; set; }
        }

        protected override JObject Data()
        {
            var xmlString = Tool.GetWebContent("http://opendata.epa.gov.tw/ws/Data/UV/?format=xml", Encoding.UTF8);

            var xmlDocument = new System.Xml.XmlDocument();
            xmlDocument.LoadXml(xmlString);
            var jsonString = JsonConvert.SerializeXmlNode(xmlDocument);

            var jObject = JsonConvert.DeserializeObject<JObject>(jsonString);

            return jObject;
        }

        public override void Dispose()
        {
            lock (LockObj)
            {
                UvData.Clear();
                UvData = null;
            }
        }

        protected override void Save(JObject jObj)
        {
            var jObject = JsonConvert.DeserializeObject<List<UvModel>>(jObj["UV"]["Data"].ToString());
            lock (LockObj)
            {
                UvData = jObject;
            }
        }
        
        private static List<UvModel> UvData { get; set; }

        public static List<UvModel> Get
        {
            get
            {
                lock (_staticLockObj)
                {
                    return UvData;
                }
            }
        }
    }
}
