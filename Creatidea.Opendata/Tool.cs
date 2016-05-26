using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Creatidea.Opendata
{
    public class Tool
    {
        /// <summary>
        /// 讀取網頁內容
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        //public static string GetWebContent(string url)
        //{
        //    var data = string.Empty;

        //    //將要取得HTML原如碼的網頁放在WebRequest.Create(@”網址” )
        //    var webRequest = WebRequest.Create(url);
        //    //Method選擇GET
        //    webRequest.Method = "GET";
        //    //取得WebRequest的回覆
        //    var webResponse = webRequest.GetResponse();
        //    //Streamreader讀取回覆
        //    var streamReader = new StreamReader(webResponse.GetResponseStream());
        //    //將全文轉成string
        //    data = streamReader.ReadToEnd();
        //    //關掉StreamReader
        //    streamReader.Close();

        //    return data;
        //}

        /// <summary>
        /// Downloads the content of the and get unzip file.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="needFileName">Name of the need file.</param>
        /// <returns></returns>
        public static Dictionary<string, string> DownloadAndGetUnzipFileContent(string url)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
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
        /// Compresses the specified json string.
        /// </summary>
        /// <param name="jsonStr">The json string.</param>
        /// <returns></returns>
        public byte[] Compress(string jsonStr)
        {
            MemoryStream zippedStream = new MemoryStream();
            using (var gzip = new GZipStream(zippedStream, CompressionMode.Compress))
            {
                var bfmt = new BinaryFormatter();
                bfmt.Serialize(gzip, jsonStr);
                gzip.Flush();
            }

            byte[] zippedBuf = zippedStream.ToArray();
            return zippedBuf;
        }

        /// <summary>
        /// Decompresses the specified zipped json.
        /// </summary>
        /// <param name="zippedJson">The zipped json.</param>
        /// <returns></returns>
        public string Decompress(byte[] zippedJson)
        {
            string jsonStr;

            var zippedStream = new MemoryStream(zippedJson);
            using (var gzip = new GZipStream(zippedStream, CompressionMode.Decompress))
            {
                BinaryFormatter bfmt = new BinaryFormatter();
                jsonStr = (string)bfmt.Deserialize(gzip);
            }

            return jsonStr;
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
    }
}
