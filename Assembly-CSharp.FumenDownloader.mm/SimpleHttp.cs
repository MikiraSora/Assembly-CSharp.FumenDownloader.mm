using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using static MU3.User.UserOption;

namespace DpPatches.FumenDownloader
{
    internal static class SimpleHttp
    {
        private static string tempFolder;

        static SimpleHttp()
        {
            ServicePointManager.ServerCertificateValidationCallback += MyRemoteCertificateValidationCallback;

            tempFolder = Path.Combine(Path.GetTempPath(), "FumenDownloader/Downloads");
            Directory.CreateDirectory(tempFolder);
        }

        static bool MyRemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public static bool GetString(string url, out string content)
        {
            content = string.Empty;
            var request = HttpWebRequest.Create(url);
            request.Method = "GET";

            try
            {
                PatchLog.WriteLine($"SimpleHttp.DownloadFile() get string started {url}");
                var response = request.GetResponse();
                var buffer = new byte[1024];

                using var ms = new MemoryStream();
                using var ns = response.GetResponseStream();

                while (true)
                {
                    var read = ns.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        break;
                    ms.Write(buffer, 0, read);
                }

                content = Encoding.UTF8.GetString(ms.ToArray());
                return true;
            }
            catch (Exception e)
            {
                PatchLog.WriteLine($"SimpleHttp.GetString() throw exception {url} : {e.Message}");
                return false;
            }
        }

        public static bool DownloadFile(string url, string savePath)
        {
            var request = HttpWebRequest.Create(url);
            request.Method = "GET";

            try
            {
                PatchLog.WriteLine($"SimpleHttp.DownloadFile() download file started {url} -> {savePath}");
                var response = request.GetResponse() as HttpWebResponse;

                if (response.StatusCode != HttpStatusCode.OK)
                    PatchLog.WriteLine($"http request {url} return statusCode {response.StatusCode}");

                var buffer = new byte[1024];

                using var fs = File.OpenWrite(savePath);
                using var ns = response.GetResponseStream();

                while (true)
                {
                    var read = ns.Read(buffer, 0, buffer.Length);
                    if (read <= 0)
                        break;
                    fs.Write(buffer, 0, read);
                }

                return true;
            }
            catch (Exception e)
            {
                PatchLog.WriteLine($"SimpleHttp.DownloadFile() throw exception {url} : {e.Message}");
                return false;
            }
        }

        static string GetFileNameFromResponse(WebResponse response)
        {
            string contentDisposition = response.Headers["Content-Disposition"];
            if (!string.IsNullOrEmpty(contentDisposition))
            {
                const string fileNameKey = "filename=";
                int index = contentDisposition.IndexOf(fileNameKey, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    string fileName = contentDisposition.Substring(index + fileNameKey.Length).Trim('"');
                    return fileName;
                }
            }
            return null;
        }
    }
}
