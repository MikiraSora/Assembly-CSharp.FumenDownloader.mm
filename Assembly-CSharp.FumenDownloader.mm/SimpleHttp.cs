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
            string tempPath = null;

            try
            {
                PatchLog.WriteLine($"SimpleHttp.DownloadFile() download file started {url} -> {savePath}");

                using var webResponse = request.GetResponse();
                var response = webResponse as HttpWebResponse;
                if (response == null)
                {
                    PatchLog.WriteLine($"SimpleHttp.DownloadFile() non-http response {url}");
                    return false;
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    PatchLog.WriteLine($"http request {url} return statusCode {response.StatusCode}");
                    return false;
                }

                var folderPath = Path.GetDirectoryName(savePath);
                if (!string.IsNullOrEmpty(folderPath))
                    Directory.CreateDirectory(folderPath);

                // Write to a sibling temp file first, then replace.
                // File.OpenWrite/OpenOrCreate does not truncate, so a shorter
                // payload would leave stale trailing bytes (broken Music.xml).
                tempPath = savePath + ".tmp";
                var buffer = new byte[1024];
                var expectedLength = response.ContentLength;

                using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var ns = response.GetResponseStream())
                {
                    if (ns == null)
                    {
                        PatchLog.WriteLine($"SimpleHttp.DownloadFile() null response stream {url}");
                        return false;
                    }

                    while (true)
                    {
                        var read = ns.Read(buffer, 0, buffer.Length);
                        if (read <= 0)
                            break;
                        fs.Write(buffer, 0, read);
                    }

                    fs.Flush();

                    if (expectedLength >= 0 && fs.Length != expectedLength)
                    {
                        PatchLog.WriteLine($"SimpleHttp.DownloadFile() length mismatch {url}: expected {expectedLength}, actual {fs.Length}");
                        return false;
                    }
                }

                if (File.Exists(savePath))
                    File.Delete(savePath);
                File.Move(tempPath, savePath);
                tempPath = null;

                return true;
            }
            catch (Exception e)
            {
                PatchLog.WriteLine($"SimpleHttp.DownloadFile() throw exception {url} : {e.Message}");
                return false;
            }
            finally
            {
                if (tempPath != null)
                {
                    try
                    {
                        if (File.Exists(tempPath))
                            File.Delete(tempPath);
                    }
                    catch
                    {
                        // best-effort cleanup
                    }
                }
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
