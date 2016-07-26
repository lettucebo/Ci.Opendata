using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Creatidea.Opendata
{
    public class Tool
    {
        /// <summary>
        /// 讀取網頁內容(zip)
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        public static Dictionary<string, string> DownloadAndGetUnzipFileContent(string url)
        {
            var result = new Dictionary<string, string>();
            //取得日期
            var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);

            var tempPath = Path.GetTempPath();
            if (!tempPath.EndsWith("\\"))
            {
                tempPath += "\\";
            }
            var fileName = $"temp_{date.ToString("yyyyMMdd")}_{Guid.NewGuid().ToString("N")}";
            var downloadPath = tempPath + fileName;

            #region 下載檔案
            var webClient = new WebClient();
            webClient.DownloadFile(url, downloadPath);
            #endregion

            #region 解壓縮

            var content = string.Empty;
            if (File.Exists(downloadPath))
            {
                using (var archive = ZipFile.OpenRead(downloadPath))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (result.ContainsKey(entry.FullName))
                        {
                            continue;
                        }

                        var extractTempPath = downloadPath += $"_{Guid.NewGuid().ToString("N")}";
                        entry.ExtractToFile(extractTempPath, true);
                        content = File.ReadAllText(extractTempPath);
                        result.Add(entry.FullName, content);
                        File.Delete(extractTempPath);
                        break;
                    }
                }

                File.Delete(downloadPath);
            }

            #endregion

            return result;
        }

        /// <summary>
        /// 讀取網頁內容(gz)
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="encoding">The encoding.</param>
        /// <param name="postData">The post data.</param>
        /// <returns></returns>
        public static string GetGzContent(string url, Encoding encoding, string postData = "")
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.126 Safari/533.4";

            if (!string.IsNullOrEmpty(postData))
            {
                webRequest.Method = "POST";
                var lbPostBuffer = encoding.GetBytes(postData);

                webRequest.ContentLength = lbPostBuffer.Length;

                var postStream = webRequest.GetRequestStream();
                postStream.Write(lbPostBuffer, 0, lbPostBuffer.Length);
                postStream.Close();
            }

            var webResponse = (HttpWebResponse)webRequest.GetResponse();

            Stream responseStream = webResponse.GetResponseStream();

            #region 解壓縮

            var content = string.Empty;

            if (responseStream != null)
            {
                using (var decompressionStream = new GZipStream(responseStream, CompressionMode.Decompress))
                {
                    using (var sr = new StreamReader(decompressionStream, encoding))
                    {
                        content = sr.ReadToEnd();
                    }
                }
            }

            #endregion

            return content;
        }

        /// <summary>
        /// Simple routine to retrieve HTTP Content as a string with
        /// optional POST data and gZip encoding.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="encoding"></param>
        /// <param name="postData">The post data.</param>
        /// <param name="gZip">if set to <c>true</c> [g zip].</param>
        /// <param name="onlyGzip">if set to <c>true</c> [only gzip].</param>
        /// <returns></returns>
        public static string GetWebContent(string url, Encoding encoding, string postData = "", bool gZip = false, bool onlyGzip = false)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.UserAgent = "Mozilla/5.0 (Windows; U; Windows NT 6.1; en-US) AppleWebKit/533.4 (KHTML, like Gecko) Chrome/5.0.375.126 Safari/533.4";
            if (gZip)
            {
                webRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            }

            if (!string.IsNullOrEmpty(postData))
            {
                webRequest.Method = "POST";
                var lbPostBuffer = encoding.GetBytes(postData);

                webRequest.ContentLength = lbPostBuffer.Length;

                var postStream = webRequest.GetRequestStream();
                postStream.Write(lbPostBuffer, 0, lbPostBuffer.Length);
                postStream.Close();
            }

            var webResponse = (HttpWebResponse)webRequest.GetResponse();

            Stream responseStream = responseStream = webResponse.GetResponseStream();

            if (webResponse.ContentEncoding.ToLower().Contains("gzip") || onlyGzip)
                responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
            else if (webResponse.ContentEncoding.ToLower().Contains("deflate"))
                responseStream = new DeflateStream(responseStream, CompressionMode.Decompress);

            var reader = new StreamReader(responseStream, encoding);

            string content = reader.ReadToEnd();
            webResponse.Close();
            responseStream.Close();

            return content;
        }
        
        public class CoordinateTransform
        {
            double a = 6378137.0;
            double b = 6356752.314245;
            double lon0 = 121 * Math.PI / 180;
            double k0 = 0.9999;
            int dx = 250000;

            public CoordinateTransform()
            {
                //
                // TODO: 在此加入建構函式的程式碼
                //
            }

            //給WGS84經緯度度分秒轉成TWD97坐標
            public string lonlat_To_twd97(int lonD, int lonM, int lonS, int latD, int latM, int latS)
            {
                double RadianLon = (double)(lonD) + (double)lonM / 60 + (double)lonS / 3600;
                double RadianLat = (double)(latD) + (double)latM / 60 + (double)latS / 3600;
                return Cal_lonlat_To_twd97(RadianLon, RadianLat);
            }
            //給WGS84經緯度弧度轉成TWD97坐標
            public string lonlat_To_twd97(double RadianLon, double RadianLat)
            {
                return Cal_lonlat_To_twd97(RadianLon, RadianLat);
            }

            //給TWD97坐標 轉成 WGS84 度分秒字串  (type1傳度分秒   2傳弧度)
            public string TWD97_To_lonlat(double XValue, double YValue, int Type)
            {

                string lonlat = "";

                if (Type == 1)
                {
                    string[] Answer = Cal_TWD97_To_lonlat(XValue, YValue).Split(',');
                    int LonDValue = (int)double.Parse(Answer[0]);
                    int LonMValue = (int)((double.Parse(Answer[0]) - LonDValue) * 60);
                    int LonSValue = (int)((((double.Parse(Answer[0]) - LonDValue) * 60) - LonMValue) * 60);

                    int LatDValue = (int)double.Parse(Answer[1]);
                    int LatMValue = (int)((double.Parse(Answer[1]) - LatDValue) * 60);
                    int LatSValue = (int)((((double.Parse(Answer[1]) - LatDValue) * 60) - LatMValue) * 60);

                    lonlat = LonDValue + "度" + LonMValue + "分" + LonSValue + "秒," + LatDValue + "度" + LatMValue + "分" + LatSValue + "秒,";
                }
                else if (Type == 2)
                {
                    lonlat = Cal_TWD97_To_lonlat(XValue, YValue);
                }

                return lonlat;
            }



            private string Cal_lonlat_To_twd97(double lon, double lat)
            {
                string TWD97 = "";

                lon = (lon / 180) * Math.PI;
                lat = (lat / 180) * Math.PI;

                //---------------------------------------------------------
                double e = Math.Pow((1 - Math.Pow(b, 2) / Math.Pow(a, 2)), 0.5);
                double e2 = Math.Pow(e, 2) / (1 - Math.Pow(e, 2));
                double n = (a - b) / (a + b);
                double nu = a / Math.Pow((1 - (Math.Pow(e, 2)) * (Math.Pow(Math.Sin(lat), 2))), 0.5);
                double p = lon - lon0;
                double A = a * (1 - n + (5 / 4) * (Math.Pow(n, 2) - Math.Pow(n, 3)) + (81 / 64) * (Math.Pow(n, 4) - Math.Pow(n, 5)));
                double B = (3 * a * n / 2.0) * (1 - n + (7 / 8.0) * (Math.Pow(n, 2) - Math.Pow(n, 3)) + (55 / 64.0) * (Math.Pow(n, 4) - Math.Pow(n, 5)));
                double C = (15 * a * (Math.Pow(n, 2)) / 16.0) * (1 - n + (3 / 4.0) * (Math.Pow(n, 2) - Math.Pow(n, 3)));
                double D = (35 * a * (Math.Pow(n, 3)) / 48.0) * (1 - n + (11 / 16.0) * (Math.Pow(n, 2) - Math.Pow(n, 3)));
                double E = (315 * a * (Math.Pow(n, 4)) / 51.0) * (1 - n);

                double S = A * lat - B * Math.Sin(2 * lat) + C * Math.Sin(4 * lat) - D * Math.Sin(6 * lat) + E * Math.Sin(8 * lat);

                //計算Y值
                double K1 = S * k0;
                double K2 = k0 * nu * Math.Sin(2 * lat) / 4.0;
                double K3 = (k0 * nu * Math.Sin(lat) * (Math.Pow(Math.Cos(lat), 3)) / 24.0) * (5 - Math.Pow(Math.Tan(lat), 2) + 9 * e2 * Math.Pow((Math.Cos(lat)), 2) + 4 * (Math.Pow(e2, 2)) * (Math.Pow(Math.Cos(lat), 4)));
                double y = K1 + K2 * (Math.Pow(p, 2)) + K3 * (Math.Pow(p, 4));

                //計算X值
                double K4 = k0 * nu * Math.Cos(lat);
                double K5 = (k0 * nu * (Math.Pow(Math.Cos(lat), 3)) / 6.0) * (1 - Math.Pow(Math.Tan(lat), 2) + e2 * (Math.Pow(Math.Cos(lat), 2)));
                double x = K4 * p + K5 * (Math.Pow(p, 3)) + dx;

                TWD97 = x.ToString() + "," + y.ToString();
                return TWD97;
            }


            private string Cal_TWD97_To_lonlat(double x, double y)
            {

                double dy = 0;
                double e = Math.Pow((1 - Math.Pow(b, 2) / Math.Pow(a, 2)), 0.5);

                x -= dx;
                y -= dy;

                // Calculate the Meridional Arc
                double M = y / k0;

                // Calculate Footprint Latitude
                double mu = M / (a * (1.0 - Math.Pow(e, 2) / 4.0 - 3 * Math.Pow(e, 4) / 64.0 - 5 * Math.Pow(e, 6) / 256.0));
                double e1 = (1.0 - Math.Pow((1.0 - Math.Pow(e, 2)), 0.5)) / (1.0 + Math.Pow((1.0 - Math.Pow(e, 2)), 0.5));

                double J1 = (3 * e1 / 2 - 27 * Math.Pow(e1, 3) / 32.0);
                double J2 = (21 * Math.Pow(e1, 2) / 16 - 55 * Math.Pow(e1, 4) / 32.0);
                double J3 = (151 * Math.Pow(e1, 3) / 96.0);
                double J4 = (1097 * Math.Pow(e1, 4) / 512.0);

                double fp = mu + J1 * Math.Sin(2 * mu) + J2 * Math.Sin(4 * mu) + J3 * Math.Sin(6 * mu) + J4 * Math.Sin(8 * mu);

                // Calculate Latitude and Longitude

                double e2 = Math.Pow((e * a / b), 2);
                double C1 = Math.Pow(e2 * Math.Cos(fp), 2);
                double T1 = Math.Pow(Math.Tan(fp), 2);
                double R1 = a * (1 - Math.Pow(e, 2)) / Math.Pow((1 - Math.Pow(e, 2) * Math.Pow(Math.Sin(fp), 2)), (3.0 / 2.0));
                double N1 = a / Math.Pow((1 - Math.Pow(e, 2) * Math.Pow(Math.Sin(fp), 2)), 0.5);

                double D = x / (N1 * k0);

                // 計算緯度
                double Q1 = N1 * Math.Tan(fp) / R1;
                double Q2 = (Math.Pow(D, 2) / 2.0);
                double Q3 = (5 + 3 * T1 + 10 * C1 - 4 * Math.Pow(C1, 2) - 9 * e2) * Math.Pow(D, 4) / 24.0;
                double Q4 = (61 + 90 * T1 + 298 * C1 + 45 * Math.Pow(T1, 2) - 3 * Math.Pow(C1, 2) - 252 * e2) * Math.Pow(D, 6) / 720.0;
                double lat = fp - Q1 * (Q2 - Q3 + Q4);

                // 計算經度
                double Q5 = D;
                double Q6 = (1 + 2 * T1 + C1) * Math.Pow(D, 3) / 6;
                double Q7 = (5 - 2 * C1 + 28 * T1 - 3 * Math.Pow(C1, 2) + 8 * e2 + 24 * Math.Pow(T1, 2)) * Math.Pow(D, 5) / 120.0;
                double lon = lon0 + (Q5 - Q6 + Q7) / Math.Cos(fp);

                lat = (lat * 180) / Math.PI; //緯
                lon = (lon * 180) / Math.PI; //經


                string lonlat = lon + "," + lat;
                return lonlat;
            }


        }
    }

    public static class DataTableExtensions
    {
        public static IList<T> ToList<T>(this DataTable table) where T : new()
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            IList<T> result = new List<T>();

            //取得DataTable所有的row data
            foreach (var row in table.Rows)
            {
                var item = MappingItem<T>((DataRow)row, properties);

                result.Add(item);
            }

            return result;
        }

        private static T MappingItem<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            T item = new T();

            foreach (var property in properties)
            {
                if (row.Table.Columns.Contains(property.Name))
                {
                    //針對欄位的型態去轉換
                    if (property.PropertyType == typeof(DateTime))
                    {
                        DateTime dt = new DateTime();

                        if (DateTime.TryParse(row[property.Name].ToString(), out dt))
                        {
                            property.SetValue(item, dt, null);
                        }
                        else
                        {
                            property.SetValue(item, null, null);
                        }
                    }
                    else if (property.PropertyType == typeof(decimal))
                    {
                        decimal val = new decimal();

                        decimal.TryParse(row[property.Name].ToString(), out val);

                        property.SetValue(item, val, null);
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        double val = new double();

                        double.TryParse(row[property.Name].ToString(), out val);

                        property.SetValue(item, val, null);
                    }
                    else if (property.PropertyType == typeof (int))
                    {
                        int val = new int();

                        int.TryParse(row[property.Name].ToString(), out val);

                        property.SetValue(item, val, null);
                    }
                    else if (property.PropertyType == typeof(float))
                    {
                        float val = new float();

                        float.TryParse(row[property.Name].ToString(), out val);

                        property.SetValue(item, val, null);
                    }
                    else
                    {
                        if (row[property.Name] != DBNull.Value)
                        {
                            property.SetValue(item, row[property.Name], null);
                        }
                    }
                }
            }

            return item;
        }
    }
}
