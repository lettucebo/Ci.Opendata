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
        /// <returns></returns>
        public static string GetGzFileContent(string url)
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
                var fi = new FileInfo(downloadPath);

                using (FileStream originalFileStream = fi.OpenRead())
                {
                    using (GZipStream decompressionStream = new GZipStream(originalFileStream, CompressionMode.Decompress))
                    {
                        using (var sr = new StreamReader(decompressionStream))
                        {
                            content = sr.ReadToEnd();
                        }
                    }
                }

                File.Delete(downloadPath);
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
    }
}
