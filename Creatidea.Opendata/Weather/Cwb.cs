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
    /// 一般天氣預報-今明36小時天氣預報
    /// </summary>
    public class Cwb : OpenData
    {
        public override JObject Data()
        {
            //http://opendata.cwb.gov.tw/datalist
            var dataId = "F-C0032-001";
            var key = "CWB-CF47827D-C7E4-4A69-AF55-BEFA971106ED";

            var xmlString = Tool.GetWebContent(string.Format("http://opendata.cwb.gov.tw/opendataapi?dataid={0}&authorizationkey={1}", dataId, key), Encoding.UTF8);
            //var xmlString = Tool.GetWebContent("http://opendata.cwb.gov.tw/opendata/DIV2/O-A0001-001.xml", Encoding.UTF8);
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
                WatherData.Clear();
                WatherData = null;
            }
        }

        protected override void ToMemory(JObject jObj)
        {
            var jObject = JsonConvert.DeserializeObject<List<WatherModel>>(jObj["cwbopendata"]["dataset"]["location"].ToString());
            lock (LockObj)
            {
                WatherData = jObject;
            }
        }

        private static List<WatherModel> WatherData { get; set; }

        public static List<WatherModel> Get
        {
            get
            {
                lock (StaticLockObj)
                {
                    return WatherData;
                }
            }
        }

        /// <summary>
        /// 氣象局OpenData資料架構
        /// </summary>
        public class WatherModel
        {
            public string LocationName { get; set; }
            public List<WeatherElementModel> WeatherElement { get; set; }
            public class WeatherElementModel
            {
                public string ElementName { get; set; }
                public TimeModel[] Time { get; set; }
                public class TimeModel
                {
                    public DateTime StartTime { get; set; }
                    public DateTime EndTime { get; set; }

                    public ParameterModel Parameter { get; set; }
                    public class ParameterModel
                    {
                        public string ParameterName { get; set; }
                        public string ParameterUnit { get; set; }
                        public string ParameterValue { get; set; }
                    }
                }
            }

        }
    }
}
